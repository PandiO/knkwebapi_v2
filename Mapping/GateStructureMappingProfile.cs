using AutoMapper;
using knkwebapi_v2.Dtos;
using knkwebapi_v2.Models;

namespace knkwebapi_v2.Mapping
{
    public class GateStructureMappingProfile : Profile
    {
        public GateStructureMappingProfile()
        {
            // GateStructure -> GateStructureDto (full read with embedded navigations)
            CreateMap<GateStructure, GateStructureDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.AllowEntry, opt => opt.MapFrom(src => src.AllowEntry))
                .ForMember(dest => dest.AllowExit, opt => opt.MapFrom(src => src.AllowExit))
                .ForMember(dest => dest.WgRegionId, opt => opt.MapFrom(src => src.WgRegionId))
                .ForMember(dest => dest.LocationId, opt => opt.MapFrom(src => src.LocationId))
                .ForMember(dest => dest.StreetId, opt => opt.MapFrom(src => src.StreetId))
                .ForMember(dest => dest.DistrictId, opt => opt.MapFrom(src => src.DistrictId))
                .ForMember(dest => dest.HouseNumber, opt => opt.MapFrom(src => src.HouseNumber))
                // GateStructure-specific fields
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
                .ForMember(dest => dest.CanRespawn, opt => opt.MapFrom(src => src.CanRespawn))
                .ForMember(dest => dest.IsDestroyed, opt => opt.MapFrom(src => src.IsDestroyed))
                .ForMember(dest => dest.IsInvincible, opt => opt.MapFrom(src => src.IsInvincible))
                .ForMember(dest => dest.IsOpened, opt => opt.MapFrom(src => src.IsOpened))
                .ForMember(dest => dest.HealthCurrent, opt => opt.MapFrom(src => src.HealthCurrent))
                .ForMember(dest => dest.HealthMax, opt => opt.MapFrom(src => src.HealthMax))
                .ForMember(dest => dest.FaceDirection, opt => opt.MapFrom(src => src.FaceDirection))
                .ForMember(dest => dest.RespawnRateSeconds, opt => opt.MapFrom(src => src.RespawnRateSeconds))
                .ForMember(dest => dest.IconMaterialRefId, opt => opt.MapFrom(src => src.IconMaterialRefId))
                .ForMember(dest => dest.RegionClosedId, opt => opt.MapFrom(src => src.RegionClosedId))
                .ForMember(dest => dest.RegionOpenedId, opt => opt.MapFrom(src => src.RegionOpenedId))
                // Lightweight embedded Street
                .ForMember(dest => dest.Street, opt => opt.MapFrom(s => s.Street == null ? null : new GateStructureStreetDto
                {
                    Id = s.Street.Id,
                    Name = s.Street.Name
                }))
                // Lightweight embedded District
                .ForMember(dest => dest.District, opt => opt.MapFrom(s => s.District == null ? null : new GateStructureDistrictDto
                {
                    Id = s.District.Id,
                    Name = s.District.Name,
                    Description = s.District.Description,
                    AllowEntry = s.District.AllowEntry,
                    AllowExit = s.District.AllowExit,
                    WgRegionId = s.District.WgRegionId
                }))
                // Lightweight embedded IconMaterialRef
                .ForMember(dest => dest.IconMaterialRef, opt => opt.MapFrom(s => s.IconMaterial == null ? null : new MinecraftMaterialRefDto
                {
                    Id = s.IconMaterial.Id,
                    NamespaceKey = s.IconMaterial.NamespaceKey,
                    LegacyName = s.IconMaterial.LegacyName,
                    Category = s.IconMaterial.Category,
                    IconUrl = s.IconMaterial.IconUrl
                }));

            // GateStructureDto -> GateStructure (create/update)
            CreateMap<GateStructureDto, GateStructure>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id ?? 0))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.AllowEntry, opt => opt.MapFrom(src => src.AllowEntry))
                .ForMember(dest => dest.AllowExit, opt => opt.MapFrom(src => src.AllowExit))
                .ForMember(dest => dest.WgRegionId, opt => opt.MapFrom(src => src.WgRegionId))
                .ForMember(dest => dest.LocationId, opt => opt.MapFrom(src => src.LocationId))
                .ForMember(dest => dest.StreetId, opt => opt.MapFrom(src => src.StreetId))
                .ForMember(dest => dest.DistrictId, opt => opt.MapFrom(src => src.DistrictId))
                .ForMember(dest => dest.HouseNumber, opt => opt.MapFrom(src => src.HouseNumber))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive ?? false))
                .ForMember(dest => dest.CanRespawn, opt => opt.MapFrom(src => src.CanRespawn ?? true))
                .ForMember(dest => dest.IsDestroyed, opt => opt.MapFrom(src => src.IsDestroyed ?? false))
                .ForMember(dest => dest.IsInvincible, opt => opt.MapFrom(src => src.IsInvincible ?? true))
                .ForMember(dest => dest.IsOpened, opt => opt.MapFrom(src => src.IsOpened ?? false))
                .ForMember(dest => dest.HealthCurrent, opt => opt.MapFrom(src => src.HealthCurrent ?? 500.0))
                .ForMember(dest => dest.HealthMax, opt => opt.MapFrom(src => src.HealthMax ?? 500.0))
                .ForMember(dest => dest.FaceDirection, opt => opt.MapFrom(src => src.FaceDirection ?? "north"))
                .ForMember(dest => dest.RespawnRateSeconds, opt => opt.MapFrom(src => src.RespawnRateSeconds ?? 300))
                .ForMember(dest => dest.IconMaterialRefId, opt => opt.MapFrom(src => src.IconMaterialRefId))
                .ForMember(dest => dest.RegionClosedId, opt => opt.MapFrom(src => src.RegionClosedId ?? string.Empty))
                .ForMember(dest => dest.RegionOpenedId, opt => opt.MapFrom(src => src.RegionOpenedId ?? string.Empty))
                // Ignore navigation properties for create/update
                .ForMember(dest => dest.Street, opt => opt.Ignore())
                .ForMember(dest => dest.District, opt => opt.Ignore())
                .ForMember(dest => dest.Location, opt => opt.Ignore())
                .ForMember(dest => dest.IconMaterial, opt => opt.Ignore());

            // GateStructure -> GateStructureListDto (for search results)
            CreateMap<GateStructure, GateStructureListDto>()
                .ForMember(dest => dest.id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.wgRegionId, opt => opt.MapFrom(src => src.WgRegionId))
                .ForMember(dest => dest.houseNumber, opt => opt.MapFrom(src => src.HouseNumber))
                .ForMember(dest => dest.streetId, opt => opt.MapFrom(src => src.StreetId))
                .ForMember(dest => dest.streetName, opt => opt.MapFrom(src => src.Street != null ? src.Street.Name : null))
                .ForMember(dest => dest.districtId, opt => opt.MapFrom(src => src.DistrictId))
                .ForMember(dest => dest.districtName, opt => opt.MapFrom(src => src.District != null ? src.District.Name : null))
                .ForMember(dest => dest.isActive, opt => opt.MapFrom(src => src.IsActive))
                .ForMember(dest => dest.healthCurrent, opt => opt.MapFrom(src => src.HealthCurrent))
                .ForMember(dest => dest.isDestroyed, opt => opt.MapFrom(src => src.IsDestroyed));

            // PagedQuery DTO conversions
            CreateMap<PagedQueryDto, PagedQuery>();
        }
    }
}
