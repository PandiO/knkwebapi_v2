using System;

namespace knkwebapi_v2.Models;

public class WorkflowSession
{
    public int Id { get; set; }
    public Guid SessionGuid { get; set; } = Guid.NewGuid();

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    // Optional linkage to a FormConfiguration to drive steps
    public int? FormConfigurationId { get; set; }
    public FormConfiguration? FormConfiguration { get; set; }

    public string? EntityTypeName { get; set; }
    public int? EntityId { get; set; }

    public int CurrentStepIndex { get; set; } = 0;
    public string Status { get; set; } = "InProgress"; // InProgress, Paused, Completed, Abandoned

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    public ICollection<StepProgress> Steps { get; set; } = new List<StepProgress>();
    public ICollection<WorldTask> WorldTasks { get; set; } = new List<WorldTask>();
}
