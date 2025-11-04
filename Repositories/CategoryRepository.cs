using knkwebapi_v2.Models;
using knkwebapi_v2.Properties;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Responsibility guidance (best practices):
/// - Repository: encapsulates data access only (CRUD, queries, EF Core usage). 
///   Do NOT contain business rules, orchestration, or HTTP concerns here.
/// - Service: contains business logic, validation beyond simple model validation, transaction orchestration,
///   and coordinates multiple repositories when needed.
/// - Controller: handles HTTP layer responsibilities (request/response mapping, model validation, auth),
///   calls services (not repositories) and shapes HTTP responses.
/// </summary>
namespace knkwebapi_v2.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly KnKDbContext _context;

        public CategoryRepository(KnKDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Category>> GetAllAsync()
        {
            return await _context.Categories.ToListAsync();
        }
        public async Task<Category?> GetByIdAsync(int id)
        {
            return await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
        }
        public async Task AddCategoryAsync(Category category)
        {
            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateCategoryAsync(Category category)
        {
            _context.Categories.Update(category);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteCategoryAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
            }
        }
    }
}