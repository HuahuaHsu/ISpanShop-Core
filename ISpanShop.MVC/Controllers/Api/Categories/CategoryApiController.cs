using ISpanShop.MVC.Models.Dto;
using ISpanShop.Services.Categories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ISpanShop.MVC.Controllers.Api.Categories
{
    /// <summary>前台分類 API（唯讀，不需登入）</summary>
    [ApiController]
    [Route("api/categories")]
    [Produces("application/json")]
    public class CategoryApiController : ControllerBase
    {
        private readonly CategoryManageService _categorySvc;

        public CategoryApiController(CategoryManageService categorySvc)
        {
            _categorySvc = categorySvc;
        }

        // ──────────────────────────────────────────────────────────
        // GET /api/categories
        // 回傳啟用中的主分類（ParentId == null, IsVisible == true）
        // ──────────────────────────────────────────────────────────

        /// <summary>
        /// 取得啟用中的主分類列表，含每個主分類底下的上架商品數（含所有子分類）
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _categorySvc.GetMainCategoriesAsync();

            var items = categories.Select(c => new CategoryListItemDto
            {
                Id           = c.Id,
                Name         = c.Name,
                ParentId     = null,    // 主分類
                IconUrl      = string.IsNullOrEmpty(c.ImageUrl) ? null : c.ImageUrl,
                SortOrder    = c.SortOrder,
                ProductCount = c.ProductCount
            }).ToList();

            return Ok(new
            {
                success = true,
                data    = items,
                message = ""
            });
        }

        // ──────────────────────────────────────────────────────────
        // GET /api/categories/{id}/children
        // 回傳指定主分類底下的子分類清單
        // ──────────────────────────────────────────────────────────

        /// <summary>
        /// 取得指定主分類底下的子分類列表，含各子分類的上架商品數
        /// </summary>
        /// <param name="id">主分類 Id</param>
        [HttpGet("{id:int}/children")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetChildren(int id)
        {
            var children = await _categorySvc.GetChildCategoriesAsync(id);

            var items = children.Select(c => new CategoryListItemDto
            {
                Id           = c.Id,
                Name         = c.Name,
                ParentId     = c.ParentId,
                IconUrl      = string.IsNullOrEmpty(c.ImageUrl) ? null : c.ImageUrl,
                SortOrder    = c.SortOrder,
                ProductCount = c.ProductCount
            }).ToList();

            return Ok(new
            {
                success = true,
                data    = items,
                message = ""
            });
        }
    }
}
