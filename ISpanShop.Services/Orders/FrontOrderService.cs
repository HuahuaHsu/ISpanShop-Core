using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ISpanShop.Common.Enums;
using ISpanShop.Models.DTOs.Orders;
using ISpanShop.Models.EfModels;
using ISpanShop.Repositories.Orders;
using ISpanShop.Services.Coupons;
using ISpanShop.Services.Payments;
using ISpanShop.Models.DTOs.Members;

namespace ISpanShop.Services.Orders
{
    public class FrontOrderService : IFrontOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly PointService _pointService;
        private readonly ICouponService _couponService;

        public FrontOrderService(
            IOrderRepository orderRepository,
            PointService pointService,
            ICouponService couponService)
        {
            _orderRepository = orderRepository;
            _pointService = pointService;
            _couponService = couponService;
        }

        public async Task<List<FrontOrderListDto>> GetMemberOrdersAsync(int memberId)
        {
            var orders = await _orderRepository.GetOrdersByMemberIdAsync(memberId);
            
            return orders.Select(o => {
                var firstDetail = o.OrderDetails.FirstOrDefault();
                return new FrontOrderListDto
                {
                    Id = o.Id,
                    OrderNumber = o.OrderNumber.StartsWith("ORD") ? o.OrderNumber : "ORD" + o.OrderNumber,
                    CreatedAt = o.CreatedAt,
                    FinalAmount = o.FinalAmount,
                    DiscountAmount = o.DiscountAmount,
                    LevelDiscount = o.LevelDiscount, // 從資料庫讀取
                    PointDiscount = o.PointDiscount,
                    PromotionDiscount = o.PromotionDiscount, // 從資料庫讀取活動折抵
                    Status = (OrderStatus)(o.Status ?? 0),
                    StatusName = GetStatusName(o.Status),
                    StoreName = o.Store?.StoreName ?? "未知商店",
                    StoreId = o.StoreId,
                    SellerId = o.Store?.UserId ?? 0,
                    FirstProductName = firstDetail?.ProductName,
                    FirstProductImage = GetFinalImage(firstDetail),
                    TotalItemCount = o.OrderDetails.Sum(od => od.Quantity),
                    IsReviewed = o.OrderReviews.Any()
                };
            }).ToList();
        }

        public async Task<FrontOrderDetailDto> GetOrderDetailAsync(long orderId, int memberId)
        {
            var o = await _orderRepository.GetOrderByIdAsync(orderId);
            
            if (o == null || o.UserId != memberId)
            {
                return null;
            }

            // [安全性強化] 如果是為了結帳而讀取，或是在任何詳情檢查中
            // 這裡保留通用讀取，但在下面 API 調用處會做更嚴格的攔截。
            
            return new FrontOrderDetailDto
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber.StartsWith("ORD") ? o.OrderNumber : "ORD" + o.OrderNumber,
                CreatedAt = o.CreatedAt,
                PaymentDate = o.PaymentDate,
                CompletedAt = o.CompletedAt,
                TotalAmount = o.TotalAmount,
                ShippingFee = o.ShippingFee,
                PointDiscount = o.PointDiscount,
                DiscountAmount = o.DiscountAmount,
                LevelDiscount = o.LevelDiscount, // 從資料庫讀取
                CouponId = o.CouponId,
                CouponTitle = o.Coupon?.Title ?? (o.CouponId.HasValue ? "優惠券" : null),
                PromotionDiscount = o.PromotionDiscount, // 從資料庫讀取活動折抵
                FinalAmount = o.FinalAmount,
                Status = (OrderStatus)(o.Status ?? 0),
                StatusName = GetStatusName(o.Status),
                StoreId = o.StoreId,
                SellerId = o.Store?.UserId ?? 0,
                StoreName = o.Store?.StoreName ?? "未知商店",
                StoreStatus = o.Store?.StoreStatus ?? 1,
                RecipientName = o.RecipientName,
                RecipientPhone = o.RecipientPhone,
                RecipientAddress = o.RecipientAddress,
                Note = o.Note,
                Items = o.OrderDetails.Select(od => {
                    var tags = new List<string>();
                    
                    // 檢查單品活動：如果結帳單價小於商品原價
                    decimal originalPrice = od.Product?.ProductVariants?.FirstOrDefault(v => v.Id == od.VariantId)?.Price ?? od.Product?.MinPrice ?? 0;
                    if (originalPrice > 0 && od.Price < originalPrice)
                    {
                        tags.Add("單品特價優惠");
                    }
                    
                    // 檢查滿額活動：整筆訂單有活動折抵
                    if ((o.PromotionDiscount ?? 0) > 0)
                    {
                        tags.Add("符合賣場滿額活動");
                    }

                    return new FrontOrderItemDto
                    {
                        Id = od.Id,
                        ProductId = od.ProductId,
                        VariantId = od.VariantId,
                        ProductName = od.ProductName,
                        VariantName = od.VariantName,
                        CoverImage = GetFinalImage(od),
                        Price = od.Price ?? 0,
                        Quantity = od.Quantity,
                        StoreStatus = o.Store?.StoreStatus ?? 1,
                        PromotionTags = tags.Distinct().ToList()
                    };
                }).ToList(),
                IsReviewed = o.OrderReviews.Any(),
                
                // 抓取最新的一筆退貨申請作為資訊展示
                ReturnInfo = o.ReturnRequests.OrderByDescending(r => r.CreatedAt).Select(r => new FrontReturnDetailDto
                {
                    ReasonCategory = r.ReasonCategory,
                    ReasonDescription = r.ReasonDescription,
                    RefundAmount = r.RefundAmount,
                    Status = r.Status,
                    CreatedAt = r.CreatedAt,
                    ImageUrls = r.ReturnRequestImages.Select(img => img.ImageUrl).ToList(),
                    // 關鍵：抓取退貨品項清單
                    Items = r.ReturnRequestItems.Select(ri => {
                        var img = GetFinalImage(ri.OrderDetail);
                        // 統一補斜線邏輯，這與 o.OrderDetails.Select 的處理保持一致
                        if (!string.IsNullOrEmpty(img) && !img.StartsWith("http") && !img.StartsWith("/"))
                        {
                            img = "/" + img;
                        }

                        var tags = new List<string>();
                        decimal originalPrice = ri.OrderDetail.Product?.ProductVariants?.FirstOrDefault(v => v.Id == ri.OrderDetail.VariantId)?.Price ?? ri.OrderDetail.Product?.MinPrice ?? 0;
                        if (originalPrice > 0 && ri.OrderDetail.Price < originalPrice) tags.Add("單品特價優惠");
                        if ((o.PromotionDiscount ?? 0) > 0) tags.Add("符合賣場滿額活動");

                        return new FrontReturnItemDto
                        {
                            ProductName = ri.OrderDetail.ProductName,
                            VariantName = ri.OrderDetail.VariantName,
                            CoverImage = img,
                            Price = ri.OrderDetail.Price ?? 0,
                            ReturnQuantity = ri.Quantity,
                            PromotionTags = tags.Distinct().ToList()
                        };
                    }).ToList()
                }).FirstOrDefault()
            };
        }

        public async Task<bool> CancelOrderAsync(long orderId, int memberId)
        {
            var o = await _orderRepository.GetOrderByIdAsync(orderId);
            if (o == null || o.UserId != memberId) return false;

            // 只有待付款(0)或待出貨(1)可以取消
            if (o.Status != 0 && o.Status != 1) return false;

            // 退回點數與優惠券
            await ReturnOrderAssetsAsync(o);

            await _orderRepository.UpdateStatusAsync(orderId, 4); // 4 = 已取消
            return true;
        }

        public async Task<bool> ConfirmReceiptAsync(long orderId, int memberId)
        {
            var o = await _orderRepository.GetOrderByIdAsync(orderId);
            if (o == null || o.UserId != memberId) return false;

            // 只有運送中(2)可以確認收貨
            if (o.Status != 2) return false;

            await _orderRepository.UpdateStatusAsync(orderId, 3); // 3 = 已完成
            return true;
        }

        public async Task<bool> RequestReturnAsync(long orderId, int memberId, FrontReturnRequestDto dto)
        {
            var o = await _orderRepository.GetOrderByIdAsync(orderId);
            if (o == null || o.UserId != memberId) return false;

            // 只有待出貨(1)、運送中(2)或已完成(3)可以申請退貨
            if (o.Status != 1 && o.Status != 2 && o.Status != 3) return false;

            // 1. 計算退款金額 (含折抵比例)
            decimal itemsOriginalTotal = 0;
            foreach (var item in dto.Items)
            {
                var detail = o.OrderDetails.FirstOrDefault(od => od.Id == item.OrderDetailId);
                if (detail != null)
                {
                    int returnQty = Math.Min(item.Quantity, detail.Quantity);
                    itemsOriginalTotal += (detail.Price ?? 0) * returnQty;
                }
            }

            decimal totalRefund = 0;
            // 判斷是否為全退：明細數量一致 且 每個品項數量都退滿
            bool isFullReturn = dto.Items.Count == o.OrderDetails.Count &&
                                dto.Items.All(i => i.Quantity == o.OrderDetails.First(od => od.Id == i.OrderDetailId).Quantity);

            if (isFullReturn)
            {
                totalRefund = o.FinalAmount;
            }
            else
            {
                decimal orderTotal = o.TotalAmount; // 商品總原價
                if (orderTotal > 0)
                {
                    // 按原價比例分攤折扣
                    decimal ratio = itemsOriginalTotal / orderTotal;
                    decimal totalDiscount = (o.PointDiscount ?? 0) + (o.DiscountAmount ?? 0);
                    decimal proportionDiscount = Math.Round(totalDiscount * ratio);

                    totalRefund = itemsOriginalTotal - proportionDiscount;
                    if (totalRefund < 0) totalRefund = 0;
                }
            }

            // 2. 建立退貨申請實體
            var request = new ReturnRequest
            {
                OrderId = orderId,
                ReasonCategory = dto.ReasonCategory,
                ReasonDescription = dto.ReasonDescription, // 移除舊的明細拼湊，保持乾淨
                RefundAmount = totalRefund,
                Status = 0, // 待處理
                CreatedAt = DateTime.Now,
                ReturnRequestImages = dto.ImageUrls?.Select(url => new ReturnRequestImage
                {
                    ImageUrl = url,
                    CreatedAt = DateTime.Now
                }).ToList() ?? new List<ReturnRequestImage>(),

                // 3. 關鍵：寫入新表 ReturnRequestItems
                ReturnRequestItems = dto.Items.Select(i => new ReturnRequestItem
                {
                    OrderDetailId = i.OrderDetailId,
                    Quantity = i.Quantity
                }).ToList()
            };

            await _orderRepository.CreateReturnRequestAsync(request);
            await _orderRepository.UpdateStatusAsync(orderId, 5); // 5 = 退貨/款中

            return true;
        }
        private async Task ReturnOrderAssetsAsync(Order o)
        {
            // 1. 退回點數
            if (o.PointDiscount.HasValue && o.PointDiscount.Value > 0)
            {
                await _pointService.UpdatePointsAsync(new PointUpdateDTO
                {
                    UserId = o.UserId,
                    ChangeAmount = o.PointDiscount.Value,
                    Description = $"訂單取消/退貨退回 (訂單編號: {o.OrderNumber})",
                    OrderNumber = o.OrderNumber
                });
            }

            // 2. 退回優惠券
            if (o.CouponId.HasValue)
            {
                await _couponService.ReturnCouponAsync(o.Id);
            }
        }

        private string GetFinalImage(OrderDetail od)
        {
            if (od == null) return null;
            
            // 1. 優先使用訂單成立時的快照圖
            if (!string.IsNullOrEmpty(od.CoverImage)) return od.CoverImage;

            // 2. 如果快照圖缺失，嘗試從 Product 關聯中抓取
            if (od.Product != null)
            {
                // 優先使用該變體 (Variant) 的圖片
                var variantImage = od.Product.ProductVariants?
                    .FirstOrDefault(v => v.Id == od.VariantId)?
                    .ProductImages?.FirstOrDefault()?.ImageUrl;
                
                if (!string.IsNullOrEmpty(variantImage)) return variantImage;

                // 使用產品主圖
                var mainImage = od.Product.ProductImages?.FirstOrDefault(pi => pi.IsMain == true)?.ImageUrl;
                if (!string.IsNullOrEmpty(mainImage)) return mainImage;

                // 最後手段：使用產品的第一張圖
                return od.Product.ProductImages?.FirstOrDefault()?.ImageUrl;
            }

            return null;
        }

        private string GetStatusName(byte? status)
        {
            return status switch
            {
                0 => "待付款",
                1 => "待出貨",
                2 => "運送中",
                3 => "已完成",
                4 => "已取消",
                5 => "退貨/款中",
                6 => "已退款",
                _ => "未知"
            };
        }
    }
}
