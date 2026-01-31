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

        public async Task<GateStructureDto?> GetByIdWithSnapshotsAsync(int id)
        {
            if (id <= 0) return null;
            var gateStructure = await _repo.GetByIdWithSnapshotsAsync(id);
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

            // Validate health values
            if (gateStructureDto.HealthCurrent.HasValue && gateStructureDto.HealthMax.HasValue)
            {
                if (gateStructureDto.HealthCurrent.Value > gateStructureDto.HealthMax.Value)
                    throw new ArgumentException("HealthCurrent cannot exceed HealthMax.", nameof(gateStructureDto));
            }

            // Validate gate type
            var validGateTypes = new[] { "SLIDING", "TRAP", "DRAWBRIDGE", "DOUBLE_DOORS" };
            if (!string.IsNullOrEmpty(gateStructureDto.GateType) && !validGateTypes.Contains(gateStructureDto.GateType))
                throw new ArgumentException($"Invalid GateType. Must be one of: {string.Join(", ", validGateTypes)}", nameof(gateStructureDto));

            // Validate motion type
            var validMotionTypes = new[] { "VERTICAL", "LATERAL", "ROTATION" };
            if (!string.IsNullOrEmpty(gateStructureDto.MotionType) && !validMotionTypes.Contains(gateStructureDto.MotionType))
                throw new ArgumentException($"Invalid MotionType. Must be one of: {string.Join(", ", validMotionTypes)}", nameof(gateStructureDto));

            // Validate geometry mode
            var validGeometryModes = new[] { "PLANE_GRID", "FLOOD_FILL" };
            if (!string.IsNullOrEmpty(gateStructureDto.GeometryDefinitionMode) && !validGeometryModes.Contains(gateStructureDto.GeometryDefinitionMode))
                throw new ArgumentException($"Invalid GeometryDefinitionMode. Must be one of: {string.Join(", ", validGeometryModes)}", nameof(gateStructureDto));

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

            // Validate health values
            if (gateStructureDto.HealthCurrent.HasValue && gateStructureDto.HealthMax.HasValue)
            {
                if (gateStructureDto.HealthCurrent.Value > gateStructureDto.HealthMax.Value)
                    throw new ArgumentException("HealthCurrent cannot exceed HealthMax.", nameof(gateStructureDto));
            }

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

            // Delete associated block snapshots first
            await _repo.DeleteBlockSnapshotsByGateIdAsync(id);
            
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

        public async Task<IEnumerable<GateStructureDto>> GetActiveGatesAsync()
        {
            var gates = await _repo.GetActiveGatesAsync();
            return _mapper.Map<IEnumerable<GateStructureDto>>(gates);
        }

        public async Task UpdateHealthAsync(int id, double newHealth)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid id.", nameof(id));
            if (newHealth < 0)
                throw new ArgumentException("Health cannot be negative.", nameof(newHealth));

            var existing = await _repo.GetByIdAsync(id);
            if (existing == null)
                throw new KeyNotFoundException($"GateStructure with id {id} not found.");

            if (newHealth > existing.HealthMax)
                throw new ArgumentException($"Health cannot exceed HealthMax ({existing.HealthMax}).", nameof(newHealth));

            await _repo.UpdateGateHealthAsync(id, newHealth);
        }

        public async Task UpdateStateAsync(int id, bool isOpened, bool isDestroyed)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid id.", nameof(id));

            var existing = await _repo.GetByIdAsync(id);
            if (existing == null)
                throw new KeyNotFoundException($"GateStructure with id {id} not found.");

            await _repo.UpdateGateStateAsync(id, isOpened, isDestroyed);
        }

        public async Task<IEnumerable<GateBlockSnapshotDto>> GetBlockSnapshotsAsync(int gateId)
        {
            if (gateId <= 0)
                throw new ArgumentException("Invalid gateId.", nameof(gateId));

            var snapshots = await _repo.GetBlockSnapshotsByGateIdAsync(gateId);
            return _mapper.Map<IEnumerable<GateBlockSnapshotDto>>(snapshots);
        }

        public async Task AddBlockSnapshotsAsync(int gateId, IEnumerable<GateBlockSnapshotDto> snapshots)
        {
            if (gateId <= 0)
                throw new ArgumentException("Invalid gateId.", nameof(gateId));
            if (snapshots == null || !snapshots.Any())
                throw new ArgumentException("Snapshots collection cannot be null or empty.", nameof(snapshots));

            var existing = await _repo.GetByIdAsync(gateId);
            if (existing == null)
                throw new KeyNotFoundException($"GateStructure with id {gateId} not found.");

            var snapshotEntities = _mapper.Map<IEnumerable<GateBlockSnapshot>>(snapshots);
            
            // Ensure all snapshots have the correct GateStructureId
            foreach (var snapshot in snapshotEntities)
            {
                snapshot.GateStructureId = gateId;
            }

            await _repo.AddBlockSnapshotsAsync(snapshotEntities);
        }

        public async Task AddBlockSnapshotsAsync(int gateId, IEnumerable<GateBlockSnapshotCreateDto> snapshots)
        {
            if (gateId <= 0)
                throw new ArgumentException("Invalid gateId.", nameof(gateId));
            if (snapshots == null || !snapshots.Any())
                throw new ArgumentException("Snapshots collection cannot be null or empty.", nameof(snapshots));

            var existing = await _repo.GetByIdAsync(gateId);
            if (existing == null)
                throw new KeyNotFoundException($"GateStructure with id {gateId} not found.");

            var snapshotEntities = _mapper.Map<IEnumerable<GateBlockSnapshot>>(snapshots);

            foreach (var snapshot in snapshotEntities)
            {
                snapshot.GateStructureId = gateId;
            }

            await _repo.AddBlockSnapshotsAsync(snapshotEntities);
        }

        public async Task ClearBlockSnapshotsAsync(int gateId)
        {
            if (gateId <= 0)
                throw new ArgumentException("Invalid gateId.", nameof(gateId));

            await _repo.DeleteBlockSnapshotsByGateIdAsync(gateId);
        }
    }
}
