using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;

namespace knkwebapi_v2.Dtos
{
    // Read DTO - Full representation
    public class EnchantmentDefinitionReadDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("key")]
        public string Key { get; set; } = string.Empty;

        [JsonPropertyName("displayName")]
        public string DisplayName { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("isCustom")]
        public bool IsCustom { get; set; }

        [JsonPropertyName("maxLevel")]
        public int MaxLevel { get; set; } = 1;

        [JsonPropertyName("minecraftEnchantmentRefId")]
        public int? MinecraftEnchantmentRefId { get; set; }

        [JsonPropertyName("baseEnchantmentRef")]
        public MinecraftEnchantmentRefNavDto? BaseEnchantmentRef { get; set; }

        [JsonPropertyName("defaultForBlueprints")]
        public List<ItemBlueprintNavDto> DefaultForBlueprints { get; set; } = new();

        [JsonPropertyName("abilityDefinition")]
        public AbilityDefinitionReadDto? AbilityDefinition { get; set; }
    }

    // Create DTO
    public class EnchantmentDefinitionCreateDto
    {
        [Required]
        [JsonPropertyName("key")]
        public string Key { get; set; } = string.Empty;

        [Required]
        [JsonPropertyName("displayName")]
        public string DisplayName { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("isCustom")]
        public bool IsCustom { get; set; }

        [JsonPropertyName("maxLevel")]
        public int MaxLevel { get; set; } = 1;

        [JsonPropertyName("minecraftEnchantmentRefId")]
        public int? MinecraftEnchantmentRefId { get; set; }

        // Optional: for hybrid picker - namespace key if enchantment not persisted yet
        [JsonPropertyName("enchantmentNamespaceKey")]
        public string? EnchantmentNamespaceKey { get; set; }

        [JsonPropertyName("abilityDefinition")]
        public AbilityDefinitionUpsertDto? AbilityDefinition { get; set; }
    }

    // Update DTO
    public class EnchantmentDefinitionUpdateDto
    {
        [Required]
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [Required]
        [JsonPropertyName("key")]
        public string Key { get; set; } = string.Empty;

        [Required]
        [JsonPropertyName("displayName")]
        public string DisplayName { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("isCustom")]
        public bool IsCustom { get; set; }

        [JsonPropertyName("maxLevel")]
        public int MaxLevel { get; set; } = 1;

        [JsonPropertyName("minecraftEnchantmentRefId")]
        public int? MinecraftEnchantmentRefId { get; set; }

        [JsonPropertyName("enchantmentNamespaceKey")]
        public string? EnchantmentNamespaceKey { get; set; }

        [JsonPropertyName("abilityDefinition")]
        public AbilityDefinitionUpsertDto? AbilityDefinition { get; set; }
    }

    // List DTO - For search/list views
    public class EnchantmentDefinitionListDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("key")]
        public string Key { get; set; } = string.Empty;

        [JsonPropertyName("displayName")]
        public string DisplayName { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("maxLevel")]
        public int MaxLevel { get; set; } = 1;

        [JsonPropertyName("isCustom")]
        public bool IsCustom { get; set; }

        [JsonPropertyName("minecraftEnchantmentRefId")]
        public int? MinecraftEnchantmentRefId { get; set; }

        [JsonPropertyName("baseEnchantmentNamespaceKey")]
        public string? BaseEnchantmentNamespaceKey { get; set; }

        [JsonPropertyName("blueprintCount")]
        public int BlueprintCount { get; set; }
    }

    // MinecraftEnchantmentRefNavDto for relationship
    public class MinecraftEnchantmentRefNavDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("namespaceKey")]
        public string NamespaceKey { get; set; } = string.Empty;

        [JsonPropertyName("displayName")]
        public string? DisplayName { get; set; }

        [JsonPropertyName("maxLevel")]
        public int? MaxLevel { get; set; }

        [JsonPropertyName("iconUrl")]
        public string? IconUrl { get; set; }
    }

    public class AbilityDefinitionReadDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("enchantmentDefinitionId")]
        public int EnchantmentDefinitionId { get; set; }

        [JsonPropertyName("abilityKey")]
        public string AbilityKey { get; set; } = string.Empty;

        [JsonPropertyName("runtimeConfigJson")]
        public string? RuntimeConfigJson { get; set; }

        [JsonPropertyName("futureUserAssignmentContract")]
        public string? FutureUserAssignmentContract { get; set; }
    }

    public class AbilityDefinitionUpsertDto
    {
        [Required]
        [JsonPropertyName("abilityKey")]
        public string AbilityKey { get; set; } = string.Empty;

        [JsonPropertyName("runtimeConfigJson")]
        public string? RuntimeConfigJson { get; set; }

        [JsonPropertyName("futureUserAssignmentContract")]
        public string? FutureUserAssignmentContract { get; set; }
    }
}
