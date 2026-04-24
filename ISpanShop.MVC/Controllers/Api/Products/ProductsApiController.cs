using ISpanShop.Services.Products;
using ISpanShop.MVC.Models.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace ISpanShop.MVC.Controllers.Api.Products
{
    /// <summary>
    /// 前台商品公開 API（GET 端點不需登入）
    /// </summary>
    [ApiController]
    [Route("api/products")]
    [Produces("application/json")]
    public class ProductsApiController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ILogger<ProductsApiController> _logger;

        public ProductsApiController(IProductService productService, ILogger<ProductsApiController> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        // ──────────────────────────────────────────────────────────
        // GET /api/products
        // 前台商品總覽，類似蝦皮「每日新發現」商品牆
        // ──────────────────────────────────────────────────────────

        /// <summary>
        /// 取得上架中的商品列表（分類篩選、關鍵字搜尋、品牌、價格區間、排序、分頁）
        /// </summary>
        /// <param name="categoryId">主分類篩選（可選，自動展開子分類）</param>
        /// <param name="subCategoryId">子分類篩選（優先於 categoryId，直接篩）</param>
        /// <param name="brandIds">品牌篩選（可傳多個，e.g. ?brandIds=1&amp;brandIds=2）</param>
        /// <param name="minPrice">最低價格（以 MinPrice 比較，可選）</param>
        /// <param name="maxPrice">最高價格（以 MinPrice 比較，可選）</param>
        /// <param name="keyword">關鍵字搜尋商品名稱（可選）</param>
        /// <param name="sortBy">排序：latest / priceAsc / priceDesc / soldCount（預設 latest）</param>
        /// <param name="page">頁碼（預設 1）</param>
        /// <param name="pageSize">每頁筆數（預設 20，上限 50）</param>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetProducts(
            [FromQuery] int?     categoryId    = null,
            [FromQuery] int?     subCategoryId = null,
            [FromQuery] int[]?   brandIds      = null,
            [FromQuery] decimal? minPrice      = null,
            [FromQuery] decimal? maxPrice      = null,
            [FromQuery] string?  keyword       = null,
            [FromQuery] string?  sortBy        = null,
            [FromQuery] int      page          = 1,
            [FromQuery] int      pageSize      = 20)
        {
            page     = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 50);

            var result = await _productService.GetFrontActiveProductsAsync(
                categoryId, keyword, sortBy ?? "latest", page, pageSize,
                subCategoryId, brandIds, minPrice, maxPrice);

            var items = result.Data.Select(p => new ProductListItemDto
            {
                Id            = p.Id,
                Name          = p.Name,
                Price         = p.MinPrice ?? 0m,
                OriginalPrice = null,
                ImageUrl      = p.MainImageUrl ?? string.Empty,
                SoldCount     = p.TotalSales ?? 0,
                TotalStock    = p.TotalStock,
                Location      = string.Empty,
                CategoryId    = p.CategoryId,
                Rating        = null
            }).ToList();

            return Ok(new
            {
                success = true,
                data = new ProductListResponseDto
                {
                    Items    = items,
                    Total    = result.TotalCount,
                    Page     = result.CurrentPage,
                    PageSize = result.PageSize
                },
                message = ""
            });
        }

        // ──────────────────────────────────────────────────────────
        // GET /api/products/{id}
        // 前台商品詳情頁，一次回出大部分資料
        // ──────────────────────────────────────────────────────────

        /// <summary>
        /// 取得單一商品完整資訊（含規格、圖片、品牌、商店、分類路徑）
        /// </summary>
        /// <param name="id">商品 ID</param>
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var (product, rating, reviewCount, storeProductCount) =
                await _productService.GetProductDetailAsync(id);

            if (product == null)
                return NotFound(new { success = false, data = (object?)null, message = "商品不存在或已下架" });

            // 非同步累加瀏覽次數（fire-and-forget，不阻塞回應）
            _ = _productService.IncrementViewCountAsync(id);

            // ── 分類路徑（由子向上回溯到根）───────────────────────
            var categoryPath = new List<CategoryPathItemDto>();
            var cat = product.Category;
            while (cat != null)
            {
                categoryPath.Insert(0, new CategoryPathItemDto { Id = cat.Id, Name = cat.Name });
                cat = cat.Parent;
            }

            // ── 品牌 ──────────────────────────────────────────────
            BrandInfoDto? brandDto = null;
            if (product.Brand != null)
                brandDto = new BrandInfoDto
                {
                    Id      = product.Brand.Id,
                    Name    = product.Brand.Name,
                    LogoUrl = product.Brand.LogoUrl
                };

            // ── 商店（部分欄位 DB 尚無，回 null）─────────────────
            int? joinedYearsAgo = product.Store?.CreatedAt.HasValue == true
                ? (int?)Math.Floor((DateTime.Now - product.Store.CreatedAt!.Value).TotalDays / 365.25)
                : null;

            var storeDto = new StoreInfoDto
            {
                Id             = product.Store?.Id ?? 0,
                UserId         = product.Store?.UserId ?? 0,
                Name           = product.Store?.StoreName ?? string.Empty,
                Status         = product.Store?.StoreStatus ?? 1,
                LogoUrl        = null,
                Rating         = null,
                ProductCount   = storeProductCount,
                FollowerCount  = null,
                Location       = null,
                ResponseRate   = null,
                JoinedYearsAgo = joinedYearsAgo
            };

            // ── 圖片（主圖排前，依 SortOrder）────────────────────
            var images = product.ProductImages
                .OrderByDescending(img => img.IsMain == true)
                .ThenBy(img => img.SortOrder ?? 999)
                .Select(img => new ProductImageDto
                {
                    Id        = img.Id,
                    Url       = img.ImageUrl ?? string.Empty,
                    IsMain    = img.IsMain == true,
                    SortOrder = img.SortOrder ?? 0
                })
                .ToList();

            // ── 有效 Variants（IsDeleted != true）────────────────
            var activeVariants = product.ProductVariants
                .Where(v => v.IsDeleted != true)
                .OrderBy(v => v.Id)
                .ToList();

            // ── 價格區間 ──────────────────────────────────────────
            decimal minP, maxP;
            if (activeVariants.Any())
            {
                minP = activeVariants.Min(v => v.Price);
                maxP = activeVariants.Max(v => v.Price);
            }
            else
            {
                minP = product.MinPrice ?? 0m;
                maxP = product.MaxPrice ?? product.MinPrice ?? 0m;
            }

            // ── 解析每個 variant 的 SpecValueJson（Object 格式）────────
            // 實際 DB 格式：{"色號":"蜜桃粉","容量":"標準裝"}（JSON 物件）
            // SpecDefinitionJson 在 DB 全部為 null，不使用。
            var jsonOpts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            var parsedSpecMaps = new List<Dictionary<string, string>>(activeVariants.Count);
            foreach (var v in activeVariants)
            {
                if (string.IsNullOrWhiteSpace(v.SpecValueJson))
                {
                    parsedSpecMaps.Add(new Dictionary<string, string>());
                    continue;
                }
                try
                {
                    var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(v.SpecValueJson, jsonOpts)
                               ?? new Dictionary<string, string>();
                    parsedSpecMaps.Add(dict);
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(ex, "VariantId={VariantId} SpecValueJson 無法解析，已略過", v.Id);
                    parsedSpecMaps.Add(new Dictionary<string, string>());
                }
            }

            // ── 規格軸彙整（key 保持首次出現順序，options 去重保持順序）
            var seenAxisNames    = new HashSet<string>();
            var orderedAxisNames = new List<string>();
            var specOptionsMap   = new Dictionary<string, List<string>>();
            var seenOptionsMap   = new Dictionary<string, HashSet<string>>();

            foreach (var dict in parsedSpecMaps)
            {
                foreach (var kvp in dict)
                {
                    if (seenAxisNames.Add(kvp.Key))
                    {
                        orderedAxisNames.Add(kvp.Key);
                        specOptionsMap[kvp.Key] = new List<string>();
                        seenOptionsMap[kvp.Key] = new HashSet<string>();
                    }
                    if (seenOptionsMap[kvp.Key].Add(kvp.Value))
                        specOptionsMap[kvp.Key].Add(kvp.Value);
                }
            }

            var specs = orderedAxisNames
                .Select(name => new ProductSpecDto { Name = name, Options = specOptionsMap[name] })
                .ToList();

            // ── Variants DTO（specValues 直接用解析好的 Dictionary）────
            var variantDtos = activeVariants.Select((v, idx) => new ProductVariantDto
            {
                Id            = v.Id,
                SpecValues    = parsedSpecMaps[idx],
                Price         = v.Price,
                OriginalPrice = null,
                Stock         = v.Stock ?? 0,
                ImageUrl      = v.ProductImages.FirstOrDefault()?.ImageUrl
            }).ToList();

            // ── 組裝最終 DTO ──────────────────────────────────────
            var dto = new ProductDetailDto
            {
                Id                  = product.Id,
                Name                = product.Name,
                Description         = product.Description,
                CategoryId          = product.CategoryId,
                CategoryPath        = categoryPath,
                Brand               = brandDto,
                Store               = storeDto,
                Images              = images,
                PriceRange          = new PriceRangeDto { Min = minP, Max = maxP },
                OriginalPriceRange  = null,
                DiscountRate        = null,
                Specs               = specs,
                Variants            = variantDtos,
                TotalStock          = activeVariants.Sum(v => v.Stock ?? 0),
                SoldCount           = product.TotalSales ?? 0,
                Rating              = rating,
                ReviewCount         = reviewCount,
                IsOnShelf           = product.Status == 1,
                CreatedAt           = product.CreatedAt,
                ViewCount           = product.ViewCount ?? 0,
                AttributesJson      = product.AttributesJson
            };

            return Ok(new { success = true, data = dto, message = "" });
        }

        // ──────────────────────────────────────────────────────────
        // GET /api/products/{id}/related?limit=12
        // 逛逛賣場其他好物 / 猜你喜歡
        // ──────────────────────────────────────────────────────────

        /// <summary>
        /// 取得同分類相關商品（排除自身、只回上架中、依銷量排序）
        /// </summary>
        /// <param name="id">商品 ID</param>
        /// <param name="limit">回傳筆數（預設 12，上限 50）</param>
        [HttpGet("{id:int}/related")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetRelatedAsync(int id, [FromQuery] int limit = 12)
        {
            // 先確認商品存在且上架（借用詳情方法的 null 判斷）
            var (product, _, _, _) = await _productService.GetProductDetailAsync(id);
            if (product == null)
                return NotFound(new { success = false, data = (object?)null, message = "商品不存在或已下架" });

            var related = await _productService.GetRelatedProductsAsync(id, product.CategoryId, limit);

            var items = related.Select(p => new ProductListItemDto
            {
                Id            = p.Id,
                Name          = p.Name,
                Price         = p.MinPrice ?? 0m,
                OriginalPrice = null,
                ImageUrl      = p.MainImageUrl ?? string.Empty,
                SoldCount     = p.TotalSales ?? 0,
                TotalStock    = p.TotalStock,
                Location      = string.Empty,
                CategoryId    = p.CategoryId,
                Rating        = null
            }).ToList();

            return Ok(new { success = true, data = items, message = "" });
        }

        // ──────────────────────────────────────────────────────────
        // GET /api/hot-keywords
        // 熱搜關鍵字（取瀏覽次數最高的上架商品名稱）
        // ──────────────────────────────────────────────────────────

        /// <summary>
        /// 取得熱搜關鍵字（瀏覽次數最高的前 8 筆上架商品名稱，超過 10 字截斷）
        /// </summary>
        [HttpGet("/api/hot-keywords")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetHotKeywords()
        {
            var keywords = await _productService.GetHotKeywordsAsync();
            return Ok(new { success = true, data = keywords });
        }
    }
}
