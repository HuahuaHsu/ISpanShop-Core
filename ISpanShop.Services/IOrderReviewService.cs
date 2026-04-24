using ISpanShop.Models.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ISpanShop.Services
{
    public interface IOrderReviewService
    {
        // 取得所有評論 (後台要看全部)
        Task<List<OrderReviewDto>> GetAllAsync();
        
        // [新增] 前台新增評論 (會自動偵測敏感字)
        Task AddReviewAsync(OrderReviewDto dto);

        // [新增] 取得特定商品的評價清單 (前台詳情頁顯示)
        Task<List<ISpanShop.Models.DTOs.Products.FrontProductReviewVm>> GetReviewsByProductIdAsync(int productId);

        // 切換評論的隱藏狀態 (Soft Delete)
        Task ToggleHiddenStatusAsync(int id);
    }
}