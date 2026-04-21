using ISpanShop.Models.DTOs.Categories;
using ISpanShop.Models.EfModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ISpanShop.MVC.Controllers.Api.Categories
{
    /// <summary>分類 API（唯讀）</summary>
    [ApiController]
    [Route("api/categories")]
    [Produces("application/json")]
    public class CategoriesApiController : ControllerBase
    {
        private readonly ISpanShopDBContext _context;

        public CategoriesApiController(ISpanShopDBContext context)
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
        [ProducesResponseType(typeof(ApiResponse<List<CategoryAttributeResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<ApiResponse<List<CategoryAttributeResponse>>> GetAttributes(int categoryId)
        {
            var categoryExists = _context.Categories.Any(c => c.Id == categoryId);
            if (!categoryExists)
                return NotFound(new ApiResponse<List<CategoryAttributeResponse>>
                {
                    Success = false,
                    Data = null,
                    Message = "分類不存在"
                });

            var mappings = _context.CategoryAttributeMappings
                .Where(m => m.CategoryId == categoryId && m.CategoryAttribute.IsActive)
                .Include(m => m.CategoryAttribute)
                    .ThenInclude(s => s.CategoryAttributeOptions)
                .OrderBy(m => m.Sort)
                .ToList();

            var result = mappings.Select(m => new CategoryAttributeResponse
            {
                Id           = m.CategoryAttribute.Id,
                Name         = m.CategoryAttribute.Name,
                InputType    = m.CategoryAttribute.InputType,
                IsRequired   = m.CategoryAttribute.IsRequired,
                IsFilterable = m.IsFilterable,
                IsMultiple   = m.CategoryAttribute.InputType == "checkbox",
                MaxSelect    = m.CategoryAttribute.InputType == "checkbox" ? (int?)null : null,
                AllowCustom  = m.CategoryAttribute.AllowCustomInput,
                Options      = m.CategoryAttribute.CategoryAttributeOptions
                    .OrderBy(o => o.SortOrder)
                    .Select(o => new AttributeOptionDto
                    {
                        Id = o.Id,
                        Value = o.OptionName
                    })
                    .ToList()
            }).ToList();

            return Ok(new ApiResponse<List<CategoryAttributeResponse>>
            {
                Success = true,
                Data = result,
                Message = ""
            });
        }
    }
}
