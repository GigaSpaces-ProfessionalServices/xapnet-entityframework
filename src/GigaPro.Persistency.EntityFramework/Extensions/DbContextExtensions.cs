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

        public static void Merge(this DbContext context, object entity)
        {
            throw new NotImplementedException();
        }
    }
}