using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text.RegularExpressions;
using GigaSpaces.Core.Persistency;

namespace GigaPro.Persistency.EntityFramework.Queries
{
    internal class SimpleFromQueryHandler : IQueryHandler
    {
        private readonly Regex _pattern = new Regex("(?<=FROM(\\s)+)[\\w\\.]+\\s{0,}$", RegexOptions.Compiled);

        public bool CanParse(Query q)
        {
            return _pattern.IsMatch(q.SqlQuery);
        }

        public IQueryable Parse(Query query, DbContext context, IEnumerable<ExtendedEntityType> dbEntities,
            out ExtendedEntityType foundType)
        {
            foundType = null;
            var match = _pattern.Match(query.SqlQuery);
            
            FindType(dbEntities, ref foundType, match);

            ThrowIfTypeNotFound(foundType, match);
            return context.Set(foundType.Type);
        }

        private static void FindType(IEnumerable<ExtendedEntityType> dbEntities, ref ExtendedEntityType foundType, Match match)
        {
            foreach (var extendedEntityType in dbEntities)
            {
                if (extendedEntityType.IsType(match.Value.Trim()))
                {
                    foundType = extendedEntityType;
                    break;
                }
            }
        }

        private static void ThrowIfTypeNotFound(ExtendedEntityType foundType, Match match)
        {
            if (foundType == null)
            {
                throw new TypeLoadException("Cound not find type: " + match.Value);
            }
        }
    }
}