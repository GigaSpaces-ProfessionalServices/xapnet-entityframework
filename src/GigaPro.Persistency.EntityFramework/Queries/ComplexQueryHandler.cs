using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
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

            if (components.Length > 2)
            {
                IList<EqualityExpression> expressions = new List<EqualityExpression>();

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

            throw new NotImplementedException();
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
                    modifiedPosition += 1;

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