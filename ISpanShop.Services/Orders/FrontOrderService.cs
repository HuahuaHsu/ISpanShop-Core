using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ISpanShop.Common.Enums;
using ISpanShop.Models.DTOs.Orders;
using ISpanShop.Models.EfModels;
using ISpanShop.Repositories.Orders;

namespace ISpanShop.Services.Orders
{
    public class FrontOrderService : IFrontOrderService
    {
        private readonly IOrderRepository _orderRepository;

        public FrontOrderService(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<List<FrontOrderListDto>> GetMemberOrdersAsync(int memberId)
        {
            var orders = await _orderRepository.GetOrdersByMemberIdAsync(memberId);
            
            return orders.Select(o => {
                var firstDetail = o.OrderDetails.FirstOrDefault();
                return new FrontOrderListDto
                {
                    Id = o.Id,
                    OrderNumber = o.OrderNumber,
                    CreatedAt = o.CreatedAt,
                    FinalAmount = o.FinalAmount,
                    Status = (OrderStatus)(o.Status ?? 0),
                    StatusName = GetStatusName(o.Status),
                    StoreName = o.Store?.StoreName ?? "未知商店",
                    FirstProductName = firstDetail?.ProductName,
                    FirstProductImage = GetFinalImage(firstDetail),
                    TotalItemCount = o.OrderDetails.Sum(od => od.Quantity)
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

            return new FrontOrderDetailDto
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                CreatedAt = o.CreatedAt,
                PaymentDate = o.PaymentDate,
                CompletedAt = o.CompletedAt,
                TotalAmount = o.TotalAmount,
                ShippingFee = o.ShippingFee,
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
                }).ToList()
            };
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
