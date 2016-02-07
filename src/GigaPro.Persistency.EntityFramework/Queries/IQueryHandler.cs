using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using GigaSpaces.Core.Persistency;

namespace GigaPro.Persistency.EntityFramework.Queries
{
    public interface IQueryHandler
    {
        bool CanParse(Query q);

        IQueryable Parse(Query query, DbContext context, IEnumerable<ExtendedEntityType> dbEntities, out ExtendedEntityType foundType);
    }
}