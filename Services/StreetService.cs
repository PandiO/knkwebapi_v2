using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using knkwebapi_v2.Dtos;
using knkwebapi_v2.Models;
using knkwebapi_v2.Repositories;

namespace knkwebapi_v2.Services
{
    public class StreetService : IStreetService
    {
        private readonly IStreetRepository _repo;
        private readonly IMapper _mapper;

        public StreetService(IStreetRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<IEnumerable<StreetDto>> GetAllAsync()
        {
            var streets = await _repo.GetAllAsync();
            return _mapper.Map<IEnumerable<StreetDto>>(streets);
        }

        public async Task<StreetDto?> GetByIdAsync(int id)
        {
            if (id <= 0) return null;
            var street = await _repo.GetByIdAsync(id);
            return _mapper.Map<StreetDto>(street);
        }

        public async Task<StreetDto> CreateAsync(StreetDto streetDto)
        {
            if (streetDto == null) throw new ArgumentNullException(nameof(streetDto));
            if (string.IsNullOrWhiteSpace(streetDto.Name)) throw new ArgumentException("Street name is required.", nameof(streetDto));

            var street = _mapper.Map<Street>(streetDto);
            await _repo.AddStreetAsync(street);
            return _mapper.Map<StreetDto>(street);
        }

        public async Task UpdateAsync(int id, StreetDto streetDto)
        {
            if (streetDto == null) throw new ArgumentNullException(nameof(streetDto));
            if (id <= 0) throw new ArgumentException("Invalid id.", nameof(id));
            if (string.IsNullOrWhiteSpace(streetDto.Name)) throw new ArgumentException("Street name is required.", nameof(streetDto));
            
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) throw new KeyNotFoundException($"Street with id {id} not found.");

            existing.Name = streetDto.Name;

            await _repo.UpdateStreetAsync(existing);
        }

        public async Task DeleteAsync(int id)
        {
            if (id <= 0) throw new ArgumentException("Invalid id.", nameof(id));
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) throw new KeyNotFoundException($"Street with id {id} not found.");

            await _repo.DeleteStreetAsync(id);
        }

        public async Task<PagedResultDto<StreetListDto>> SearchAsync(PagedQueryDto queryDto)
        {
            if (queryDto == null) throw new ArgumentNullException(nameof(queryDto));

            var query = _mapper.Map<PagedQuery>(queryDto);
            var result = await _repo.SearchAsync(query);
            var resultDto = _mapper.Map<PagedResultDto<StreetListDto>>(result);

            return resultDto;
        }
    }
}
