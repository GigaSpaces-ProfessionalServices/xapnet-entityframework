using System.Collections;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace GigaPro.Persistency.EntityFramework.Extensions
{
    public static class QueryableExtensions
    {
        /// <summary>
        /// Performs a paged query against the database.
        /// </summary>
        /// <param name="source">The sorce query.</param>
        /// <param name="entity">The entity to query against.</param>
        /// <param name="skip">The number of records to skip in the query.</param>
        /// <param name="batch">The number of records to return in the query.</param>
        /// <returns>The query results.</returns>
        public static IList LoadPage(this IQueryable source, ExtendedEntityType entity, int skip, int batch)
        {
            var output = source;
            output = output.Order(entity);
            output = output.Skip(skip);
            output = output.Take(batch);

            return output.ToList();
        }

        private static Expression CreateExpression(ExtendedEntityType entity, PropertyInfo pi)
        {
            var parameterExpression = Expression.Parameter(entity.Type, "x");
            var memberExpression = Expression.Property(parameterExpression, pi);
            return Expression.Lambda(memberExpression, parameterExpression);
        }

        public static IQueryable Order(this IQueryable source, ExtendedEntityType entity)
        {
            var output = source;

            var keyProperties = entity.KeyProperties;
            for (var x = 0; x < keyProperties.Length; x++)
            {
                if (x == 0)
                    output = Queryable.OrderBy((dynamic) output, (dynamic)CreateExpression(entity, keyProperties[x]));
                else
                    output = Queryable.ThenBy((dynamic) output, (dynamic)CreateExpression(entity, keyProperties[x]));
            }

            return output;
        }

        public static IQueryable Skip(this IQueryable source, int skip)
        {
            return Queryable.Skip((dynamic) source, skip);
        }

        public static IQueryable Take(this IQueryable source, int batch)
        {
            return Queryable.Take((dynamic) source, batch);
        }

        /// <summary>
        /// Executes the query and adds the results to an <see cref="IList"/>, thus closing the database reader.
        /// </summary>
        /// <param name="source">The database query.</param>
        /// <returns>The result sets.</returns>
        public static IList ToList(this IQueryable source)
        {
            IList output = new ArrayList();

            foreach (var obj in source)
            {
                output.Add(obj);
            }

            return output;
        }
    }
}