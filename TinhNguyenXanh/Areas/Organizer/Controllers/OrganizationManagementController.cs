using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TinhNguyenXanh.Data;
using TinhNguyenXanh.DTOs;
using TinhNguyenXanh.Interfaces;
using TinhNguyenXanh.Services;

[Area("Organizer")]
[Authorize(Roles = "Organizer")]
public class OrganizationController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IOrganizationService _orgService; // DÙNG SERVICE

    public OrganizationController(
        UserManager<ApplicationUser> userManager,
        IOrganizationService orgService) // INJECT SERVICE
    {
        _userManager = userManager;
        _orgService = orgService;
    }

    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var org = await _orgService.GetByUserIdAsync(user.Id); // DÙNG SERVICE
        if (org == null)
        {
            TempData["Error"] = "Không tìm thấy tổ chức.";
            return RedirectToAction("Index", "Dashboard");
        }

        return View(org);
    }

    [HttpGet]
    public async Task<IActionResult> Edit()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var org = await _orgService.GetByUserIdAsync(user.Id);
        if (org == null) return RedirectToAction("Profile");

        return View(org);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(OrganizationDTO model, IFormFile? avatarFile, IFormFile? verificationDocFile)
    {
        // === LOG MODELSTATE ĐỂ XEM LỖI ===
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(x => x.Value.Errors.Count > 0)
                .ToDictionary(
                    x => x.Key,
                    x => x.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                );
            Console.WriteLine("[EDIT] ModelState Errors: " + System.Text.Json.JsonSerializer.Serialize(errors));
            TempData["Error"] = "Vui lòng kiểm tra lại thông tin.";
            return View(model);
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        // === THÊM VALIDATION TỪ SERVICE ===
        try
        {
            // Tái sử dụng ValidateModel từ Register
            var service = (OrganizationService)HttpContext.RequestServices.GetService(typeof(IOrganizationService))!;
            var validateMethod = typeof(OrganizationService).GetMethod("ValidateModel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            validateMethod!.Invoke(service, new object[] { model, user.Id });
        }
        catch (Exception ex) when (ex.InnerException is ArgumentException argEx)
        {
            ModelState.AddModelError("", argEx.Message);
            return View(model);
        }

        var result = await _orgService.UpdateAsync(model, user.Id, avatarFile, verificationDocFile);
        if (!result)
        {
            TempData["Error"] = "Cập nhật thất bại.";
            return View(model);
        }

        TempData["Success"] = "Cập nhật thành công!";
        return RedirectToAction("Profile");
    }
}