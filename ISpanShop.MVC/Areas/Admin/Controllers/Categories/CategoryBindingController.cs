using Microsoft.AspNetCore.Mvc;
using ISpanShop.MVC.Areas.Admin.Controllers;
using ISpanShop.MVC.Middleware;
using ISpanShop.Services.Products;
using ISpanShop.Services.Categories;
using ISpanShop.Services.Inventories;
using ISpanShop.Models.DTOs.Categories;

namespace ISpanShop.MVC.Areas.Admin.Controllers.Categories
{
    [RequirePermission("product_manage")]
    public class CategoryBindingController : AdminBaseController
    {
        private readonly CategoryAttributeService _attributeService;

        public CategoryBindingController(CategoryAttributeService attributeService)
        {
            _attributeService = attributeService;
        }

        // 首頁：只渲染分類列表，規格用 AJAX 載入
        public IActionResult Index()
        {
            var categories = _attributeService.GetAllCategories();
            return View(categories);
        }

        // AJAX：取得某分類的所有規格（含是否已綁定）
        [HttpGet]
        public IActionResult GetSpecsForCategory(int categoryId)
        {
            var allSpecs = _attributeService.GetAll();
            var boundIds = _attributeService.GetBoundAttributeIds(categoryId);
            var categoryName = _attributeService.GetAllCategories()
                .FirstOrDefault(c => c.Id == categoryId)?.Name ?? "";

            var result = allSpecs.Select(s => new {
                attributeId  = s.Id,
                attributeName = s.Name,
                inputType    = s.InputType,
                isRequired   = s.IsRequired,
                isBound      = boundIds.Contains(s.Id),
                options      = s.Options
            });

            return Json(new { categoryName, specs = result });
        }

        // AJAX：儲存分類規格綁定（先取得現有綁定，再逐一 Toggle 差異）
        [HttpPost]
        public IActionResult SaveBindings([FromBody] SaveBindingsDto dto)
        {
            if (dto == null) return BadRequest(new { success = false });

            var newIds     = dto.AttributeIds ?? new System.Collections.Generic.List<int>();
            var currentIds = _attributeService.GetBoundAttributeIds(dto.CategoryId);

            foreach (var id in newIds.Except(currentIds))
                _attributeService.ToggleBinding(dto.CategoryId, id, true);

            foreach (var id in currentIds.Except(newIds))
                _attributeService.ToggleBinding(dto.CategoryId, id, false);

            return Json(new { success = true });
        }
    }
}
