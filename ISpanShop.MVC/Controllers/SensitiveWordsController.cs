using ISpanShop.Models.DTOs;
using ISpanShop.Models.EfModels;
using ISpanShop.MVC.Models.ViewModels;
using ISpanShop.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering; // 修正 1: 解決 SelectList 找不到的問題
using Microsoft.EntityFrameworkCore;    // 修正 2: 解決 DbContext 相關擴充方法

namespace ISpanShop.MVC.Controllers
{
	public class SensitiveWordsController : Controller
	{
		private readonly ISensitiveWordService _service;
		private readonly ISpanShopDBContext _context; // 修正 3: 宣告 _context 才能使用

		public SensitiveWordsController(ISensitiveWordService service, ISpanShopDBContext context)
		{
			_service = service;
			_context = context; // 修正 4: 注入資料庫上下文
		}

		// --- 1. 列表 (Index) ---
		public async Task<IActionResult> Index()
		{
			var dtos = await _service.GetAllAsync();
			var vms = dtos.Select(d => new SensitiveWordVm
			{
				Id = d.Id,
				Word = d.Word,
				Category = d.Category,
				IsActive = d.IsActive
			}).ToList();
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