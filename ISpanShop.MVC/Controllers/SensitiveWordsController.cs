using ISpanShop.Models.DTOs;
using ISpanShop.Models.EfModels;
using ISpanShop.MVC.Models.ViewModels;
using ISpanShop.Services;
using Microsoft.AspNetCore.Mvc;

namespace ISpanShop.MVC.Controllers
{
	public class SensitiveWordsController : Controller
	{
		private readonly ISensitiveWordService _service;

		public SensitiveWordsController(ISensitiveWordService service)
		{
			_service = service;
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
		// 這裡通常加在 Index 之後
		[HttpGet]
		public IActionResult Create()
		{
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
					Category = vm.Category,
					IsActive = vm.IsActive
				};
				await _service.CreateAsync(dto);
				return RedirectToAction(nameof(Index));
			}
			return View(vm);
		}
		// <-- 確保所有 Action 都在這個大括號裡面
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
				Category = dto.Category,
				IsActive = dto.IsActive
			};
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
					Category = vm.Category,
					IsActive = vm.IsActive
				};
				await _service.UpdateAsync(dto);
				return RedirectToAction(nameof(Index));
			}
			return View(vm);
		}

		// --- 6. 刪除功能 (通常直接 POST 即可) ---
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Delete(int id)
		{
			await _service.DeleteAsync(id);
			return RedirectToAction(nameof(Index));
		}
	}
}