using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Text;
using GigaPro.Persistency.EntityFramework.Collections;
using GigaPro.Persistency.EntityFramework.Queries;
using GigaSpaces.Core.Persistency;

namespace GigaPro.Persistency.EntityFramework.Extensions
{
    public static class DbContextExtensions
    {
        private static readonly IList<IQueryHandler> QueryHandlers = new List<IQueryHandler> { new SimpleFromQueryHandler()};
         
        /// <summary>
        /// Translates a <see cref="Query"/> and then performs the translation against the EntityFramework database context.
        /// </summary>
        /// <param name="context">The EntityFramework database context.</param>
        /// <param name="dbContextTypes">A list of entity types for use during translation.</param>
        /// <param name="query">The GigaSpaces query to translate.</param>
        /// <param name="batchSize">The batch size for querying the database.</param>
        /// <returns></returns>
        public static IDataEnumerator GigaSpaceQuery(this DbContext context, IEnumerable<ExtendedEntityType> dbContextTypes, Query query, int batchSize)
        {

            foreach (var handler in QueryHandlers)
            {
                if (handler.CanParse(query))
                {
                    ExtendedEntityType targetType;
                    return new EntityFrameworkBatchedEnumerator(handler.Parse(query, context, dbContextTypes, out targetType), targetType, batchSize);
                }
            }

            throw new InvalidOperationException(FormatInvalidOperationMessage(query));
        }

        private static string FormatInvalidOperationMessage(Query query)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("Unsupported SqlQuery - Query information below.");
            builder.AppendFormat("\n\n**** SqlQuery [{0}]****\n", query.SqlQuery);

            if (query.Parameters != null)
            {
                foreach (var parameter in query.Parameters)
                {
                    builder.AppendFormat("\n**** SqlParameter [{0}]****\n", parameter);
                }
            }

            return builder.ToString();
        }
    }
}