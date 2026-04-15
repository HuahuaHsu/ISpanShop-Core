using ISpanShop.Models.DTOs.Categories;
using ISpanShop.MVC.Areas.Admin.Models.Products;
using ISpanShop.MVC.Areas.Admin.Models.Categories;
using ISpanShop.Services.Products;
using ISpanShop.Services.Categories;
using ISpanShop.Services.Inventories;
using Microsoft.AspNetCore.Mvc;
using ISpanShop.MVC.Areas.Admin.Controllers;
using ISpanShop.MVC.Middleware;
using System.Collections.Generic;

namespace ISpanShop.MVC.Areas.Admin.Controllers.Categories
{
    public class CategoryAttributesController : AdminBaseController
    {
        private readonly CategoryAttributeService _categoryAttributeService;

        public CategoryAttributesController(CategoryAttributeService categoryAttributeService)
        {
            _categoryAttributeService = categoryAttributeService;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            const int pageSize = 10;
            var pagedSpecs = await _categoryAttributeService.GetPagedAsync(page, pageSize);
            ViewBag.Categories    = _categoryAttributeService.GetAllCategories();
            ViewBag.AllSpecsForJs = _categoryAttributeService.GetAll();
            ViewBag.BindingPairs  = _categoryAttributeService.GetAllBindingPairs();
            return View(pagedSpecs);
        }

        /// <summary>
        /// AJAX：取得某分類已綁定的規格 ID 清單
        /// </summary>
        [HttpGet]
        public IActionResult GetBoundSpecIds(int categoryId)
        {
            var ids = _categoryAttributeService.GetBoundAttributeIds(categoryId);
            return Json(ids);
        }

        /// <summary>
        /// AJAX：取得某分類已綁定的規格列表（含 IsFilterable）
        /// </summary>
        [HttpGet]
        public IActionResult GetBoundSpecItems(int categoryId)
        {
            var items = _categoryAttributeService.GetBoundSpecItems(categoryId);
            return Json(items);
        }

        /// <summary>
        /// AJAX：取得矩陣頁所需的全部資料（規格、分類、全部綁定）
        /// </summary>
        [HttpGet]
        public IActionResult GetMatrixData()
        {
            var allCats  = _categoryAttributeService.GetAllCategories().ToList();
            var parents  = allCats.Where(c => c.ParentId == null)
                .Select(c => new { id = c.Id, name = c.Name });
            var children = allCats.Where(c => c.ParentId != null)
                .Select(c => new { id = c.Id, name = c.Name, parentId = c.ParentId });

            var specs = _categoryAttributeService.GetAll()
                .Select(s => new {
                    id         = s.Id,
                    name       = s.Name,
                    inputType  = s.InputType,
                    isRequired = s.IsRequired,
                    options    = s.Options
                });

            var bindings = _categoryAttributeService.GetAllBindingPairs();

            return Json(new {
                specs,
                categories       = children,
                parentCategories = parents,
                bindings
            });
        }

        /// <summary>
        /// AJAX：切換單一規格綁定（即時儲存）
        /// </summary>
        [HttpPost]
        public IActionResult ToggleSpec([FromBody] ToggleSpecDto dto)
        {
            if (dto == null) return BadRequest(new { success = false });
            _categoryAttributeService.ToggleBinding(dto.CategoryId, dto.AttributeId, dto.IsBound);
            return Json(new { success = true });
        }

        /// <summary>
        /// AJAX：切換 IsFilterable
        /// </summary>
        [HttpPost]
        public IActionResult ToggleFilterable([FromBody] ToggleFilterableDto dto)
        {
            if (dto == null) return BadRequest();
            _categoryAttributeService.ToggleFilterable(dto.CategoryId, dto.SpecId, dto.IsFilterable);
            return Json(new { success = true });
        }

        public IActionResult Create()
        {
            return View(new CategorySpecCreateVm());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CategorySpecCreateVm vm)
        {
            if (!ModelState.IsValid) return View(vm);

            _categoryAttributeService.Create(
                vm.Name,
                vm.InputType,
                vm.IsRequired,
                vm.AllowCustomInput,
                vm.SortOrder,
                vm.Options
            );

            TempData["SuccessMessage"] = "分類屬性新增成功！";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            var dto = _categoryAttributeService.GetById(id);
            if (dto == null) return NotFound();

            var vm = new CategorySpecEditVm
            {
                Id               = dto.Id,
                Name             = dto.Name,
                InputType        = dto.InputType,
                IsRequired       = dto.IsRequired,
                AllowCustomInput = dto.AllowCustomInput,
                SortOrder        = dto.SortOrder,
                Options          = dto.Options
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(CategorySpecEditVm vm)
        {
            if (!ModelState.IsValid) return View(vm);

            _categoryAttributeService.Update(
                vm.Id,
                vm.Name,
                vm.InputType,
                vm.IsRequired,
                vm.AllowCustomInput,
                vm.SortOrder,
                vm.Options
            );

            TempData["SuccessMessage"] = "分類屬性更新成功！";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            _categoryAttributeService.Delete(id);
            TempData["SuccessMessage"] = "分類屬性已刪除！";
            return RedirectToAction(nameof(Index));
        }

        // ──────────────────────────────────────────────────────────
        //  新版綁定管理 AJAX 端點
        // ──────────────────────────────────────────────────────────

        /// <summary>取得分類已綁定屬性（含名稱、選項、排序），新版 UI 使用</summary>
        [HttpGet]
        public IActionResult GetBoundSpecs(int categoryId)
        {
            var items = _categoryAttributeService.GetBoundSpecsWithDetails(categoryId);
            return Json(items);
        }

        /// <summary>綁定屬性到分類</summary>
        [HttpPost]
        public IActionResult BindSpec([FromBody] BindSpecDto dto)
        {
            if (dto == null) return BadRequest();
            _categoryAttributeService.BindSpec(dto.CategoryId, dto.SpecId);
            return Json(new { success = true });
        }

        /// <summary>解除分類與屬性的綁定</summary>
        [HttpPost]
        public IActionResult UnbindSpec([FromBody] BindSpecDto dto)
        {
            if (dto == null) return BadRequest();
            _categoryAttributeService.UnbindSpec(dto.CategoryId, dto.SpecId);
            return Json(new { success = true });
        }

        /// <summary>更新分類中屬性的排序順序</summary>
        [HttpPost]
        public IActionResult UpdateBindingSort([FromBody] UpdateBindingSortDto dto)
        {
            if (dto == null) return BadRequest();
            _categoryAttributeService.UpdateBindingSort(dto.CategoryId, dto.OrderedSpecIds);
            return Json(new { success = true });
        }

        /// <summary>AJAX：新增屬性到屬性庫（Modal 用）</summary>
        [HttpPost]
        public IActionResult CreateAjax([FromBody] CreateSpecAjaxDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Name))
                return Json(new { success = false, message = "屬性名稱為必填" });

            _categoryAttributeService.Create(dto.Name, dto.InputType, dto.IsRequired,
                                        dto.AllowCustomInput, dto.SortOrder, dto.Options ?? new List<string>());

            var newSpec = _categoryAttributeService.GetAll()
                .Where(s => s.Name == dto.Name)
                .OrderByDescending(s => s.Id)
                .FirstOrDefault();

            return Json(new { success = true, spec = newSpec });
        }

        /// <summary>AJAX：更新屬性（Modal 用）</summary>
        [HttpPost]
        public IActionResult EditAjax([FromBody] EditSpecAjaxDto dto)
        {
            if (dto == null) return Json(new { success = false });

            _categoryAttributeService.Update(dto.Id, dto.Name, dto.InputType, dto.IsRequired,
                                        dto.AllowCustomInput, dto.SortOrder, dto.Options ?? new List<string>());
            return Json(new { success = true });
        }

        /// <summary>AJAX：刪除屬性（先檢查是否有分類在使用）</summary>
        [HttpPost]
        public IActionResult DeleteAjax([FromBody] IdDto dto)
        {
            if (dto == null) return Json(new { success = false });

            if (_categoryAttributeService.HasBindings(dto.Id))
                return Json(new { success = false, message = "此屬性已綁定至分類，請先解除所有綁定才能刪除。" });

            _categoryAttributeService.Delete(dto.Id);
            return Json(new { success = true });
        }
    }
}
