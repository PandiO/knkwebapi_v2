You are GitHub Copilot working in a .NET Core Web API project for the “Knights and Kings” game backend.

Goal
----
Implement support for storing Minecraft-related references in the database in a version-tolerant way, without depending directly on Bukkit/Spigot types. The backend must define new models for:

1. `MinecraftMaterialRef` (represents a Minecraft material / item as stored in the DB)
2. `MinecraftBlockRef` (represents a Minecraft block + optional BlockData/state as stored in the DB)

For each of these models you must:
- Create the model in `/Models`
- Create repository interface + implementation in `/Repositories/Interfaces` and `/Repositories`
- Create service interface + implementation in `/Services/Interfaces` and `/Services`
- Create controller in `/Controllers`
- Create DTO definitions in `/Dtos`
- Create an AutoMapper mapping profile in `/Mapping` with the naming convention `${ModelName}MappingProfile.cs`
- Register the model in `KnKDbContext` and update any needed configuration
- Add EF Core migrations for all DB changes, following the existing migration style in the project

IMPORTANT: You must look at existing entities and layers (e.g. Category, Town, Street, District, Structure or similar) and copy their architecture:
- Same naming conventions for interfaces, repositories, services, DTOs and controllers
- Same async patterns, return types, error handling and pagination (if used elsewhere)
- Same DI/registration patterns (Program.cs / Startup.cs, etc.)
- Same route conventions and attribute usage on controllers
- Same way of structuring request/response DTOs and AutoMapper profiles

Do NOT invent a radically new pattern. Reuse the existing style.

New models
----------

1) MinecraftMaterialRef

Create the `MinecraftMaterialRef` model in `/Models/MinecraftMaterialRef.cs` with at least these properties:

- `int Id` – primary key
- `string NamespaceKey` – required, unique; Minecraft namespaced key like "minecraft:white_banner"
- `string? LegacyName` – optional; used for older/legacy names or migration
- `string Category` – required; e.g. "ICON", "BLOCK", "BANNER", "ITEM" to filter in UI

Apply appropriate EF Core attributes/configuration:
- Make `NamespaceKey` required and unique.
- Make `Category` required.
- `LegacyName` is nullable.

Create repository + service + controller + DTOs:
- Repository interface: `/Repositories/Interfaces/IMinecraftMaterialRefRepository.cs`
- Repository implementation: `/Repositories/MinecraftMaterialRefRepository.cs`
- Service interface: `/Services/Interfaces/IMinecraftMaterialRefService.cs`
- Service implementation: `/Services/MinecraftMaterialRefService.cs`
- Controller: `/Controllers/MinecraftMaterialRefController.cs`
  - Use the same route pattern as other controllers, e.g. `[Route("api/[controller]")]`, `[ApiController]`.
- DTOs in `/Dtos`:
  - `MinecraftMaterialRefDto` – for reading (Id, NamespaceKey, LegacyName, Category, plus any metadata you normally expose).
  - `MinecraftMaterialRefCreateDto` – for creating.
  - `MinecraftMaterialRefUpdateDto` – for updating.

The controller must expose standard REST endpoints consistent with how it is done for other entities:
- GET all, GET by id, POST, PUT, DELETE (and any extra endpoints your project usually defines).
- Use the service layer, not the repository directly.
- Use AutoMapper to map between DTOs and the entity.

In `/Mapping/MinecraftMaterialRefMappingProfile.cs`, create an AutoMapper profile:
- Map between `MinecraftMaterialRef` and the DTOs (`MinecraftMaterialRefDto`, `MinecraftMaterialRefCreateDto`, `MinecraftMaterialRefUpdateDto`).
- Follow the style and base class as other mapping profiles in `/Mapping`.

Register this model in `KnKDbContext`:
- Add a `DbSet<MinecraftMaterialRef> MinecraftMaterialRefs { get; set; }`.
- Apply configurations (fluent API) if needed, similar to other entities.

2) MinecraftBlockRef

Create the `MinecraftBlockRef` model in `/Models/MinecraftBlockRef.cs` with at least these properties:

- `int Id` – primary key
- `string NamespaceKey` – required; block’s namespaced key, e.g. "minecraft:oak_log"
- `string? BlockStateString` – optional; full BlockData/state string, e.g. "minecraft:oak_log[axis=x]"
- `string? LogicalType` – optional; semantic type like "LOG", "STAIRS", "SLAB" for filtering.

This model represents a reusable block definition (material + optional state). It is NOT tied directly to Bukkit types or to a specific world location.

Apply EF Core configuration:
- `NamespaceKey` required.
- Other fields nullable unless you see a better constraint based on existing patterns.

Create repository + service + controller + DTOs:
- Repository interface: `/Repositories/Interfaces/IMinecraftBlockRefRepository.cs`
- Repository implementation: `/Repositories/MinecraftBlockRefRepository.cs`
- Service interface: `/Services/Interfaces/IMinecraftBlockRefService.cs`
- Service implementation: `/Services/MinecraftBlockRefService.cs`
- Controller: `/Controllers/MinecraftBlockRefController.cs`
- DTOs in `/Dtos`:
  - `MinecraftBlockRefDto`
  - `MinecraftBlockRefCreateDto`
  - `MinecraftBlockRefUpdateDto`

The controller must again follow the standard pattern (GET all, GET by id, POST, PUT, DELETE, etc.) and use AutoMapper and the service.

In `/Mapping/MinecraftBlockRefMappingProfile.cs`, create the AutoMapper profile for this model and its DTOs.

Register this model in `KnKDbContext`:
- Add `DbSet<MinecraftBlockRef> MinecraftBlockRefs { get; set; }`.
- Add model configuration similar to other models.

3) Integrate with existing domain (Category icon)

Update the `Category` model (or equivalent entity that represents game categories / menu categories) so that it can use a `MinecraftMaterialRef` as an icon:

- Add an optional foreign key property:
  - `int? IconMaterialRefId` (nullable)
  - Navigation property: `MinecraftMaterialRef? IconMaterialRef`
- Configure the relationship in `KnKDbContext` using fluent API, following the style of other relationships:
  - One `MinecraftMaterialRef` can be used by many categories as icon.
- Update the Category DTOs to expose this:
  - Add `IconMaterialRefId` or an embedded DTO if this is the existing style.
- Update the Category AutoMapper mapping profile to map the new property.
- Update the Category service and any controller create/update logic to handle the icon reference (e.g., setting IconMaterialRefId from a DTO field).

4) Patterns and consistency

For ALL new classes:
- Follow the same namespaces as the rest of the project (look at existing models, repositories, services, controllers, DTOs, mapping profiles).
- Use the same visibility, attributes, async patterns and error-handling conventions.
- Ensure everything is wired up in DI:
  - Register new repositories and services in the DI container (Program.cs or Startup.cs) exactly like the existing ones.
- Ensure all controllers are discovered automatically by ASP.NET Core as usual.

5) EF Core migrations

Update EF Core migrations to include all new tables and relationships:

- Add the necessary migration(s) to the existing `Migrations` folder.
- The migration must:
  - Create the `MinecraftMaterialRefs` table with the required columns and constraints.
  - Create the `MinecraftBlockRefs` table with the required columns and constraints.
  - Update the `Categories` table (or equivalent) with the new `IconMaterialRefId` foreign key column and relationship.
- Update the `KnKDbContextModelSnapshot` to reflect these changes.
- Follow the naming and structure of existing migrations (e.g. `AddMinecraftMaterialRefAndMinecraftBlockRef` or similar).
- If it is not possible to actually run `dotnet ef migrations add` from code, at least provide the full migration class and updated model snapshot consistent with how existing migrations are written.

6) Validation and documentation

- Add any basic validation attributes (e.g. `[Required]`, `[MaxLength(...)]`) consistent with existing models.
- Add XML comments or summary comments if other models use them.
- Make sure new endpoints are clear and self-consistent so the frontend can:
  - Fetch lists of `MinecraftMaterialRef` (for icon pickers).
  - Fetch lists of `MinecraftBlockRef` (for future features like gate building).

Finally, after implementing all of this, ensure the solution builds without errors and that all new classes compile cleanly, referencing the correct namespaces and existing infrastructure.
