using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using GigaPro.Persistency.EntityFramework.Queries.Domain;
using GigaSpaces.Core.Persistency;

namespace GigaPro.Persistency.EntityFramework.Queries
{
    public class ComplexQueryHandler
    {
        private static readonly Regex WhiteSpace = new Regex("\\s+", RegexOptions.Compiled);
        
        public IQueryable Handle(Query query, DbContext context, IEnumerable<ExtendedEntityType> dbEntities, out ExtendedEntityType foundType)
        {
            var sqlQuery = (query.SqlQuery ?? string.Empty).Trim();
            var components = WhiteSpace.Split(sqlQuery);
            var entityName = components[1];
            foundType = null;


            IList<EqualityExpression> expressions = new List<EqualityExpression>();

            if (components.Length > 2)
            {

                var position = 3;
                while (position < components.Length)
                {
                    var equalityExpression = new EqualityExpression();
                    position += EvaluateComponent(position, components, equalityExpression);
                    expressions.Add(equalityExpression);
                }
            }

            FindType(dbEntities, ref foundType, entityName);
            ThrowIfTypeNotFound(foundType, entityName);

            IQueryable table = context.Set(foundType.Type);

            if (expressions.Count > 0)
                table = BuildQuery(table, foundType, expressions, query.Parameters);

            return table;
        }

        private IQueryable BuildQuery(IQueryable table, ExtendedEntityType entityType, IList<EqualityExpression> expressions, object[] parameters)
        {
            var workingTable = table;

            var parameterCount = 0;
            var firstExpression = true;
            foreach (var ex in expressions)
            {
                var parameterExpression = Expression.Parameter(entityType.Type, "p");
                var memberExpression = Expression.Property(parameterExpression, entityType.GetPropertyInfo(ex.LeftHand));

                Expression rightHandSide = null;
                if (ex.RightHand is ParameterRightHand)
                {
                    rightHandSide = Expression.Constant(parameters[parameterCount]);
                    parameterCount++;
                }

                Expression languageExpression = null;

                switch (ex.Operator)
                {
                    case Operators.Equals:
                        languageExpression = Expression.Equal(memberExpression, rightHandSide);
                        break;
                    case Operators.NotEquals:
                        break;
                    case Operators.GreaterThan:
                        break;
                    case Operators.LessThan:
                        break;
                    case Operators.GreaterThanOrEqual:
                        break;
                    case Operators.LessThanOrEqual:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (firstExpression)
                {
                    var lambdaExpression = Expression.Lambda(languageExpression, parameterExpression);
                    workingTable = Queryable.Where((dynamic)workingTable, (dynamic)lambdaExpression);
                    firstExpression = false;
                }
                else
                {
                    // TODO:
                }
            }

            return workingTable;
        }

        private static int EvaluateComponent(int position, string[] components, EqualityExpression equalityExpression)
        {
            var output = position;

            if (string.Equals("and", components[position], StringComparison.InvariantCultureIgnoreCase))
            {
                equalityExpression.LogicalOperator = LogicalOperator.And;
                output = EvaluateComponent(position, components, equalityExpression);
            }
            else if (string.Equals("or", components[position], StringComparison.InvariantCultureIgnoreCase))
            {
                equalityExpression.LogicalOperator = LogicalOperator.Or;
                output = EvaluateComponent(position, components, equalityExpression);
            }
            else
            {
                equalityExpression.LeftHand = components[position];

                var modifiedPosition = position + 1;

                if (string.Equals("is", components[modifiedPosition], StringComparison.InvariantCultureIgnoreCase))
                {
                    modifiedPosition += 1;

                    if (string.Equals("not", components[modifiedPosition], StringComparison.InvariantCultureIgnoreCase))
                    {
                        equalityExpression.RightHand = new NotNullRightHand();
                    }
                    else
                    {
                        equalityExpression.RightHand = new NullRightHand();
                    }
                }
                else
                {
                    equalityExpression.Operator = ParseOperator(components[modifiedPosition]);

                    modifiedPosition += 1;

                    if (string.Equals("?", components[modifiedPosition], StringComparison.InvariantCultureIgnoreCase))
                    {
                        equalityExpression.RightHand = new ParameterRightHand();
                    }
                    else
                    {
                        equalityExpression.RightHand = new ValueRightHand(components[modifiedPosition]);
                    }
                }

                output = modifiedPosition;
            }

            return output;
        }

        private static Operators ParseOperator(string operatorText)
        {
            var output = Operators.Equals;

            if (string.Equals("=", operatorText, StringComparison.InvariantCultureIgnoreCase))
                output = Operators.Equals;
            else if (string.Equals("<>", operatorText, StringComparison.InvariantCultureIgnoreCase))
                output = Operators.NotEquals;
            else if (string.Equals("<", operatorText, StringComparison.InvariantCultureIgnoreCase))
                output = Operators.LessThan;
            else if (string.Equals(">", operatorText, StringComparison.InvariantCultureIgnoreCase))
                output = Operators.GreaterThan;
            else if (string.Equals(">=", operatorText, StringComparison.InvariantCultureIgnoreCase))
                output = Operators.GreaterThanOrEqual;
            else if (string.Equals("<=", operatorText, StringComparison.InvariantCultureIgnoreCase))
                output = Operators.LessThanOrEqual;

            return output;
        }

        private static void FindType(IEnumerable<ExtendedEntityType> dbEntities, ref ExtendedEntityType foundType, string typeName)
        {
            foreach (var extendedEntityType in dbEntities)
            {
                if (extendedEntityType.IsType(typeName))
                {
                    foundType = extendedEntityType;
                    break;
                }
            }
        }

        private static void ThrowIfTypeNotFound(ExtendedEntityType foundType, string typeName)
        {
            if (foundType == null)
            {
                throw new TypeLoadException("Cound not find type: " + typeName);
            }
        }
    }
}