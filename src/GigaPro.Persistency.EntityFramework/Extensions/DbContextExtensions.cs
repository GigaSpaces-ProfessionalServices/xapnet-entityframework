using System.Collections.Generic;
using System.Data.Entity;
using GigaPro.Persistency.EntityFramework.Collections;
using GigaPro.Persistency.EntityFramework.Queries;
using GigaSpaces.Core.Persistency;

namespace GigaPro.Persistency.EntityFramework.Extensions
{
    public static class DbContextExtensions
    {
        private static readonly ComplexQueryHandler QueryHandler = new ComplexQueryHandler();
         
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

            ExtendedEntityType targetType;
            return new EntityFrameworkBatchedEnumerator(QueryHandler.Handle(query, context, dbContextTypes, out targetType), targetType, batchSize);
        }
    }
}