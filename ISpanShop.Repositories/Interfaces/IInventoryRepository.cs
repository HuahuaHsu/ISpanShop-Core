using System.Collections.Generic;
using ISpanShop.Models.DTOs;
using ISpanShop.Models.EfModels;

namespace ISpanShop.Repositories.Interfaces
{
    public interface IInventoryRepository
    {
        (IEnumerable<ProductVariant> Items, int TotalCount) GetVariantsPaged(InventorySearchCriteria criteria);
        int GetLowStockCount();
        int GetZeroStockCount();
        int GetTotalVariantCount();
        ProductVariant? GetVariantById(int variantId);
        void UpdateStock(int variantId, int newStock);
        void UpdateSafetyStock(int variantId, int newSafetyStock);
        void UpdateStockAndSafetyStock(int variantId, int newStock, int newSafetyStock);
        IEnumerable<(int Id, string Name)> GetStoreOptions();
        IEnumerable<(int Id, string Name)> GetCategoryOptions();
    }
}
