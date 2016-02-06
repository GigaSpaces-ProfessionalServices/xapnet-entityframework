using System;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;

namespace GigaPro.Persistency.EntityFramework
{
    public class ExtendedEntityType
    {
        private PropertyInfo[] _keyProperties = null;
        public Type Type { get; }
        public EntityType EdmEntityType { get; }

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
                        output[x] = Type.GetProperty(edmProperty.Name, BindingFlags.Public | BindingFlags.Instance);
                    }

                    _keyProperties = output;
                }

                return _keyProperties;
            }
        }

        public ExtendedEntityType(Type type, EntityType edmEntityType)
        {
            Type = type;
            EdmEntityType = edmEntityType;
        }
    }
}