using System.Collections.Generic;
using System.Threading.Tasks;
using ISpanShop.Models.DTOs.Orders;

namespace ISpanShop.Services.Orders
{
    public interface ICartService
    {
        Task<List<CartItemDto>> GetCartAsync(int userId);
        Task<bool> AddToCartAsync(int userId, AddToCartRequestDto dto);
        Task<bool> UpdateCartItemAsync(int userId, UpdateCartItemRequestDto dto);
        Task<bool> RemoveCartItemAsync(int userId, int productId, int? variantId);
        Task<bool> SyncCartAsync(int userId, List<CartItemDto> localItems);
        Task<bool> ClearCartAsync(int userId);
    }
}
