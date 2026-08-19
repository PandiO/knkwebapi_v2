using System.Threading.Tasks;
using knkwebapi_v2.Models;

namespace knkwebapi_v2.Repositories.Interfaces;

public interface IGameSettingsRepository
{
    Task<GameSettings?> GetSingletonAsync();
    Task<GameSettings> UpsertAsync(GameSettings settings);
}
