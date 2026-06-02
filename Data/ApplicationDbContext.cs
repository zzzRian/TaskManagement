using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Models.Entities;

namespace TaskManagement.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> opts) : base(opts) { }

    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ProjectMember> ProjectMembers => Set<ProjectMember>();
    public DbSet<TaskItem> Tasks => Set<TaskItem>();
    public DbSet<TaskComment> TaskComments => Set<TaskComment>();
    public DbSet<TaskAttachment> TaskAttachments => Set<TaskAttachment>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        b.Entity<RolePermission>().HasKey(x => new { x.RoleId, x.PermissionId });
        b.Entity<RolePermission>()
            .HasOne(x => x.Role).WithMany(r => r.RolePermissions).HasForeignKey(x => x.RoleId);
        b.Entity<RolePermission>()
            .HasOne(x => x.Permission).WithMany(p => p.RolePermissions).HasForeignKey(x => x.PermissionId);

        b.Entity<Permission>().HasIndex(p => p.Name).IsUnique();

        b.Entity<Project>().HasIndex(p => p.Name);
        b.Entity<Project>()
            .HasOne(p => p.CreatedBy).WithMany().HasForeignKey(p => p.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);

        b.Entity<ProjectMember>()
            .HasIndex(x => new { x.ProjectId, x.UserId }).IsUnique();
        b.Entity<ProjectMember>()
            .HasOne(x => x.Project).WithMany(p => p.Members).HasForeignKey(x => x.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
        b.Entity<ProjectMember>()
            .HasOne(x => x.User).WithMany(u => u.ProjectMemberships).HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        b.Entity<TaskItem>().HasIndex(t => t.Status);
        b.Entity<TaskItem>().HasIndex(t => t.DueDate);
        b.Entity<TaskItem>()
            .HasOne(t => t.Project).WithMany(p => p.Tasks).HasForeignKey(t => t.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
        b.Entity<TaskItem>()
            .HasOne(t => t.AssignedTo).WithMany(u => u.AssignedTasks).HasForeignKey(t => t.AssignedToId)
            .OnDelete(DeleteBehavior.SetNull);
        b.Entity<TaskItem>()
            .HasOne(t => t.CreatedBy).WithMany().HasForeignKey(t => t.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);

        b.Entity<TaskComment>()
            .HasOne(c => c.Task).WithMany(t => t.Comments).HasForeignKey(c => c.TaskId)
            .OnDelete(DeleteBehavior.Cascade);

        b.Entity<TaskAttachment>()
            .HasOne(a => a.Task).WithMany(t => t.Attachments).HasForeignKey(a => a.TaskId)
            .OnDelete(DeleteBehavior.Cascade);

        b.Entity<Notification>().HasIndex(n => n.UserId);
        b.Entity<AuditLog>().HasIndex(a => a.Timestamp);
    }
}
