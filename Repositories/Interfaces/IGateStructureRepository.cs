using knkwebapi_v2.Models;

namespace knkwebapi_v2.Repositories
{
    public interface IGateStructureRepository
    {
        Task<IEnumerable<GateStructure>> GetAllAsync();
        Task<GateStructure?> GetByIdAsync(int id);
        Task AddGateStructureAsync(GateStructure gateStructure);
        Task UpdateGateStructureAsync(GateStructure gateStructure);
        Task DeleteGateStructureAsync(int id);
        Task<PagedResult<GateStructure>> SearchAsync(PagedQuery query);
    }
}
