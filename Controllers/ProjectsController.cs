using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Data;
using TaskManagement.Models.Entities;
using TaskManagement.Models.ViewModels;
using TaskManagement.Services;

namespace TaskManagement.Controllers;

[Authorize]
public class ProjectsController : Controller
{
    private readonly IProjectService _svc;
    private readonly UserManager<ApplicationUser> _users;
    private readonly ApplicationDbContext _ctx;
    public ProjectsController(IProjectService s, UserManager<ApplicationUser> u, ApplicationDbContext c) { _svc = s; _users = u; _ctx = c; }

    public async Task<IActionResult> Index()
    {
        var uid = _users.GetUserId(User)!;
        return View(User.IsInRole("Admin") ? await _svc.GetAllAsync() : await _svc.GetForUserAsync(uid));
    }

    public async Task<IActionResult> Details(int id)
    {
        var p = await _svc.GetAsync(id); if (p == null) return NotFound();
        ViewBag.AllUsers = await _ctx.Users.ToListAsync();
        return View(p);
    }

    [Authorize(Roles = "Admin")]
    public IActionResult Create() => View(new ProjectViewModel());

    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(ProjectViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);
        if (vm.EndDate < vm.StartDate) { ModelState.AddModelError("EndDate", "Invalid"); return View(vm); }
        await _svc.CreateAsync(vm, _users.GetUserId(User)!);
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id)
    {
        var p = await _svc.GetAsync(id); if (p == null) return NotFound();
        return View(new ProjectViewModel { Id = p.Id, Name = p.Name, Description = p.Description, StartDate = p.StartDate, EndDate = p.EndDate, Status = p.Status });
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id, ProjectViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);
        await _svc.UpdateAsync(id, vm); return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id) { await _svc.DeleteAsync(id); return RedirectToAction(nameof(Index)); }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Admin")]
    public async Task<IActionResult> AddMember(int projectId, string userId, string role = "Member")
    { await _svc.AddMemberAsync(projectId, userId, role); return RedirectToAction(nameof(Details), new { id = projectId }); }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Admin")]
    public async Task<IActionResult> RemoveMember(int memberId, int projectId)
    { await _svc.RemoveMemberAsync(memberId); return RedirectToAction(nameof(Details), new { id = projectId }); }
}
