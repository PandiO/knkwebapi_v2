using System;
using System.Text.Json.Serialization;

namespace knkwebapi_v2.Dtos;

// WorkflowSession DTOs
public class WorkflowSessionCreateDto
{
    [JsonPropertyName("userId")] public int UserId { get; set; }
    [JsonPropertyName("formConfigurationId")] public int? FormConfigurationId { get; set; }
    [JsonPropertyName("entityTypeName")] public string? EntityTypeName { get; set; }
    [JsonPropertyName("entityId")] public int? EntityId { get; set; }
}

public class WorkflowSessionUpdateDto
{
    [JsonPropertyName("currentStepIndex")] public int? CurrentStepIndex { get; set; }
    [JsonPropertyName("status")] public string? Status { get; set; }
}

public class WorkflowSessionReadDto
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("sessionGuid")] public Guid SessionGuid { get; set; }
    [JsonPropertyName("userId")] public int UserId { get; set; }
    [JsonPropertyName("formConfigurationId")] public int? FormConfigurationId { get; set; }
    [JsonPropertyName("entityTypeName")] public string? EntityTypeName { get; set; }
    [JsonPropertyName("entityId")] public int? EntityId { get; set; }
    [JsonPropertyName("currentStepIndex")] public int CurrentStepIndex { get; set; }
    [JsonPropertyName("status")] public string Status { get; set; } = "InProgress";
    [JsonPropertyName("createdAt")] public string CreatedAt { get; set; } = DateTime.UtcNow.ToString("O");
    [JsonPropertyName("updatedAt")] public string? UpdatedAt { get; set; }
    [JsonPropertyName("completedAt")] public string? CompletedAt { get; set; }
}

// StepProgress DTOs
public class StepProgressCreateDto
{
    [JsonPropertyName("workflowSessionId")] public int WorkflowSessionId { get; set; }
    [JsonPropertyName("stepKey")] public string StepKey { get; set; } = null!;
    [JsonPropertyName("stepIndex")] public int StepIndex { get; set; }
}

public class StepProgressUpdateDto
{
    [JsonPropertyName("status")] public string? Status { get; set; }
}

public class StepProgressReadDto
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("workflowSessionId")] public int WorkflowSessionId { get; set; }
    [JsonPropertyName("stepKey")] public string StepKey { get; set; } = null!;
    [JsonPropertyName("stepIndex")] public int StepIndex { get; set; }
    [JsonPropertyName("status")] public string Status { get; set; } = "Pending";
    [JsonPropertyName("createdAt")] public string CreatedAt { get; set; } = DateTime.UtcNow.ToString("O");
    [JsonPropertyName("updatedAt")] public string? UpdatedAt { get; set; }
    [JsonPropertyName("completedAt")] public string? CompletedAt { get; set; }
}

// WorldTask DTOs
public class WorldTaskCreateDto
{
    [JsonPropertyName("workflowSessionId")] public int WorkflowSessionId { get; set; }
    [JsonPropertyName("stepNumber")] public int? StepNumber { get; set; }
    [JsonPropertyName("stepKey")] public string? StepKey { get; set; }
    [JsonPropertyName("fieldName")] public string? FieldName { get; set; }
    [JsonPropertyName("taskType")] public string TaskType { get; set; } = null!;
    [JsonPropertyName("assignedUserId")] public int? AssignedUserId { get; set; }
    [JsonPropertyName("inputJson")] public string? InputJson { get; set; }
}

public class WorldTaskUpdateDto
{
    [JsonPropertyName("status")] public string? Status { get; set; }
    [JsonPropertyName("outputJson")] public string? OutputJson { get; set; }
    [JsonPropertyName("errorMessage")] public string? ErrorMessage { get; set; }
}

public class ClaimTaskDto
{
    [JsonPropertyName("claimedByServerId")] public string? ClaimedByServerId { get; set; }
    [JsonPropertyName("claimedByMinecraftUsername")] public string? ClaimedByMinecraftUsername { get; set; }
}

public class CompleteTaskDto
{
    [JsonPropertyName("outputJson")] public string OutputJson { get; set; } = null!;
}

public class FailTaskDto
{
    [JsonPropertyName("errorMessage")] public string ErrorMessage { get; set; } = null!;
}

public class WorldTaskReadDto
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("workflowSessionId")] public int WorkflowSessionId { get; set; }
    [JsonPropertyName("stepNumber")] public int? StepNumber { get; set; }
    [JsonPropertyName("stepKey")] public string? StepKey { get; set; }
    [JsonPropertyName("fieldName")] public string? FieldName { get; set; }
    [JsonPropertyName("taskType")] public string TaskType { get; set; } = null!;
    [JsonPropertyName("status")] public string Status { get; set; } = "Pending";
    [JsonPropertyName("linkCode")] public string? LinkCode { get; set; }
    [JsonPropertyName("assignedUserId")] public int? AssignedUserId { get; set; }
    [JsonPropertyName("claimedByServerId")] public string? ClaimedByServerId { get; set; }
    [JsonPropertyName("claimedByMinecraftUsername")] public string? ClaimedByMinecraftUsername { get; set; }
    [JsonPropertyName("inputJson")] public string? InputJson { get; set; }
    [JsonPropertyName("outputJson")] public string? OutputJson { get; set; }
    [JsonPropertyName("errorMessage")] public string? ErrorMessage { get; set; }
    [JsonPropertyName("createdAt")] public string CreatedAt { get; set; } = DateTime.UtcNow.ToString("O");
    [JsonPropertyName("claimedAt")] public string? ClaimedAt { get; set; }
    [JsonPropertyName("updatedAt")] public string? UpdatedAt { get; set; }
    [JsonPropertyName("completedAt")] public string? CompletedAt { get; set; }
}
