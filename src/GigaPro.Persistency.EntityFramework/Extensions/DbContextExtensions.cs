using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using GigaPro.Persistency.EntityFramework.Collections;
using GigaSpaces.Core.Persistency;

namespace GigaPro.Persistency.EntityFramework.Extensions
{
    public static class DbContextExtensions
    {
        private static readonly Regex WhiteSpace = new Regex("\\s+", RegexOptions.Compiled);
         
        /// <summary>
        /// Translates a <see cref="Query"/> and then performs the translation against the EntityFramework database context.
        /// </summary>
        /// <param name="context">The EntityFramework database context.</param>
        /// <param name="dbEntityTypes">A list of entity types for use during translation.</param>
        /// <param name="query">The GigaSpaces query to translate.</param>
        /// <param name="batchSize">The batch size for querying the database.</param>
        /// <returns></returns>
        public static IDataEnumerator GigaSpaceQuery(this DbContext context, IEnumerable<ExtendedEntityType> dbEntityTypes, Query query, int batchSize)
        {
            var sqlQuery = (query.SqlQuery ?? string.Empty).Trim();
            var components = WhiteSpace.Split(sqlQuery);
            var targetType = dbEntityTypes.FindType(components[1]);

            var parameterExpression = Expression.Parameter(targetType.Type, "p");

            Expression workingExpression = null;
            if (components.Length > 2)
            {
                var componentPosition = 3;
                var paramPosition = 0;

                workingExpression = ParseExpression(ref componentPosition, ref paramPosition, parameterExpression, targetType, components, query.Parameters);

                while (componentPosition < components.Length)
                {
                    if ((componentPosition < (components.Length - 1)) && "or".Equals(components[componentPosition], StringComparison.InvariantCultureIgnoreCase))
                    {
                        componentPosition++;
                        workingExpression = Expression.Or(workingExpression, ParseExpression(ref componentPosition, ref paramPosition, parameterExpression, targetType, components, query.Parameters));
                    }
                }
            }

            IQueryable table = context.Set(targetType.Type);

            if (workingExpression != null)
                table = Queryable.Where((dynamic)table, (dynamic)Expression.Lambda(workingExpression, parameterExpression));
            
            return new EntityFrameworkBatchedEnumerator(table, targetType, batchSize);
        }

        private static Expression ParseExpression(ref int componentPosition, ref int paramPosition, ParameterExpression parameterExpression, ExtendedEntityType foundType, string[] components, object[] parameters)
        {
            Expression output = null;

            while (componentPosition < components.Length)
            {
                var propertyName = components[componentPosition];
                componentPosition++;

                var memberExpression = Expression.Property(parameterExpression, foundType.GetPropertyInfo(propertyName));

                var op = components[componentPosition];
                componentPosition++;

                var tempExpression = BuildCriteriaExpression(memberExpression, op, ref componentPosition, ref paramPosition, components, parameters);

                output = output != null ? Expression.AndAlso(output, tempExpression) : tempExpression;
            }

            return output;
        }

        private static Expression BuildCriteriaExpression(MemberExpression memberExpression, string @operator, ref int componentPosition, ref int paramPosition, IReadOnlyList<string> components, IReadOnlyList<object> parameters)
        {
            Expression output = null;

            switch (@operator.ToLower())
            {
                case "=":
                    output = Expression.Equal(memberExpression, Expression.Constant(parameters[paramPosition]));
                    paramPosition++;
                    componentPosition++;
                    break;
                case "<>":
                    output = Expression.NotEqual(memberExpression, Expression.Constant(parameters[paramPosition]));
                    paramPosition++;
                    componentPosition++;
                    break;
                case "<":
                    output = Expression.LessThan(memberExpression, Expression.Constant(parameters[paramPosition]));
                    paramPosition++;
                    componentPosition++;
                    break;
                case ">":
                    output = Expression.GreaterThan(memberExpression, Expression.Constant(parameters[paramPosition]));
                    paramPosition++;
                    componentPosition++;
                    break;
                case ">=":
                    output = Expression.GreaterThanOrEqual(memberExpression,
                        Expression.Constant(parameters[paramPosition]));
                    paramPosition++;
                    componentPosition++;
                    break;
                case "<=":
                    output = Expression.LessThanOrEqual(memberExpression, Expression.Constant(parameters[paramPosition]));
                    paramPosition++;
                    componentPosition++;
                    break;
                case "is":
                    if ("not".Equals(components[componentPosition], StringComparison.InvariantCultureIgnoreCase))
                    {
                        // incrememnt by two
                        output = Expression.NotEqual(memberExpression, Expression.Constant(null));
                        componentPosition += 2;
                    }
                    else
                    {
                        // incrememnt by 1
                        output = Expression.Equal(memberExpression, Expression.Constant(null));
                        componentPosition++;
                    }

                    break;
            }

            return output;
        }

    }
}