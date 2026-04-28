using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ISpanShop.Models.DTOs.Orders;
using ISpanShop.Models.EfModels;
using Microsoft.EntityFrameworkCore;

namespace ISpanShop.Services.Orders
{
    public class CartService : ICartService
    {
        private readonly ISpanShopDBContext _context;

        public CartService(ISpanShopDBContext context)
        {
            _context = context;
        }

        public async Task<List<CartItemDto>> GetCartAsync(int userId)
        {
            var cart = await GetOrCreateCartAsync(userId);
            var now = DateTime.Now;

            // 取得所有購物車商品對應的活動
            var productIds = cart.CartItems.Select(ci => ci.ProductId).Distinct().ToList();
            var activePromotions = await _context.PromotionItems
                .Include(pi => pi.Promotion)
                    .ThenInclude(p => p.PromotionRules)
                .Where(pi => productIds.Contains(pi.ProductId) 
                          && pi.Promotion.Status == 1 
                          && pi.Promotion.StartTime <= now 
                          && pi.Promotion.EndTime >= now
                          && !pi.Promotion.IsDeleted)
                .ToListAsync();
            
            return cart.CartItems.Select(ci => {
                var itemPromotions = activePromotions
                    .Where(ap => ap.ProductId == ci.ProductId)
                    .ToList();

                // 計算促銷價 (優先取限時特賣或限量搶購)
                decimal? promoPrice = null;
                var directPromo = itemPromotions.FirstOrDefault(ap => ap.Promotion.PromotionType == 1 || ap.Promotion.PromotionType == 3);
                if (directPromo != null)
                {
                    if (directPromo.DiscountPrice.HasValue)
                    {
                        promoPrice = directPromo.DiscountPrice.Value;
                    }
                    else if (directPromo.DiscountPercent.HasValue)
                    {
                        decimal original = ci.UnitPrice ?? ci.Variant?.Price ?? ci.Product?.MinPrice ?? 0;
                        promoPrice = Math.Round(original * (decimal)(100 - directPromo.DiscountPercent.Value) / 100m, 0);
                    }
                }

                var promoDtos = itemPromotions.Select(ap => {
                        var rule = ap.Promotion.PromotionRules.FirstOrDefault();
                        return new CartItemPromotionDto
                        {
                            PromotionId = ap.PromotionId,
                            Name = ap.Promotion.Name,
                            PromotionType = ap.Promotion.PromotionType,
                            Threshold = rule?.Threshold ?? 0,
                            DiscountValue = rule?.DiscountValue ?? 0,
                            DiscountType = rule?.DiscountType ?? 0,
                            Description = ap.Promotion.Description
                        };
                    }).ToList();

                return new CartItemDto
                {
                    Id = ci.Id,
                    StoreId = ci.StoreId,
                    StoreName = ci.Store?.StoreName ?? "未知商店",
                    StoreStatus = ci.Store?.StoreStatus ?? 1,
                    ProductId = ci.ProductId,
                    VariantId = ci.VariantId == 0 ? null : ci.VariantId,
                    ProductName = ci.Product?.Name ?? "未知商品",
                    VariantName = ci.Variant?.VariantName,
                    ProductImage = GetProductImage(ci),
                    UnitPrice = ci.UnitPrice ?? ci.Variant?.Price ?? ci.Product?.MinPrice ?? 0,
                    PromotionPrice = promoPrice,
                    Quantity = ci.Quantity,
                    Selected = true,
                    Promotions = promoDtos
                };
            }).ToList();
        }

        public async Task<bool> AddToCartAsync(int userId, AddToCartRequestDto dto)
        {
            var cart = await GetOrCreateCartAsync(userId);
            
            var existingItem = cart.CartItems.FirstOrDefault(ci => 
                ci.ProductId == dto.ProductId && 
                (ci.VariantId == (dto.VariantId ?? 0) || ci.VariantId == dto.VariantId));

            if (existingItem != null)
            {
                existingItem.Quantity += dto.Quantity;
            }
            else
            {
                // 獲取商品資訊以填寫 StoreId 和 UnitPrice
                var product = await _context.Products
                    .Include(p => p.ProductVariants)
                    .FirstOrDefaultAsync(p => p.Id == dto.ProductId);
                
                if (product == null) return false;

                var variant = product.ProductVariants.FirstOrDefault(v => v.Id == dto.VariantId);
                decimal price = variant?.Price ?? product.MinPrice ?? 0;

                cart.CartItems.Add(new CartItem
                {
                    CartId = cart.Id,
                    StoreId = product.StoreId,
                    ProductId = dto.ProductId,
                    VariantId = dto.VariantId ?? 0,
                    Quantity = dto.Quantity,
                    UnitPrice = price
                });
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateCartItemAsync(int userId, UpdateCartItemRequestDto dto)
        {
            var cart = await GetOrCreateCartAsync(userId);
            var item = cart.CartItems.FirstOrDefault(ci => 
                ci.ProductId == dto.ProductId && 
                (ci.VariantId == (dto.VariantId ?? 0) || ci.VariantId == dto.VariantId));

            if (item == null) return false;

            if (dto.Quantity <= 0)
            {
                _context.CartItems.Remove(item);
            }
            else
            {
                item.Quantity = dto.Quantity;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveCartItemAsync(int userId, int productId, int? variantId)
        {
            var cart = await GetOrCreateCartAsync(userId);
            var item = cart.CartItems.FirstOrDefault(ci => 
                ci.ProductId == productId && 
                (ci.VariantId == (variantId ?? 0) || ci.VariantId == variantId));

            if (item == null) return false;

            _context.CartItems.Remove(item);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SyncCartAsync(int userId, List<CartItemDto> localItems)
        {
            if (localItems == null || !localItems.Any()) return true;

            foreach (var item in localItems)
            {
                await AddToCartAsync(userId, new AddToCartRequestDto
                {
                    ProductId = item.ProductId,
                    VariantId = item.VariantId,
                    Quantity = item.Quantity
                });
            }
            return true;
        }

        public async Task<bool> ClearCartAsync(int userId)
        {
            var cart = await GetOrCreateCartAsync(userId);
            _context.CartItems.RemoveRange(cart.CartItems);
            await _context.SaveChangesAsync();
            return true;
        }

        private async Task<Cart> GetOrCreateCartAsync(int userId)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Store)
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                        .ThenInclude(p => p.ProductImages)
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Variant)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart { UserId = userId };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            return cart;
        }

        private string GetProductImage(CartItem ci)
        {
            if (ci.Variant != null && !string.IsNullOrEmpty(ci.Variant.SkuCode)) // 假設變體有圖或用預設
            {
                // 這裡邏輯簡化，實際應從 ProductImage 抓取與 VariantId 相關的圖
            }
            
            var mainImage = ci.Product?.ProductImages?.FirstOrDefault(pi => pi.IsMain == true)?.ImageUrl;
            return mainImage ?? ci.Product?.ProductImages?.FirstOrDefault()?.ImageUrl ?? "/placeholder.png";
        }
    }
}
