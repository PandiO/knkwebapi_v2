using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using knkwebapi_v2.Attributes;
using knkwebapi_v2.Dtos;

namespace knkwebapi_v2.Services
{
    public class MetadataService : IMetadataService
    {
        private readonly List<EntityMetadataDto> _cachedMetadata;

        public MetadataService()
        {
            _cachedMetadata = ScanEntities();
        }

        public List<EntityMetadataDto> GetAllEntityMetadata()
        {
            return _cachedMetadata;
        }

        public List<string> GetEntityNames()
        {
            return _cachedMetadata.Select(e => e.EntityName).ToList();
        }

        public EntityMetadataDto? GetEntityMetadata(string entityName)
        {
            return _cachedMetadata.FirstOrDefault(e => 
                e.EntityName.Equals(entityName, StringComparison.OrdinalIgnoreCase));
        }

        private List<EntityMetadataDto> ScanEntities()
        {
            var metadata = new List<EntityMetadataDto>();
            var assembly = Assembly.GetExecutingAssembly();

            var entityTypes = assembly.GetTypes()
                .Where(t => t.GetCustomAttribute<FormConfigurableEntityAttribute>() != null);

            foreach (var entityType in entityTypes)
            {
                var attribute = entityType.GetCustomAttribute<FormConfigurableEntityAttribute>();
                if (attribute == null) continue;

                var entityMetadata = new EntityMetadataDto
                {
                    EntityName = entityType.Name,
                    DisplayName = attribute.DisplayName,
                    Fields = GetFieldMetadata(entityType)
                };

                metadata.Add(entityMetadata);
            }

            return metadata;
        }

        private List<FieldMetadataDto> GetFieldMetadata(Type entityType)
        {
            var fields = new List<FieldMetadataDto>();
            var properties = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var property in properties)
            {
                var relatedEntityAttr = property.GetCustomAttribute<RelatedEntityFieldAttribute>();
                var fieldType = property.PropertyType;
                var underlyingType = Nullable.GetUnderlyingType(fieldType);
                var isNullable = underlyingType != null || !fieldType.IsValueType;

                var fieldMetadata = new FieldMetadataDto
                {
                    FieldName = property.Name,
                    FieldType = (underlyingType ?? fieldType).Name,
                    IsNullable = isNullable,
                    IsRelatedEntity = relatedEntityAttr != null,
                    RelatedEntityType = relatedEntityAttr?.RelatedEntityType.Name
                };

                fields.Add(fieldMetadata);
            }

            return fields;
        }
    }
}
