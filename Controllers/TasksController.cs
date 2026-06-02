using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Data;
using TaskManagement.Models.Entities;
using TaskManagement.Models.ViewModels;
using TaskManagement.Services;

namespace TaskManagement.Controllers;

[Authorize]
public class TasksController : Controller
{
    private readonly ITaskService _svc;
    private readonly ApplicationDbContext _ctx;
    private readonly UserManager<ApplicationUser> _users;
    public TasksController(ITaskService s, ApplicationDbContext c, UserManager<ApplicationUser> u) { _svc = s; _ctx = c; _users = u; }

    public async Task<IActionResult> Index()
    {
        var uid = _users.GetUserId(User)!;
        return View(User.IsInRole("Admin") ? await _svc.GetAllAsync() : await _svc.GetForUserAsync(uid));
    }

    public async Task<IActionResult> Details(int id)
    {
        var t = await _svc.GetAsync(id); if (t == null) return NotFound();
        var uid = _users.GetUserId(User)!;
        if (!User.IsInRole("Admin") && t.AssignedToId != uid && t.CreatedById != uid) return Forbid();
        return View(t);
    }

    public async Task<IActionResult> Create(int? projectId)
    {
        await LoadLists(projectId);
        return View(new TaskViewModel { ProjectId = projectId ?? 0 });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TaskViewModel vm)
    {
        if (!ModelState.IsValid) { await LoadLists(vm.ProjectId); return View(vm); }
        if (!User.IsInRole("Admin")) vm.AssignedToId = _users.GetUserId(User);
        await _svc.CreateAsync(vm, _users.GetUserId(User)!);
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var t = await _svc.GetAsync(id); if (t == null) return NotFound();
        var uid = _users.GetUserId(User)!;
        if (!User.IsInRole("Admin") && t.AssignedToId != uid && t.CreatedById != uid) return Forbid();
        await LoadLists(t.ProjectId);
        return View(new TaskViewModel { Id = t.Id, Title = t.Title, Description = t.Description, StartDate = t.StartDate, DueDate = t.DueDate, Priority = t.Priority, Status = t.Status, ProjectId = t.ProjectId, AssignedToId = t.AssignedToId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, TaskViewModel vm)
    {
        if (!ModelState.IsValid) { await LoadLists(vm.ProjectId); return View(vm); }
        if (!User.IsInRole("Admin"))
        {
            var existing = await _svc.GetAsync(id);
            if (existing != null) vm.AssignedToId = existing.AssignedToId;
        }
        await _svc.UpdateAsync(id, vm); return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id) { await _svc.DeleteAsync(id); return RedirectToAction(nameof(Index)); }

    [HttpPost]
    public async Task<IActionResult> ChangeStatus(int id, Models.Entities.TaskStatus status)
    { await _svc.ChangeStatusAsync(id, status); return Ok(); }

    private async Task LoadLists(int? projectId)
    {
        ViewBag.Projects = new SelectList(await _ctx.Projects.ToListAsync(), "Id", "Name", projectId);
        ViewBag.Users = new SelectList(await _ctx.Users.ToListAsync(), "Id", "FullName");
        ViewBag.Priorities = new SelectList(Enum.GetValues<TaskPriority>());
        ViewBag.Statuses = new SelectList(Enum.GetValues<Models.Entities.TaskStatus>());
    }
}
