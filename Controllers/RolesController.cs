using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.Services;

namespace TaskManagement.Controllers;

[Authorize(Roles = "Admin")]
public class RolesController : Controller
{
    private readonly IRoleService _svc;
    public RolesController(IRoleService s) => _svc = s;

    public async Task<IActionResult> Index() => View(await _svc.GetAllAsync());
    public IActionResult Create() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string name, string? description)
    { await _svc.CreateAsync(name, description); return RedirectToAction(nameof(Index)); }

    public async Task<IActionResult> Edit(string id)
    {
        var r = await _svc.GetAsync(id); if (r == null) return NotFound();
        ViewBag.Permissions = await _svc.GetPermissionsAsync();
        return View(r);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, string name, string? description, int[] permissionIds)
    {
        await _svc.UpdateAsync(id, name, description);
        await _svc.SetRolePermissionsAsync(id, permissionIds ?? Array.Empty<int>());
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id) { await _svc.DeleteAsync(id); return RedirectToAction(nameof(Index)); }
}
