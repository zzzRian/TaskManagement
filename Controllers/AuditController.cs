using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.Services;

namespace TaskManagement.Controllers;

[Authorize(Roles = "Admin")]
public class AuditController : Controller
{
    private readonly IAuditService _svc;
    public AuditController(IAuditService s) => _svc = s;
    public async Task<IActionResult> Index(string? userId, DateTime? from, DateTime? to)
    {
        ViewBag.UserId = userId; ViewBag.From = from; ViewBag.To = to;
        return View(await _svc.GetAsync(userId, from, to));
    }
}
