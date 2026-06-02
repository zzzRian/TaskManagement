using System.ComponentModel.DataAnnotations;
using TaskManagement.Models.Entities;

namespace TaskManagement.Models.ViewModels;

public class LoginViewModel
{
    [Required, EmailAddress] public string Email { get; set; } = "";
    [Required, DataType(DataType.Password)] public string Password { get; set; } = "";
    public bool RememberMe { get; set; }
}
public class RegisterViewModel
{
    [Required] public string FullName { get; set; } = "";
    [Required, EmailAddress] public string Email { get; set; } = "";
    [Required, DataType(DataType.Password), MinLength(6)] public string Password { get; set; } = "";
    [Required, Compare(nameof(Password)), DataType(DataType.Password)] public string ConfirmPassword { get; set; } = "";
}
public class UserViewModel
{
    public string? Id { get; set; }
    [Required] public string FullName { get; set; } = "";
    [Required, EmailAddress] public string Email { get; set; } = "";
    public string? Password { get; set; }
    public string Role { get; set; } = "Client";
    public bool IsActive { get; set; } = true;
}
public class ProjectViewModel
{
    public int Id { get; set; }
    [Required, StringLength(150, MinimumLength = 3)] public string Name { get; set; } = "";
    public string? Description { get; set; }
    [Required] public DateTime StartDate { get; set; } = DateTime.Today;
    [Required] public DateTime EndDate { get; set; } = DateTime.Today.AddDays(30);
    public ProjectStatus Status { get; set; } = ProjectStatus.Active;
}
public class TaskViewModel : IValidatableObject
{
    public int Id { get; set; }
    [Required, StringLength(200, MinimumLength = 3)] public string Title { get; set; } = "";
    [Required] public string Description { get; set; } = "";
    [Required] public DateTime StartDate { get; set; } = DateTime.Today;
    [Required] public DateTime DueDate { get; set; } = DateTime.Today.AddDays(7);
    [Required] public Entities.TaskPriority Priority { get; set; } = Entities.TaskPriority.Medium;
    [Required] public Entities.TaskStatus Status { get; set; } = Entities.TaskStatus.Pending;
    [Required] public int ProjectId { get; set; }
    public string? AssignedToId { get; set; }
    public IEnumerable<ValidationResult> Validate(ValidationContext c)
    {
        if (DueDate < StartDate)
            yield return new ValidationResult("La fecha de fin no puede ser menor que la fecha de inicio.", new[] { nameof(DueDate) });
    }
}
public class DashboardViewModel
{
    public int TotalProjects { get; set; }
    public int ActiveProjects { get; set; }
    public int FinishedProjects { get; set; }
    public int SuspendedProjects { get; set; }
    public int PendingTasks { get; set; }
    public int InProgressTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int OverdueTasks { get; set; }
    public List<TaskItem> RecentTasks { get; set; } = new();
    public List<AuditLog> RecentActivity { get; set; } = new();
}
public class ReportViewModel
{
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int PendingTasks { get; set; }
    public int InProgressTasks { get; set; }
    public int OverdueTasks { get; set; }
    public int TotalProjects { get; set; }
    public int ActiveProjects { get; set; }
    public List<TaskItem> Tasks { get; set; } = new();
}
public class ProfileViewModel
{
    [Required] public string FullName { get; set; } = "";
    [Required, EmailAddress] public string Email { get; set; } = "";
    public string? AvatarUrl { get; set; }
}
public class ChangePasswordViewModel
{
    [Required, DataType(DataType.Password)] public string CurrentPassword { get; set; } = "";
    [Required, DataType(DataType.Password), MinLength(6)] public string NewPassword { get; set; } = "";
    [Required, DataType(DataType.Password), Compare(nameof(NewPassword))] public string ConfirmPassword { get; set; } = "";
}
