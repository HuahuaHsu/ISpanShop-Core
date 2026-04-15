using System.Collections.Generic;
using System.Linq;
using ISpanShop.Models.DTOs.Inventories;
using ISpanShop.Models.DTOs.Common;
using ISpanShop.Repositories.Products;
using ISpanShop.Repositories.Categories;
using ISpanShop.Repositories.Inventories;
using ISpanShop.Services.Products;
using ISpanShop.Services.Inventories;

namespace ISpanShop.Services.Inventories
{
    public class InventoryService : IInventoryService
    {
        private readonly IInventoryRepository _inventoryRepository;

        public InventoryService(IInventoryRepository inventoryRepository)
        {
            _inventoryRepository = inventoryRepository;
        }

        public PagedResult<InventoryListDto> GetInventoryPaged(InventorySearchCriteria criteria)
        {
            var (items, total) = _inventoryRepository.GetVariantsPaged(criteria);

            var dtos = items.Select(v => new InventoryListDto
            {
                VariantId    = v.Id,
                ProductId    = v.ProductId,
                ProductName  = v.Product?.Name ?? string.Empty,
                StoreName    = v.Product?.Store?.StoreName ?? string.Empty,
                CategoryName = v.Product?.Category?.Name ?? string.Empty,
                VariantName  = v.VariantName ?? string.Empty,
                SkuCode      = v.SkuCode ?? string.Empty,
                Stock        = v.Stock ?? 0,
                SafetyStock  = v.SafetyStock ?? 0
            }).ToList();

            return PagedResult<InventoryListDto>.Create(dtos, total, criteria.PageNumber, criteria.PageSize);
        }

        public PagedResult<ProductInventoryVm> GetInventoryGroupedPaged(InventorySearchCriteria criteria)
        {
            var (variants, totalProducts) = _inventoryRepository.GetVariantsGroupedPaged(criteria);

            var groups = variants
                .GroupBy(v => v.ProductId)
                .Select(g =>
                {
                    var first = g.First();
                    var skus  = g.OrderBy(v => v.VariantName).ToList();
                    bool hasZero = skus.Any(v => (v.Stock ?? 0) == 0);
                    bool hasLow  = skus.Any(v => (v.Stock ?? 0) > 0 && (v.Stock ?? 0) <= (v.SafetyStock ?? 0));

                    return new ProductInventoryVm
                    {
                        ProductId     = g.Key,
                        ProductName   = first.Product?.Name              ?? string.Empty,
                        StoreName     = first.Product?.Store?.StoreName  ?? string.Empty,
                        CategoryName  = first.Product?.Category?.Name    ?? string.Empty,
                        TotalStock    = skus.Sum(v => v.Stock ?? 0),
                        SkuCount      = skus.Count,
                        OverallStatus = hasZero ? "zero" : hasLow ? "low" : "normal",
                        Skus = skus.Select(v => new SkuInventoryVm
                        {
                            VariantId   = v.Id,
                            VariantName = v.VariantName ?? string.Empty,
                            SkuCode     = v.SkuCode     ?? string.Empty,
                            Stock       = v.Stock       ?? 0,
                            SafetyStock = v.SafetyStock ?? 0
                        }).ToList()
                    };
                })
                .ToList();

            return PagedResult<ProductInventoryVm>.Create(groups, totalProducts, criteria.PageNumber, criteria.PageSize);
        }

        public int GetLowStockCount()
            => _inventoryRepository.GetLowStockCount();

        public int GetZeroStockCount()
            => _inventoryRepository.GetZeroStockCount();

        public int GetTotalVariantCount()
            => _inventoryRepository.GetTotalVariantCount();

        public InventoryListDto? GetVariantDetail(int variantId)
        {
            var v = _inventoryRepository.GetVariantById(variantId);
            if (v == null) return null;
            return new InventoryListDto
            {
                VariantId    = v.Id,
                ProductId    = v.ProductId,
                ProductName  = v.Product?.Name ?? string.Empty,
                StoreName    = v.Product?.Store?.StoreName ?? string.Empty,
                CategoryName = v.Product?.Category?.Name ?? string.Empty,
                VariantName  = v.VariantName ?? string.Empty,
                SkuCode      = v.SkuCode ?? string.Empty,
                Stock        = v.Stock ?? 0,
                SafetyStock  = v.SafetyStock ?? 0
            };
        }

        public void AdjustStock(int variantId, int newStock)
            => _inventoryRepository.UpdateStock(variantId, newStock);

        public void UpdateSafetyStock(int variantId, int newSafetyStock)
            => _inventoryRepository.UpdateSafetyStock(variantId, newSafetyStock);

        public void UpdateStockAndSafetyStock(int variantId, int newStock, int newSafetyStock)
            => _inventoryRepository.UpdateStockAndSafetyStock(variantId, newStock, newSafetyStock);

        public IEnumerable<(int Id, string Name)> GetStoreOptions()
            => _inventoryRepository.GetStoreOptions();

        public IEnumerable<(int Id, string Name)> GetCategoryOptions()
            => _inventoryRepository.GetCategoryOptions();

        public IEnumerable<(int Id, string Name)> GetMainCategories()
            => _inventoryRepository.GetMainCategories();

        public IEnumerable<(int Id, string Name)> GetSubCategories(int parentId)
            => _inventoryRepository.GetSubCategories(parentId);
    }
}
