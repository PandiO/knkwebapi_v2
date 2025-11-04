using System.Collections.Generic;
using System.Threading.Tasks;
using knkwebapi_v2.Models;

namespace knkwebapi_v2.Services
{
    public interface ICategoryService
    {
        Task<IEnumerable<Category>> GetAllAsync();
        Task<Category?> GetByIdAsync(int id);
        Task<Category> CreateAsync(Category category);
        Task UpdateAsync(int id, Category category);
        Task DeleteAsync(int id);
    }
}
