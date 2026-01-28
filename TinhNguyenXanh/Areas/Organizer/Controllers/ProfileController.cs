using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Area("Organizer")]
[Authorize(Roles = "Organizer")]
public class ProfileController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}