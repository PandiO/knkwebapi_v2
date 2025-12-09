using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using knkwebapi_v2.Dtos;
using knkwebapi_v2.Models;
using knkwebapi_v2.Repositories;
using knkwebapi_v2.Repositories.Interfaces;

namespace knkwebapi_v2.Services
{
    public class MinecraftMaterialRefService : IMinecraftMaterialRefService
    {
        private readonly IMinecraftMaterialRefRepository _repo;
        private readonly IMapper _mapper;

        public MinecraftMaterialRefService(IMinecraftMaterialRefRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<IEnumerable<MinecraftMaterialRefDto>> GetAllAsync()
        {
            var items = await _repo.GetAllAsync();
            return _mapper.Map<IEnumerable<MinecraftMaterialRefDto>>(items);
        }

        public async Task<MinecraftMaterialRefDto?> GetByIdAsync(int id)
        {
            if (id <= 0) return null;
            var entity = await _repo.GetByIdAsync(id);
            return _mapper.Map<MinecraftMaterialRefDto>(entity);
        }

        public async Task<MinecraftMaterialRefDto> CreateAsync(MinecraftMaterialRefCreateDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.NamespaceKey)) throw new ArgumentException("NamespaceKey is required.", nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.Category)) throw new ArgumentException("Category is required.", nameof(dto));

            var entity = _mapper.Map<MinecraftMaterialRef>(dto);
            await _repo.AddAsync(entity);
            return _mapper.Map<MinecraftMaterialRefDto>(entity);
        }

        public async Task UpdateAsync(int id, MinecraftMaterialRefUpdateDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (id <= 0) throw new ArgumentException("Invalid id.", nameof(id));
            if (string.IsNullOrWhiteSpace(dto.NamespaceKey)) throw new ArgumentException("NamespaceKey is required.", nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.Category)) throw new ArgumentException("Category is required.", nameof(dto));

            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) throw new KeyNotFoundException($"MinecraftMaterialRef with id {id} not found.");

            existing.NamespaceKey = dto.NamespaceKey;
            existing.LegacyName = dto.LegacyName;
            existing.Category = dto.Category;

            await _repo.UpdateAsync(existing);
        }

        public async Task DeleteAsync(int id)
        {
            if (id <= 0) throw new ArgumentException("Invalid id.", nameof(id));
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) throw new KeyNotFoundException($"MinecraftMaterialRef with id {id} not found.");

            await _repo.DeleteAsync(id);
        }

        public async Task<PagedResultDto<MinecraftMaterialRefListDto>> SearchAsync(PagedQueryDto queryDto)
        {
            if (queryDto == null) throw new ArgumentNullException(nameof(queryDto));

            var query = _mapper.Map<PagedQuery>(queryDto);
            var result = await _repo.SearchAsync(query);
            return _mapper.Map<PagedResultDto<MinecraftMaterialRefListDto>>(result);
        }
    }
}
