using ISpanShop.Models.DTOs.ContentModeration;
using ISpanShop.Models.EfModels;
using ISpanShop.MVC.Areas.Admin.Models.ContentModeration;
using Microsoft.AspNetCore.Mvc;
using ISpanShop.Repositories.ContentModeration;
using ISpanShop.Services.ContentModeration;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ISpanShop.MVC.Middleware;

namespace ISpanShop.MVC.Areas.Admin.Controllers.ContentModeration
{
	[Area("Admin")]
	[RequirePermission("sensitive_manage")]
	public class SensitiveWordsController : Controller
	{
		private readonly ISensitiveWordService _service;
		private readonly ISpanShopDBContext _context; // 修正 3: 宣告 _context 才能使用

		public SensitiveWordsController(ISensitiveWordService service, ISpanShopDBContext context)
		{
			_service = service;
			_context = context; // 修正 4: 注入資料庫上下文
		}

		// --- 1. 列表 (Index) (支援分頁) ---
		[HttpGet]
		public async Task<IActionResult> Index(int page = 1)
		{
			int pageSize = 10;
			var dtos = await _service.GetAllAsync();

			// 計算分頁資訊
			int totalCount = dtos.Count;
			int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

			// 確保當前頁數在合理範圍內
			page = page < 1 ? 1 : page;
			if (totalPages > 0 && page > totalPages) page = totalPages;

			// 進行分頁切割 (依照 ID 排序)
			var pagedDtos = dtos.OrderBy(d => d.Id)
							   .Skip((page - 1) * pageSize)
							   .Take(pageSize)
							   .ToList();

			var vms = pagedDtos.Select(d => new SensitiveWordVm
			{
				Id = d.Id,
				Word = d.Word,
				Category = d.Category,
				IsActive = d.IsActive
			}).ToList();

			// 將分頁資訊傳給 View
			ViewBag.CurrentPage = page;
			ViewBag.TotalPages = totalPages;

			return View(vms);
		}

		// --- 2. 顯示新增頁面 (GET) ---
		[HttpGet]
		public async Task<IActionResult> Create()
		{
			// 動態從資料庫抓取分類
			var categories = await _context.SensitiveWordCategories
							 .Select(c => new { c.Id, c.Name })
							 .ToListAsync();

			ViewBag.CategoryId = new SelectList(categories, "Id", "Name");
			return View();
		}

		// --- 3. 接收表單資料 (POST) ---
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(SensitiveWordVm vm)
		{
			if (ModelState.IsValid)
			{
				var dto = new SensitiveWordDto
				{
					Word = vm.Word,
					CategoryId = vm.CategoryId, // 修正 5: 存入 ID 而非字串
					IsActive = vm.IsActive
				};
				await _service.CreateAsync(dto);
				return RedirectToAction(nameof(Index));
			}

			// 如果驗證失敗，要重新填寫下拉選單資料
			var categories = await _context.SensitiveWordCategories.ToListAsync();
			ViewBag.CategoryId = new SelectList(categories, "Id", "Name", vm.CategoryId);
			return View(vm);
		}

		// --- 4. 顯示編輯頁面 (GET) ---
		[HttpGet]
		public async Task<IActionResult> Edit(int id)
		{
			var dto = await _service.GetByIdAsync(id);
			if (dto == null) return NotFound();

			var vm = new SensitiveWordVm
			{
				Id = dto.Id,
				Word = dto.Word,
				CategoryId = dto.CategoryId, // 修正 6: 取得 ID
				IsActive = dto.IsActive
			};

			// 編輯頁面也要下拉選單
			var categories = await _context.SensitiveWordCategories.ToListAsync();
			ViewBag.CategoryId = new SelectList(categories, "Id", "Name", vm.CategoryId);

			return View(vm);
		}

		// --- 5. 接收編輯後的資料 (POST) ---
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(SensitiveWordVm vm)
		{
			if (ModelState.IsValid)
			{
				var dto = new SensitiveWordDto
				{
					Id = vm.Id,
					Word = vm.Word,
					CategoryId = vm.CategoryId,
					IsActive = vm.IsActive
				};
				await _service.UpdateAsync(dto);
				return RedirectToAction(nameof(Index));
			}

			var categories = await _context.SensitiveWordCategories.ToListAsync();
			ViewBag.CategoryId = new SelectList(categories, "Id", "Name", vm.CategoryId);
			return View(vm);
		}

		// --- 6. 刪除功能 ---
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Delete(int id)
		{
			await _service.DeleteAsync(id);
			return RedirectToAction(nameof(Index));
		}
	}
}