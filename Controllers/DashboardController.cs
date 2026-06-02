using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Data;
using TaskManagement.Models.Entities;
using TaskManagement.Models.ViewModels;

namespace TaskManagement.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly ApplicationDbContext _ctx;
    private readonly UserManager<ApplicationUser> _users;
    public DashboardController(ApplicationDbContext c, UserManager<ApplicationUser> u) { _ctx = c; _users = u; }

    public async Task<IActionResult> Index()
    {
        var uid = _users.GetUserId(User)!;
        var isAdmin = User.IsInRole("Admin");
        var tq = _ctx.Tasks.AsQueryable();
        var pq = _ctx.Projects.AsQueryable();
        if (!isAdmin) { tq = tq.Where(t => t.AssignedToId == uid); pq = pq.Where(p => p.Members.Any(m => m.UserId == uid)); }

        var vm = new DashboardViewModel
        {
            TotalProjects = await pq.CountAsync(),
            ActiveProjects = await pq.CountAsync(p => p.Status == ProjectStatus.Active),
            FinishedProjects = await pq.CountAsync(p => p.Status == ProjectStatus.Finished),
            SuspendedProjects = await pq.CountAsync(p => p.Status == ProjectStatus.Suspended),
            PendingTasks = await tq.CountAsync(t => t.Status == Models.Entities.TaskStatus.Pending),
            InProgressTasks = await tq.CountAsync(t => t.Status == Models.Entities.TaskStatus.InProgress),
            CompletedTasks = await tq.CountAsync(t => t.Status == Models.Entities.TaskStatus.Completed),
            OverdueTasks = await tq.CountAsync(t => t.DueDate < DateTime.UtcNow && t.Status != Models.Entities.TaskStatus.Completed),
            RecentTasks = await tq.Include(t => t.Project).OrderByDescending(t => t.CreatedAt).Take(8).ToListAsync(),
            RecentActivity = isAdmin ? await _ctx.AuditLogs.OrderByDescending(a => a.Timestamp).Take(10).ToListAsync() : new()
        };
        return View(vm);
    }
}
