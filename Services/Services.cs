using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Data;
using TaskManagement.Models.Entities;
using TaskManagement.Models.ViewModels;
using TaskManagement.Repositories;

namespace TaskManagement.Services;

public interface IAuthService
{
    Task<SignInResult> LoginAsync(string email, string password, bool remember);
    Task LogoutAsync();
    Task<IdentityResult> RegisterAsync(RegisterViewModel m);
}
public class AuthService : IAuthService
{
    private readonly SignInManager<ApplicationUser> _signIn;
    private readonly UserManager<ApplicationUser> _userMgr;
    public AuthService(SignInManager<ApplicationUser> s, UserManager<ApplicationUser> u) { _signIn = s; _userMgr = u; }
    public async Task<SignInResult> LoginAsync(string email, string password, bool remember)
    {
        var user = await _userMgr.FindByEmailAsync(email);
        if (user == null || !user.IsActive) return SignInResult.Failed;
        return await _signIn.PasswordSignInAsync(user, password, remember, false);
    }
    public Task LogoutAsync() => _signIn.SignOutAsync();
    public async Task<IdentityResult> RegisterAsync(RegisterViewModel m)
    {
        var user = new ApplicationUser { UserName = m.Email, Email = m.Email, FullName = m.FullName, IsActive = true, EmailConfirmed = true };
        var r = await _userMgr.CreateAsync(user, m.Password);
        if (r.Succeeded) await _userMgr.AddToRoleAsync(user, "Client");
        return r;
    }
}

public interface IUserService
{
    Task<IEnumerable<ApplicationUser>> GetAllAsync();
    Task<ApplicationUser?> GetAsync(string id);
    Task<IdentityResult> CreateAsync(UserViewModel vm, string password, string role);
    Task<IdentityResult> UpdateAsync(string id, UserViewModel vm);
    Task<IdentityResult> DeleteAsync(string id);
    Task ToggleActiveAsync(string id);
}
public class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _mgr;
    public UserService(UserManager<ApplicationUser> m) => _mgr = m;
    public async Task<IEnumerable<ApplicationUser>> GetAllAsync() => await _mgr.Users.ToListAsync();
    public Task<ApplicationUser?> GetAsync(string id) => _mgr.FindByIdAsync(id);
    public async Task<IdentityResult> CreateAsync(UserViewModel vm, string password, string role)
    {
        var u = new ApplicationUser { UserName = vm.Email, Email = vm.Email, FullName = vm.FullName, IsActive = vm.IsActive, EmailConfirmed = true };
        var r = await _mgr.CreateAsync(u, password);
        if (r.Succeeded) await _mgr.AddToRoleAsync(u, role);
        return r;
    }
    public async Task<IdentityResult> UpdateAsync(string id, UserViewModel vm)
    {
        var u = await _mgr.FindByIdAsync(id);
        if (u == null) return IdentityResult.Failed(new IdentityError { Description = "Not found" });
        u.FullName = vm.FullName; u.Email = vm.Email; u.UserName = vm.Email; u.IsActive = vm.IsActive;
        return await _mgr.UpdateAsync(u);
    }
    public async Task<IdentityResult> DeleteAsync(string id)
    {
        var u = await _mgr.FindByIdAsync(id);
        return u == null ? IdentityResult.Failed() : await _mgr.DeleteAsync(u);
    }
    public async Task ToggleActiveAsync(string id)
    {
        var u = await _mgr.FindByIdAsync(id);
        if (u != null) { u.IsActive = !u.IsActive; await _mgr.UpdateAsync(u); }
    }
}

public interface IRoleService
{
    Task<IEnumerable<ApplicationRole>> GetAllAsync();
    Task<ApplicationRole?> GetAsync(string id);
    Task CreateAsync(string name, string? desc);
    Task UpdateAsync(string id, string name, string? desc);
    Task DeleteAsync(string id);
    Task<IEnumerable<Permission>> GetPermissionsAsync();
    Task SetRolePermissionsAsync(string roleId, IEnumerable<int> permissionIds);
}
public class RoleService : IRoleService
{
    private readonly RoleManager<ApplicationRole> _mgr;
    private readonly ApplicationDbContext _ctx;
    public RoleService(RoleManager<ApplicationRole> m, ApplicationDbContext c) { _mgr = m; _ctx = c; }
    public async Task<IEnumerable<ApplicationRole>> GetAllAsync() => await _mgr.Roles.ToListAsync();
    public Task<ApplicationRole?> GetAsync(string id) => _mgr.FindByIdAsync(id);
    public async Task CreateAsync(string name, string? desc) =>
        await _mgr.CreateAsync(new ApplicationRole { Name = name, Description = desc });
    public async Task UpdateAsync(string id, string name, string? desc)
    {
        var r = await _mgr.FindByIdAsync(id);
        if (r != null) { r.Name = name; r.Description = desc; await _mgr.UpdateAsync(r); }
    }
    public async Task DeleteAsync(string id)
    {
        var r = await _mgr.FindByIdAsync(id);
        if (r != null) await _mgr.DeleteAsync(r);
    }
    public async Task<IEnumerable<Permission>> GetPermissionsAsync() => await _ctx.Permissions.ToListAsync();
    public async Task SetRolePermissionsAsync(string roleId, IEnumerable<int> ids)
    {
        var existing = _ctx.RolePermissions.Where(x => x.RoleId == roleId);
        _ctx.RolePermissions.RemoveRange(existing);
        foreach (var pid in ids)
            _ctx.RolePermissions.Add(new RolePermission { RoleId = roleId, PermissionId = pid });
        await _ctx.SaveChangesAsync();
    }
}

public interface IProjectService
{
    Task<IEnumerable<Project>> GetAllAsync();
    Task<IEnumerable<Project>> GetForUserAsync(string userId);
    Task<Project?> GetAsync(int id);
    Task<Project> CreateAsync(ProjectViewModel vm, string userId);
    Task UpdateAsync(int id, ProjectViewModel vm);
    Task DeleteAsync(int id);
    Task AddMemberAsync(int projectId, string userId, string role);
    Task RemoveMemberAsync(int memberId);
}
public class ProjectService : IProjectService
{
    private readonly IProjectRepository _repo;
    private readonly IUnitOfWork _uow;
    private readonly ApplicationDbContext _ctx;
    public ProjectService(IProjectRepository r, IUnitOfWork u, ApplicationDbContext c) { _repo = r; _uow = u; _ctx = c; }
    public async Task<IEnumerable<Project>> GetAllAsync() =>
        await _ctx.Projects.Include(p => p.Members).Include(p => p.Tasks).ToListAsync();
    public async Task<IEnumerable<Project>> GetForUserAsync(string userId) =>
        await _ctx.Projects.Include(p => p.Tasks)
            .Where(p => p.Members.Any(m => m.UserId == userId) || p.CreatedById == userId)
            .ToListAsync();
    public Task<Project?> GetAsync(int id) => _repo.GetWithDetailsAsync(id);
    public async Task<Project> CreateAsync(ProjectViewModel vm, string userId)
    {
        var p = new Project { Name = vm.Name, Description = vm.Description, StartDate = vm.StartDate, EndDate = vm.EndDate, Status = vm.Status, CreatedById = userId };
        await _repo.AddAsync(p); await _uow.SaveAsync(); return p;
    }
    public async Task UpdateAsync(int id, ProjectViewModel vm)
    {
        var p = await _repo.GetByIdAsync(id); if (p == null) return;
        p.Name = vm.Name; p.Description = vm.Description; p.StartDate = vm.StartDate; p.EndDate = vm.EndDate; p.Status = vm.Status;
        _repo.Update(p); await _uow.SaveAsync();
    }
    public async Task DeleteAsync(int id)
    {
        var p = await _repo.GetByIdAsync(id); if (p == null) return;
        _repo.Remove(p); await _uow.SaveAsync();
    }
    public async Task AddMemberAsync(int projectId, string userId, string role)
    {
        if (await _ctx.ProjectMembers.AnyAsync(m => m.ProjectId == projectId && m.UserId == userId)) return;
        _ctx.ProjectMembers.Add(new ProjectMember { ProjectId = projectId, UserId = userId, Role = role });
        await _ctx.SaveChangesAsync();
    }
    public async Task RemoveMemberAsync(int memberId)
    {
        var m = await _ctx.ProjectMembers.FindAsync(memberId);
        if (m != null) { _ctx.ProjectMembers.Remove(m); await _ctx.SaveChangesAsync(); }
    }
}

public interface ITaskService
{
    Task<IEnumerable<TaskItem>> GetAllAsync();
    Task<IEnumerable<TaskItem>> GetForUserAsync(string userId);
    Task<TaskItem?> GetAsync(int id);
    Task<TaskItem> CreateAsync(TaskViewModel vm, string userId);
    Task UpdateAsync(int id, TaskViewModel vm);
    Task DeleteAsync(int id);
    Task ChangeStatusAsync(int id, Models.Entities.TaskStatus s);
}
public class TaskService : ITaskService
{
    private readonly ITaskRepository _repo;
    private readonly IUnitOfWork _uow;
    private readonly ApplicationDbContext _ctx;
    public TaskService(ITaskRepository r, IUnitOfWork u, ApplicationDbContext c) { _repo = r; _uow = u; _ctx = c; }
    public async Task<IEnumerable<TaskItem>> GetAllAsync() =>
        await _ctx.Tasks.Include(t => t.Project).Include(t => t.AssignedTo).ToListAsync();
    public Task<IEnumerable<TaskItem>> GetForUserAsync(string uid) => _repo.GetByUserAsync(uid);
    public Task<TaskItem?> GetAsync(int id) => _repo.GetWithDetailsAsync(id);
    public async Task<TaskItem> CreateAsync(TaskViewModel vm, string userId)
    {
        var t = new TaskItem
        {
            Title = vm.Title, Description = vm.Description, StartDate = vm.StartDate, DueDate = vm.DueDate,
            Priority = vm.Priority, Status = vm.Status, ProjectId = vm.ProjectId,
            AssignedToId = string.IsNullOrWhiteSpace(vm.AssignedToId) ? null : vm.AssignedToId,
            CreatedById = userId
        };
        await _repo.AddAsync(t); await _uow.SaveAsync(); return t;
    }
    public async Task UpdateAsync(int id, TaskViewModel vm)
    {
        var t = await _repo.GetByIdAsync(id); if (t == null) return;
        t.Title = vm.Title; t.Description = vm.Description; t.StartDate = vm.StartDate; t.DueDate = vm.DueDate;
        t.Priority = vm.Priority; t.Status = vm.Status; t.ProjectId = vm.ProjectId;
        t.AssignedToId = string.IsNullOrWhiteSpace(vm.AssignedToId) ? null : vm.AssignedToId;
        if (vm.Status == Models.Entities.TaskStatus.Completed && t.CompletedAt == null) t.CompletedAt = DateTime.UtcNow;
        _repo.Update(t); await _uow.SaveAsync();
    }
    public async Task DeleteAsync(int id)
    {
        var t = await _repo.GetByIdAsync(id); if (t == null) return;
        _repo.Remove(t); await _uow.SaveAsync();
    }
    public async Task ChangeStatusAsync(int id, Models.Entities.TaskStatus s)
    {
        var t = await _repo.GetByIdAsync(id); if (t == null) return;
        t.Status = s;
        if (s == Models.Entities.TaskStatus.Completed) t.CompletedAt = DateTime.UtcNow;
        _repo.Update(t); await _uow.SaveAsync();
    }
}

public interface IKanbanService
{
    Task<Dictionary<Models.Entities.TaskStatus, List<TaskItem>>> GetBoardAsync(string? userId = null);
}
public class KanbanService : IKanbanService
{
    private readonly ApplicationDbContext _ctx;
    public KanbanService(ApplicationDbContext c) => _ctx = c;
    public async Task<Dictionary<Models.Entities.TaskStatus, List<TaskItem>>> GetBoardAsync(string? userId = null)
    {
        var q = _ctx.Tasks.Include(t => t.Project).Include(t => t.AssignedTo).AsQueryable();
        if (userId != null) q = q.Where(t => t.AssignedToId == userId);
        var list = await q.ToListAsync();
        return Enum.GetValues<Models.Entities.TaskStatus>()
            .ToDictionary(s => s, s => list.Where(t => t.Status == s).ToList());
    }
}

public interface ICalendarService
{
    Task<IEnumerable<object>> GetEventsAsync(string? userId = null);
}
public class CalendarService : ICalendarService
{
    private readonly ApplicationDbContext _ctx;
    public CalendarService(ApplicationDbContext c) => _ctx = c;
    public async Task<IEnumerable<object>> GetEventsAsync(string? userId = null)
    {
        var q = _ctx.Tasks.Include(t => t.Project).AsQueryable();
        if (userId != null) q = q.Where(t => t.AssignedToId == userId);
        var tasks = await q.Select(t => new {
            id = "t" + t.Id, title = t.Title, start = t.StartDate, end = t.DueDate,
            color = t.Status == Models.Entities.TaskStatus.Completed ? "#16a34a"
                  : t.Status == Models.Entities.TaskStatus.InProgress ? "#2563eb"
                  : t.Status == Models.Entities.TaskStatus.InReview ? "#f59e0b" : "#64748b",
            url = "/Tasks/Details/" + t.Id
        }).ToListAsync();
        return tasks;
    }
}

public interface IReportService
{
    Task<ReportViewModel> BuildAsync(string? userId = null);
}
public class ReportService : IReportService
{
    private readonly ApplicationDbContext _ctx;
    public ReportService(ApplicationDbContext c) => _ctx = c;
    public async Task<ReportViewModel> BuildAsync(string? userId = null)
    {
        var tq = _ctx.Tasks.AsQueryable();
        var pq = _ctx.Projects.AsQueryable();
        if (userId != null) { tq = tq.Where(t => t.AssignedToId == userId); pq = pq.Where(p => p.Members.Any(m => m.UserId == userId)); }
        return new ReportViewModel
        {
            TotalTasks = await tq.CountAsync(),
            CompletedTasks = await tq.CountAsync(t => t.Status == Models.Entities.TaskStatus.Completed),
            PendingTasks = await tq.CountAsync(t => t.Status == Models.Entities.TaskStatus.Pending),
            InProgressTasks = await tq.CountAsync(t => t.Status == Models.Entities.TaskStatus.InProgress),
            OverdueTasks = await tq.CountAsync(t => t.DueDate < DateTime.UtcNow && t.Status != Models.Entities.TaskStatus.Completed),
            TotalProjects = await pq.CountAsync(),
            ActiveProjects = await pq.CountAsync(p => p.Status == ProjectStatus.Active),
            Tasks = await tq.Include(t => t.Project).Include(t => t.AssignedTo).Take(200).ToListAsync()
        };
    }
}

public interface INotificationService
{
    Task<IEnumerable<Notification>> GetForUserAsync(string userId);
    Task AddAsync(string userId, string title, string msg, string? url = null);
    Task MarkReadAsync(int id);
}
public class NotificationService : INotificationService
{
    private readonly ApplicationDbContext _ctx;
    public NotificationService(ApplicationDbContext c) => _ctx = c;
    public async Task AddAsync(string userId, string title, string msg, string? url = null)
    { _ctx.Notifications.Add(new Notification { UserId = userId, Title = title, Message = msg, Url = url }); await _ctx.SaveChangesAsync(); }
    public async Task<IEnumerable<Notification>> GetForUserAsync(string uid) =>
        await _ctx.Notifications.Where(n => n.UserId == uid).OrderByDescending(n => n.CreatedAt).Take(50).ToListAsync();
    public async Task MarkReadAsync(int id)
    {
        var n = await _ctx.Notifications.FindAsync(id);
        if (n != null) { n.IsRead = true; await _ctx.SaveChangesAsync(); }
    }
}

public interface IAuditService
{
    Task LogAsync(string action, string entity, string? entityId = null, string? details = null);
    Task<IEnumerable<AuditLog>> GetAsync(string? userId = null, DateTime? from = null, DateTime? to = null);
}
public class AuditService : IAuditService
{
    private readonly ApplicationDbContext _ctx;
    private readonly IHttpContextAccessor _http;
    public AuditService(ApplicationDbContext c, IHttpContextAccessor h) { _ctx = c; _http = h; }
    public async Task LogAsync(string action, string entity, string? entityId = null, string? details = null)
    {
        var u = _http.HttpContext?.User;
        _ctx.AuditLogs.Add(new AuditLog
        {
            Action = action, Entity = entity, EntityId = entityId, Details = details,
            UserName = u?.Identity?.Name,
            UserId = u?.Claims.FirstOrDefault(c => c.Type.EndsWith("nameidentifier"))?.Value,
            IpAddress = _http.HttpContext?.Connection.RemoteIpAddress?.ToString()
        });
        await _ctx.SaveChangesAsync();
    }
    public async Task<IEnumerable<AuditLog>> GetAsync(string? userId = null, DateTime? from = null, DateTime? to = null)
    {
        var q = _ctx.AuditLogs.AsQueryable();
        if (userId != null) q = q.Where(a => a.UserId == userId);
        if (from.HasValue) q = q.Where(a => a.Timestamp >= from);
        if (to.HasValue) q = q.Where(a => a.Timestamp <= to);
        return await q.OrderByDescending(a => a.Timestamp).Take(500).ToListAsync();
    }
}
