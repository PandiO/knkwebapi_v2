# BACKEND REQUIREMENTS: DisplayConfiguration Feature v2

**Datum:** 10 december 2025  
**Status:** Final Requirements - Klaar voor implementatie

---

## 1. OVERZICHT

### 1.1 Doel
Implementatie van een DisplayConfiguration systeem parallel aan FormConfiguration voor het **tonen** van entity data. Het systeem biedt herbruikbare, configureerbare display templates voor entities binnen Knights and Kings.

### 1.2 Architectuur Principes
- Volgt exact dezelfde patronen als FormConfiguration
- Models → DTOs → Repositories → Services → Controllers
- AutoMapper voor DTO mappings
- EF Core migrations voor database schema
- Dependency Injection via ServiceCollectionExtensions

---

## 2. DATABASE MODELS

### 2.1 DisplayConfiguration

**Namespace:** `knkwebapi_v2.Models`

**Doel:** Template voor frontend om entity informatie geordend te tonen in DisplayWizard component.

**Properties:**

| Property | Type | Required | Default | Beschrijving |
|----------|------|----------|---------|--------------|
| Id | int | Yes | - | Primary key |
| ConfigurationGuid | Guid | Yes | Guid.NewGuid() | Unique identifier voor tracking |
| Name | string | Yes | "{EntityTypeName} Display Template" | Configuratie naam |
| EntityTypeName | string | Yes | - | Entity class naam (bijv. "Town") |
| IsDefault | bool | Yes | false | Eén default per EntityTypeName |
| Description | string? | No | null | Optionele beschrijving |
| CreatedAt | DateTime | Yes | DateTime.UtcNow | Aanmaak timestamp |
| UpdatedAt | DateTime? | No | null | Wijzig timestamp |
| SectionOrderJson | string | Yes | "[]" | JSON array met geordende Section GUIDs |
| IsDraft | bool | Yes | true | Draft = incomplete/invalid toegestaan |
| Sections | List\<DisplaySection\> | Yes | new() | Navigation property |

**Database Configuratie:**
```csharp
// In KnKDbContext.cs OnModelCreating
modelBuilder.Entity<DisplayConfiguration>()
    .HasMany(dc => dc.Sections)
    .WithOne(ds => ds.DisplayConfiguration)
    .HasForeignKey(ds => ds.DisplayConfigurationId)
    .OnDelete(DeleteBehavior.Cascade);

modelBuilder.Entity<DisplayConfiguration>()
    .HasIndex(dc => new { dc.EntityTypeName, dc.IsDefault })
    .HasDatabaseName("IX_DisplayConfiguration_EntityType_Default");

modelBuilder.Entity<DisplayConfiguration>()
    .HasIndex(dc => dc.IsDraft)
    .HasDatabaseName("IX_DisplayConfiguration_IsDraft");
```

**Validatie Regels:**
- `EntityTypeName` moet bestaan in EntityMetadata
- Maximaal 1 `IsDefault = true` per EntityTypeName
- `SectionOrderJson` moet valid JSON array zijn
- Bij `IsDraft = false`: volledige validatie vereist

---

### 2.2 DisplaySection

**Namespace:** `knkwebapi_v2.Models`

**Doel:** Groepeert display fields, ondersteunt relationship binding en nested sections voor collections.

**Properties:**

| Property | Type | Required | Default | Beschrijving |
|----------|------|----------|---------|--------------|
| Id | int | Yes | - | Primary key |
| SectionGuid | Guid | Yes | Guid.NewGuid() | Gebruikt in SectionOrderJson |
| SectionName | string | Yes | - | Sectie titel |
| Description | string? | No | null | Optionele beschrijving |
| IsReusable | bool | Yes | false | Template in library |
| SourceSectionId | int? | No | null | Bron bij clonen |
| IsLinkedToSource | bool | Yes | false | Link (true) of Copy (false) mode |
| FieldOrderJson | string | Yes | "[]" | JSON array met geordende Field GUIDs |
| RelatedEntityPropertyName | string? | No | null | Property naam (bijv. "TownHouse", "Districts") |
| RelatedEntityTypeName | string? | No | null | Entity type naam (bijv. "Structure", "District") |
| IsCollection | bool | Yes | false | True als property een ICollection\<T\> is |
| ActionButtonsConfigJson | string | Yes | "{}" | JSON object met button configuratie |
| ParentSectionId | int? | No | null | **NIEUW:** Voor nested sections (subsections) |
| CreatedAt | DateTime | Yes | DateTime.UtcNow | Aanmaak timestamp |
| UpdatedAt | DateTime? | No | null | Wijzig timestamp |
| DisplayConfigurationId | int? | No | null | NULL als IsReusable = true |
| DisplayConfiguration | DisplayConfiguration? | No | null | Navigation property |
| Fields | List\<DisplayField\> | Yes | new() | Navigation property |
| SubSections | List\<DisplaySection\> | Yes | new() | **NIEUW:** Child sections voor collections |
| ParentSection | DisplaySection? | No | null | **NIEUW:** Parent section reference |

**Nested Sections Concept:**
Voor collection properties (bijv. `Town.Districts`) kan een DisplaySection subsections bevatten:
- **Parent Section:** Gebonden aan `Districts` collection property
- **SubSections:** Template voor elke District entry in de collection
- Frontend itereert over collection data en rendert subsection per entry

**Voorbeeld Hiërarchie:**
```
DisplayConfiguration: "Town Detail View"
├── Section: "General Info"
│   ├── Field: Name
│   └── Field: Description
└── Section: "Districts" (RelatedEntityPropertyName = "Districts", IsCollection = true)
    └── SubSection: "District Template" (ParentSectionId = [Districts Section Id])
        ├── Field: District.Name
        ├── Field: District.Population
        └── Field: District.Area
```

**Database Configuratie:**
```csharp
modelBuilder.Entity<DisplaySection>()
    .HasMany(ds => ds.Fields)
    .WithOne(df => df.DisplaySection)
    .HasForeignKey(df => df.DisplaySectionId)
    .OnDelete(DeleteBehavior.Cascade);

modelBuilder.Entity<DisplaySection>()
    .HasMany(ds => ds.SubSections)
    .WithOne(ss => ss.ParentSection)
    .HasForeignKey(ss => ss.ParentSectionId)
    .OnDelete(DeleteBehavior.Cascade);

modelBuilder.Entity<DisplaySection>()
    .HasIndex(ds => ds.IsReusable)
    .HasDatabaseName("IX_DisplaySection_IsReusable");

modelBuilder.Entity<DisplaySection>()
    .HasIndex(ds => ds.ParentSectionId)
    .HasDatabaseName("IX_DisplaySection_ParentSectionId");
```

**Validatie Regels:**
- `RelatedEntityPropertyName` en `RelatedEntityTypeName` beide NULL of beide gevuld
- Als `RelatedEntityPropertyName` set: property moet bestaan in EntityMetadata
- Als `IsCollection = true`: property moet ICollection\<T\> zijn
- Als `ParentSectionId` set: parent moet `IsCollection = true` hebben
- `FieldOrderJson` moet valid JSON array zijn
- `ActionButtonsConfigJson` moet valid JSON object zijn

**ActionButtonsConfigJson Structure:**
```json
// Voor single relationship (bijv. TownHouse)
{
  "showViewButton": true,
  "showEditButton": true,
  "showSelectButton": true,
  "showUnlinkButton": true,
  "showCreateButton": false
}

// Voor collection relationship (bijv. Districts)
{
  "showViewButton": true,
  "showEditButton": true,
  "showAddButton": true,
  "showRemoveButton": true,
  "showCreateButton": true
}
```

---

### 2.3 DisplayField

**Namespace:** `knkwebapi_v2.Models`

**Doel:** Toont entity property of custom template text met variable interpolatie.

**Properties:**

| Property | Type | Required | Default | Beschrijving |
|----------|------|----------|---------|--------------|
| Id | int | Yes | - | Primary key |
| FieldGuid | Guid | Yes | Guid.NewGuid() | Gebruikt in FieldOrderJson |
| FieldName | string? | No | null | Property naam (bijv. "Name", "TownHouse.Street.Name") |
| Label | string | Yes | - | Display label |
| Description | string? | No | null | Help text |
| TemplateText | string? | No | null | Custom text met ${...} variabelen |
| FieldType | string? | No | null | Type hint (bijv. "String", "DateTime", "Integer") |
| IsReusable | bool | Yes | false | Template in library |
| SourceFieldId | int? | No | null | Bron bij clonen |
| IsLinkedToSource | bool | Yes | false | Link (true) of Copy (false) mode |
| CreatedAt | DateTime | Yes | DateTime.UtcNow | Aanmaak timestamp |
| UpdatedAt | DateTime? | No | null | Wijzig timestamp |
| DisplaySectionId | int? | No | null | NULL als IsReusable = true |
| DisplaySection | DisplaySection? | No | null | Navigation property |

**Template Text Interpolatie:**

**Ondersteunde variabelen (max 2 lagen diepte):**
```
${Name}                          // Direct property
${TownHouse.Name}               // 1 laag nested
${TownHouse.Street.Name}        // 2 lagen nested (MAXIMUM)
${Districts.Count}              // Collection count
${CreatedAt}                    // DateTime property
```

**Ondersteunde calculations (basis):**
```
${Districts.Count + Streets.Count}     // Optellen
${Districts.Count - 5}                 // Aftrekken
${Area * 2}                           // Vermenigvuldigen
${Population / Area}                   // Delen
```

**NIET ondersteund (toekomstige features):**
```
${Districts.Count > 0 ? "Yes" : "No"}  // Conditionals
${Districts[0].Name}                   // Array indexing
```

**Database Configuratie:**
```csharp
modelBuilder.Entity<DisplayField>()
    .HasIndex(df => df.IsReusable)
    .HasDatabaseName("IX_DisplayField_IsReusable");
```

**Validatie Regels:**
- Minimaal `FieldName` OF `TemplateText` moet gevuld zijn
- Als `FieldName` set: property moet bestaan in EntityMetadata
- Als `TemplateText` set: alle ${...} variabelen moeten valideren
- Nested properties max 2 lagen diep
- Collection properties alleen voor `.Count`

---

## 3. DTOs

### 3.1 Core DTOs

**DisplayConfigurationDto:**
```csharp
namespace knkwebapi_v2.Dtos
{
    public class DisplayConfigurationDto
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }
        
        [JsonPropertyName("configurationGuid")]
        public string? ConfigurationGuid { get; set; }
        
        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;
        
        [JsonPropertyName("entityTypeName")]
        public string EntityTypeName { get; set; } = null!;
        
        [JsonPropertyName("isDefault")]
        public bool IsDefault { get; set; }
        
        [JsonPropertyName("description")]
        public string? Description { get; set; }
        
        [JsonPropertyName("sectionOrderJson")]
        public string? SectionOrderJson { get; set; }
        
        [JsonPropertyName("isDraft")]
        public bool IsDraft { get; set; } = true;
        
        [JsonPropertyName("createdAt")]
        public string? CreatedAt { get; set; }
        
        [JsonPropertyName("updatedAt")]
        public string? UpdatedAt { get; set; }
        
        [JsonPropertyName("sections")]
        public List<DisplaySectionDto> Sections { get; set; } = new();
    }
}
```

**DisplaySectionDto:**
```csharp
namespace knkwebapi_v2.Dtos
{
    public class DisplaySectionDto
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }
        
        [JsonPropertyName("sectionGuid")]
        public string? SectionGuid { get; set; }
        
        [JsonPropertyName("displayConfigurationId")]
        public string? DisplayConfigurationId { get; set; }
        
        [JsonPropertyName("sectionName")]
        public string SectionName { get; set; } = null!;
        
        [JsonPropertyName("description")]
        public string? Description { get; set; }
        
        [JsonPropertyName("isReusable")]
        public bool IsReusable { get; set; }
        
        [JsonPropertyName("sourceSectionId")]
        public string? SourceSectionId { get; set; }
        
        [JsonPropertyName("isLinkedToSource")]
        public bool IsLinkedToSource { get; set; }
        
        [JsonPropertyName("fieldOrderJson")]
        public string? FieldOrderJson { get; set; }
        
        [JsonPropertyName("relatedEntityPropertyName")]
        public string? RelatedEntityPropertyName { get; set; }
        
        [JsonPropertyName("relatedEntityTypeName")]
        public string? RelatedEntityTypeName { get; set; }
        
        [JsonPropertyName("isCollection")]
        public bool IsCollection { get; set; }
        
        [JsonPropertyName("actionButtonsConfigJson")]
        public string? ActionButtonsConfigJson { get; set; }
        
        [JsonPropertyName("parentSectionId")]
        public string? ParentSectionId { get; set; }
        
        [JsonPropertyName("createdAt")]
        public string? CreatedAt { get; set; }
        
        [JsonPropertyName("updatedAt")]
        public string? UpdatedAt { get; set; }
        
        [JsonPropertyName("fields")]
        public List<DisplayFieldDto> Fields { get; set; } = new();
        
        [JsonPropertyName("subSections")]
        public List<DisplaySectionDto> SubSections { get; set; } = new();
    }
}
```

**DisplayFieldDto:**
```csharp
namespace knkwebapi_v2.Dtos
{
    public class DisplayFieldDto
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }
        
        [JsonPropertyName("fieldGuid")]
        public string? FieldGuid { get; set; }
        
        [JsonPropertyName("displaySectionId")]
        public string? DisplaySectionId { get; set; }
        
        [JsonPropertyName("fieldName")]
        public string? FieldName { get; set; }
        
        [JsonPropertyName("label")]
        public string Label { get; set; } = null!;
        
        [JsonPropertyName("description")]
        public string? Description { get; set; }
        
        [JsonPropertyName("templateText")]
        public string? TemplateText { get; set; }
        
        [JsonPropertyName("fieldType")]
        public string? FieldType { get; set; }
        
        [JsonPropertyName("isReusable")]
        public bool IsReusable { get; set; }
        
        [JsonPropertyName("sourceFieldId")]
        public string? SourceFieldId { get; set; }
        
        [JsonPropertyName("isLinkedToSource")]
        public bool IsLinkedToSource { get; set; }
        
        [JsonPropertyName("createdAt")]
        public string? CreatedAt { get; set; }
        
        [JsonPropertyName("updatedAt")]
        public string? UpdatedAt { get; set; }
    }
}
```

### 3.2 Helper DTOs

**ActionButtonsConfigDto:**
```csharp
namespace knkwebapi_v2.Dtos
{
    /// <summary>
    /// Typed representation of ActionButtonsConfigJson.
    /// Used for validation and type-safe serialization.
    /// </summary>
    public class ActionButtonsConfigDto
    {
        // Common buttons
        [JsonPropertyName("showViewButton")]
        public bool ShowViewButton { get; set; }
        
        [JsonPropertyName("showEditButton")]
        public bool ShowEditButton { get; set; }
        
        // Single relationship buttons
        [JsonPropertyName("showSelectButton")]
        public bool ShowSelectButton { get; set; }
        
        [JsonPropertyName("showUnlinkButton")]
        public bool ShowUnlinkButton { get; set; }
        
        // Collection relationship buttons
        [JsonPropertyName("showAddButton")]
        public bool ShowAddButton { get; set; }
        
        [JsonPropertyName("showRemoveButton")]
        public bool ShowRemoveButton { get; set; }
        
        // Both types
        [JsonPropertyName("showCreateButton")]
        public bool ShowCreateButton { get; set; }
    }
}
```

**ValidationResultDto:**
```csharp
namespace knkwebapi_v2.Dtos
{
    public class DisplayValidationResultDto
    {
        [JsonPropertyName("isValid")]
        public bool IsValid { get; set; }
        
        [JsonPropertyName("errors")]
        public List<string> Errors { get; set; } = new();
        
        [JsonPropertyName("warnings")]
        public List<string> Warnings { get; set; } = new();
        
        [JsonPropertyName("fieldErrors")]
        public Dictionary<string, List<string>> FieldErrors { get; set; } = new();
    }
}
```

---

## 4. REPOSITORIES

### 4.1 IDisplayConfigurationRepository

```csharp
namespace knkwebapi_v2.Repositories.Interfaces
{
    public interface IDisplayConfigurationRepository
    {
        Task<IEnumerable<DisplayConfiguration>> GetAllAsync(bool includeDrafts = true);
        Task<DisplayConfiguration?> GetByIdAsync(int id, bool includeRelated = true);
        Task<DisplayConfiguration?> GetByEntityTypeNameAsync(
            string entityTypeName, 
            bool defaultOnly = false,
            bool includeDrafts = true);
        Task<IEnumerable<DisplayConfiguration>> GetAllByEntityTypeNameAsync(
            string entityTypeName,
            bool includeDrafts = true);
        Task<DisplayConfiguration> CreateAsync(DisplayConfiguration config);
        Task UpdateAsync(DisplayConfiguration config);
        Task DeleteAsync(int id);
        Task<bool> IsDefaultExistsAsync(
            string entityTypeName, 
            int? excludeId = null);
        Task<IEnumerable<string>> GetEntityTypeNamesAsync();
    }
}
```

**Implementatie Details:**
- `GetByIdAsync`: Include Sections, SubSections, Fields wanneer `includeRelated = true`
- `GetAllAsync`: Filter drafts wanneer `includeDrafts = false`
- `IsDefaultExistsAsync`: Check uniqueness voor default configs
- Transaction support voor complexe save operaties

---

### 4.2 IDisplaySectionRepository

```csharp
namespace knkwebapi_v2.Repositories.Interfaces
{
    public interface IDisplaySectionRepository
    {
        Task<IEnumerable<DisplaySection>> GetAllReusableAsync();
        Task<DisplaySection?> GetByIdAsync(int id, bool includeRelated = true);
        Task<DisplaySection> CreateAsync(DisplaySection section);
        Task UpdateAsync(DisplaySection section);
        Task DeleteAsync(int id);
        Task<DisplaySection?> GetSourceSectionAsync(int sourceSectionId);
    }
}
```

**Implementatie Details:**
- `GetByIdAsync`: Include Fields en SubSections wanneer `includeRelated = true`
- `GetAllReusableAsync`: Alleen sections met `IsReusable = true`
- `GetSourceSectionAsync`: Voor linked sections om source data op te halen

---

### 4.3 IDisplayFieldRepository

```csharp
namespace knkwebapi_v2.Repositories.Interfaces
{
    public interface IDisplayFieldRepository
    {
        Task<IEnumerable<DisplayField>> GetAllReusableAsync();
        Task<DisplayField?> GetByIdAsync(int id);
        Task<DisplayField> CreateAsync(DisplayField field);
        Task UpdateAsync(DisplayField field);
        Task DeleteAsync(int id);
        Task<DisplayField?> GetSourceFieldAsync(int sourceFieldId);
    }
}
```

**Implementatie Details:**
- `GetAllReusableAsync`: Alleen fields met `IsReusable = true`
- `GetSourceFieldAsync`: Voor linked fields om source data op te halen

---

## 5. SERVICES

### 5.1 IDisplayConfigurationService

```csharp
namespace knkwebapi_v2.Services.Interfaces
{
    public interface IDisplayConfigurationService
    {
        Task<IEnumerable<DisplayConfigurationDto>> GetAllAsync(bool includeDrafts = true);
        Task<DisplayConfigurationDto?> GetByIdAsync(int id);
        Task<DisplayConfigurationDto?> GetDefaultByEntityTypeNameAsync(
            string entityTypeName,
            bool includeDrafts = false);
        Task<IEnumerable<DisplayConfigurationDto>> GetAllByEntityTypeNameAsync(
            string entityTypeName,
            bool includeDrafts = true);
        Task<IEnumerable<string>> GetEntityTypeNamesAsync();
        Task<DisplayConfigurationDto> CreateAsync(DisplayConfigurationDto dto);
        Task UpdateAsync(int id, DisplayConfigurationDto dto);
        Task DeleteAsync(int id);
        Task<DisplayValidationResultDto> ValidateAsync(int id);
        Task<DisplayConfigurationDto> PublishAsync(int id); // IsDraft = false
    }
}
```

**Service Logic:**
- `CreateAsync`: Set `IsDraft = true` default, validate basic structure
- `UpdateAsync`: Preserveer `IsDraft` status, `UpdatedAt = DateTime.UtcNow`
- `ValidateAsync`: Roep IDisplayTemplateValidationService aan
- `PublishAsync`: Set `IsDraft = false`, voer volledige validatie uit

---

### 5.2 IDisplaySectionService

```csharp
namespace knkwebapi_v2.Services.Interfaces
{
    public interface IDisplaySectionService
    {
        Task<IEnumerable<DisplaySectionDto>> GetAllReusableAsync();
        Task<DisplaySectionDto?> GetByIdAsync(int id);
        Task<DisplaySectionDto> CreateReusableAsync(DisplaySectionDto dto);
        Task UpdateAsync(int id, DisplaySectionDto dto);
        Task DeleteAsync(int id);
        Task<DisplaySectionDto> CloneSectionAsync(
            int sourceSectionId, 
            ReuseLinkMode linkMode);
        Task<DisplaySectionDto> ResolveLinkedSectionAsync(DisplaySectionDto section);
    }
}
```

**Service Logic:**
- `CloneSectionAsync`: Clone met Link of Copy mode, inclusief SubSections en Fields
- `ResolveLinkedSectionAsync`: Als `IsLinkedToSource = true`, merge source properties

---

### 5.3 IDisplayFieldService

```csharp
namespace knkwebapi_v2.Services.Interfaces
{
    public interface IDisplayFieldService
    {
        Task<IEnumerable<DisplayFieldDto>> GetAllReusableAsync();
        Task<DisplayFieldDto?> GetByIdAsync(int id);
        Task<DisplayFieldDto> CreateReusableAsync(DisplayFieldDto dto);
        Task UpdateAsync(int id, DisplayFieldDto dto);
        Task DeleteAsync(int id);
        Task<DisplayFieldDto> CloneFieldAsync(
            int sourceFieldId, 
            ReuseLinkMode linkMode);
        Task<DisplayFieldDto> ResolveLinkedFieldAsync(DisplayFieldDto field);
    }
}
```

**Service Logic:**
- `CloneFieldAsync`: Clone met Link of Copy mode
- `ResolveLinkedFieldAsync`: Als `IsLinkedToSource = true`, merge source properties

---

### 5.4 IDisplayTemplateValidationService

```csharp
namespace knkwebapi_v2.Services.Interfaces
{
    public interface IDisplayTemplateValidationService
    {
        /// <summary>
        /// Volledige validatie van DisplayConfiguration.
        /// Gebruikt bij PublishAsync (IsDraft = false).
        /// </summary>
        Task<DisplayValidationResultDto> ValidateConfigurationAsync(
            DisplayConfigurationDto config);
        
        /// <summary>
        /// Valideer specifieke section tegen EntityMetadata.
        /// </summary>
        Task<DisplayValidationResultDto> ValidateSectionAsync(
            DisplaySectionDto section,
            string targetEntityTypeName);
        
        /// <summary>
        /// Valideer field FieldName of TemplateText variabelen.
        /// </summary>
        Task<DisplayValidationResultDto> ValidateFieldAsync(
            DisplayFieldDto field,
            string targetEntityTypeName);
        
        /// <summary>
        /// Parse en valideer template text variabelen.
        /// Checks syntax, property existence, max 2 lagen nesting.
        /// </summary>
        Task<DisplayValidationResultDto> ValidateTemplateTextAsync(
            string templateText,
            string targetEntityTypeName);
        
        /// <summary>
        /// Valideer ActionButtonsConfigJson structuur.
        /// </summary>
        Task<DisplayValidationResultDto> ValidateActionButtonsConfigAsync(
            string actionButtonsConfigJson,
            bool isCollection);
    }
}
```

**Validatie Details:**

**ConfigurationAsync:**
- EntityTypeName bestaat in EntityMetadata
- Maximaal 1 IsDefault per EntityTypeName
- SectionOrderJson is valid JSON array
- Alle sections valideren recursief

**SectionAsync:**
- RelatedEntityPropertyName bestaat als property
- RelatedEntityTypeName matcht property type
- IsCollection = true → property is ICollection<T>
- ParentSectionId → parent.IsCollection = true
- ActionButtonsConfigJson valid volgens isCollection

**FieldAsync:**
- Als FieldName: property bestaat in metadata
- Als TemplateText: ValidateTemplateTextAsync
- Max 2 lagen nesting in property paths

**TemplateTextAsync:**
- Regex extract `${...}` variabelen
- Valideer elke variabele:
  - `${Name}` → property exists
  - `${Town.Street.Name}` → max 2 lagen, elk level exists
  - `${Districts.Count}` → property is collection
  - `${Districts.Count + Streets.Count}` → beide collections
- Check calculations syntax (basis operators: +, -, *, /)

**ActionButtonsConfigAsync:**
- Parse JSON naar ActionButtonsConfigDto
- Als isCollection = false: showAddButton/showRemoveButton = false
- Als isCollection = true: showSelectButton/showUnlinkButton = false

---

## 6. CONTROLLERS

### 6.1 DisplayConfigurationsController

**Base Route:** `/api/displayconfigurations`

**Endpoints:**

| Method | Route | Beschrijving |
|--------|-------|--------------|
| GET | `/` | Alle configs (query param: `?includeDrafts=true`) |
| GET | `/{id:int}` | Config by ID |
| GET | `/{entityName}` | Default config voor entity (query: `?includeDrafts=false`) |
| GET | `/{entityName}/all` | Alle configs voor entity |
| GET | `/entity-names` | Lijst van alle EntityTypeNames |
| POST | `/` | Nieuwe config aanmaken (IsDraft = true) |
| PUT | `/{id:int}` | Config updaten |
| DELETE | `/{id:int}` | Config verwijderen |
| POST | `/{id:int}/validate` | Valideer config |
| POST | `/{id:int}/publish` | Publish (IsDraft = false) |

**Controller Attributes:**
```csharp
[ApiController]
[Route("api/[controller]")]
public class DisplayConfigurationsController : ControllerBase
```

---

### 6.2 DisplaySectionsController

**Base Route:** `/api/displaysections`

**Endpoints:**

| Method | Route | Beschrijving |
|--------|-------|--------------|
| GET | `/reusable` | Alle reusable section templates |
| GET | `/{id:int}` | Section by ID |
| POST | `/reusable` | Nieuwe reusable section |
| PUT | `/{id:int}` | Section updaten |
| DELETE | `/{id:int}` | Section verwijderen |
| POST | `/{id:int}/clone` | Clone section (body: `{ "linkMode": "Copy"\|"Link" }`) |

---

### 6.3 DisplayFieldsController

**Base Route:** `/api/displayfields`

**Endpoints:**

| Method | Route | Beschrijving |
|--------|-------|--------------|
| GET | `/reusable` | Alle reusable field templates |
| GET | `/{id:int}` | Field by ID |
| POST | `/reusable` | Nieuwe reusable field |
| PUT | `/{id:int}` | Field updaten |
| DELETE | `/{id:int}` | Field verwijderen |
| POST | `/{id:int}/clone` | Clone field (body: `{ "linkMode": "Copy"\|"Link" }`) |

---

## 7. AUTOMAPPER PROFILES

**Bestand:** `Mapping/DisplayMappingProfile.cs`

**Mappings:**

```csharp
public class DisplayMappingProfile : Profile
{
    public DisplayMappingProfile()
    {
        // DisplayConfiguration
        CreateMap<DisplayConfiguration, DisplayConfigurationDto>()
            .ForMember(dest => dest.Id, 
                opt => opt.MapFrom(src => src.Id.ToString()))
            .ForMember(dest => dest.ConfigurationGuid, 
                opt => opt.MapFrom(src => src.ConfigurationGuid.ToString()))
            .ForMember(dest => dest.CreatedAt, 
                opt => opt.MapFrom(src => src.CreatedAt.ToString("o")))
            .ForMember(dest => dest.UpdatedAt, 
                opt => opt.MapFrom(src => src.UpdatedAt.HasValue 
                    ? src.UpdatedAt.Value.ToString("o") : null));
        
        CreateMap<DisplayConfigurationDto, DisplayConfiguration>()
            .ForMember(dest => dest.Id, 
                opt => opt.MapFrom(src => string.IsNullOrEmpty(src.Id) 
                    ? 0 : int.Parse(src.Id)))
            .ForMember(dest => dest.ConfigurationGuid, 
                opt => opt.MapFrom(src => string.IsNullOrEmpty(src.ConfigurationGuid) 
                    ? Guid.NewGuid() : Guid.Parse(src.ConfigurationGuid)))
            .ForMember(dest => dest.UpdatedAt, 
                opt => opt.MapFrom(src => string.IsNullOrEmpty(src.UpdatedAt) 
                    ? (DateTime?)null : DateTime.Parse(src.UpdatedAt)));
        
        // DisplaySection (zelfde patroon)
        CreateMap<DisplaySection, DisplaySectionDto>()
            .ForMember(dest => dest.Id, 
                opt => opt.MapFrom(src => src.Id.ToString()))
            // ... rest van mappings
        
        CreateMap<DisplaySectionDto, DisplaySection>()
            // ... reverse mappings
        
        // DisplayField (zelfde patroon)
        CreateMap<DisplayField, DisplayFieldDto>()
            // ... mappings
        
        CreateMap<DisplayFieldDto, DisplayField>()
            // ... reverse mappings
    }
}
```

---

## 8. DEPENDENCY INJECTION

**Bestand:** `DependencyInjection/ServiceCollectionExtensions.cs`

**Registraties toevoegen:**

```csharp
// DisplayConfiguration Repositories
services.AddScoped<IDisplayConfigurationRepository, DisplayConfigurationRepository>();
services.AddScoped<IDisplaySectionRepository, DisplaySectionRepository>();
services.AddScoped<IDisplayFieldRepository, DisplayFieldRepository>();

// DisplayConfiguration Services
services.AddScoped<IDisplayConfigurationService, DisplayConfigurationService>();
services.AddScoped<IDisplaySectionService, DisplaySectionService>();
services.AddScoped<IDisplayFieldService, DisplayFieldService>();
services.AddScoped<IDisplayTemplateValidationService, DisplayTemplateValidationService>();
```

---

## 9. DATABASE MIGRATIONS

**Commands:**
```bash
dotnet ef migrations add AddDisplayConfigurationEntities --context KnKDbContext
dotnet ef database update --context KnKDbContext
```

**Verwachte Tabellen:**
- `display_configurations` (9 kolommen + navigation)
- `display_sections` (16 kolommen + navigation)
- `display_fields` (13 kolommen + navigation)

**Foreign Keys:**
- `display_sections.DisplayConfigurationId` → `display_configurations.Id` (CASCADE)
- `display_sections.ParentSectionId` → `display_sections.Id` (CASCADE)
- `display_fields.DisplaySectionId` → `display_sections.Id` (CASCADE)

**Indexes:**
- `IX_DisplayConfiguration_EntityType_Default` (EntityTypeName, IsDefault)
- `IX_DisplayConfiguration_IsDraft` (IsDraft)
- `IX_DisplaySection_IsReusable` (IsReusable)
- `IX_DisplaySection_ParentSectionId` (ParentSectionId)
- `IX_DisplayField_IsReusable` (IsReusable)

---

## 10. IMPLEMENTATIE VOLGORDE

**Fase 1: Database Foundation**
1. Models aanmaken (`DisplayConfiguration`, `DisplaySection`, `DisplayField`)
2. DTOs aanmaken (alle DTO classes)
3. DbContext uitbreiden met nieuwe DbSets
4. Migration aanmaken en toepassen

**Fase 2: Data Access Layer**
5. Repository interfaces definieren
6. Repository implementaties (EF Core queries)
7. AutoMapper profiles configureren

**Fase 3: Business Logic Layer**
8. Service interfaces definieren
9. Basic CRUD services implementeren (zonder validatie)
10. Clone/Link mode logic implementeren

**Fase 4: Validation Layer**
11. Template text parser implementeren (regex + property lookup)
12. DisplayTemplateValidationService implementeren
13. ActionButtonsConfig validation

**Fase 5: API Layer**
14. Controllers implementeren
15. Endpoint testing (Postman/curl)
16. DI registraties toevoegen

**Fase 6: Testing & Refinement**
17. Unit tests voor services
18. Integration tests voor repositories
19. End-to-end API tests
20. Performance optimalisatie (query tuning)

---

## 11. TEMPLATE TEXT PARSER SPECIFICATIE

### 11.1 Syntax

**Variabelen:** `${PropertyPath}`
**Calculations:** `${PropertyPath operator PropertyPath}`

**Operators:**
- `+` Optellen
- `-` Aftrekken
- `*` Vermenigvuldigen
- `/` Delen

### 11.2 Parsing Logica

```csharp
public class TemplateTextParser
{
    // Regex pattern voor variabelen: ${...}
    private const string VariablePattern = @"\$\{([^}]+)\}";
    
    public List<TemplateVariable> ExtractVariables(string templateText)
    {
        var matches = Regex.Matches(templateText, VariablePattern);
        var variables = new List<TemplateVariable>();
        
        foreach (Match match in matches)
        {
            string expression = match.Groups[1].Value.Trim();
            variables.Add(ParseExpression(expression));
        }
        
        return variables;
    }
    
    private TemplateVariable ParseExpression(string expression)
    {
        // Check for calculation operators
        var operators = new[] { '+', '-', '*', '/' };
        foreach (var op in operators)
        {
            if (expression.Contains(op))
            {
                var parts = expression.Split(op, 2);
                return new TemplateVariable
                {
                    Type = VariableType.Calculation,
                    LeftOperand = parts[0].Trim(),
                    Operator = op.ToString(),
                    RightOperand = parts[1].Trim()
                };
            }
        }
        
        // Simple property reference
        return new TemplateVariable
        {
            Type = VariableType.Property,
            PropertyPath = expression
        };
    }
    
    public async Task<ValidationResult> ValidateVariable(
        TemplateVariable variable, 
        string entityTypeName,
        IMetadataService metadataService)
    {
        if (variable.Type == VariableType.Property)
        {
            return await ValidatePropertyPath(
                variable.PropertyPath, 
                entityTypeName, 
                metadataService);
        }
        else // Calculation
        {
            var leftValidation = await ValidatePropertyPath(
                variable.LeftOperand, 
                entityTypeName, 
                metadataService);
            var rightValidation = await ValidatePropertyPath(
                variable.RightOperand, 
                entityTypeName, 
                metadataService);
            
            // Merge results
            return MergeValidationResults(leftValidation, rightValidation);
        }
    }
    
    private async Task<ValidationResult> ValidatePropertyPath(
        string propertyPath,
        string entityTypeName,
        IMetadataService metadataService)
    {
        var parts = propertyPath.Split('.');
        
        // Max 2 lagen diepte: Property of Property.SubProperty
        if (parts.Length > 3)
        {
            return ValidationResult.Error(
                $"Property path '{propertyPath}' exceeds max depth of 2 layers");
        }
        
        // Validate elk level via EntityMetadata
        string currentEntityType = entityTypeName;
        
        for (int i = 0; i < parts.Length; i++)
        {
            string propertyName = parts[i];
            
            // Special case: .Count op collection
            if (propertyName == "Count" && i > 0)
            {
                // Verify previous property was collection
                continue;
            }
            
            var metadata = await metadataService
                .GetEntityMetadataAsync(currentEntityType);
            
            var property = metadata.Properties
                .FirstOrDefault(p => p.Name == propertyName);
            
            if (property == null)
            {
                return ValidationResult.Error(
                    $"Property '{propertyName}' not found on entity '{currentEntityType}'");
            }
            
            // Update currentEntityType voor volgende iteration
            if (property.IsNavigationProperty)
            {
                currentEntityType = property.RelatedEntityType;
            }
        }
        
        return ValidationResult.Success();
    }
}

public class TemplateVariable
{
    public VariableType Type { get; set; }
    public string PropertyPath { get; set; } // Voor Type = Property
    public string LeftOperand { get; set; }  // Voor Type = Calculation
    public string Operator { get; set; }     // Voor Type = Calculation
    public string RightOperand { get; set; } // Voor Type = Calculation
}

public enum VariableType
{
    Property,
    Calculation
}
```

### 11.3 Voorbeelden

**Input:**
```
"This town has ${Districts.Count} districts and ${Streets.Count} streets, total: ${Districts.Count + Streets.Count}"
```

**Parsed Variables:**
```json
[
  { "type": "Property", "propertyPath": "Districts.Count" },
  { "type": "Property", "propertyPath": "Streets.Count" },
  { 
    "type": "Calculation", 
    "leftOperand": "Districts.Count", 
    "operator": "+", 
    "rightOperand": "Streets.Count" 
  }
]
```

**Validation:**
- Verify `Town.Districts` is ICollection → OK
- Verify `Town.Streets` is ICollection → OK
- Both operands are numeric (Count) → OK
- Result: Valid ✓

---

## 12. ACTION BUTTONS VALIDATIE

### 12.1 Validatie Regels

**Voor Single Relationships (IsCollection = false):**
- `showViewButton` ✓ Allowed
- `showEditButton` ✓ Allowed
- `showSelectButton` ✓ Allowed (change linked entity)
- `showUnlinkButton` ✓ Allowed (set to null)
- `showCreateButton` ✓ Allowed (create new entity inline)
- `showAddButton` ✗ NOT allowed (collection only)
- `showRemoveButton` ✗ NOT allowed (collection only)

**Voor Collection Relationships (IsCollection = true):**
- `showViewButton` ✓ Allowed (per item)
- `showEditButton` ✓ Allowed (per item)
- `showAddButton` ✓ Allowed (add existing to collection)
- `showRemoveButton` ✓ Allowed (remove from collection)
- `showCreateButton` ✓ Allowed (create & add to collection)
- `showSelectButton` ✗ NOT allowed (single only)
- `showUnlinkButton` ✗ NOT allowed (single only)

### 12.2 Validation Logic

```csharp
public async Task<DisplayValidationResultDto> ValidateActionButtonsConfigAsync(
    string actionButtonsConfigJson,
    bool isCollection)
{
    var result = new DisplayValidationResultDto { IsValid = true };
    
    try
    {
        var config = JsonSerializer.Deserialize<ActionButtonsConfigDto>(
            actionButtonsConfigJson);
        
        if (config == null)
        {
            result.IsValid = false;
            result.Errors.Add("Invalid ActionButtonsConfigJson format");
            return result;
        }
        
        if (isCollection)
        {
            // Collection: add/remove allowed, select/unlink NOT allowed
            if (config.ShowSelectButton || config.ShowUnlinkButton)
            {
                result.IsValid = false;
                result.Errors.Add(
                    "showSelectButton and showUnlinkButton are not allowed for collections");
            }
        }
        else
        {
            // Single: select/unlink allowed, add/remove NOT allowed
            if (config.ShowAddButton || config.ShowRemoveButton)
            {
                result.IsValid = false;
                result.Errors.Add(
                    "showAddButton and showRemoveButton are not allowed for single relationships");
            }
        }
    }
    catch (JsonException ex)
    {
        result.IsValid = false;
        result.Errors.Add($"JSON parsing error: {ex.Message}");
    }
    
    return result;
}
```

---

## 13. LINKED SOURCE RESOLUTION

### 13.1 DisplaySection Resolution

Wanneer `IsLinkedToSource = true`, moeten properties van source geladen worden:

```csharp
public async Task<DisplaySectionDto> ResolveLinkedSectionAsync(
    DisplaySectionDto section)
{
    if (!section.IsLinkedToSource || section.SourceSectionId == null)
    {
        return section; // Not linked, return as-is
    }
    
    var sourceSection = await _repository.GetSourceSectionAsync(
        int.Parse(section.SourceSectionId));
    
    if (sourceSection == null)
    {
        throw new NotFoundException(
            $"Source section {section.SourceSectionId} not found");
    }
    
    // Merge: Copy display properties from source
    var resolved = section.Clone();
    resolved.SectionName = sourceSection.SectionName;
    resolved.Description = sourceSection.Description;
    resolved.Fields = _mapper.Map<List<DisplayFieldDto>>(sourceSection.Fields);
    
    // Keep instance-specific customizations
    // resolved.FieldOrderJson blijft van instance zelf
    
    return resolved;
}
```

### 13.2 DisplayField Resolution

```csharp
public async Task<DisplayFieldDto> ResolveLinkedFieldAsync(
    DisplayFieldDto field)
{
    if (!field.IsLinkedToSource || field.SourceFieldId == null)
    {
        return field;
    }
    
    var sourceField = await _repository.GetSourceFieldAsync(
        int.Parse(field.SourceFieldId));
    
    if (sourceField == null)
    {
        throw new NotFoundException(
            $"Source field {field.SourceFieldId} not found");
    }
    
    // Merge: Copy all properties from source
    var resolved = _mapper.Map<DisplayFieldDto>(sourceField);
    resolved.Id = field.Id; // Keep instance ID
    resolved.DisplaySectionId = field.DisplaySectionId; // Keep relationship
    
    return resolved;
}
```

---

## 14. TESTING STRATEGIE

### 14.1 Unit Tests

**DisplayConfigurationService:**
- ✓ CreateAsync sets IsDraft = true
- ✓ UpdateAsync preserves IsDraft
- ✓ PublishAsync validates and sets IsDraft = false
- ✓ ValidateAsync calls validation service

**DisplayTemplateValidationService:**
- ✓ Template text parsing extracts all variables
- ✓ Property path validation (1 layer, 2 layers, 3 layers = error)
- ✓ Collection.Count validation
- ✓ Calculation validation (both operands exist)
- ✓ ActionButtonsConfig validation (collection vs single)

**TemplateTextParser:**
- ✓ Extract simple variables: `${Name}`
- ✓ Extract nested variables: `${Town.Street.Name}`
- ✓ Extract calculations: `${A + B}`
- ✓ Reject invalid syntax: `${Invalid..Path}`
- ✓ Reject too deep nesting: `${A.B.C.D}`

### 14.2 Integration Tests

**Repository Layer:**
- ✓ Cascade delete: Config → Sections → Fields
- ✓ Cascade delete: Section → SubSections → Fields
- ✓ IsDefault uniqueness per EntityTypeName
- ✓ Include related entities in queries

**Service Layer:**
- ✓ Clone section (Copy mode) → modify → verify independence
- ✓ Clone section (Link mode) → modify source → verify propagation
- ✓ Clone field (Copy mode) → modify → verify independence
- ✓ Clone field (Link mode) → modify source → verify propagation

### 14.3 API Tests

**DisplayConfigurationsController:**
- ✓ POST /api/displayconfigurations (create draft)
- ✓ PUT /api/displayconfigurations/{id} (update)
- ✓ POST /api/displayconfigurations/{id}/validate (validation)
- ✓ POST /api/displayconfigurations/{id}/publish (publish)
- ✓ GET /api/displayconfigurations/{entityName} (get default)
- ✓ DELETE /api/displayconfigurations/{id} (cascade delete)

**DisplaySectionsController:**
- ✓ POST /api/displaysections/reusable (create template)
- ✓ POST /api/displaysections/{id}/clone?linkMode=Copy
- ✓ POST /api/displaysections/{id}/clone?linkMode=Link

**DisplayFieldsController:**
- ✓ POST /api/displayfields/reusable (create template)
- ✓ POST /api/displayfields/{id}/clone?linkMode=Copy
- ✓ POST /api/displayfields/{id}/clone?linkMode=Link

---

## 15. ERROR HANDLING

### 15.1 Custom Exceptions

```csharp
public class DisplayConfigurationValidationException : Exception
{
    public DisplayValidationResultDto ValidationResult { get; }
    
    public DisplayConfigurationValidationException(
        DisplayValidationResultDto result) 
        : base("Display configuration validation failed")
    {
        ValidationResult = result;
    }
}

public class PublishDraftException : Exception
{
    public PublishDraftException(int configId, DisplayValidationResultDto result)
        : base($"Cannot publish draft config {configId}: {result.Errors.Count} errors")
    {
        ConfigId = configId;
        ValidationResult = result;
    }
    
    public int ConfigId { get; }
    public DisplayValidationResultDto ValidationResult { get; }
}
```

### 15.2 Controller Error Responses

```csharp
[HttpPost("{id:int}/publish")]
public async Task<ActionResult<DisplayConfigurationDto>> PublishAsync(int id)
{
    try
    {
        var result = await _service.PublishAsync(id);
        return Ok(result);
    }
    catch (PublishDraftException ex)
    {
        return BadRequest(new 
        { 
            message = ex.Message,
            validation = ex.ValidationResult
        });
    }
    catch (NotFoundException ex)
    {
        return NotFound(new { message = ex.Message });
    }
}
```

---

## 16. PERFORMANCE OVERWEGINGEN

### 16.1 Query Optimization

**Eager Loading:**
```csharp
public async Task<DisplayConfiguration?> GetByIdAsync(int id, bool includeRelated = true)
{
    var query = _context.DisplayConfigurations.AsQueryable();
    
    if (includeRelated)
    {
        query = query
            .Include(dc => dc.Sections)
                .ThenInclude(s => s.Fields)
            .Include(dc => dc.Sections)
                .ThenInclude(s => s.SubSections)
                    .ThenInclude(ss => ss.Fields);
    }
    
    return await query.FirstOrDefaultAsync(dc => dc.Id == id);
}
```

**Projection voor lijsten:**
```csharp
public async Task<IEnumerable<DisplayConfiguration>> GetAllAsync(bool includeDrafts = true)
{
    var query = _context.DisplayConfigurations
        .Select(dc => new DisplayConfiguration
        {
            Id = dc.Id,
            Name = dc.Name,
            EntityTypeName = dc.EntityTypeName,
            IsDefault = dc.IsDefault,
            IsDraft = dc.IsDraft
            // Exclude Sections voor performance
        });
    
    if (!includeDrafts)
    {
        query = query.Where(dc => !dc.IsDraft);
    }
    
    return await query.ToListAsync();
}
```

### 16.2 Caching Strategy

**Entity Metadata:**
- Cache EntityMetadata in memory (IMemoryCache)
- TTL: 1 hour (metadata changes zelden)
- Invalideer bij applicatie update

**Reusable Templates:**
- Cache reusable sections/fields
- TTL: 30 minuten
- Invalideer bij template updates

---

## 17. FRONTEND INTEGRATION NOTES

### 17.1 DisplayWizard Data Structure

**Backend response voor rendering:**
```json
{
  "configurationId": "123",
  "entityTypeName": "Town",
  "entityData": {
    "id": 1,
    "name": "Amsterdam",
    "districts": [
      { "id": 10, "name": "Centrum", "population": 50000 },
      { "id": 11, "name": "West", "population": 75000 }
    ],
    "townHouse": {
      "id": 100,
      "name": "City Hall",
      "street": { "id": 200, "name": "Dam Square" }
    }
  },
  "sections": [
    {
      "sectionId": "section-1",
      "sectionName": "General Info",
      "fields": [
        {
          "fieldId": "field-1",
          "label": "Town Name",
          "fieldName": "name",
          "templateText": null,
          "value": "Amsterdam"
        }
      ]
    },
    {
      "sectionId": "section-2",
      "sectionName": "Districts",
      "isCollection": true,
      "relatedEntityPropertyName": "districts",
      "actionButtons": {
        "showViewButton": true,
        "showAddButton": true
      },
      "collectionData": [...], // Array van district objects
      "subSections": [
        {
          "sectionName": "District Template",
          "fields": [
            {
              "label": "District Name",
              "fieldName": "name"
            }
          ]
        }
      ]
    }
  ]
}
```

**Frontend rendering:**
1. Render elke section sequentieel
2. Voor collection sections: itereer over `collectionData`
3. Voor elke entry: render `subSections[0]` met entry data
4. Template text: replace `${...}` met resolved values

### 17.2 API Endpoints voor Frontend

**Get display data:**
```
GET /api/displayconfigurations/{entityName}?defaultOnly=true
→ Returns default config structure

GET /api/{entityName}/{id}/display?configId={configId}
→ Returns entity data + rendered display config
```

---

## 18. FUTURE ENHANCEMENTS

### 18.1 Phase 2 Features (niet nu implementeren)

**Conditionals in Template Text:**
```
${Districts.Count > 0 ? "Has districts" : "No districts"}
```

**Advanced Calculations:**
```
${(Population / Area).toFixed(2)} per km²
```

**Custom Formatters:**
```
${CreatedAt|date:'dd-MM-yyyy'}
${Price|currency:'EUR'}
```

**Dynamic Action Buttons:**
- Conditional button visibility based on entity state
- Custom actions via webhook URLs

### 18.2 Performance Optimizations

**GraphQL Support:**
- Client specifies exact fields needed
- Reduces over-fetching

**Server-Side Caching:**
- Cache rendered display data
- Invalideer bij entity updates

---

## 19. SAMENVATTING ANTWOORDEN OP VRAGEN

| Vraag | Antwoord |
|-------|----------|
| Template text diepte | Max 2 lagen: `${Entity.Related.Property}` |
| Calculations | Ja, basis operators: +, -, *, / |
| Conditionals | Nee, toekomstige feature |
| Backend resolved values | Nee, stuur template + raw data |
| Collection sections | Optie A + nested subsections (ParentSectionId) |
| ActionButtonsConfig | Typed DTO met validatie |
| Link mode voor fields | Ja, zelfde als sections |
| Auto-validatie | Hybrid: IsDraft flag, validate on publish |
| Nested relationships | Ja, via SubSections (ParentSectionId) |
| Metadata uitbreiding | Hergebruik bestaande EntityMetadata |

---

## 20. IMPLEMENTATIE CHECKLIST

### Phase 1: Models & Database
- [ ] DisplayConfiguration model
- [ ] DisplaySection model (met ParentSectionId, SubSections)
- [ ] DisplayField model
- [ ] DTOs (Configuration, Section, Field, ActionButtonsConfig, ValidationResult)
- [ ] DbContext uitbreiden
- [ ] Migration aanmaken en testen

### Phase 2: Repositories
- [ ] IDisplayConfigurationRepository + implementatie
- [ ] IDisplaySectionRepository + implementatie
- [ ] IDisplayFieldRepository + implementatie
- [ ] Unit tests voor repositories

### Phase 3: Services - Basic CRUD
- [ ] IDisplayConfigurationService + implementatie
- [ ] IDisplaySectionService + implementatie
- [ ] IDisplayFieldService + implementatie
- [ ] Clone/Link logic voor sections en fields
- [ ] ResolveLinked methods

### Phase 4: Validation
- [ ] TemplateTextParser class
- [ ] IDisplayTemplateValidationService + implementatie
- [ ] ValidateConfigurationAsync
- [ ] ValidateSectionAsync
- [ ] ValidateFieldAsync
- [ ] ValidateTemplateTextAsync
- [ ] ValidateActionButtonsConfigAsync
- [ ] Unit tests voor validatie

### Phase 5: API
- [ ] DisplayConfigurationsController
- [ ] DisplaySectionsController
- [ ] DisplayFieldsController
- [ ] Error handling middleware
- [ ] API tests (Postman collection)

### Phase 6: Integration
- [ ] AutoMapper profiles
- [ ] DI registrations
- [ ] Integration tests
- [ ] Performance testing

### Phase 7: Documentation
- [ ] API documentation (Swagger)
- [ ] Frontend integration guide
- [ ] Database schema diagram

---

---

## 21. BESTANDSLOCATIES EN NAAMGEVING

### 21.1 Models
**Locatie:** `/Models/`

**Bestandsnamen:**
- `DisplayConfiguration.cs`
- `DisplaySection.cs`
- `DisplayField.cs`

**Namespace:** `knkwebapi_v2.Models`

### 21.2 DTOs
**Locatie:** `/Dtos/`

**Bestand:** `DisplayDtos.cs` (alle DTOs in één bestand, net als `FormDtos.cs`)

**Classes in bestand:**
- `DisplayConfigurationDto`
- `DisplaySectionDto`
- `DisplayFieldDto`
- `ActionButtonsConfigDto`
- `DisplayValidationResultDto`

**Namespace:** `knkwebapi_v2.Dtos`

### 21.3 Repositories
**Locatie:** `/Repositories/` en `/Repositories/Interfaces/`

**Interface bestanden:**
- `Interfaces/IDisplayConfigurationRepository.cs`
- `Interfaces/IDisplaySectionRepository.cs`
- `Interfaces/IDisplayFieldRepository.cs`

**Implementation bestanden:**
- `DisplayConfigurationRepository.cs`
- `DisplaySectionRepository.cs`
- `DisplayFieldRepository.cs`

**Namespace:** 
- Interfaces: `knkwebapi_v2.Repositories.Interfaces`
- Implementations: `knkwebapi_v2.Repositories`

### 21.4 Services
**Locatie:** `/Services/` en `/Services/Interfaces/`

**Interface bestanden:**
- `Interfaces/IDisplayConfigurationService.cs`
- `Interfaces/IDisplaySectionService.cs`
- `Interfaces/IDisplayFieldService.cs`
- `Interfaces/IDisplayTemplateValidationService.cs`

**Implementation bestanden:**
- `DisplayConfigurationService.cs`
- `DisplaySectionService.cs`
- `DisplayFieldService.cs`
- `DisplayTemplateValidationService.cs`
- `DisplayTemplateTextParser.cs` (helper class, geen interface)

**Namespace:**
- Interfaces: `knkwebapi_v2.Services.Interfaces`
- Implementations: `knkwebapi_v2.Services`

### 21.5 Controllers
**Locatie:** `/Controllers/`

**Bestandsnamen:**
- `DisplayConfigurationsController.cs`
- `DisplaySectionsController.cs`
- `DisplayFieldsController.cs`

**Namespace:** `knkwebapi_v2.Controllers`

### 21.6 Mapping
**Locatie:** `/Mapping/`

**Bestand:** `DisplayMappingProfile.cs`

**Namespace:** `knkwebapi_v2.Mapping`

---

## 22. DATABASE CONTEXT UITBREIDING

### 22.1 DbSets Toevoegen aan KnKDbContext.cs

**Locatie:** `/Data/KnKDbContext.cs` (of waar de DbContext zich bevindt)

```csharp
public class KnKDbContext : DbContext
{
    // Existing DbSets...
    public DbSet<FormConfiguration> FormConfigurations { get; set; }
    public DbSet<FormStep> FormSteps { get; set; }
    public DbSet<FormField> FormFields { get; set; }
    
    // NEW: DisplayConfiguration DbSets
    public DbSet<DisplayConfiguration> DisplayConfigurations { get; set; }
    public DbSet<DisplaySection> DisplaySections { get; set; }
    public DbSet<DisplayField> DisplayFields { get; set; }
    
    // Rest of context...
}
```

### 22.2 OnModelCreating Configuratie

In `OnModelCreating` method, voeg toe:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);
    
    // Existing configurations...
    
    // DisplayConfiguration relationships
    modelBuilder.Entity<DisplayConfiguration>()
        .HasMany(dc => dc.Sections)
        .WithOne(ds => ds.DisplayConfiguration)
        .HasForeignKey(ds => ds.DisplayConfigurationId)
        .OnDelete(DeleteBehavior.Cascade);

    modelBuilder.Entity<DisplayConfiguration>()
        .HasIndex(dc => new { dc.EntityTypeName, dc.IsDefault })
        .HasDatabaseName("IX_DisplayConfiguration_EntityType_Default");

    modelBuilder.Entity<DisplayConfiguration>()
        .HasIndex(dc => dc.IsDraft)
        .HasDatabaseName("IX_DisplayConfiguration_IsDraft");
    
    // DisplaySection relationships
    modelBuilder.Entity<DisplaySection>()
        .HasMany(ds => ds.Fields)
        .WithOne(df => df.DisplaySection)
        .HasForeignKey(df => df.DisplaySectionId)
        .OnDelete(DeleteBehavior.Cascade);

    modelBuilder.Entity<DisplaySection>()
        .HasMany(ds => ds.SubSections)
        .WithOne(ss => ss.ParentSection)
        .HasForeignKey(ss => ss.ParentSectionId)
        .OnDelete(DeleteBehavior.Cascade);

    modelBuilder.Entity<DisplaySection>()
        .HasIndex(ds => ds.IsReusable)
        .HasDatabaseName("IX_DisplaySection_IsReusable");

    modelBuilder.Entity<DisplaySection>()
        .HasIndex(ds => ds.ParentSectionId)
        .HasDatabaseName("IX_DisplaySection_ParentSectionId");
    
    // DisplayField indexes
    modelBuilder.Entity<DisplayField>()
        .HasIndex(df => df.IsReusable)
        .HasDatabaseName("IX_DisplayField_IsReusable");
}
```

---

## 23. COMPLETE CODE TEMPLATES

### 23.1 DisplayConfiguration Model (Compleet)

```csharp
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace knkwebapi_v2.Models
{
    /// <summary>
    /// Represents a display configuration template for rendering entity data.
    /// Similar to FormConfiguration but for read-only display purposes.
    /// Multiple configurations can exist per entity type, with one marked as default.
    /// 
    /// DRAFT MODE:
    /// - IsDraft = true: Configuration can be incomplete, validation is relaxed
    /// - IsDraft = false: Full validation required, configuration is production-ready
    /// This allows administrators to save work in progress without completing everything.
    /// </summary>
    public class DisplayConfiguration
    {
        public int Id { get; set; }
        
        public Guid ConfigurationGuid { get; set; } = Guid.NewGuid();
        
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = null!;
        
        [Required]
        [MaxLength(100)]
        public string EntityTypeName { get; set; } = null!;
        
        public bool IsDefault { get; set; } = false;
        
        [MaxLength(500)]
        public string? Description { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        /// <summary>
        /// JSON array storing ordered Section GUIDs: ["guid-1", "guid-2", ...].
        /// Allows flexible section reordering without database updates.
        /// </summary>
        [Required]
        public string SectionOrderJson { get; set; } = "[]";
        
        /// <summary>
        /// Draft mode allows incomplete configurations to be saved.
        /// When false, full validation is enforced.
        /// </summary>
        public bool IsDraft { get; set; } = true;
        
        // Navigation properties
        public List<DisplaySection> Sections { get; set; } = new();
    }
}
```

### 23.2 DisplaySection Model (Compleet)

```csharp
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace knkwebapi_v2.Models
{
    /// <summary>
    /// Represents a section grouping display fields.
    /// Can be bound to relationship properties for displaying related entities.
    /// Supports nested sections for collection relationships.
    /// 
    /// REUSABILITY PATTERN (same as FormStep):
    /// - Sections can be marked as "reusable templates" (IsReusable = true, DisplayConfigurationId = null).
    /// - When adding a reusable section to a configuration, it is CLONED or LINKED.
    /// - COPY mode: Full independent clone, changes don't propagate.
    /// - LINK mode: References source, changes propagate from source.
    /// </summary>
    public class DisplaySection
    {
        public int Id { get; set; }
        
        public Guid SectionGuid { get; set; } = Guid.NewGuid();
        
        [Required]
        [MaxLength(200)]
        public string SectionName { get; set; } = null!;
        
        [MaxLength(500)]
        public string? Description { get; set; }
        
        public bool IsReusable { get; set; } = false;
        
        public int? SourceSectionId { get; set; }
        
        /// <summary>
        /// Link mode (true): Properties loaded from source, changes propagate.
        /// Copy mode (false): Full clone, independent after creation.
        /// </summary>
        public bool IsLinkedToSource { get; set; } = false;
        
        /// <summary>
        /// JSON array storing ordered Field GUIDs: ["guid-1", "guid-2", ...].
        /// Each section instance has its own field order.
        /// </summary>
        [Required]
        public string FieldOrderJson { get; set; } = "[]";
        
        /// <summary>
        /// Property name of the related entity (e.g., "TownHouse", "Districts").
        /// When set, this section is dedicated to displaying that relationship.
        /// NULL means section displays fields from the main entity.
        /// </summary>
        [MaxLength(100)]
        public string? RelatedEntityPropertyName { get; set; }
        
        /// <summary>
        /// Entity type name of the related property (e.g., "Structure", "District").
        /// Must be NULL if RelatedEntityPropertyName is NULL.
        /// </summary>
        [MaxLength(100)]
        public string? RelatedEntityTypeName { get; set; }
        
        /// <summary>
        /// True if RelatedEntityPropertyName points to an ICollection property.
        /// Determines which action buttons are valid.
        /// </summary>
        public bool IsCollection { get; set; } = false;
        
        /// <summary>
        /// JSON object defining available action buttons.
        /// Structure depends on IsCollection (different buttons for single vs collection).
        /// </summary>
        [Required]
        public string ActionButtonsConfigJson { get; set; } = "{}";
        
        /// <summary>
        /// For nested sections: ID of parent section.
        /// Used to create subsection templates for collection items.
        /// NULL for top-level sections.
        /// </summary>
        public int? ParentSectionId { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        // Foreign keys
        public int? DisplayConfigurationId { get; set; }
        
        // Navigation properties
        public DisplayConfiguration? DisplayConfiguration { get; set; }
        public List<DisplayField> Fields { get; set; } = new();
        public List<DisplaySection> SubSections { get; set; } = new();
        public DisplaySection? ParentSection { get; set; }
    }
}
```

### 23.3 DisplayField Model (Compleet)

```csharp
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace knkwebapi_v2.Models
{
    /// <summary>
    /// Represents a single display field (read-only, no input).
    /// Can reference entity properties directly or use template text with variables.
    /// 
    /// TEMPLATE TEXT FEATURE:
    /// Supports variable interpolation with ${...} syntax:
    /// - Simple properties: ${Name}
    /// - Nested properties (max 2 levels): ${TownHouse.Street.Name}
    /// - Collection counts: ${Districts.Count}
    /// - Basic calculations: ${Districts.Count + Streets.Count}
    /// 
    /// REUSABILITY PATTERN (same as FormField):
    /// - Fields can be marked as reusable templates.
    /// - COPY mode: Independent clone.
    /// - LINK mode: References source, changes propagate.
    /// </summary>
    public class DisplayField
    {
        public int Id { get; set; }
        
        public Guid FieldGuid { get; set; } = Guid.NewGuid();
        
        /// <summary>
        /// Property name from entity (e.g., "Name", "TownHouse.Street.Name").
        /// Can be NULL if TemplateText is used instead.
        /// </summary>
        [MaxLength(200)]
        public string? FieldName { get; set; }
        
        [Required]
        [MaxLength(200)]
        public string Label { get; set; } = null!;
        
        [MaxLength(500)]
        public string? Description { get; set; }
        
        /// <summary>
        /// Custom display text with ${...} variable interpolation.
        /// Takes precedence over FieldName if both are set.
        /// Example: "This town has ${Districts.Count} districts"
        /// </summary>
        public string? TemplateText { get; set; }
        
        /// <summary>
        /// Type hint for formatting (e.g., "String", "DateTime", "Integer").
        /// Used by frontend for proper value formatting.
        /// </summary>
        [MaxLength(50)]
        public string? FieldType { get; set; }
        
        public bool IsReusable { get; set; } = false;
        
        public int? SourceFieldId { get; set; }
        
        public bool IsLinkedToSource { get; set; } = false;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        // Foreign keys
        public int? DisplaySectionId { get; set; }
        
        // Navigation properties
        public DisplaySection? DisplaySection { get; set; }
    }
}
```

---

## 24. REPOSITORY IMPLEMENTATIE VOORBEELDEN

### 24.1 DisplayConfigurationRepository (Compleet)

```csharp
using Microsoft.EntityFrameworkCore;
using knkwebapi_v2.Data;
using knkwebapi_v2.Models;
using knkwebapi_v2.Repositories.Interfaces;

namespace knkwebapi_v2.Repositories
{
    public class DisplayConfigurationRepository : IDisplayConfigurationRepository
    {
        private readonly KnKDbContext _context;

        public DisplayConfigurationRepository(KnKDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<DisplayConfiguration>> GetAllAsync(bool includeDrafts = true)
        {
            var query = _context.DisplayConfigurations
                .Include(dc => dc.Sections)
                    .ThenInclude(s => s.Fields)
                .Include(dc => dc.Sections)
                    .ThenInclude(s => s.SubSections)
                        .ThenInclude(ss => ss.Fields)
                .AsQueryable();

            if (!includeDrafts)
            {
                query = query.Where(dc => !dc.IsDraft);
            }

            return await query.ToListAsync();
        }

        public async Task<DisplayConfiguration?> GetByIdAsync(int id, bool includeRelated = true)
        {
            if (!includeRelated)
            {
                return await _context.DisplayConfigurations
                    .FirstOrDefaultAsync(dc => dc.Id == id);
            }

            return await _context.DisplayConfigurations
                .Include(dc => dc.Sections)
                    .ThenInclude(s => s.Fields)
                .Include(dc => dc.Sections)
                    .ThenInclude(s => s.SubSections)
                        .ThenInclude(ss => ss.Fields)
                .FirstOrDefaultAsync(dc => dc.Id == id);
        }

        public async Task<DisplayConfiguration?> GetByEntityTypeNameAsync(
            string entityTypeName, 
            bool defaultOnly = false,
            bool includeDrafts = true)
        {
            var query = _context.DisplayConfigurations
                .Include(dc => dc.Sections)
                    .ThenInclude(s => s.Fields)
                .Include(dc => dc.Sections)
                    .ThenInclude(s => s.SubSections)
                        .ThenInclude(ss => ss.Fields)
                .Where(dc => dc.EntityTypeName == entityTypeName);

            if (defaultOnly)
            {
                query = query.Where(dc => dc.IsDefault);
            }

            if (!includeDrafts)
            {
                query = query.Where(dc => !dc.IsDraft);
            }

            return await query.FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<DisplayConfiguration>> GetAllByEntityTypeNameAsync(
            string entityTypeName,
            bool includeDrafts = true)
        {
            var query = _context.DisplayConfigurations
                .Include(dc => dc.Sections)
                    .ThenInclude(s => s.Fields)
                .Include(dc => dc.Sections)
                    .ThenInclude(s => s.SubSections)
                        .ThenInclude(ss => ss.Fields)
                .Where(dc => dc.EntityTypeName == entityTypeName);

            if (!includeDrafts)
            {
                query = query.Where(dc => !dc.IsDraft);
            }

            return await query.ToListAsync();
        }

        public async Task<DisplayConfiguration> CreateAsync(DisplayConfiguration config)
        {
            await _context.DisplayConfigurations.AddAsync(config);
            await _context.SaveChangesAsync();
            return config;
        }

        public async Task UpdateAsync(DisplayConfiguration config)
        {
            _context.DisplayConfigurations.Update(config);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var config = await _context.DisplayConfigurations.FindAsync(id);
            if (config != null)
            {
                _context.DisplayConfigurations.Remove(config);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> IsDefaultExistsAsync(string entityTypeName, int? excludeId = null)
        {
            var query = _context.DisplayConfigurations
                .Where(dc => dc.EntityTypeName == entityTypeName && dc.IsDefault);

            if (excludeId.HasValue)
            {
                query = query.Where(dc => dc.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<IEnumerable<string>> GetEntityTypeNamesAsync()
        {
            return await _context.DisplayConfigurations
                .Select(dc => dc.EntityTypeName)
                .Distinct()
                .ToListAsync();
        }
    }
}
```

---

## 25. SERVICE IMPLEMENTATIE VOORBEELDEN

### 25.1 DisplayConfigurationService (Basis Structuur)

```csharp
using AutoMapper;
using knkwebapi_v2.Dtos;
using knkwebapi_v2.Models;
using knkwebapi_v2.Repositories.Interfaces;
using knkwebapi_v2.Services.Interfaces;

namespace knkwebapi_v2.Services
{
    public class DisplayConfigurationService : IDisplayConfigurationService
    {
        private readonly IDisplayConfigurationRepository _repo;
        private readonly IMapper _mapper;
        private readonly IDisplayTemplateValidationService _validationService;
        private readonly IMetadataService _metadataService;

        public DisplayConfigurationService(
            IDisplayConfigurationRepository repo,
            IMapper mapper,
            IDisplayTemplateValidationService validationService,
            IMetadataService metadataService)
        {
            _repo = repo;
            _mapper = mapper;
            _validationService = validationService;
            _metadataService = metadataService;
        }

        public async Task<IEnumerable<DisplayConfigurationDto>> GetAllAsync(bool includeDrafts = true)
        {
            var configs = await _repo.GetAllAsync(includeDrafts);
            return _mapper.Map<IEnumerable<DisplayConfigurationDto>>(configs);
        }

        public async Task<DisplayConfigurationDto?> GetByIdAsync(int id)
        {
            if (id <= 0) return null;
            var config = await _repo.GetByIdAsync(id);
            return config == null ? null : _mapper.Map<DisplayConfigurationDto>(config);
        }

        public async Task<DisplayConfigurationDto?> GetDefaultByEntityTypeNameAsync(
            string entityTypeName,
            bool includeDrafts = false)
        {
            var config = await _repo.GetByEntityTypeNameAsync(
                entityTypeName, 
                defaultOnly: true, 
                includeDrafts: includeDrafts);
            return config == null ? null : _mapper.Map<DisplayConfigurationDto>(config);
        }

        public async Task<IEnumerable<DisplayConfigurationDto>> GetAllByEntityTypeNameAsync(
            string entityTypeName,
            bool includeDrafts = true)
        {
            var configs = await _repo.GetAllByEntityTypeNameAsync(entityTypeName, includeDrafts);
            return _mapper.Map<IEnumerable<DisplayConfigurationDto>>(configs);
        }

        public async Task<IEnumerable<string>> GetEntityTypeNamesAsync()
        {
            return await _repo.GetEntityTypeNamesAsync();
        }

        public async Task<DisplayConfigurationDto> CreateAsync(DisplayConfigurationDto dto)
        {
            if (dto == null) 
                throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.EntityTypeName))
                throw new ArgumentException("EntityTypeName is required.", nameof(dto));

            // Check if default already exists
            if (dto.IsDefault)
            {
                var defaultExists = await _repo.IsDefaultExistsAsync(dto.EntityTypeName);
                if (defaultExists)
                {
                    throw new InvalidOperationException(
                        $"A default DisplayConfiguration for '{dto.EntityTypeName}' already exists.");
                }
            }

            var entity = _mapper.Map<DisplayConfiguration>(dto);
            entity.CreatedAt = DateTime.UtcNow;
            entity.IsDraft = true; // Always start as draft

            var created = await _repo.CreateAsync(entity);
            return _mapper.Map<DisplayConfigurationDto>(created);
        }

        public async Task UpdateAsync(int id, DisplayConfigurationDto dto)
        {
            if (dto == null) 
                throw new ArgumentNullException(nameof(dto));
            if (id <= 0) 
                throw new ArgumentException("Invalid id.", nameof(id));

            var existing = await _repo.GetByIdAsync(id, includeRelated: false);
            if (existing == null)
                throw new KeyNotFoundException($"DisplayConfiguration with id {id} not found.");

            // Check if default already exists (excluding current)
            if (dto.IsDefault)
            {
                var defaultExists = await _repo.IsDefaultExistsAsync(
                    dto.EntityTypeName, 
                    excludeId: id);
                if (defaultExists)
                {
                    throw new InvalidOperationException(
                        $"Another default DisplayConfiguration for '{dto.EntityTypeName}' already exists.");
                }
            }

            var entity = _mapper.Map<DisplayConfiguration>(dto);
            entity.Id = id;
            entity.CreatedAt = existing.CreatedAt;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.IsDraft = existing.IsDraft; // Preserve draft status

            await _repo.UpdateAsync(entity);
        }

        public async Task DeleteAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid id.", nameof(id));

            await _repo.DeleteAsync(id);
        }

        public async Task<DisplayValidationResultDto> ValidateAsync(int id)
        {
            var config = await _repo.GetByIdAsync(id);
            if (config == null)
                throw new KeyNotFoundException($"DisplayConfiguration with id {id} not found.");

            return await _validationService.ValidateConfigurationAsync(
                _mapper.Map<DisplayConfigurationDto>(config));
        }

        public async Task<DisplayConfigurationDto> PublishAsync(int id)
        {
            var config = await _repo.GetByIdAsync(id);
            if (config == null)
                throw new KeyNotFoundException($"DisplayConfiguration with id {id} not found.");

            // Perform full validation
            var dto = _mapper.Map<DisplayConfigurationDto>(config);
            var validationResult = await _validationService.ValidateConfigurationAsync(dto);

            if (!validationResult.IsValid)
            {
                throw new InvalidOperationException(
                    $"Cannot publish DisplayConfiguration {id}: Validation failed with {validationResult.Errors.Count} errors.");
            }

            // Set to published state
            config.IsDraft = false;
            config.UpdatedAt = DateTime.UtcNow;

            await _repo.UpdateAsync(config);
            return _mapper.Map<DisplayConfigurationDto>(config);
        }
    }
}
```

---

## 26. CONTROLLER IMPLEMENTATIE VOORBEELDEN

### 26.1 DisplayConfigurationsController (Compleet)

```csharp
using Microsoft.AspNetCore.Mvc;
using knkwebapi_v2.Dtos;
using knkwebapi_v2.Services.Interfaces;

namespace knkwebapi_v2.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DisplayConfigurationsController : ControllerBase
    {
        private readonly IDisplayConfigurationService _service;
        private readonly ILogger<DisplayConfigurationsController> _logger;

        public DisplayConfigurationsController(
            IDisplayConfigurationService service,
            ILogger<DisplayConfigurationsController> logger)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>
        /// Get all display configurations.
        /// </summary>
        /// <param name="includeDrafts">Include draft configurations (default: true)</param>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DisplayConfigurationDto>>> GetAllAsync(
            [FromQuery] bool includeDrafts = true)
        {
            try
            {
                var configs = await _service.GetAllAsync(includeDrafts);
                return Ok(configs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving display configurations");
                return StatusCode(500, new { message = "An error occurred while retrieving configurations." });
            }
        }

        /// <summary>
        /// Get display configuration by ID.
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<DisplayConfigurationDto>> GetByIdAsync(int id)
        {
            try
            {
                var config = await _service.GetByIdAsync(id);
                if (config == null)
                    return NotFound(new { message = $"DisplayConfiguration with id {id} not found." });

                return Ok(config);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving display configuration {Id}", id);
                return StatusCode(500, new { message = "An error occurred while retrieving the configuration." });
            }
        }

        /// <summary>
        /// Get default display configuration for entity type.
        /// </summary>
        [HttpGet("{entityName}")]
        public async Task<ActionResult<DisplayConfigurationDto>> GetDefaultByEntityTypeNameAsync(
            string entityName,
            [FromQuery] bool includeDrafts = false)
        {
            try
            {
                var config = await _service.GetDefaultByEntityTypeNameAsync(entityName, includeDrafts);
                if (config == null)
                    return NotFound(new { message = $"No default DisplayConfiguration found for '{entityName}'." });

                return Ok(config);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving default configuration for {EntityName}", entityName);
                return StatusCode(500, new { message = "An error occurred while retrieving the configuration." });
            }
        }

        /// <summary>
        /// Get all display configurations for entity type.
        /// </summary>
        [HttpGet("{entityName}/all")]
        public async Task<ActionResult<IEnumerable<DisplayConfigurationDto>>> GetAllByEntityTypeNameAsync(
            string entityName,
            [FromQuery] bool includeDrafts = true)
        {
            try
            {
                var configs = await _service.GetAllByEntityTypeNameAsync(entityName, includeDrafts);
                return Ok(configs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving configurations for {EntityName}", entityName);
                return StatusCode(500, new { message = "An error occurred while retrieving configurations." });
            }
        }

        /// <summary>
        /// Get list of all entity type names with display configurations.
        /// </summary>
        [HttpGet("entity-names")]
        public async Task<ActionResult<IEnumerable<string>>> GetEntityTypeNamesAsync()
        {
            try
            {
                var names = await _service.GetEntityTypeNamesAsync();
                return Ok(names);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving entity type names");
                return StatusCode(500, new { message = "An error occurred while retrieving entity names." });
            }
        }

        /// <summary>
        /// Create new display configuration (starts as draft).
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<DisplayConfigurationDto>> CreateAsync(
            [FromBody] DisplayConfigurationDto config)
        {
            try
            {
                var created = await _service.CreateAsync(config);
                return CreatedAtAction(
                    nameof(GetByIdAsync), 
                    new { id = created.Id }, 
                    created);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating display configuration");
                return StatusCode(500, new { message = "An error occurred while creating the configuration." });
            }
        }

        /// <summary>
        /// Update existing display configuration.
        /// </summary>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateAsync(int id, [FromBody] DisplayConfigurationDto config)
        {
            try
            {
                await _service.UpdateAsync(id, config);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating display configuration {Id}", id);
                return StatusCode(500, new { message = "An error occurred while updating the configuration." });
            }
        }

        /// <summary>
        /// Delete display configuration.
        /// </summary>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            try
            {
                await _service.DeleteAsync(id);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting display configuration {Id}", id);
                return StatusCode(500, new { message = "An error occurred while deleting the configuration." });
            }
        }

        /// <summary>
        /// Validate display configuration against entity metadata.
        /// </summary>
        [HttpPost("{id:int}/validate")]
        public async Task<ActionResult<DisplayValidationResultDto>> ValidateAsync(int id)
        {
            try
            {
                var result = await _service.ValidateAsync(id);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating display configuration {Id}", id);
                return StatusCode(500, new { message = "An error occurred while validating the configuration." });
            }
        }

        /// <summary>
        /// Publish display configuration (sets IsDraft = false after validation).
        /// </summary>
        [HttpPost("{id:int}/publish")]
        public async Task<ActionResult<DisplayConfigurationDto>> PublishAsync(int id)
        {
            try
            {
                var result = await _service.PublishAsync(id);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing display configuration {Id}", id);
                return StatusCode(500, new { message = "An error occurred while publishing the configuration." });
            }
        }
    }
}
```

---

## 27. AUTOMAPPER PROFILE (COMPLEET)

```csharp
using AutoMapper;
using knkwebapi_v2.Dtos;
using knkwebapi_v2.Models;

namespace knkwebapi_v2.Mapping
{
    public class DisplayMappingProfile : Profile
    {
        public DisplayMappingProfile()
        {
            // Helper method voor int parsing (hergebruik van FormMappingProfile patroon)
            int ToInt(string? s) => string.IsNullOrEmpty(s) ? 0 : int.Parse(s);
            int? ToNullableInt(string? s) => string.IsNullOrEmpty(s) ? null : int.Parse(s);

            // DisplayConfiguration mappings
            CreateMap<DisplayConfiguration, DisplayConfigurationDto>()
                .ForMember(d => d.Id, o => o.MapFrom(s => s.Id.ToString()))
                .ForMember(d => d.ConfigurationGuid, o => o.MapFrom(s => s.ConfigurationGuid.ToString()))
                .ForMember(d => d.Name, o => o.MapFrom(s => s.Name))
                .ForMember(d => d.EntityTypeName, o => o.MapFrom(s => s.EntityTypeName))
                .ForMember(d => d.IsDefault, o => o.MapFrom(s => s.IsDefault))
                .ForMember(d => d.Description, o => o.MapFrom(s => s.Description))
                .ForMember(d => d.SectionOrderJson, o => o.MapFrom(s => s.SectionOrderJson))
                .ForMember(d => d.IsDraft, o => o.MapFrom(s => s.IsDraft))
                .ForMember(d => d.CreatedAt, o => o.MapFrom(s => s.CreatedAt.ToString("o")))
                .ForMember(d => d.UpdatedAt, o => o.MapFrom(s => s.UpdatedAt.HasValue ? s.UpdatedAt.Value.ToString("o") : null))
                .ForMember(d => d.Sections, o => o.MapFrom(s => s.Sections));

            CreateMap<DisplayConfigurationDto, DisplayConfiguration>()
                .ForMember(d => d.Id, o => o.MapFrom(s => ToInt(s.Id)))
                .ForMember(d => d.ConfigurationGuid, o => o.MapFrom(s => 
                    string.IsNullOrEmpty(s.ConfigurationGuid) ? Guid.NewGuid() : Guid.Parse(s.ConfigurationGuid)))
                .ForMember(d => d.Name, o => o.MapFrom(s => s.Name))
                .ForMember(d => d.EntityTypeName, o => o.MapFrom(s => s.EntityTypeName))
                .ForMember(d => d.IsDefault, o => o.MapFrom(s => s.IsDefault))
                .ForMember(d => d.Description, o => o.MapFrom(s => s.Description))
                .ForMember(d => d.SectionOrderJson, o => o.MapFrom(s => s.SectionOrderJson ?? "[]"))
                .ForMember(d => d.IsDraft, o => o.MapFrom(s => s.IsDraft))
                .ForMember(d => d.CreatedAt, o => o.MapFrom(s => 
                    string.IsNullOrEmpty(s.CreatedAt) ? DateTime.UtcNow : DateTime.Parse(s.CreatedAt)))
                .ForMember(d => d.UpdatedAt, o => o.MapFrom(s => 
                    string.IsNullOrEmpty(s.UpdatedAt) ? (DateTime?)null : DateTime.Parse(s.UpdatedAt)))
                .ForMember(d => d.Sections, o => o.MapFrom(s => s.Sections));

            // DisplaySection mappings
            CreateMap<DisplaySection, DisplaySectionDto>()
                .ForMember(d => d.Id, o => o.MapFrom(s => s.Id.ToString()))
                .ForMember(d => d.SectionGuid, o => o.MapFrom(s => s.SectionGuid.ToString()))
                .ForMember(d => d.DisplayConfigurationId, o => o.MapFrom(s => 
                    s.DisplayConfigurationId.HasValue ? s.DisplayConfigurationId.Value.ToString() : null))
                .ForMember(d => d.SectionName, o => o.MapFrom(s => s.SectionName))
                .ForMember(d => d.Description, o => o.MapFrom(s => s.Description))
                .ForMember(d => d.IsReusable, o => o.MapFrom(s => s.IsReusable))
                .ForMember(d => d.SourceSectionId, o => o.MapFrom(s => 
                    s.SourceSectionId.HasValue ? s.SourceSectionId.Value.ToString() : null))
                .ForMember(d => d.IsLinkedToSource, o => o.MapFrom(s => s.IsLinkedToSource))
                .ForMember(d => d.FieldOrderJson, o => o.MapFrom(s => s.FieldOrderJson))
                .ForMember(d => d.RelatedEntityPropertyName, o => o.MapFrom(s => s.RelatedEntityPropertyName))
                .ForMember(d => d.RelatedEntityTypeName, o => o.MapFrom(s => s.RelatedEntityTypeName))
                .ForMember(d => d.IsCollection, o => o.MapFrom(s => s.IsCollection))
                .ForMember(d => d.ActionButtonsConfigJson, o => o.MapFrom(s => s.ActionButtonsConfigJson))
                .ForMember(d => d.ParentSectionId, o => o.MapFrom(s => 
                    s.ParentSectionId.HasValue ? s.ParentSectionId.Value.ToString() : null))
                .ForMember(d => d.CreatedAt, o => o.MapFrom(s => s.CreatedAt.ToString("o")))
                .ForMember(d => d.UpdatedAt, o => o.MapFrom(s => 
                    s.UpdatedAt.HasValue ? s.UpdatedAt.Value.ToString("o") : null))
                .ForMember(d => d.Fields, o => o.MapFrom(s => s.Fields))
                .ForMember(d => d.SubSections, o => o.MapFrom(s => s.SubSections));

            CreateMap<DisplaySectionDto, DisplaySection>()
                .ForMember(d => d.Id, o => o.MapFrom(s => ToInt(s.Id)))
                .ForMember(d => d.SectionGuid, o => o.MapFrom(s => 
                    string.IsNullOrEmpty(s.SectionGuid) ? Guid.NewGuid() : Guid.Parse(s.SectionGuid)))
                .ForMember(d => d.DisplayConfigurationId, o => o.MapFrom(s => ToNullableInt(s.DisplayConfigurationId)))
                .ForMember(d => d.SectionName, o => o.MapFrom(s => s.SectionName))
                .ForMember(d => d.Description, o => o.MapFrom(s => s.Description))
                .ForMember(d => d.IsReusable, o => o.MapFrom(s => s.IsReusable))
                .ForMember(d => d.SourceSectionId, o => o.MapFrom(s => ToNullableInt(s.SourceSectionId)))
                .ForMember(d => d.IsLinkedToSource, o => o.MapFrom(s => s.IsLinkedToSource))
                .ForMember(d => d.FieldOrderJson, o => o.MapFrom(s => s.FieldOrderJson ?? "[]"))
                .ForMember(d => d.RelatedEntityPropertyName, o => o.MapFrom(s => s.RelatedEntityPropertyName))
                .ForMember(d => d.RelatedEntityTypeName, o => o.MapFrom(s => s.RelatedEntityTypeName))
                .ForMember(d => d.IsCollection, o => o.MapFrom(s => s.IsCollection))
                .ForMember(d => d.ActionButtonsConfigJson, o => o.MapFrom(s => s.ActionButtonsConfigJson ?? "{}"))
                .ForMember(d => d.ParentSectionId, o => o.MapFrom(s => ToNullableInt(s.ParentSectionId)))
                .ForMember(d => d.CreatedAt, o => o.MapFrom(s => 
                    string.IsNullOrEmpty(s.CreatedAt) ? DateTime.UtcNow : DateTime.Parse(s.CreatedAt)))
                .ForMember(d => d.UpdatedAt, o => o.MapFrom(s => 
                    string.IsNullOrEmpty(s.UpdatedAt) ? (DateTime?)null : DateTime.Parse(s.UpdatedAt)))
                .ForMember(d => d.DisplayConfiguration, o => o.Ignore())
                .ForMember(d => d.ParentSection, o => o.Ignore())
                .ForMember(d => d.Fields, o => o.MapFrom(s => s.Fields))
                .ForMember(d => d.SubSections, o => o.MapFrom(s => s.SubSections));

            // DisplayField mappings
            CreateMap<DisplayField, DisplayFieldDto>()
                .ForMember(d => d.Id, o => o.MapFrom(s => s.Id.ToString()))
                .ForMember(d => d.FieldGuid, o => o.MapFrom(s => s.FieldGuid.ToString()))
                .ForMember(d => d.DisplaySectionId, o => o.MapFrom(s => 
                    s.DisplaySectionId.HasValue ? s.DisplaySectionId.Value.ToString() : null))
                .ForMember(d => d.FieldName, o => o.MapFrom(s => s.FieldName))
                .ForMember(d => d.Label, o => o.MapFrom(s => s.Label))
                .ForMember(d => d.Description, o => o.MapFrom(s => s.Description))
                .ForMember(d => d.TemplateText, o => o.MapFrom(s => s.TemplateText))
                .ForMember(d => d.FieldType, o => o.MapFrom(s => s.FieldType))
                .ForMember(d => d.IsReusable, o => o.MapFrom(s => s.IsReusable))
                .ForMember(d => d.SourceFieldId, o => o.MapFrom(s => 
                    s.SourceFieldId.HasValue ? s.SourceFieldId.Value.ToString() : null))
                .ForMember(d => d.IsLinkedToSource, o => o.MapFrom(s => s.IsLinkedToSource))
                .ForMember(d => d.CreatedAt, o => o.MapFrom(s => s.CreatedAt.ToString("o")))
                .ForMember(d => d.UpdatedAt, o => o.MapFrom(s => 
                    s.UpdatedAt.HasValue ? s.UpdatedAt.Value.ToString("o") : null));

            CreateMap<DisplayFieldDto, DisplayField>()
                .ForMember(d => d.Id, o => o.MapFrom(s => ToInt(s.Id)))
                .ForMember(d => d.FieldGuid, o => o.MapFrom(s => 
                    string.IsNullOrEmpty(s.FieldGuid) ? Guid.NewGuid() : Guid.Parse(s.FieldGuid)))
                .ForMember(d => d.DisplaySectionId, o => o.MapFrom(s => ToNullableInt(s.DisplaySectionId)))
                .ForMember(d => d.FieldName, o => o.MapFrom(s => s.FieldName))
                .ForMember(d => d.Label, o => o.MapFrom(s => s.Label))
                .ForMember(d => d.Description, o => o.MapFrom(s => s.Description))
                .ForMember(d => d.TemplateText, o => o.MapFrom(s => s.TemplateText))
                .ForMember(d => d.FieldType, o => o.MapFrom(s => s.FieldType))
                .ForMember(d => d.IsReusable, o => o.MapFrom(s => s.IsReusable))
                .ForMember(d => d.SourceFieldId, o => o.MapFrom(s => ToNullableInt(s.SourceFieldId)))
                .ForMember(d => d.IsLinkedToSource, o => o.MapFrom(s => s.IsLinkedToSource))
                .ForMember(d => d.CreatedAt, o => o.MapFrom(s => 
                    string.IsNullOrEmpty(s.CreatedAt) ? DateTime.UtcNow : DateTime.Parse(s.CreatedAt)))
                .ForMember(d => d.UpdatedAt, o => o.MapFrom(s => 
                    string.IsNullOrEmpty(s.UpdatedAt) ? (DateTime?)null : DateTime.Parse(s.UpdatedAt)))
                .ForMember(d => d.DisplaySection, o => o.Ignore());
        }
    }
}
```

---

## 28. MIGRATION COMMANDO'S

### 28.1 Migratie Aanmaken

```bash
# Navigeer naar project root
cd /Users/pandi/Documents/Werk/KnightsAndKings/Repository/knkwebapi_v2

# Maak migratie aan
dotnet ef migrations add AddDisplayConfigurationEntities

# Controleer migratie bestanden
ls -la Migrations/*DisplayConfiguration*
```

### 28.2 Database Updaten

```bash
# Apply migratie naar database
dotnet ef database update

# Verify tables created
# Connect to MySQL and run:
# SHOW TABLES LIKE 'display%';
```

### 28.3 Rollback (indien nodig)

```bash
# Rollback naar vorige migratie
dotnet ef database update [PreviousMigrationName]

# Verwijder migratie bestanden
dotnet ef migrations remove
```

---

## 29. TESTING STRATEGIE (UITGEBREID)

### 29.1 Unit Test Voorbeelden

**DisplayTemplateValidationServiceTests.cs:**
```csharp
[Fact]
public async Task ValidateTemplateText_SimpleProperty_ReturnsValid()
{
    // Arrange
    var templateText = "Town name: ${Name}";
    var entityTypeName = "Town";
    
    // Act
    var result = await _service.ValidateTemplateTextAsync(templateText, entityTypeName);
    
    // Assert
    Assert.True(result.IsValid);
    Assert.Empty(result.Errors);
}

[Fact]
public async Task ValidateTemplateText_NestedProperty_TwoLevels_ReturnsValid()
{
    // Arrange
    var templateText = "Located at ${Location.Name}";
    var entityTypeName = "Town";
    
    // Act
    var result = await _service.ValidateTemplateTextAsync(templateText, entityTypeName);
    
    // Assert
    Assert.True(result.IsValid);
}

[Fact]
public async Task ValidateTemplateText_ExceedsMaxDepth_ReturnsInvalid()
{
    // Arrange
    var templateText = "${Town.Location.District.Name}"; // 3 levels
    var entityTypeName = "Town";
    
    // Act
    var result = await _service.ValidateTemplateTextAsync(templateText, entityTypeName);
    
    // Assert
    Assert.False(result.IsValid);
    Assert.Contains("exceeds max depth", result.Errors[0]);
}

[Fact]
public async Task ValidateTemplateText_Calculation_ReturnsValid()
{
    // Arrange
    var templateText = "Total: ${Districts.Count + Streets.Count}";
    var entityTypeName = "Town";
    
    // Act
    var result = await _service.ValidateTemplateTextAsync(templateText, entityTypeName);
    
    // Assert
    Assert.True(result.IsValid);
}
```

### 29.2 Integration Test Voorbeelden

**DisplayConfigurationRepositoryTests.cs:**
```csharp
[Fact]
public async Task CreateAndRetrieve_CascadeIncludesSubSections()
{
    // Arrange
    var config = new DisplayConfiguration
    {
        Name = "Test Config",
        EntityTypeName = "Town",
        Sections = new List<DisplaySection>
        {
            new DisplaySection
            {
                SectionName = "Districts",
                IsCollection = true,
                SubSections = new List<DisplaySection>
                {
                    new DisplaySection
                    {
                        SectionName = "District Template",
                        Fields = new List<DisplayField>
                        {
                            new DisplayField { Label = "District Name", FieldName = "Name" }
                        }
                    }
                }
            }
        }
    };
    
    // Act
    var created = await _repo.CreateAsync(config);
    var retrieved = await _repo.GetByIdAsync(created.Id);
    
    // Assert
    Assert.NotNull(retrieved);
    Assert.Single(retrieved.Sections);
    Assert.Single(retrieved.Sections[0].SubSections);
    Assert.Single(retrieved.Sections[0].SubSections[0].Fields);
}
```

---

## 30. DEPENDENCY INJECTION (COMPLEET)

**ServiceCollectionExtensions.cs toevoegingen:**

```csharp
namespace knkwebapi_v2.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Existing services...
            services.AddScoped<IFormConfigurationRepository, FormConfigurationRepository>();
            services.AddScoped<IFormConfigurationService, FormConfigurationService>();
            // ... etc
            
            // DisplayConfiguration Repositories
            services.AddScoped<IDisplayConfigurationRepository, DisplayConfigurationRepository>();
            services.AddScoped<IDisplaySectionRepository, DisplaySectionRepository>();
            services.AddScoped<IDisplayFieldRepository, DisplayFieldRepository>();

            // DisplayConfiguration Services
            services.AddScoped<IDisplayConfigurationService, DisplayConfigurationService>();
            services.AddScoped<IDisplaySectionService, DisplaySectionService>();
            services.AddScoped<IDisplayFieldService, DisplayFieldService>();
            services.AddScoped<IDisplayTemplateValidationService, DisplayTemplateValidationService>();
            
            // Helper services (singleton for stateless utilities)
            services.AddSingleton<DisplayTemplateTextParser>();

            return services;
        }
    }
}
```

---

**DOCUMENT STATUS: COMPLEET EN KLAAR VOOR IMPLEMENTATIE**

Alle requirements zijn gespecificeerd, alle vragen beantwoord, alle edge cases gedocumenteerd, en volledige code voorbeelden toegevoegd. Backend implementatie kan nu starten volgens de beschreven volgorde met concrete code templates.
