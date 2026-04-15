using ISpanShop.Models.EfModels;
using ISpanShop.MVC.Areas.Admin.Models.ContentModeration;
using Microsoft.AspNetCore.Mvc;
using ISpanShop.Repositories.ContentModeration;
using ISpanShop.Services.ContentModeration;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using ISpanShop.MVC.Middleware;

namespace ISpanShop.MVC.Areas.Admin.Controllers.ContentModeration
{
    [Area("Admin")]
    [RequirePermission("sensitive_manage")]
    public class SensitiveWordCategoriesController : Controller
    {
        private readonly ISpanShopDBContext _context;

        public SensitiveWordCategoriesController(ISpanShopDBContext context)
        {
            _context = context;
        }

        // --- 1. 列表 (支援分頁) ---
        [HttpGet]
        public async Task<IActionResult> Index(int page = 1)
        {
            int pageSize = 10;
            var query = _context.SensitiveWordCategories
                .Include(c => c.SensitiveWords);

            // 計算分頁資訊
            int totalCount = await query.CountAsync();
            int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            // 確保當前頁數在合理範圍內
            page = page < 1 ? 1 : page;
            if (totalPages > 0 && page > totalPages) page = totalPages;

            // 進行分頁切割 (依照 ID 排序)
            var pagedCategories = await query
                .OrderBy(c => c.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new SensitiveWordCategoryVm
                {
                    Id = c.Id,
                    Name = c.Name,
                    WordCount = c.SensitiveWords.Count
                })
                .ToListAsync();

            // 將分頁資訊傳給 View
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(pagedCategories);
        }

        // --- 2. 新增 (GET/POST) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SensitiveWordCategoryVm vm)
        {
            if (ModelState.IsValid)
            {
                var category = new SensitiveWordCategory
                {
                    Name = vm.Name
                };
                _context.SensitiveWordCategories.Add(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return RedirectToAction(nameof(Index));
        }

        // --- 3. 編輯 (POST) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SensitiveWordCategoryVm vm)
        {
            if (ModelState.IsValid)
            {
                var category = await _context.SensitiveWordCategories.FindAsync(vm.Id);
                if (category == null) return NotFound();

                category.Name = vm.Name;
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return RedirectToAction(nameof(Index));
        }

        // --- 4. 刪除 (POST) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _context.SensitiveWordCategories.FindAsync(id);
            if (category == null) return NotFound();

            // 檢查是否還有敏感字屬於此分類
            bool hasWords = await _context.SensitiveWords.AnyAsync(w => w.CategoryId == id);
            if (hasWords)
            {
                TempData["ErrorMessage"] = "無法刪除：此分類下仍有敏感字。";
                return RedirectToAction(nameof(Index));
            }

            _context.SensitiveWordCategories.Remove(category);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}