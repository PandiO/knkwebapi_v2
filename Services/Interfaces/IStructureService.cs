using System.Collections.Generic;
using System.Threading.Tasks;
using knkwebapi_v2.Dtos;

namespace knkwebapi_v2.Services
{
    public interface IStructureService
    {
        Task<IEnumerable<StructureDto>> GetAllAsync();
        Task<StructureDto?> GetByIdAsync(int id);
        Task<StructureDto> CreateAsync(StructureDto structureDto);
        Task UpdateAsync(int id, StructureDto structureDto);
        Task DeleteAsync(int id);
        Task<PagedResultDto<StructureListDto>> SearchAsync(PagedQueryDto query);
    }
}
