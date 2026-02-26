using AutoMapper;
using knkwebapi_v2.Dtos;
using knkwebapi_v2.Models;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace knkwebapi_v2.Mapping
{
    public class EntityTypeConfigurationProfile : Profile
    {
        public EntityTypeConfigurationProfile()
        {
            // Model -> ReadDto
            CreateMap<EntityTypeConfiguration, EntityTypeConfigurationReadDto>()
                .ForMember(dest => dest.DefaultTableColumns, opt => opt.MapFrom(src => ParseColumns(src.DefaultTableColumnsJson)))
                .ReverseMap();

            // CreateDto -> Model
            CreateMap<EntityTypeConfigurationCreateDto, EntityTypeConfiguration>()
                .ForMember(dest => dest.DefaultTableColumnsJson, opt => opt.MapFrom(src => SerializeColumns(src.DefaultTableColumns)))
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

            // UpdateDto -> Model
            CreateMap<EntityTypeConfigurationUpdateDto, EntityTypeConfiguration>()
                .ForMember(dest => dest.DefaultTableColumnsJson, opt => opt.MapFrom(src => SerializeColumns(src.DefaultTableColumns)))
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
        }

        private static List<string>? ParseColumns(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            try
            {
                var parsed = JsonSerializer.Deserialize<List<string>>(json);
                return parsed?
                    .Where(c => !string.IsNullOrWhiteSpace(c))
                    .Select(c => c.Trim())
                    .Distinct(System.StringComparer.OrdinalIgnoreCase)
                    .ToList();
            }
            catch
            {
                return null;
            }
        }

        private static string? SerializeColumns(List<string>? columns)
        {
            if (columns == null)
            {
                return null;
            }

            var cleaned = columns
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Select(c => c.Trim())
                .Distinct(System.StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (cleaned.Count == 0)
            {
                return null;
            }

            return JsonSerializer.Serialize(cleaned);
        }
    }
}
