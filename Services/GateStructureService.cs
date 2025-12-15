using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using knkwebapi_v2.Dtos;
using knkwebapi_v2.Models;
using knkwebapi_v2.Repositories;
using knkwebapi_v2.Repositories.Interfaces;
using knkwebapi_v2.Services.Interfaces;

namespace knkwebapi_v2.Services
{
    public class GateStructureService : IGateStructureService
    {
        private readonly IGateStructureRepository _repo;
        private readonly IMapper _mapper;

        public GateStructureService(IGateStructureRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<IEnumerable<GateStructureDto>> GetAllAsync()
        {
            var gateStructures = await _repo.GetAllAsync();
            return _mapper.Map<IEnumerable<GateStructureDto>>(gateStructures);
        }

        public async Task<GateStructureDto?> GetByIdAsync(int id)
        {
            if (id <= 0) return null;
            var gateStructure = await _repo.GetByIdAsync(id);
            return _mapper.Map<GateStructureDto>(gateStructure);
        }

        public async Task<GateStructureDto> CreateAsync(GateStructureDto gateStructureDto)
        {
            if (gateStructureDto == null) 
                throw new ArgumentNullException(nameof(gateStructureDto));
            if (string.IsNullOrWhiteSpace(gateStructureDto.Name)) 
                throw new ArgumentException("GateStructure name is required.", nameof(gateStructureDto));
            if (gateStructureDto.StreetId <= 0) 
                throw new ArgumentException("Valid StreetId is required.", nameof(gateStructureDto));
            if (gateStructureDto.DistrictId <= 0) 
                throw new ArgumentException("Valid DistrictId is required.", nameof(gateStructureDto));

            // TODO: Validate that HealthCurrent <= HealthMax
            var gateStructure = _mapper.Map<GateStructure>(gateStructureDto);
            await _repo.AddGateStructureAsync(gateStructure);
            return _mapper.Map<GateStructureDto>(gateStructure);
        }

        public async Task UpdateAsync(int id, GateStructureDto gateStructureDto)
        {
            if (gateStructureDto == null) 
                throw new ArgumentNullException(nameof(gateStructureDto));
            if (id <= 0) 
                throw new ArgumentException("Invalid id.", nameof(id));
            if (string.IsNullOrWhiteSpace(gateStructureDto.Name)) 
                throw new ArgumentException("GateStructure name is required.", nameof(gateStructureDto));

            var existing = await _repo.GetByIdAsync(id);
            if (existing == null)
                throw new KeyNotFoundException($"GateStructure with id {id} not found.");

            _mapper.Map(gateStructureDto, existing);
            await _repo.UpdateGateStructureAsync(existing);
        }

        public async Task DeleteAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid id.", nameof(id));

            var existing = await _repo.GetByIdAsync(id);
            if (existing == null)
                throw new KeyNotFoundException($"GateStructure with id {id} not found.");

            await _repo.DeleteGateStructureAsync(id);
        }

        public async Task<PagedResultDto<GateStructureListDto>> SearchAsync(PagedQueryDto query)
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));

            var result = await _repo.SearchAsync(_mapper.Map<PagedQuery>(query));
            return new PagedResultDto<GateStructureListDto>
            {
                Items = _mapper.Map<List<GateStructureListDto>>(result.Items),
                TotalCount = result.TotalCount,
                PageNumber = result.PageNumber,
                PageSize = result.PageSize
            };
        }
    }
}
