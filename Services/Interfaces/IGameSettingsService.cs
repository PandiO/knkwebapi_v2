using System.Threading.Tasks;
using knkwebapi_v2.Dtos;

namespace knkwebapi_v2.Services.Interfaces;

public interface IGameSettingsService
{
    Task<GameSettingsReadDto> GetAsync();
    Task<GameSettingsReadDto> UpdateAsync(GameSettingsUpdateDto dto);
    Task<GameSettingsReadDto> UpdateRuntimeWorldsAsync(GameSettingsRuntimeWorldsUpdateDto dto);
}
