using ISpanShop.Models.DTOs;
using ISpanShop.Repositories.Orders;
using ISpanShop.Services.ContentModeration; // 加入命名空間
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ISpanShop.Services
{
    public class OrderReviewService : IOrderReviewService
    {
        private readonly IOrderReviewRepository _repo;
        private readonly ISensitiveWordService _sensitiveWordService; // 注入工具

        public OrderReviewService(IOrderReviewRepository repo, ISensitiveWordService sensitiveWordService)
        {
            _repo = repo;
            _sensitiveWordService = sensitiveWordService;
        }

        public async Task<List<OrderReviewDto>> GetAllAsync()
        {
            var entities = await _repo.GetAllAsync();
            var dtos = new List<OrderReviewDto>();

            foreach (var e in entities)
            {
                // 自動偵測內容是否包含違禁詞
                bool isAutoFlagged = await _sensitiveWordService.HasSensitiveWordAsync(e.Comment);

                dtos.Add(new OrderReviewDto
                {
                    Id = e.Id,
                    UserId = e.UserId,
                    OrderId = e.OrderId,
                    Rating = e.Rating,
                    Comment = e.Comment,
                    StoreReply = e.StoreReply,
                    IsHidden = e.IsHidden ?? false, 
                    IsAutoFlagged = isAutoFlagged, // 賦予系統掃描後的狀態
                    CreatedAt = e.CreatedAt ?? DateTime.MinValue,
                    ImageUrls = e.ReviewImages.Select(img => img.ImageUrl).ToList(),
                    ProductMainImage = e.Order?.OrderDetails?.FirstOrDefault()?.Product?.ProductImages?
                                        .FirstOrDefault(pi => pi.IsMain == true)?.ImageUrl 
                                        ?? e.Order?.OrderDetails?.FirstOrDefault()?.Product?.ProductImages?.FirstOrDefault()?.ImageUrl
                                        ?? "/images/no-image.png"
                });
            }

            return dtos.OrderByDescending(x => x.CreatedAt).ToList();
        }

        // [新增] 前台新增評論 (包含自動審查邏輯)
        public async Task AddReviewAsync(OrderReviewDto dto)
        {
            // 自動偵測內容是否包含違禁詞
            bool hasSensitiveWord = await _sensitiveWordService.HasSensitiveWordAsync(dto.Comment);

            var entity = new ISpanShop.Models.EfModels.OrderReview
            {
                UserId = dto.UserId,
                OrderId = dto.OrderId,
                Rating = dto.Rating,
                Comment = dto.Comment,
                // 如果有敏感字，寫入當下就直接被「隱藏」
                IsHidden = hasSensitiveWord, 
                CreatedAt = dto.CreatedAt,
                ReviewImages = dto.ImageUrls.Select(url => new ISpanShop.Models.EfModels.ReviewImage
                {
                    ImageUrl = url
                }).ToList()
            };

            await _repo.CreateAsync(entity);
        }

        public async Task ToggleHiddenStatusAsync(int id)
        {
            var review = await _repo.GetByIdAsync(id);
            if (review != null)
            {
                // 核心邏輯：狀態反轉 (如果被隱藏就解開，如果沒隱藏就隱藏)
                bool currentStatus = review.IsHidden ?? false;
                review.IsHidden = !currentStatus;

                await _repo.UpdateAsync(review);
            }
        }

        public async Task<List<ISpanShop.Models.DTOs.Products.FrontProductReviewVm>> GetReviewsByProductIdAsync(int productId)
        {
            var entities = await _repo.GetAllAsync();

            // 除錯：先不檢查 IsHidden 看看資料有沒有出來
            var reviews = entities
                .Where(r => r.Order != null && r.Order.OrderDetails.Any(od => od.ProductId == productId))
                .Select(r => new ISpanShop.Models.DTOs.Products.FrontProductReviewVm
                {
                    Id = r.Id,
                    UserName = MaskUserName(r.User?.Account ?? "Guest"),
                    UserAvatar = r.User?.MemberProfile?.AvatarUrl ?? "/images/default-avatar.png",
                    Rating = r.Rating,
                    Comment = r.Comment,
                    StoreReply = r.StoreReply,
                    CreatedAt = r.CreatedAt ?? DateTime.MinValue,
                    VariantName = r.Order.OrderDetails.FirstOrDefault(od => od.ProductId == productId)?.VariantName,
                    ImageUrls = r.ReviewImages.Select(ri => ri.ImageUrl).ToList()
                })
                .OrderByDescending(r => r.CreatedAt)
                .ToList();

            return reviews;
        }

        private string MaskUserName(string name)
        {
            if (string.IsNullOrEmpty(name)) return "****";
            if (name.Length <= 2) return name[0] + "****";
            return name[0] + "****" + name[name.Length - 1];
        }
    }
}