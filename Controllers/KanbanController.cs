using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.Models.Entities;
using TaskManagement.Services;

namespace TaskManagement.Controllers;

[Authorize]
public class KanbanController : Controller
{
    private readonly IKanbanService _svc;
    private readonly ITaskService _tasks;
    private readonly UserManager<ApplicationUser> _users;
    public KanbanController(IKanbanService s, ITaskService t, UserManager<ApplicationUser> u) { _svc = s; _tasks = t; _users = u; }

    public async Task<IActionResult> Index()
    {
        var uid = User.IsInRole("Admin") ? null : _users.GetUserId(User);
        ViewBag.IsGlobal = User.IsInRole("Admin");
        return View(await _svc.GetBoardAsync(uid));
    }

    [HttpPost]
    public async Task<IActionResult> Move(int id, Models.Entities.TaskStatus status)
    { await _tasks.ChangeStatusAsync(id, status); return Ok(); }
}
