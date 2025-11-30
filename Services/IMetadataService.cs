using System.Collections.Generic;
using knkwebapi_v2.Dtos;

namespace knkwebapi_v2.Services
{
    public interface IMetadataService
    {
        List<EntityMetadataDto> GetAllEntityMetadata();
        EntityMetadataDto? GetEntityMetadata(string entityName);
    }
}
