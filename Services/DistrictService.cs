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
    public class DistrictService : IDistrictService
    {
        private readonly IDistrictRepository _repo;
        private readonly ITownRepository _townRepo;
        private readonly ILocationRepository _locationRepo;
        private readonly IMapper _mapper;

        public DistrictService(
            IDistrictRepository repo,
            ITownRepository townRepo,
            ILocationRepository locationRepo,
            IMapper mapper)
        {
            _repo = repo;
            _townRepo = townRepo;
            _locationRepo = locationRepo;
            _mapper = mapper;
        }

        public async Task<IEnumerable<DistrictDto>> GetAllAsync()
        {
            var districts = await _repo.GetAllAsync();
            return _mapper.Map<IEnumerable<DistrictDto>>(districts);
        }

        public async Task<DistrictDto?> GetByIdAsync(int id)
        {
            if (id <= 0) return null;
            var district = await _repo.GetByIdAsync(id);
            return _mapper.Map<DistrictDto>(district);
        }

        public async Task<DistrictDto> CreateAsync(DistrictDto districtDto)
        {
            if (districtDto == null) throw new ArgumentNullException(nameof(districtDto));
            if (string.IsNullOrWhiteSpace(districtDto.Name)) throw new ArgumentException("District name is required.", nameof(districtDto));
            if (string.IsNullOrWhiteSpace(districtDto.WgRegionId)) throw new ArgumentException("WgRegionId is required.", nameof(districtDto));
            if (districtDto.TownId <= 0) throw new ArgumentException("TownId is required.", nameof(districtDto));

            // Validate that Town exists
            var town = await _townRepo.GetByIdAsync(districtDto.TownId);
            if (town == null) throw new ArgumentException($"Town with id {districtDto.TownId} not found.", nameof(districtDto));

            // Validate LocationId if provided
            if (districtDto.LocationId.HasValue)
            {
                var location = await _locationRepo.GetByIdAsync(districtDto.LocationId.Value);
                if (location == null)
                    throw new ArgumentException($"Location with id {districtDto.LocationId} not found.", nameof(districtDto));
            }

            var district = _mapper.Map<District>(districtDto);
            district.CreatedAt = DateTime.UtcNow;
            await _repo.AddDistrictAsync(district);
            return _mapper.Map<DistrictDto>(district);
        }

        public async Task UpdateAsync(int id, DistrictDto districtDto)
        {
            if (districtDto == null) throw new ArgumentNullException(nameof(districtDto));
            if (id <= 0) throw new ArgumentException("Invalid id.", nameof(id));
            if (string.IsNullOrWhiteSpace(districtDto.Name)) throw new ArgumentException("District name is required.", nameof(districtDto));
            if (string.IsNullOrWhiteSpace(districtDto.WgRegionId)) throw new ArgumentException("WgRegionId is required.", nameof(districtDto));
            if (districtDto.TownId <= 0) throw new ArgumentException("TownId is required.", nameof(districtDto));
            
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) throw new KeyNotFoundException($"District with id {id} not found.");

            // Validate that Town exists if changing
            if (existing.TownId != districtDto.TownId)
            {
                var town = await _townRepo.GetByIdAsync(districtDto.TownId);
                if (town == null) throw new ArgumentException($"Town with id {districtDto.TownId} not found.", nameof(districtDto));
            }
            // Validate LocationId if provided
            if (districtDto.LocationId.HasValue)
            {
                var location = await _locationRepo.GetByIdAsync(districtDto.LocationId.Value);
                if (location == null)
                    throw new ArgumentException($"Location with id {districtDto.LocationId} not found.", nameof(districtDto));
            }

            existing.Name = districtDto.Name;
            existing.Description = districtDto.Description;
            existing.AllowEntry = districtDto.AllowEntry ?? true;
            existing.AllowExit = districtDto.AllowExit ?? true;
            existing.WgRegionId = districtDto.WgRegionId;
            existing.LocationId = districtDto.LocationId;
            existing.TownId = districtDto.TownId;

            await _repo.UpdateDistrictAsync(existing);
        }

        public async Task DeleteAsync(int id)
        {
            if (id <= 0) throw new ArgumentException("Invalid id.", nameof(id));
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) throw new KeyNotFoundException($"District with id {id} not found.");

            await _repo.DeleteDistrictAsync(id);
        }

        public async Task<PagedResultDto<DistrictListDto>> SearchAsync(PagedQueryDto queryDto)
        {
            if (queryDto == null) throw new ArgumentNullException(nameof(queryDto));

            var query = _mapper.Map<PagedQuery>(queryDto);
            var result = await _repo.SearchAsync(query);
            var resultDto = _mapper.Map<PagedResultDto<DistrictListDto>>(result);

            return resultDto;
        }
    }
}
