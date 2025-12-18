using System;

namespace knkwebapi_v2.Models;

public class StepProgress
{
    public int Id { get; set; }
    public int WorkflowSessionId { get; set; }
    public WorkflowSession WorkflowSession { get; set; } = null!;

    // Identifier for mapping, can be the StepName or a unique key
    public string StepKey { get; set; } = null!;
    public int StepIndex { get; set; }

    public string Status { get; set; } = "Pending"; // Pending, InProgress, Completed
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
