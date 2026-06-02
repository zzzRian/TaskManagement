using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.Models.Entities;
using TaskManagement.Models.ViewModels;

namespace TaskManagement.Controllers;

[Authorize]
public class ProfileController : Controller
{
    private readonly UserManager<ApplicationUser> _users;
    private readonly IWebHostEnvironment _env;
    public ProfileController(UserManager<ApplicationUser> u, IWebHostEnvironment e) { _users = u; _env = e; }

    public async Task<IActionResult> Index()
    {
        var u = await _users.GetUserAsync(User);
        return View(new ProfileViewModel { FullName = u!.FullName, Email = u.Email!, AvatarUrl = u.AvatarUrl });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(ProfileViewModel vm, IFormFile? avatar)
    {
        var u = await _users.GetUserAsync(User);
        u!.FullName = vm.FullName; u.Email = vm.Email; u.UserName = vm.Email;
        if (avatar != null && avatar.Length > 0)
        {
            var dir = Path.Combine(_env.WebRootPath, "uploads");
            Directory.CreateDirectory(dir);
            var name = $"{Guid.NewGuid()}{Path.GetExtension(avatar.FileName)}";
            using var fs = System.IO.File.Create(Path.Combine(dir, name));
            await avatar.CopyToAsync(fs);
            u.AvatarUrl = "/uploads/" + name;
        }
        await _users.UpdateAsync(u);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel vm)
    {
        var u = await _users.GetUserAsync(User);
        var r = await _users.ChangePasswordAsync(u!, vm.CurrentPassword, vm.NewPassword);
        if (r.Succeeded) TempData["msg"] = "Contraseña actualizada";
        else TempData["msg"] = string.Join(", ", r.Errors.Select(e => e.Description));
        return RedirectToAction(nameof(Index));
    }
}
