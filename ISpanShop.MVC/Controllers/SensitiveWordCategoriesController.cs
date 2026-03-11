using ISpanShop.Models.EfModels;
using ISpanShop.Models.EFModels;
using ISpanShop.MVC.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace ISpanShop.MVC.Controllers
{
    public class SensitiveWordCategoriesController : Controller
    {
        private readonly ISpanShopDBContext _context;

        public SensitiveWordCategoriesController(ISpanShopDBContext context)
        {
            _context = context;
        }

        // --- 1. 列表 ---
        public async Task<IActionResult> Index()
        {
            var categories = await _context.SensitiveWordCategories
                .Include(c => c.SensitiveWords)
                .Select(c => new SensitiveWordCategoryVm
                {
                    Id = c.Id,
                    Name = c.Name,
                    WordCount = c.SensitiveWords.Count
                })
                .ToListAsync();

            return View(categories);
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