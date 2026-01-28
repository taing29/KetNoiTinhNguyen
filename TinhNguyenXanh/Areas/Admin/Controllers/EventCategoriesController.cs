using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TinhNguyenXanh.Data;
using TinhNguyenXanh.Interfaces;
using TinhNguyenXanh.Models;

namespace TinhNguyenXanh.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class EventCategoriesController : Controller
    {
        private readonly IEventCategoryRepository _categoryRepo;

        public EventCategoriesController(IEventCategoryRepository categoryRepo)
        {
            _categoryRepo = categoryRepo;
        }

        // GET: Admin/EventCategories
        public async Task<IActionResult> Index(string searchString, int page = 1)
        {
            int pageSize = 10;
            var categories = string.IsNullOrEmpty(searchString)
                ? await _categoryRepo.GetAllCategoriesAsync()
                : await _categoryRepo.GetAllCategoriesAsync(); // bạn có thể thêm filter ở repo nếu cần

            var filtered = categories
                .Where(c => string.IsNullOrEmpty(searchString) ||
                           c.Name.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                .OrderBy(c => c.Name);

            var total = filtered.Count();
            var items = filtered
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.SearchString = searchString;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(total / (double)pageSize);

            return View(items);
        }

        // GET: Admin/EventCategories/Create
        public IActionResult Create() => View();

        // POST: Admin/EventCategories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EventCategory category)
        {
            if (ModelState.IsValid)
            {
                await _categoryRepo.AddCategoryAsync(category);
                TempData["Success"] = $"Đã thêm danh mục \"{category.Name}\" thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // GET: Admin/EventCategories/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var category = await _categoryRepo.GetCategoryByIdAsync(id);
            if (category == null) return NotFound();
            return View(category);
        }

        // POST: Admin/EventCategories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EventCategory category)
        {
            if (id != category.Id) return NotFound();

            if (ModelState.IsValid)
            {
                await _categoryRepo.UpdateCategoryAsync(category);
                TempData["Success"] = $"Đã cập nhật danh mục \"{category.Name}\"!";
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // POST: Admin/EventCategories/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _categoryRepo.GetCategoryByIdAsync(id);
            if (category == null) return Json(new { success = false, message = "Không tìm thấy danh mục!" });

            // Kiểm tra xem danh mục có hoạt động không
            var hasEvents = await _categoryRepo.CategoryHasEventsAsync(id);
            if (hasEvents)
                return Json(new { success = false, message = "Không thể xóa! Danh mục đang có hoạt động." });

            await _categoryRepo.DeleteCategoryAsync(id);
            return Json(new { success = true, message = $"Đã xóa danh mục \"{category.Name}\"!" });
        }
    }
}