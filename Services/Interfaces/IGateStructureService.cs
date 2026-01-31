using System.Collections.Generic;
using System.Threading.Tasks;
using knkwebapi_v2.Dtos;

namespace knkwebapi_v2.Services
{
    public interface IGateStructureService
    {
        Task<IEnumerable<GateStructureDto>> GetAllAsync();
        Task<GateStructureDto?> GetByIdAsync(int id);
        Task<GateStructureDto?> GetByIdWithSnapshotsAsync(int id);
        Task<IEnumerable<GateStructureDto>> GetGatesByDomainAsync(int domainId);
        Task<GateStructureDto> CreateAsync(GateStructureDto gateStructureDto);
        Task UpdateAsync(int id, GateStructureDto gateStructureDto);
        Task DeleteAsync(int id);
        Task<PagedResultDto<GateStructureListDto>> SearchAsync(PagedQueryDto query);
        
        // Gate-specific operations
        Task<IEnumerable<GateStructureDto>> GetActiveGatesAsync();
        Task UpdateHealthAsync(int id, double newHealth);
        Task UpdateStateAsync(int id, bool isOpened, bool isDestroyed);
        
        // Block snapshot operations
        Task<IEnumerable<GateBlockSnapshotDto>> GetBlockSnapshotsAsync(int gateId);
        Task AddBlockSnapshotsAsync(int gateId, IEnumerable<GateBlockSnapshotDto> snapshots);
        Task AddBlockSnapshotsAsync(int gateId, IEnumerable<GateBlockSnapshotCreateDto> snapshots);
        Task ClearBlockSnapshotsAsync(int gateId);
    }
}
