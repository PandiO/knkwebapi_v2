using System;

namespace knkwebapi_v2.Models;

public class WorldTask
{
    public int Id { get; set; }

    // Association to the workflow
    public int WorkflowSessionId { get; set; }
    public WorkflowSession WorkflowSession { get; set; } = null!;

    // Optional mapping to a step to advance when completed
    public string? StepKey { get; set; }

    public string TaskType { get; set; } = null!; // e.g., LocationSelection, RegionClaim, etc.
    public string Status { get; set; } = "Pending"; // Pending, Accepted, InProgress, Completed, Cancelled

    // Who should handle this task (can be same as session user)
    public int? AssignedUserId { get; set; }
    public User? AssignedUser { get; set; }

    // Flexible payload to attach refs like Location/Region etc
    public string? PayloadJson { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
