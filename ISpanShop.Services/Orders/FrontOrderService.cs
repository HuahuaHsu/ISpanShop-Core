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
                CouponId = o.CouponId,
                CouponTitle = o.Coupon?.Title ?? (o.CouponId.HasValue ? "優惠券" : null),
                FinalAmount = o.FinalAmount,
                Status = (OrderStatus)(o.Status ?? 0),
                StatusName = GetStatusName(o.Status),
                StoreName = o.Store?.StoreName ?? "未知商店",
                RecipientName = o.RecipientName,
                RecipientPhone = o.RecipientPhone,
                RecipientAddress = o.RecipientAddress,
                Note = o.Note,
                Items = o.OrderDetails.Select(od => new FrontOrderItemDto
                {
                    Id = od.Id,
                    ProductId = od.ProductId,
                    VariantId = od.VariantId,
                    ProductName = od.ProductName,
                    VariantName = od.VariantName,
                    CoverImage = GetFinalImage(od),
                    Price = od.Price ?? 0,
                    Quantity = od.Quantity
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
                    ImageUrls = r.ReturnRequestImages.Select(img => img.ImageUrl).ToList()
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

            // 計算退款金額 (含折抵比例)
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
                    decimal ratio = itemsOriginalTotal / orderTotal;
                    decimal totalDiscount = (o.PointDiscount ?? 0) + (o.DiscountAmount ?? 0);
                    decimal proportionDiscount = Math.Round(totalDiscount * ratio);
                    
                    totalRefund = itemsOriginalTotal - proportionDiscount;
                    if (totalRefund < 0) totalRefund = 0;
                }
            }

            // 如果是部分退貨，我們把資訊寫在 ReasonDescription 中
            var itemsInfo = string.Join("\n", dto.Items.Select(i => {
                var d = o.OrderDetails.FirstOrDefault(od => od.Id == i.OrderDetailId);
                return $"- {d?.ProductName ?? "未知"}: {i.Quantity} 件";
            }));

            var request = new ReturnRequest
            {
                OrderId = orderId,
                ReasonCategory = dto.ReasonCategory,
                ReasonDescription = $"{dto.ReasonDescription}\n\n退貨清單:\n{itemsInfo}",
                RefundAmount = totalRefund,
                Status = 0, // 待處理
                CreatedAt = DateTime.Now,
                ReturnRequestImages = dto.ImageUrls?.Select(url => new ReturnRequestImage
                {
                    ImageUrl = url,
                    CreatedAt = DateTime.Now
                }).ToList() ?? new List<ReturnRequestImage>()
            };

            await _orderRepository.CreateReturnRequestAsync(request);
            await _orderRepository.UpdateStatusAsync(orderId, 5); // 5 = 退貨/款中

            // 退回點數與優惠券 (假設發起退貨就先退回，或可依需求改為管理員核准後才退)
            await ReturnOrderAssetsAsync(o);

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
