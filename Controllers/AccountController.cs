using Microsoft.AspNetCore.Mvc;
using TaskManagement.Models.ViewModels;
using TaskManagement.Services;

namespace TaskManagement.Controllers;

public class AccountController : Controller
{
    private readonly IAuthService _auth;
    public AccountController(IAuthService a) => _auth = a;

    [HttpGet] public IActionResult Login() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel m)
    {
        if (!ModelState.IsValid) return View(m);
        var r = await _auth.LoginAsync(m.Email, m.Password, m.RememberMe);
        if (r.Succeeded) return RedirectToAction("Index", "Dashboard");
        ModelState.AddModelError("", "Credenciales inválidas o usuario inactivo.");
        return View(m);
    }

    [HttpGet] public IActionResult Register() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel m)
    {
        if (!ModelState.IsValid) return View(m);
        var r = await _auth.RegisterAsync(m);
        if (r.Succeeded) return RedirectToAction("Login");
        foreach (var e in r.Errors) ModelState.AddModelError("", e.Description);
        return View(m);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout() { await _auth.LogoutAsync(); return RedirectToAction("Login"); }

    public IActionResult AccessDenied() => View();
}
