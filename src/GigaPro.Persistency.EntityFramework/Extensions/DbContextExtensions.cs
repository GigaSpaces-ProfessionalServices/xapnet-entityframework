using System;
using System.Collections;
using System.Data.Entity;
using GigaSpaces.Core.Persistency;

namespace GigaPro.Persistency.EntityFramework.Extensions
{
    public static class DbContextExtensions
    {
        public static IEnumerable GigaSpaceQuery(this DbContext context, Query query)
        {
            throw new NotImplementedException();
        }

        public static void Merge(this DbContext context, object entity)
        {
            throw new NotImplementedException();
        }
    }
}