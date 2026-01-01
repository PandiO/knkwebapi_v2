using System;
using AutoMapper;
using knkwebapi_v2.Dtos;
using knkwebapi_v2.Models;

namespace knkwebapi_v2.Mapping
{
    public class WorkflowMappingProfile : Profile
    {
        public WorkflowMappingProfile()
        {
            // WorkflowSession
            CreateMap<WorkflowSessionCreateDto, WorkflowSession>()
                .ForMember(d => d.Id, o => o.Ignore())
                .ForMember(d => d.SessionGuid, o => o.MapFrom(_ => Guid.NewGuid()))
                .ForMember(d => d.Status, o => o.MapFrom(_ => "InProgress"))
                .ForMember(d => d.CreatedAt, o => o.MapFrom(_ => DateTime.UtcNow))
                .ForMember(d => d.UpdatedAt, o => o.Ignore())
                .ForMember(d => d.CompletedAt, o => o.Ignore())
                .ForMember(d => d.RowVersion, o => o.Ignore())
                .ForMember(d => d.User, o => o.Ignore())
                .ForMember(d => d.FormConfiguration, o => o.Ignore())
                .ForMember(d => d.Steps, o => o.Ignore())
                .ForMember(d => d.WorldTasks, o => o.Ignore());

            CreateMap<WorkflowSession, WorkflowSessionReadDto>()
                .ForMember(d => d.CreatedAt, o => o.MapFrom(s => s.CreatedAt.ToString("O")))
                .ForMember(d => d.UpdatedAt, o => o.MapFrom(s => s.UpdatedAt.HasValue ? s.UpdatedAt.Value.ToString("O") : null))
                .ForMember(d => d.CompletedAt, o => o.MapFrom(s => s.CompletedAt.HasValue ? s.CompletedAt.Value.ToString("O") : null));

            // StepProgress
            CreateMap<StepProgressCreateDto, StepProgress>()
                .ForMember(d => d.Id, o => o.Ignore())
                .ForMember(d => d.Status, o => o.MapFrom(_ => "Pending"))
                .ForMember(d => d.CreatedAt, o => o.MapFrom(_ => DateTime.UtcNow))
                .ForMember(d => d.UpdatedAt, o => o.Ignore())
                .ForMember(d => d.CompletedAt, o => o.Ignore())
                .ForMember(d => d.WorkflowSession, o => o.Ignore());

            CreateMap<StepProgress, StepProgressReadDto>()
                .ForMember(d => d.CreatedAt, o => o.MapFrom(s => s.CreatedAt.ToString("O")))
                .ForMember(d => d.UpdatedAt, o => o.MapFrom(s => s.UpdatedAt.HasValue ? s.UpdatedAt.Value.ToString("O") : null))
                .ForMember(d => d.CompletedAt, o => o.MapFrom(s => s.CompletedAt.HasValue ? s.CompletedAt.Value.ToString("O") : null));

            // WorldTask
            CreateMap<WorldTaskCreateDto, WorldTask>()
                .ForMember(d => d.Id, o => o.Ignore())
                .ForMember(d => d.LinkCode, o => o.Ignore()) // Generated in service
                .ForMember(d => d.Status, o => o.MapFrom(_ => "Pending"))
                .ForMember(d => d.CreatedAt, o => o.MapFrom(_ => DateTime.UtcNow))
                .ForMember(d => d.InputJson, o => o.MapFrom(s => s.InputJson ?? s.PayloadJson))
                .ForMember(d => d.ClaimedAt, o => o.Ignore())
                .ForMember(d => d.UpdatedAt, o => o.Ignore())
                .ForMember(d => d.CompletedAt, o => o.Ignore())
                .ForMember(d => d.ClaimedByServerId, o => o.Ignore())
                .ForMember(d => d.ClaimedByMinecraftUsername, o => o.Ignore())
                .ForMember(d => d.OutputJson, o => o.Ignore())
                .ForMember(d => d.ErrorMessage, o => o.Ignore())
                .ForMember(d => d.PayloadJson, o => o.MapFrom(s => s.PayloadJson ?? s.InputJson))
                .ForMember(d => d.WorkflowSession, o => o.Ignore())
                .ForMember(d => d.AssignedUser, o => o.Ignore());

            CreateMap<WorldTask, WorldTaskReadDto>()
                .ForMember(d => d.CreatedAt, o => o.MapFrom(s => s.CreatedAt.ToString("O")))
                .ForMember(d => d.ClaimedAt, o => o.MapFrom(s => s.ClaimedAt.HasValue ? s.ClaimedAt.Value.ToString("O") : null))
                .ForMember(d => d.UpdatedAt, o => o.MapFrom(s => s.UpdatedAt.HasValue ? s.UpdatedAt.Value.ToString("O") : null))
                .ForMember(d => d.CompletedAt, o => o.MapFrom(s => s.CompletedAt.HasValue ? s.CompletedAt.Value.ToString("O") : null));
        }
    }
}
