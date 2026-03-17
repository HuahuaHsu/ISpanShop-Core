using System.Collections.Generic;
using ISpanShop.Models.DTOs.Inventories;
using ISpanShop.Models.DTOs.Common;
using ISpanShop.Models.EfModels;

namespace ISpanShop.Repositories.Inventories
{
    public interface IInventoryRepository
    {
        (IEnumerable<ProductVariant> Items, int TotalCount) GetVariantsPaged(InventorySearchCriteria criteria);
        (IEnumerable<ProductVariant> Variants, int TotalProductCount) GetVariantsGroupedPaged(InventorySearchCriteria criteria);
        int GetLowStockCount();
        int GetZeroStockCount();
        int GetTotalVariantCount();
        ProductVariant? GetVariantById(int variantId);
        void UpdateStock(int variantId, int newStock);
        void UpdateSafetyStock(int variantId, int newSafetyStock);
        void UpdateStockAndSafetyStock(int variantId, int newStock, int newSafetyStock);
        IEnumerable<(int Id, string Name)> GetStoreOptions();
        IEnumerable<(int Id, string Name)> GetCategoryOptions();
        IEnumerable<(int Id, string Name)> GetMainCategories();
        IEnumerable<(int Id, string Name)> GetSubCategories(int parentId);
    }
}
