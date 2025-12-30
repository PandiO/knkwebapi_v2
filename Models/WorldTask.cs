using System;

namespace knkwebapi_v2.Models;

public class WorldTask
{
    public int Id { get; set; }

    // Association to the workflow
    public int WorkflowSessionId { get; set; }
    public WorkflowSession WorkflowSession { get; set; } = null!;

    // Step mapping
    public int? StepNumber { get; set; }
    public string? StepKey { get; set; }
    public string? FieldName { get; set; }

    public string TaskType { get; set; } = null!; // e.g., "CaptureLocation", "DefineRegion", etc.
    public string Status { get; set; } = "Pending"; // Pending, InProgress, Completed, Failed, Cancelled

    // Link code for claiming (6-10 chars, unique, nullable after claim)
    public string? LinkCode { get; set; }

    // Who should handle this task
    public int? AssignedUserId { get; set; }
    public User? AssignedUser { get; set; }

    // Server and player claim info
    public string? ClaimedByServerId { get; set; }
    public string? ClaimedByMinecraftUsername { get; set; }

    // Flexible payload for input/output data
    public string? InputJson { get; set; }
    public string? OutputJson { get; set; }
    public string? ErrorMessage { get; set; }

    // Legacy support for existing code
    [Obsolete("Use InputJson/OutputJson instead")]
    public string? PayloadJson { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ClaimedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
