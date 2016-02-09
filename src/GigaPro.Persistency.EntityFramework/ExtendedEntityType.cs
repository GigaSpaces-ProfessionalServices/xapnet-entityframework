using System;
using System.Data.Entity.Core.Metadata.Edm;
using System.Reflection;

namespace GigaPro.Persistency.EntityFramework
{
    public class ExtendedEntityType
    {
        private PropertyInfo[] _keyProperties;

        /// <summary>
        /// Gets the .NET type for this entity.
        /// </summary>
        public Type Type { get; }

        /// <summary>
        ///  Gets the EntityFramework EDM metadata information for this entity.
        /// </summary>
        public EntityType EdmEntityType { get; }

        /// <summary>
        /// Gets the <see cref="PropertyInfo"/> for all members used as keys for the entity.
        /// </summary>
        public PropertyInfo[] KeyProperties
        {
            get
            {
                if (_keyProperties == null)
                {
                    var edmKeyProperties = EdmEntityType.KeyProperties;
                    var output = new PropertyInfo[edmKeyProperties.Count];

                    for (var x = 0; x < edmKeyProperties.Count; x++)
                    {
                        var edmProperty = edmKeyProperties[x];
                        output[x] = GetPropertyInfo(edmProperty.Name);
                    }

                    _keyProperties = output;
                }

                var propertyOutput = new PropertyInfo[_keyProperties.Length];
                _keyProperties.CopyTo(propertyOutput, 0);
                return propertyOutput;
            }
        }

        /// <summary>
        /// Creates a new instance of <see cref="EdmEntityType"/>.
        /// </summary>
        /// <param name="type">The .NET type for this entity.</param>
        /// <param name="edmEntityType">The EntityFramework EDM metadata for this entity.</param>
        public ExtendedEntityType(Type type, EntityType edmEntityType)
        {
            Type = type;
            EdmEntityType = edmEntityType;
        }

        /// <summary>
        /// Determines if this entity represents <paramref name="typeFullName"/>.
        /// </summary>
        /// <param name="typeFullName">The full name of the parameter to check.</param>
        /// <returns>True if the type's full name matches <paramref name="typeFullName"/>; otherwise False.</returns>
        public bool IsType(string typeFullName)
        {
            var workingCopy = typeFullName ?? string.Empty;

            return string.Equals(workingCopy.Trim(), Type.FullName, StringComparison.InvariantCultureIgnoreCase);
        }

        public PropertyInfo GetPropertyInfo(string propertyName)
        {
            var propertyInfo = Type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);

            if (propertyInfo == null)
            {
                 throw new Exception($"Property '{propertyName}' doesn't exist.");
            }

            return propertyInfo;
        }
    }
}