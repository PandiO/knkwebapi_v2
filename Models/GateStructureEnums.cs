namespace knkwebapi_v2.Models;

public enum GateType
{
    SLIDING,
    TRAP,
    DRAWBRIDGE,
    DOUBLE_DOORS
}

public enum GeometryDefinitionMode
{
    PLANE_GRID,
    FLOOD_FILL
}

public enum MotionType
{
    VERTICAL,
    LATERAL,
    ROTATION
}

public enum TileEntityPolicy
{
    NONE,
    DECORATIVE_ONLY,
    ALL
}

public enum HealthDisplayMode
{
    ALWAYS,
    DAMAGED_ONLY,
    NEVER,
    SIEGE_ONLY
}