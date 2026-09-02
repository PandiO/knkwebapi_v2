using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace knkwebapi_v2.Dtos
{
    /// <summary>
    /// Well-known WorldTask.TaskType values that are handled without a player (headless).
    /// </summary>
    public static class WorldTaskTypes
    {
        public const string GateBlockScan = "GateBlockScan";
    }

    /// <summary>
    /// InputJson payload for a GateBlockScan WorldTask.
    /// The plugin re-fetches full gate geometry via GateStructuresApi.getById(GateStructureId);
    /// this payload only needs to route the task to the right gate.
    /// </summary>
    public class GateBlockScanRequestDto
    {
        [JsonPropertyName("gateStructureId")]
        public int GateStructureId { get; set; }
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum GateBlockScanStatus
    {
        Success,
        Warning,
        Failed
    }

    /// <summary>
    /// OutputJson payload produced by the plugin once a GateBlockScan WorldTask finishes.
    /// </summary>
    public class GateBlockScanResultDto
    {
        [JsonPropertyName("status")]
        public GateBlockScanStatus Status { get; set; }

        [JsonPropertyName("blockCount")]
        public int BlockCount { get; set; }

        [JsonPropertyName("snapshots")]
        public List<GateBlockSnapshotCreateDto> Snapshots { get; set; } = new();

        [JsonPropertyName("warnings")]
        public List<string> Warnings { get; set; } = new();

        [JsonPropertyName("errorMessage")]
        public string? ErrorMessage { get; set; }
    }
}
