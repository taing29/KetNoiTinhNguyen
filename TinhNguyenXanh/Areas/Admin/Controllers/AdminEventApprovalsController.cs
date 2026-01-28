using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TinhNguyenXanh.Interfaces;
using TinhNguyenXanh.Models;

namespace TinhNguyenXanh.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")] 
    public class AdminEventApprovalsController : Controller
    {
        private readonly IEventRepository _eventRepository;

        public AdminEventApprovalsController(IEventRepository eventRepository)
        {
            _eventRepository = eventRepository;
        }

        public async Task<IActionResult> Index(string search = "", int page = 1)
        {
            int pageSize = 10;
            var query = _eventRepository.GetAllEventsAsync().Result
                .Where(e => e.Status == "draft" || e.Status == "pending");

            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                query = query.Where(e =>
                    e.Title.Contains(search) ||
                    (e.Organization != null && e.Organization.Name.Contains(search)) ||
                    (e.Description != null && e.Description.Contains(search))
                );
            }

            var totalItems = query.Count();
            var items = query
                .OrderByDescending(e => e.StartTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.Search = search;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            return View(items);
        }

        // POST: Duyệt hoặc từ chối
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string actionType)
        {
            var evt = await _eventRepository.GetEventByIdAsync(id);
            if (evt == null)
                return NotFound();

            if (actionType == "approve")
            {
                evt.Status = "approved";
            }
            else if (actionType == "reject")
            {
                evt.Status = "rejected";
            }
            else
            {
                return BadRequest();
            }

            await _eventRepository.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Hoạt động \"{evt.Title}\" đã được {(actionType == "approve" ? "được duyệt" : "bị từ chối")} thành công!";

            return RedirectToAction(nameof(Index));
        }
    }
}