using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.Models.Entities;
using TaskManagement.Services;

namespace TaskManagement.Controllers;

[Authorize]
public class CalendarController : Controller
{
    private readonly ICalendarService _svc;
    private readonly UserManager<ApplicationUser> _users;
    public CalendarController(ICalendarService s, UserManager<ApplicationUser> u) { _svc = s; _users = u; }
    public IActionResult Index() => View();
    public async Task<IActionResult> Events()
    {
        var uid = User.IsInRole("Admin") ? null : _users.GetUserId(User);
        return Json(await _svc.GetEventsAsync(uid));
    }
}
