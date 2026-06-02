using Microsoft.AspNetCore.Mvc;
namespace TaskManagement.Controllers;
public class HomeController : Controller
{
    public IActionResult Error() => View();
}
