using ISpanShop.Models.DTOs;
using ISpanShop.Services.Interfaces;
using ISpanShop.WebAPI.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace ISpanShop.WebAPI.Controllers
{
    /// <summary>
    /// 賣家庫存管理 API
    /// </summary>
    [ApiController]
    [Route("api/seller/inventory")]
    [Produces("application/json")]
    public class SellerInventoryController : ControllerBase
    {
        private readonly IInventoryService _inventoryService;

        public SellerInventoryController(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        // ────────────────────────────────────────────────────
        // GET api/seller/inventory
        // ────────────────────────────────────────────────────

        /// <summary>
        /// 取得分頁庫存列表，支援關鍵字、分類、狀態、庫存範圍、排序篩選
        /// </summary>
        /// <param name="keyword">搜尋商品名稱、規格名稱或 SKU</param>
        /// <param name="categoryId">分類 ID</param>
        /// <param name="status">all（預設）/ low（低庫存）/ outOfStock（零庫存）</param>
        /// <param name="stockMin">庫存下限</param>
        /// <param name="stockMax">庫存上限</param>
        /// <param name="sortBy">stock_asc | stock_desc | name_asc | safetyStock</param>
        /// <param name="page">頁碼（從 1 開始）</param>
        /// <param name="pageSize">每頁筆數（1–100，預設 20）</param>
        /// <param name="sellerId">賣家 ID（預留，目前不過濾）</param>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResultDto<InventoryItemDto>), StatusCodes.Status200OK)]
        public ActionResult<PagedResultDto<InventoryItemDto>> GetList(
            [FromQuery] string? keyword    = null,
            [FromQuery] int?    categoryId = null,
            [FromQuery] string? status     = null,
            [FromQuery] int?    stockMin   = null,
            [FromQuery] int?    stockMax   = null,
            [FromQuery] string? sortBy     = null,
            [FromQuery] int     page       = 1,
            [FromQuery] int     pageSize   = 20,
            [FromQuery] int?    sellerId   = null   // 預留：身份驗證後可依此過濾
        )
        {
            var criteria = new InventorySearchCriteria
            {
                Keyword     = keyword,
                CategoryId  = categoryId,
                StockStatus = MapStatus(status),
                MinStock    = stockMin,
                MaxStock    = stockMax,
                SortBy      = MapSortBy(sortBy),
                PageNumber  = page < 1 ? 1 : page,
                PageSize    = pageSize is < 1 or > 100 ? 20 : pageSize
            };

            var result = _inventoryService.GetInventoryPaged(criteria);

            return Ok(new PagedResultDto<InventoryItemDto>
            {
                Items      = result.Data.Select(ToItemDto).ToList(),
                TotalCount = result.TotalCount,
                Page       = result.CurrentPage,
                PageSize   = result.PageSize,
                TotalPages = result.TotalPages
            });
        }

        // ────────────────────────────────────────────────────
        // GET api/seller/inventory/summary
        // ────────────────────────────────────────────────────

        /// <summary>取得庫存統計摘要（總規格數、低庫存、零庫存、正常）</summary>
        [HttpGet("summary")]
        [ProducesResponseType(typeof(InventorySummaryDto), StatusCodes.Status200OK)]
        public ActionResult<InventorySummaryDto> GetSummary(
            [FromQuery] int? sellerId = null)  // 預留
        {
            var total  = _inventoryService.GetTotalVariantCount();
            var low    = _inventoryService.GetLowStockCount();
            var zero   = _inventoryService.GetZeroStockCount();
            var normal = Math.Max(0, total - low);

            return Ok(new InventorySummaryDto
            {
                TotalVariants  = total,
                LowStockCount  = low,
                ZeroStockCount = zero,
                NormalCount    = normal
            });
        }

        // ────────────────────────────────────────────────────
        // GET api/seller/inventory/{variantId}
        // ────────────────────────────────────────────────────

        /// <summary>取得單一規格庫存詳情</summary>
        [HttpGet("{variantId:int}")]
        [ProducesResponseType(typeof(InventoryDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<InventoryDetailDto> GetDetail(int variantId)
        {
            var dto = _inventoryService.GetVariantDetail(variantId);
            if (dto == null)
                return NotFound(new { message = $"找不到規格 ID {variantId}" });

            return Ok(ToDetailDto(dto));
        }

        // ────────────────────────────────────────────────────
        // PUT api/seller/inventory/{variantId}/stock
        // ────────────────────────────────────────────────────

        /// <summary>調整規格現有庫存</summary>
        [HttpPut("{variantId:int}/stock")]
        [ProducesResponseType(typeof(InventoryDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<InventoryDetailDto> UpdateStock(
            int variantId,
            [FromBody] UpdateStockRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (_inventoryService.GetVariantDetail(variantId) is null)
                return NotFound(new { message = $"找不到規格 ID {variantId}" });

            _inventoryService.AdjustStock(variantId, request.Stock);

            return Ok(ToDetailDto(_inventoryService.GetVariantDetail(variantId)!));
        }

        // ────────────────────────────────────────────────────
        // PUT api/seller/inventory/{variantId}/safety-stock
        // ────────────────────────────────────────────────────

        /// <summary>調整規格安全庫存</summary>
        [HttpPut("{variantId:int}/safety-stock")]
        [ProducesResponseType(typeof(InventoryDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<InventoryDetailDto> UpdateSafetyStock(
            int variantId,
            [FromBody] UpdateSafetyStockRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (_inventoryService.GetVariantDetail(variantId) is null)
                return NotFound(new { message = $"找不到規格 ID {variantId}" });

            _inventoryService.UpdateSafetyStock(variantId, request.SafetyStock);

            return Ok(ToDetailDto(_inventoryService.GetVariantDetail(variantId)!));
        }

        // ────────────────────────────────────────────────────
        // PUT api/seller/inventory/{variantId}
        // ────────────────────────────────────────────────────

        /// <summary>同時調整庫存與安全庫存</summary>
        [HttpPut("{variantId:int}")]
        [ProducesResponseType(typeof(InventoryDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<InventoryDetailDto> UpdateInventory(
            int variantId,
            [FromBody] UpdateInventoryRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (_inventoryService.GetVariantDetail(variantId) is null)
                return NotFound(new { message = $"找不到規格 ID {variantId}" });

            _inventoryService.UpdateStockAndSafetyStock(variantId, request.Stock, request.SafetyStock);

            return Ok(ToDetailDto(_inventoryService.GetVariantDetail(variantId)!));
        }

        // ════════════════════════════════════════════════════
        // Private helpers
        // ════════════════════════════════════════════════════

        /// <summary>前台 status 字串 → 後端 StockStatus（InventorySearchCriteria 用）</summary>
        private static string MapStatus(string? status) => status switch
        {
            "low"        => "low",
            "outOfStock" => "zero",
            _            => ""
        };

        /// <summary>前台 sortBy → 後端 SortBy</summary>
        private static string MapSortBy(string? sortBy) => sortBy switch
        {
            "stock_asc"   => "stock_asc",
            "stock_desc"  => "stock_desc",
            "name_asc"    => "name_asc",
            "safetyStock" => "safety_asc",
            _             => ""
        };

        /// <summary>後端 InventoryListDto → 前台 status 字串</summary>
        private static string ResolveStatus(InventoryListDto dto)
            => dto.IsZeroStock ? "outOfStock"
             : dto.IsLowStock  ? "low"
             : "normal";

        private static InventoryItemDto ToItemDto(InventoryListDto dto) => new()
        {
            VariantId    = dto.VariantId,
            ProductId    = dto.ProductId,
            ProductName  = dto.ProductName,
            VariantName  = dto.VariantName,
            SkuCode      = dto.SkuCode,
            CategoryName = dto.CategoryName,
            Stock        = dto.Stock,
            SafetyStock  = dto.SafetyStock,
            Status       = ResolveStatus(dto)
        };

        private static InventoryDetailDto ToDetailDto(InventoryListDto dto) => new()
        {
            VariantId    = dto.VariantId,
            ProductId    = dto.ProductId,
            ProductName  = dto.ProductName,
            VariantName  = dto.VariantName,
            SkuCode      = dto.SkuCode,
            CategoryName = dto.CategoryName,
            StoreName    = dto.StoreName,
            Stock        = dto.Stock,
            SafetyStock  = dto.SafetyStock,
            Status       = ResolveStatus(dto)
        };
    }
}
