using System.Collections.Generic;
using ISpanShop.Models.DTOs;

namespace ISpanShop.Services.Interfaces
{
    public interface IInventoryService
    {
        PagedResult<InventoryListDto> GetInventoryPaged(InventorySearchCriteria criteria);
        int GetLowStockCount();
        int GetZeroStockCount();
        int GetTotalVariantCount();
        InventoryListDto? GetVariantDetail(int variantId);
        void AdjustStock(int variantId, int newStock);
        void UpdateSafetyStock(int variantId, int newSafetyStock);
        void UpdateStockAndSafetyStock(int variantId, int newStock, int newSafetyStock);
        IEnumerable<(int Id, string Name)> GetStoreOptions();
        IEnumerable<(int Id, string Name)> GetCategoryOptions();
    }
}
