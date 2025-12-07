using System;
using knkwebapi_v2.Models;
using Microsoft.EntityFrameworkCore;

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
    public virtual DbSet<Category> Categories { get; set; } = null!;
    public DbSet<FormConfiguration> FormConfigurations { get; set; }
    public DbSet<FormStep> FormSteps { get; set; }
    public DbSet<FormField> FormFields { get; set; }
    public DbSet<FieldValidation> FieldValidations { get; set; }
    public DbSet<StepCondition> StepConditions { get; set; }
    public DbSet<FormSubmissionProgress> FormSubmissionProgresses { get; set; }
    public virtual DbSet<Location> Locations { get; set; } = null!;

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
        });
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");
            entity.ToTable("categories");
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
        
        // FormField relationships
        modelBuilder.Entity<FormField>()
            .HasMany(f => f.Validations)
            .WithOne(v => v.FormField)
            .HasForeignKey(v => v.FormFieldId)
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

        modelBuilder.Entity<Domain>()
            .HasOne(d => d.Location)
            .WithOne()
            .HasForeignKey<Domain>(d => d.LocationId)
            .OnDelete(DeleteBehavior.Cascade);

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
