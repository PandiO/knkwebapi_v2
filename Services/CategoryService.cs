using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using knkwebapi_v2.Dtos;
using knkwebapi_v2.Models;
using knkwebapi_v2.Repositories;

namespace knkwebapi_v2.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _repo;
        private readonly IMapper _mapper;

        public CategoryService(ICategoryRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CategoryDto>> GetAllAsync()
        {
            var categories = await _repo.GetAllAsync();
            return _mapper.Map<IEnumerable<CategoryDto>>(categories);
        }

        public async Task<CategoryDto?> GetByIdAsync(int id)
        {
            if (id <= 0) return null;
            var category = await _repo.GetByIdAsync(id);
            return _mapper.Map<CategoryDto>(category);
        }

        public async Task<CategoryDto> CreateAsync(CategoryDto categoryDto)
        {
            if (categoryDto == null) throw new ArgumentNullException(nameof(categoryDto));
            if (string.IsNullOrWhiteSpace(categoryDto.Name)) throw new ArgumentException("Category name is required.", nameof(categoryDto));

            // Validate parent category if provided
            if (categoryDto.ParentCategoryId.HasValue && categoryDto.ParentCategoryId > 0)
            {
                var parentExists = await _repo.GetByIdAsync(categoryDto.ParentCategoryId.Value);
                if (parentExists == null)
                    throw new ArgumentException($"Parent Category with id {categoryDto.ParentCategoryId} not found.", nameof(categoryDto));
            }

            var category = _mapper.Map<Category>(categoryDto);
            await _repo.AddCategoryAsync(category);
            return _mapper.Map<CategoryDto>(category);
        }

        public async Task UpdateAsync(int id, CategoryDto categoryDto)
        {
            if (categoryDto == null) throw new ArgumentNullException(nameof(categoryDto));
            if (id <= 0) throw new ArgumentException("Invalid id.", nameof(id));
            if (string.IsNullOrWhiteSpace(categoryDto.Name)) throw new ArgumentException("Category name is required.", nameof(categoryDto));
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) throw new KeyNotFoundException($"Category with id {id} not found.");

            // Validate parent category if changed
            if (categoryDto.ParentCategoryId.HasValue && categoryDto.ParentCategoryId > 0 &&
                (!existing.ParentCategoryId.HasValue || existing.ParentCategoryId != categoryDto.ParentCategoryId))
            {
                // Prevent circular reference (parent cannot be the category itself)
                if (categoryDto.ParentCategoryId == id)
                    throw new ArgumentException("A category cannot be its own parent.", nameof(categoryDto));

                var parentExists = await _repo.GetByIdAsync(categoryDto.ParentCategoryId.Value);
                if (parentExists == null)
                    throw new ArgumentException($"Parent Category with id {categoryDto.ParentCategoryId} not found.", nameof(categoryDto));
            }

            // Apply allowed updates
            existing.Name = categoryDto.Name;
            existing.ItemtypeId = categoryDto.ItemtypeId;
            existing.ParentCategoryId = categoryDto.ParentCategoryId;

            await _repo.UpdateCategoryAsync(existing);
        }

        public async Task DeleteAsync(int id)
        {
            if (id <= 0) throw new ArgumentException("Invalid id.", nameof(id));
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) throw new KeyNotFoundException($"Category with id {id} not found.");

            await _repo.DeleteCategoryAsync(id);
        }

        public async Task<PagedResultDto<CategoryListDto>> SearchAsync(PagedQueryDto queryDto)
        {
            if (queryDto == null) throw new ArgumentNullException(nameof(queryDto));

            var query = _mapper.Map<PagedQuery>(queryDto);

            // Execute search
            var result = await _repo.SearchAsync(query);

            // Map result to DTO
            var resultDto = _mapper.Map<PagedResultDto<CategoryListDto>>(result);

            return resultDto;
        }
    }
}
