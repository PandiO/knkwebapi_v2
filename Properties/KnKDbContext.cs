using System;
using knkwebapi_v2.Models;
using Microsoft.EntityFrameworkCore;

// Updated with FieldValidationRule relationship configuration
namespace knkwebapi_v2.Properties;

public partial class KnKDbContext : DbContext
{
    public KnKDbContext()
    {
    }

    public KnKDbContext(DbContextOptions<KnKDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Domain> Domains { get; set; } = null!;
    public virtual DbSet<User> Users { get; set; } = null!;
    public virtual DbSet<LinkCode> LinkCodes { get; set; } = null!;
    public virtual DbSet<Category> Categories { get; set; } = null!;
    public DbSet<FormConfiguration> FormConfigurations { get; set; }
    public DbSet<FormStep> FormSteps { get; set; }
    public DbSet<FormField> FormFields { get; set; }
    public DbSet<FieldValidation> FieldValidations { get; set; }
    public DbSet<FieldValidationRule> FieldValidationRules { get; set; }
    public DbSet<StepCondition> StepConditions { get; set; }
    public DbSet<FormSubmissionProgress> FormSubmissionProgresses { get; set; }
    
    // DisplayConfiguration DbSets
    public DbSet<DisplayConfiguration> DisplayConfigurations { get; set; }
    public DbSet<DisplaySection> DisplaySections { get; set; }
    public DbSet<DisplayField> DisplayFields { get; set; }
    
    // Entity Type Configuration (admin-configurable entity display properties)
    public DbSet<EntityTypeConfiguration> EntityTypeConfigurations { get; set; }
    
    public virtual DbSet<Location> Locations { get; set; } = null!;
    public virtual DbSet<Street> Streets { get; set; } = null!;
    public virtual DbSet<Town> Towns { get; set; } = null!;
    public virtual DbSet<District> Districts { get; set; } = null!;
    public virtual DbSet<Structure> Structures { get; set; } = null!;
    public virtual DbSet<MinecraftMaterialRef> MinecraftMaterialRefs { get; set; } = null!;
    public virtual DbSet<MinecraftBlockRef> MinecraftBlockRefs { get; set; } = null!;
    public virtual DbSet<MinecraftEnchantmentRef> MinecraftEnchantmentRefs { get; set; } = null!;
    public virtual DbSet<ItemBlueprint> ItemBlueprints { get; set; } = null!;
    public virtual DbSet<EnchantmentDefinition> EnchantmentDefinitions { get; set; } = null!;
    public virtual DbSet<AbilityDefinition> AbilityDefinitions { get; set; } = null!;
    // Workflow + Tasks
    public virtual DbSet<WorkflowSession> WorkflowSessions { get; set; } = null!;
    public virtual DbSet<StepProgress> StepProgresses { get; set; } = null!;
    public virtual DbSet<WorldTask> WorldTasks { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_general_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Domain>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");
            entity.ToTable("domains");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");
            entity.ToTable("users");
            
            // Unique constraints on Username, Email, UUID (with null handling)
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.Uuid).IsUnique();
            
            // Relationship to LinkCodes
            entity.HasMany(e => e.LinkCodes)
                .WithOne(lc => lc.User)
                .HasForeignKey(lc => lc.UserId)
                .OnDelete(DeleteBehavior.Restrict); // No cascade; soft-delete handles cleanup
        });

        modelBuilder.Entity<LinkCode>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");
            entity.ToTable("linkcodes");
            
            // Unique constraint on Code - cannot be reused
            entity.HasIndex(e => e.Code).IsUnique();
            
            // Index on ExpiresAt for efficient cleanup queries
            entity.HasIndex(e => e.ExpiresAt);
            
            // FK to User (no cascade - handled at application level)
            entity.HasOne(e => e.User)
                .WithMany(u => u.LinkCodes)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");
            entity.ToTable("categories");

            entity.HasOne(c => c.IconMaterialRef)
                .WithMany()
                .HasForeignKey(c => c.IconMaterialRefId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<Location>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");
            entity.ToTable("locations");
        });

        base.OnModelCreating(modelBuilder);
        
        // FormConfiguration relationships
        modelBuilder.Entity<FormConfiguration>()
            .HasMany(fc => fc.Steps)
            .WithOne(s => s.FormConfiguration)
            .HasForeignKey(s => s.FormConfigurationId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // FormStep relationships
        modelBuilder.Entity<FormStep>()
            .HasMany(s => s.Fields)
            .WithOne(f => f.FormStep)
            .HasForeignKey(f => f.FormStepId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<FormStep>()
            .HasMany(s => s.StepConditions)
            .WithOne(sc => sc.FormStep)
            .HasForeignKey(sc => sc.FormStepId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // FormStep self-referencing relationship for many-to-many child steps
        modelBuilder.Entity<FormStep>()
            .HasMany(s => s.ChildFormSteps)
            .WithOne(cs => cs.ParentStep)
            .HasForeignKey(cs => cs.ParentStepId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<FormStep>()
            .HasOne(s => s.SubConfiguration)
            .WithMany()
            .HasForeignKey(s => s.SubConfigurationId)
            .OnDelete(DeleteBehavior.Restrict);
        
        // FormField relationships
        modelBuilder.Entity<FormField>()
            .HasMany(f => f.Validations)
            .WithOne(v => v.FormField)
            .HasForeignKey(v => v.FormFieldId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<FormField>()
            .HasMany(f => f.ValidationRules)
            .WithOne(vr => vr.FormField)
            .HasForeignKey(vr => vr.FormFieldId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<FormField>()
            .HasOne(f => f.DependsOnField)
            .WithMany(f => f.DependentFields)
            .HasForeignKey(f => f.DependsOnFieldId)
            .OnDelete(DeleteBehavior.Restrict);
        
        modelBuilder.Entity<FormField>()
            .HasOne(f => f.SubConfiguration)
            .WithMany()
            .HasForeignKey(f => f.SubConfigurationId)
            .OnDelete(DeleteBehavior.Restrict);
        
        // FieldValidationRule relationships
        modelBuilder.Entity<FieldValidationRule>()
            .HasOne(vr => vr.DependsOnField)
            .WithMany()
            .HasForeignKey(vr => vr.DependsOnFieldId)
            .OnDelete(DeleteBehavior.Restrict);
        
        // FormSubmissionProgress relationships
        modelBuilder.Entity<FormSubmissionProgress>()
            .HasOne(p => p.User)
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<FormSubmissionProgress>()
            .HasOne(p => p.FormConfiguration)
            .WithMany(fc => fc.SubmissionProgresses)
            .HasForeignKey(p => p.FormConfigurationId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<FormSubmissionProgress>()
            .HasOne(p => p.ParentProgress)
            .WithMany()
            .HasForeignKey(p => p.ParentProgressId)
            .OnDelete(DeleteBehavior.Restrict);
        
        // Indexes for performance
        modelBuilder.Entity<FormConfiguration>()
            .HasIndex(fc => fc.EntityTypeName);
        
        modelBuilder.Entity<FormConfiguration>()
            .HasIndex(fc => fc.ConfigurationGuid)
            .IsUnique();
        
        modelBuilder.Entity<FormStep>()
            .HasIndex(s => s.IsReusable);
        
        modelBuilder.Entity<FormField>()
            .HasIndex(f => f.IsReusable);

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

        modelBuilder.Entity<Domain>()
            .HasOne(d => d.Location)
            .WithOne()
            .HasForeignKey<Domain>(d => d.LocationId)
            .OnDelete(DeleteBehavior.Cascade);

        // Street configuration
        modelBuilder.Entity<Street>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");
            entity.ToTable("streets");
        });

        // Town configuration
        modelBuilder.Entity<Town>(entity =>
        {
            entity.ToTable("towns");
        });

        // Town-Street many-to-many
        modelBuilder.Entity<Town>()
            .HasMany(t => t.Streets)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "TownStreet",
                j => j.HasOne<Street>().WithMany().HasForeignKey("StreetId"),
                j => j.HasOne<Town>().WithMany().HasForeignKey("TownId"));

        // District configuration
        modelBuilder.Entity<District>(entity =>
        {
            entity.ToTable("districts");
        });

        // District-Town many-to-one (required)
        modelBuilder.Entity<District>()
            .HasOne(d => d.Town)
            .WithMany(t => t.Districts)
            .HasForeignKey(d => d.TownId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        // District-Street many-to-many
        modelBuilder.Entity<District>()
            .HasMany(d => d.Streets)
            .WithMany(s => s.Districts)
            .UsingEntity<Dictionary<string, object>>(
                "DistrictStreet",
                j => j.HasOne<Street>().WithMany().HasForeignKey("StreetId"),
                j => j.HasOne<District>().WithMany().HasForeignKey("DistrictId"));

        // Structure configuration
        modelBuilder.Entity<Structure>(entity =>
        {
            entity.ToTable("structures");
        });

        modelBuilder.Entity<MinecraftMaterialRef>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");
            entity.ToTable("minecraftmaterialrefs");
            entity.HasIndex(e => e.NamespaceKey).IsUnique();
            entity.Property(e => e.NamespaceKey).IsRequired().HasMaxLength(191);
            entity.Property(e => e.Category).IsRequired();
        });

        modelBuilder.Entity<MinecraftBlockRef>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");
            entity.ToTable("minecraftblockrefs");
            entity.Property(e => e.NamespaceKey).IsRequired().HasMaxLength(191);
        });

        modelBuilder.Entity<MinecraftEnchantmentRef>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");
            entity.ToTable("minecraftenchantmentrefs");
            entity.HasIndex(e => e.NamespaceKey).IsUnique();
            entity.Property(e => e.NamespaceKey).IsRequired().HasMaxLength(191);
        });

        modelBuilder.Entity<AbilityDefinition>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");
            entity.ToTable("AbilityDefinitions");

            entity.Property(e => e.AbilityKey)
                .IsRequired()
                .HasMaxLength(191);

            entity.HasIndex(e => e.EnchantmentDefinitionId)
                .IsUnique();
        });

        // EntityTypeConfiguration model configuration
        modelBuilder.Entity<EntityTypeConfiguration>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("entity_type_configurations");
            
            entity.Property(e => e.EntityTypeName)
                .IsRequired()
                .HasMaxLength(191);
            
            entity.Property(e => e.IconKey)
                .HasMaxLength(50);
            
            entity.Property(e => e.CustomIconUrl)
                .HasMaxLength(500);
            
            entity.Property(e => e.DisplayColor)
                .HasMaxLength(7);

            entity.Property(e => e.DefaultTableColumnsJson)
                .HasColumnType("longtext");
            
            // CreatedAt and UpdatedAt are set in C# (DateTime.UtcNow), not via SQL defaults
            // This avoids MySQL timezone issues
            entity.Property(e => e.CreatedAt)
                .HasColumnType("datetime");
            
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("datetime");
            
            entity.HasIndex(e => e.EntityTypeName)
                .IsUnique()
                .HasDatabaseName("IX_EntityTypeConfiguration_EntityTypeName");
        });
        // Structure-Street many-to-one (required)
        modelBuilder.Entity<Structure>()
            .HasOne(s => s.Street)
            .WithMany(st => st.Structures)
            .HasForeignKey(s => s.StreetId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        // Structure-District many-to-one (required)
        modelBuilder.Entity<Structure>()
            .HasOne(s => s.District)
            .WithMany(d => d.Structures)
            .HasForeignKey(s => s.DistrictId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        // ItemBlueprintDefaultEnchantment join entity with composite primary key
        modelBuilder.Entity<ItemBlueprintDefaultEnchantment>()
            .HasKey(e => new { e.ItemBlueprintId, e.EnchantmentDefinitionId });

        modelBuilder.Entity<ItemBlueprintDefaultEnchantment>()
            .HasOne(e => e.ItemBlueprint)
            .WithMany(ib => ib.DefaultEnchantments)
            .HasForeignKey(e => e.ItemBlueprintId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ItemBlueprintDefaultEnchantment>()
            .HasOne(e => e.EnchantmentDefinition)
            .WithMany(ed => ed.DefaultForBlueprints)
            .HasForeignKey(e => e.EnchantmentDefinitionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<EnchantmentDefinition>()
            .HasOne(e => e.AbilityDefinition)
            .WithOne(a => a.EnchantmentDefinition)
            .HasForeignKey<AbilityDefinition>(a => a.EnchantmentDefinitionId)
            .OnDelete(DeleteBehavior.Cascade);

        // WorkflowSession configuration
        modelBuilder.Entity<WorkflowSession>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("workflow_sessions");

            entity.Property(e => e.SessionGuid)
                .IsRequired();

            entity.Property(e => e.EntityTypeName)
                .HasMaxLength(191);

            entity.Property(e => e.CreatedAt)
                .HasColumnType("datetime");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("datetime");

            entity.HasIndex(e => e.SessionGuid)
                .IsUnique();

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.FormConfiguration)
                .WithMany()
                .HasForeignKey(e => e.FormConfigurationId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // StepProgress configuration
        modelBuilder.Entity<StepProgress>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("step_progress");

            entity.Property(e => e.StepKey)
                .IsRequired()
                .HasMaxLength(191);

            entity.Property(e => e.Status)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.CreatedAt)
                .HasColumnType("datetime");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("datetime");
            entity.Property(e => e.CompletedAt)
                .HasColumnType("datetime");

            entity.HasIndex(e => new { e.WorkflowSessionId, e.StepKey })
                .IsUnique();

            entity.HasOne(e => e.WorkflowSession)
                .WithMany(s => s.Steps)
                .HasForeignKey(e => e.WorkflowSessionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // WorldTask configuration
        modelBuilder.Entity<WorldTask>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("world_tasks");

            entity.Property(e => e.TaskType)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Status)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.PayloadJson)
                .HasColumnType("longtext");

            entity.Property(e => e.CreatedAt)
                .HasColumnType("datetime");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("datetime");
            entity.Property(e => e.CompletedAt)
                .HasColumnType("datetime");

            entity.HasIndex(e => new { e.WorkflowSessionId, e.Status });
            entity.HasIndex(e => e.AssignedUserId);

            entity.HasOne(e => e.WorkflowSession)
                .WithMany(s => s.WorldTasks)
                .HasForeignKey(e => e.WorkflowSessionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.AssignedUser)
                .WithMany()
                .HasForeignKey(e => e.AssignedUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
