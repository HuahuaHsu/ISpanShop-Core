using ISpanShop.Models.EfModels;
using ISpanShop.WebAPI.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ISpanShop.WebAPI.Controllers
{
    /// <summary>分類 API（唯讀）</summary>
    [ApiController]
    [Route("api/categories")]
    [Produces("application/json")]
    public class CategoriesController : ControllerBase
    {
        private readonly ISpanShopDBContext _context;

        public CategoriesController(ISpanShopDBContext context)
        {
            _context = context;
        }

        // ──────────────────────────────────────────────────────────
        // GET api/categories/{categoryId}/attributes
        // 賣家選擇分類後，取得該分類綁定的規格屬性列表
        // ──────────────────────────────────────────────────────────
        /// <summary>
        /// 取得分類綁定的規格屬性（賣家建立/編輯商品時使用）
        /// </summary>
        /// <param name="categoryId">子分類 ID</param>
        /// <returns>屬性列表（含名稱、輸入類型、選項、是否必填）</returns>
        [HttpGet("{categoryId:int}/attributes")]
        [ProducesResponseType(typeof(List<CategoryAttributeResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<List<CategoryAttributeResponse>> GetAttributes(int categoryId)
        {
            // 確認分類存在
            var categoryExists = _context.Categories.Any(c => c.Id == categoryId);
            if (!categoryExists)
                return NotFound(new { message = "分類不存在" });

            var mappings = _context.CategorySpecMappings
                .Where(m => m.CategoryId == categoryId && m.CategorySpec.IsActive)
                .Include(m => m.CategorySpec)
                    .ThenInclude(s => s.CategorySpecOptions)
                .OrderBy(m => m.CategorySpec.SortOrder)
                .ToList();

            var result = mappings.Select(m => new CategoryAttributeResponse
            {
                Id           = m.CategorySpec.Id,
                Name         = m.CategorySpec.Name,
                InputType    = m.CategorySpec.InputType,
                IsRequired   = m.CategorySpec.IsRequired,
                IsFilterable = m.IsFilterable,
                SortOrder    = m.CategorySpec.SortOrder,
                Options      = m.CategorySpec.CategorySpecOptions
                    .OrderBy(o => o.SortOrder)
                    .Select(o => o.OptionName)
                    .ToList()
            }).ToList();

            return Ok(result);
        }
    }
}
