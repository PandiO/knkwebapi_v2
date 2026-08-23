using System.Collections.Generic;
using System.Text.Json.Serialization;
using knkwebapi_v2.Enums;

namespace knkwebapi_v2.Dtos
{
    public class DisplayConditionDto
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("displayConditionGroupId")]
        public string? DisplayConditionGroupId { get; set; }

        /// <summary>Read-only echo of the resolved foreign key.</summary>
        [JsonPropertyName("sourceFormFieldId")]
        public string? SourceFormFieldId { get; set; }

        /// <summary>The identifier the builder writes; resolved to a foreign key on save.</summary>
        [JsonPropertyName("sourceFieldGuid")]
        public string SourceFieldGuid { get; set; } = null!;

        [JsonPropertyName("operator")]
        public ConditionOperator Operator { get; set; } = ConditionOperator.Equals;

        [JsonPropertyName("valueJson")]
        public string ValueJson { get; set; } = "null";

        [JsonPropertyName("order")]
        public int Order { get; set; }
    }

    public class DisplayConditionGroupDto
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("targetType")]
        public DisplayConditionTargetType TargetType { get; set; }

        [JsonPropertyName("innerLogic")]
        public DisplayConditionLogic InnerLogic { get; set; } = DisplayConditionLogic.And;

        [JsonPropertyName("combineWithPreviousLogic")]
        public DisplayConditionLogic CombineWithPreviousLogic { get; set; } = DisplayConditionLogic.Or;

        [JsonPropertyName("order")]
        public int Order { get; set; }

        [JsonPropertyName("isActive")]
        public bool IsActive { get; set; } = true;

        [JsonPropertyName("conditions")]
        public List<DisplayConditionDto> Conditions { get; set; } = new();
    }
}
