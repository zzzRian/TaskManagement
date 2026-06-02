using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.Models.Entities;
using TaskManagement.Services;

namespace TaskManagement.Controllers;

[Authorize]
public class NotificationsController : Controller
{
    private readonly INotificationService _svc;
    private readonly UserManager<ApplicationUser> _users;
    public NotificationsController(INotificationService s, UserManager<ApplicationUser> u) { _svc = s; _users = u; }
    public async Task<IActionResult> Index() => View(await _svc.GetForUserAsync(_users.GetUserId(User)!));
    [HttpPost]
    public async Task<IActionResult> MarkRead(int id) { await _svc.MarkReadAsync(id); return Ok(); }
}
