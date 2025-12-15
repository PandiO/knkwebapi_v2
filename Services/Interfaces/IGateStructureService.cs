using System.Collections.Generic;
using System.Threading.Tasks;
using knkwebapi_v2.Dtos;

namespace knkwebapi_v2.Services
{
    public interface IGateStructureService
    {
        Task<IEnumerable<GateStructureDto>> GetAllAsync();
        Task<GateStructureDto?> GetByIdAsync(int id);
        Task<GateStructureDto> CreateAsync(GateStructureDto gateStructureDto);
        Task UpdateAsync(int id, GateStructureDto gateStructureDto);
        Task DeleteAsync(int id);
        Task<PagedResultDto<GateStructureListDto>> SearchAsync(PagedQueryDto query);
    }
}
