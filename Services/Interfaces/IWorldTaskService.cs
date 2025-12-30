using knkwebapi_v2.Dtos;

namespace knkwebapi_v2.Services.Interfaces
{
    public interface IWorldTaskService
    {
        Task<WorldTaskReadDto> CreateAsync(WorldTaskCreateDto dto);
        Task<WorldTaskReadDto?> GetByIdAsync(int id);
        Task<WorldTaskReadDto?> GetByLinkCodeAsync(string linkCode);
        Task<List<WorldTaskReadDto>> ListByStatusAsync(string status, string? serverId = null);
        Task<PagedResultDto<WorldTaskReadDto>> SearchAsync(PagedQueryDto query);
        Task<List<WorldTaskReadDto>> GetBySessionAsync(int sessionId);
        Task<PagedResultDto<WorldTaskReadDto>> GetByUserAsync(int userId, PagedQueryDto query);
        Task<WorldTaskReadDto> ClaimAsync(int id, ClaimTaskDto dto);
        Task<WorldTaskReadDto> CompleteAsync(int id, CompleteTaskDto dto);
        Task<WorldTaskReadDto> FailAsync(int id, FailTaskDto dto);
        Task<WorldTaskReadDto> UpdateStatusAsync(int id, string status, string? payloadJson = null);
        Task DeleteAsync(int id);
    }
}
