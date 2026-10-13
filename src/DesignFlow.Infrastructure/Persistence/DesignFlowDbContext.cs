using DesignFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DesignFlow.Infrastructure.Persistence;

public sealed class DesignFlowDbContext : DbContext
{
    public DesignFlowDbContext(DbContextOptions<DesignFlowDbContext> options) : base(options)
    {
    }

    public DbSet<Role> Roles => Set<Role>();

    public DbSet<User> Users => Set<User>();

    public DbSet<UserRole> UserRoles => Set<UserRole>();

    public DbSet<Client> Clients => Set<Client>();

    public DbSet<Project> Projects => Set<Project>();

    public DbSet<ProjectRequirement> ProjectRequirements => Set<ProjectRequirement>();

    public DbSet<Milestone> Milestones => Set<Milestone>();

    public DbSet<ProjectTask> Tasks => Set<ProjectTask>();

    public DbSet<Issue> Issues => Set<Issue>();

    public DbSet<RevitModel> RevitModels => Set<RevitModel>();

    public DbSet<ValidationRun> ValidationRuns => Set<ValidationRun>();

    public DbSet<ValidationResult> ValidationResults => Set<ValidationResult>();

    public DbSet<AutomationJob> AutomationJobs => Set<AutomationJob>();

    public DbSet<DrawingRevision> DrawingRevisions => Set<DrawingRevision>();

    public DbSet<Approval> Approvals => Set<Approval>();

    public DbSet<AiCommandLog> AiCommandLogs => Set<AiCommandLog>();

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    public DbSet<Notification> Notifications => Set<Notification>();

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                if (entry.Entity.CreatedAt == default)
                {
                    entry.Entity.CreatedAt = now;
                }

                entry.Entity.UpdatedAt = now;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("dbo");
        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("Roles");
            entity.HasKey(item => item.RoleId);
            entity.Property(item => item.Name).HasMaxLength(50);
            entity.HasIndex(item => item.Name).IsUnique();
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(item => item.UserId);
            entity.Property(item => item.Email).HasMaxLength(256);
            entity.Property(item => item.PasswordHash).HasMaxLength(500);
            entity.Property(item => item.FirstName).HasMaxLength(100);
            entity.Property(item => item.LastName).HasMaxLength(100);
            entity.HasIndex(item => item.Email).IsUnique();
            entity.Ignore(item => item.FullName);
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.ToTable("UserRoles");
            entity.HasKey(item => item.UserRoleId);
            entity.HasIndex(item => new { item.UserId, item.RoleId }).IsUnique();
            entity.HasOne(item => item.User).WithMany(user => user.UserRoles).HasForeignKey(item => item.UserId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(item => item.Role).WithMany(role => role.UserRoles).HasForeignKey(item => item.RoleId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Client>(entity =>
        {
            entity.ToTable("Clients");
            entity.HasKey(item => item.ClientId);
            entity.Property(item => item.Status).HasConversion<string>().HasMaxLength(30);
            entity.HasOne(item => item.PortalUser).WithMany().HasForeignKey(item => item.PortalUserId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Project>(entity =>
        {
            entity.ToTable("Projects");
            entity.HasKey(item => item.ProjectId);
            entity.Property(item => item.Code).HasMaxLength(50);
            entity.Property(item => item.Status).HasConversion<string>().HasMaxLength(40);
            entity.Property(item => item.Budget).HasColumnType("decimal(18,2)");
            entity.Property(item => item.Spent).HasColumnType("decimal(18,2)");
            entity.Property(item => item.Currency).HasMaxLength(3).IsFixedLength();
            entity.HasIndex(item => item.Code).IsUnique();
            entity.HasOne(item => item.Client).WithMany(client => client.Projects).HasForeignKey(item => item.ClientId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(item => item.ProjectManager).WithMany().HasForeignKey(item => item.ProjectManagerId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(item => item.LeadDesigner).WithMany().HasForeignKey(item => item.LeadDesignerId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ProjectRequirement>(entity =>
        {
            entity.ToTable("ProjectRequirements");
            entity.HasKey(item => item.RequirementId);
            entity.Property(item => item.Category).HasConversion<string>().HasMaxLength(80);
            entity.Property(item => item.Priority).HasConversion<string>().HasMaxLength(20);
            entity.Property(item => item.Source).HasConversion<string>().HasMaxLength(40);
            entity.Property(item => item.Status).HasConversion<string>().HasMaxLength(30);
            entity.HasOne(item => item.Project).WithMany(project => project.Requirements).HasForeignKey(item => item.ProjectId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Milestone>(entity =>
        {
            entity.ToTable("Milestones");
            entity.HasKey(item => item.MilestoneId);
            entity.Property(item => item.Status).HasConversion<string>().HasMaxLength(30);
            entity.HasOne(item => item.Project).WithMany(project => project.Milestones).HasForeignKey(item => item.ProjectId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ProjectTask>(entity =>
        {
            entity.ToTable("Tasks");
            entity.HasKey(item => item.TaskId);
            entity.Property(item => item.Status).HasConversion<string>().HasMaxLength(30);
            entity.Property(item => item.Priority).HasConversion<string>().HasMaxLength(20);
            entity.Property(item => item.Source).HasConversion<string>().HasMaxLength(40);
            entity.Property(item => item.EstimatedHours).HasColumnType("decimal(8,2)");
            entity.HasOne(item => item.Project).WithMany(project => project.Tasks).HasForeignKey(item => item.ProjectId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(item => item.Milestone).WithMany().HasForeignKey(item => item.MilestoneId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(item => item.Requirement).WithMany().HasForeignKey(item => item.RequirementId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(item => item.Assignee).WithMany().HasForeignKey(item => item.AssigneeId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<RevitModel>(entity =>
        {
            entity.ToTable("RevitModels");
            entity.HasKey(item => item.RevitModelId);
            entity.Property(item => item.Status).HasConversion<string>().HasMaxLength(30);
            entity.HasOne(item => item.Project).WithMany(project => project.Models).HasForeignKey(item => item.ProjectId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ValidationRun>(entity =>
        {
            entity.ToTable("ValidationRuns");
            entity.HasKey(item => item.ValidationRunId);
            entity.Property(item => item.Status).HasConversion<string>().HasMaxLength(30);
            entity.HasOne(item => item.Model).WithMany(model => model.ValidationRuns).HasForeignKey(item => item.RevitModelId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(item => item.Project).WithMany().HasForeignKey(item => item.ProjectId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(item => item.TriggeredBy).WithMany().HasForeignKey(item => item.TriggeredById).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ValidationResult>(entity =>
        {
            entity.ToTable("ValidationResults");
            entity.HasKey(item => item.ValidationResultId);
            entity.Property(item => item.Status).HasConversion<string>().HasMaxLength(20);
            entity.Property(item => item.CheckCode).HasMaxLength(80);
            entity.HasOne(item => item.ValidationRun).WithMany(run => run.Results).HasForeignKey(item => item.ValidationRunId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Issue>(entity =>
        {
            entity.ToTable("Issues");
            entity.HasKey(item => item.IssueId);
            entity.Property(item => item.Severity).HasConversion<string>().HasMaxLength(20);
            entity.Property(item => item.Status).HasConversion<string>().HasMaxLength(30);
            entity.HasOne(item => item.Project).WithMany(project => project.Issues).HasForeignKey(item => item.ProjectId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(item => item.ValidationRun).WithMany().HasForeignKey(item => item.ValidationRunId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(item => item.ValidationResult).WithMany().HasForeignKey(item => item.ValidationResultId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(item => item.RevitModel).WithMany().HasForeignKey(item => item.RevitModelId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(item => item.AssignedTo).WithMany().HasForeignKey(item => item.AssignedToId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AutomationJob>(entity =>
        {
            entity.ToTable("AutomationJobs");
            entity.HasKey(item => item.AutomationJobId);
            entity.Property(item => item.Status).HasConversion<string>().HasMaxLength(30);
            entity.HasOne(item => item.Project).WithMany().HasForeignKey(item => item.ProjectId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(item => item.RevitModel).WithMany().HasForeignKey(item => item.RevitModelId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<DrawingRevision>(entity =>
        {
            entity.ToTable("DrawingRevisions");
            entity.HasKey(item => item.DrawingRevisionId);
            entity.HasOne(item => item.Project).WithMany(project => project.DrawingRevisions).HasForeignKey(item => item.ProjectId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Approval>(entity =>
        {
            entity.ToTable("Approvals");
            entity.HasKey(item => item.ApprovalId);
            entity.Property(item => item.EntityType).HasConversion<string>().HasMaxLength(50);
            entity.Property(item => item.Status).HasConversion<string>().HasMaxLength(30);
            entity.HasOne(item => item.Project).WithMany(project => project.Approvals).HasForeignKey(item => item.ProjectId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AiCommandLog>(entity =>
        {
            entity.ToTable("AiCommandLogs");
            entity.HasKey(item => item.AiCommandLogId);
            entity.Property(item => item.Status).HasConversion<string>().HasMaxLength(30);
            entity.HasOne(item => item.User).WithMany().HasForeignKey(item => item.UserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(item => item.Project).WithMany().HasForeignKey(item => item.ProjectId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("AuditLogs");
            entity.HasKey(item => item.AuditLogId);
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.ToTable("Notifications");
            entity.HasKey(item => item.NotificationId);
            entity.Property(item => item.Type).HasConversion<string>().HasMaxLength(50);
            entity.HasOne(item => item.User).WithMany().HasForeignKey(item => item.UserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(item => item.Project).WithMany().HasForeignKey(item => item.ProjectId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
