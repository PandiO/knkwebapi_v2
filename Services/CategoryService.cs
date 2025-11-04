using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using knkwebapi_v2.Models;
using knkwebapi_v2.Repositories;

namespace knkwebapi_v2.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _repo;

        public CategoryService(ICategoryRepository repo)
        {
            _repo = repo;
        }

        public async Task<IEnumerable<Category>> GetAllAsync()
        {
            return await _repo.GetAllAsync();
        }

        public async Task<Category?> GetByIdAsync(int id)
        {
            if (id <= 0) return null;
            return await _repo.GetByIdAsync(id);
        }

        public async Task<Category> CreateAsync(Category category)
        {
            if (category == null) throw new ArgumentNullException(nameof(category));
            if (string.IsNullOrWhiteSpace(category.Name)) throw new ArgumentException("Category name is required.", nameof(category));

            await _repo.AddCategoryAsync(category);
            return category; // EF will populate Id after SaveChanges in repository
        }

        public async Task UpdateAsync(int id, Category category)
        {
            if (category == null) throw new ArgumentNullException(nameof(category));
            if (id <= 0) throw new ArgumentException("Invalid id.", nameof(id));
            if (string.IsNullOrWhiteSpace(category.Name)) throw new ArgumentException("Category name is required.", nameof(category));

            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) throw new KeyNotFoundException($"Category with id {id} not found.");

            // Apply allowed updates (simple example)
            existing.Name = category.Name;
            existing.ItemtypeId = category.ItemtypeId;
            existing.ParentCategoryId = category.ParentCategoryId;

            await _repo.UpdateCategoryAsync(existing);
        }

        public async Task DeleteAsync(int id)
        {
            if (id <= 0) throw new ArgumentException("Invalid id.", nameof(id));
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) throw new KeyNotFoundException($"Category with id {id} not found.");

            await _repo.DeleteCategoryAsync(id);
        }
    }
}
