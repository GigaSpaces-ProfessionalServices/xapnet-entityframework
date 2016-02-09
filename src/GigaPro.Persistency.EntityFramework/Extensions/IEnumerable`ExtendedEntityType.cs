using System;
using System.Collections.Generic;
using System.Linq;

namespace GigaPro.Persistency.EntityFramework.Extensions
{
    public static class EnumerableExtendedEntityType
    {
        /// <summary>
        /// Finds the entity based on the <paramref name="entityName"/>.
        /// </summary>
        /// <param name="entities">The list of entities to search.</param>
        /// <param name="entityName">The name of the entity to find.</param>
        /// <returns>The <see cref="ExtendedEntityType"/> with a matching name.</returns>
        public static ExtendedEntityType FindType(this IEnumerable<ExtendedEntityType> entities, string entityName)
        {
            var output = entities.FirstOrDefault(entity => entity.IsType(entityName));

            if(output == null)
                throw new TypeLoadException($"Could not find type: {entityName}");

            return output;
        }
    }
}