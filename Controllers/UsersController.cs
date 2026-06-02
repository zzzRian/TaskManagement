using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Data;
using TaskManagement.Models.ViewModels;
using TaskManagement.Services;

namespace TaskManagement.Controllers;

[Authorize(Roles = "Admin")]
public class UsersController : Controller
{
    private readonly IUserService _svc;
    private readonly ApplicationDbContext _ctx;
    private readonly Microsoft.AspNetCore.Identity.RoleManager<Models.Entities.ApplicationRole> _roles;
    public UsersController(IUserService s, ApplicationDbContext c, Microsoft.AspNetCore.Identity.RoleManager<Models.Entities.ApplicationRole> r) { _svc = s; _ctx = c; _roles = r; }

    public async Task<IActionResult> Index(string? q)
    {
        var list = await _svc.GetAllAsync();
        if (!string.IsNullOrWhiteSpace(q))
            list = list.Where(u => (u.FullName ?? "").Contains(q, StringComparison.OrdinalIgnoreCase) ||
                                   (u.Email ?? "").Contains(q, StringComparison.OrdinalIgnoreCase));
        ViewBag.Q = q;
        return View(list);
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.Roles = new SelectList(await _roles.Roles.Select(r => r.Name).ToListAsync());
        return View(new UserViewModel());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UserViewModel vm)
    {
        if (!ModelState.IsValid || string.IsNullOrEmpty(vm.Password))
        {
            ViewBag.Roles = new SelectList(await _roles.Roles.Select(r => r.Name).ToListAsync());
            if (string.IsNullOrEmpty(vm.Password)) ModelState.AddModelError("Password", "Required");
            return View(vm);
        }
        var r = await _svc.CreateAsync(vm, vm.Password!, vm.Role);
        if (r.Succeeded) return RedirectToAction(nameof(Index));
        foreach (var e in r.Errors) ModelState.AddModelError("", e.Description);
        ViewBag.Roles = new SelectList(await _roles.Roles.Select(rr => rr.Name).ToListAsync());
        return View(vm);
    }

    public async Task<IActionResult> Edit(string id)
    {
        var u = await _svc.GetAsync(id); if (u == null) return NotFound();
        return View(new UserViewModel { Id = u.Id, FullName = u.FullName, Email = u.Email!, IsActive = u.IsActive });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, UserViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);
        await _svc.UpdateAsync(id, vm);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id) { await _svc.DeleteAsync(id); return RedirectToAction(nameof(Index)); }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(string id) { await _svc.ToggleActiveAsync(id); return RedirectToAction(nameof(Index)); }
}
