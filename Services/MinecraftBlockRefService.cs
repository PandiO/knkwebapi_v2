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
    public class MinecraftBlockRefService : IMinecraftBlockRefService
    {
        private readonly IMinecraftBlockRefRepository _repo;
        private readonly IMapper _mapper;

        public MinecraftBlockRefService(IMinecraftBlockRefRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<IEnumerable<MinecraftBlockRefDto>> GetAllAsync()
        {
            var items = await _repo.GetAllAsync();
            return _mapper.Map<IEnumerable<MinecraftBlockRefDto>>(items);
        }

        public async Task<MinecraftBlockRefDto?> GetByIdAsync(int id)
        {
            if (id <= 0) return null;
            var entity = await _repo.GetByIdAsync(id);
            return _mapper.Map<MinecraftBlockRefDto>(entity);
        }

        public async Task<MinecraftBlockRefDto> CreateAsync(MinecraftBlockRefCreateDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.NamespaceKey)) throw new ArgumentException("NamespaceKey is required.", nameof(dto));

            var entity = _mapper.Map<MinecraftBlockRef>(dto);
            await _repo.AddAsync(entity);
            return _mapper.Map<MinecraftBlockRefDto>(entity);
        }

        public async Task UpdateAsync(int id, MinecraftBlockRefUpdateDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (id <= 0) throw new ArgumentException("Invalid id.", nameof(id));
            if (string.IsNullOrWhiteSpace(dto.NamespaceKey)) throw new ArgumentException("NamespaceKey is required.", nameof(dto));

            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) throw new KeyNotFoundException($"MinecraftBlockRef with id {id} not found.");

            existing.NamespaceKey = dto.NamespaceKey;
            existing.BlockStateString = dto.BlockStateString;
            existing.LogicalType = dto.LogicalType;

            await _repo.UpdateAsync(existing);
        }

        public async Task DeleteAsync(int id)
        {
            if (id <= 0) throw new ArgumentException("Invalid id.", nameof(id));
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) throw new KeyNotFoundException($"MinecraftBlockRef with id {id} not found.");

            await _repo.DeleteAsync(id);
        }

        public async Task<PagedResultDto<MinecraftBlockRefListDto>> SearchAsync(PagedQueryDto queryDto)
        {
            if (queryDto == null) throw new ArgumentNullException(nameof(queryDto));

            var query = _mapper.Map<PagedQuery>(queryDto);
            var result = await _repo.SearchAsync(query);
            return _mapper.Map<PagedResultDto<MinecraftBlockRefListDto>>(result);
        }
    }
}
