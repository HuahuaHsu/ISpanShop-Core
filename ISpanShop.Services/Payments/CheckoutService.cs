using ISpanShop.Models.DTOs.Orders;
using ISpanShop.Models.DTOs.Members;
using ISpanShop.Models.EfModels;
using ISpanShop.Services.Coupons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ISpanShop.Services.Payments
{
	public class CheckoutService
	{
		private readonly ISpanShopDBContext _context;
		private readonly PointService _pointService;
		private readonly PaymentService _paymentService;
		private readonly ICouponService _couponService;

		public CheckoutService(
			ISpanShopDBContext context, 
			PointService pointService, 
			PaymentService paymentService,
			ICouponService couponService)
		{
			_context = context;
			_pointService = pointService;
			_paymentService = paymentService;
			_couponService = couponService;
		}

		public async Task<(bool IsSuccess, string Message, string? OrderNumber)> CreateOrderAsync(CheckoutRequestDTO dto)
		{
			var strategy = _context.Database.CreateExecutionStrategy();

			return await strategy.ExecuteAsync(async () =>
			{
				using (var transaction = await _context.Database.BeginTransactionAsync())
				{
					try
					{
						// --- A. 計算原始總額 ---
						decimal subtotal = dto.Items.Sum(x => x.UnitPrice * x.Quantity);
						decimal shippingFee = 60;
						decimal pointDiscountAmount = 0;
						decimal couponDiscountAmount = 0;
						Coupon? coupon = null;

						var orderNumber = "ORD" + DateTime.Now.ToString("yyyyMMddHHmmss");

						// --- B. 處理優惠券驗證與試算 ---
						if (dto.CouponId.HasValue)
						{
							var productIds = dto.Items.Select(i => i.ProductId).Distinct().ToList();
							var valRes = await _couponService.ValidateCouponAsync(dto.UserId, dto.CouponId.Value, subtotal, productIds);

							if (!valRes.IsValid) return (false, valRes.Message, null);

							coupon = valRes.Coupon;
							if (coupon != null)
							{
								if (coupon.CouponType == 1) // 固定金額
								{
									couponDiscountAmount = coupon.DiscountValue;
								}
								else if (coupon.CouponType == 2) // 百分比
								{
									couponDiscountAmount = Math.Round(subtotal * (coupon.DiscountValue / 100m), 0);
									if (coupon.MaximumDiscount.HasValue)
									{
										couponDiscountAmount = Math.Min(couponDiscountAmount, coupon.MaximumDiscount.Value);
									}
								}
								// 確保折扣不超過總額
								couponDiscountAmount = Math.Min(couponDiscountAmount, subtotal);
							}
						}

						// --- C. 處理點數折抵邏輯 (在扣除優惠券後的剩餘金額基礎上折抵) ---
						decimal remainingAmount = subtotal - couponDiscountAmount;

						if (dto.UsePoints)
						{
							int balance = await _pointService.GetBalanceAsync(dto.UserId);
							pointDiscountAmount = Math.Min(balance, remainingAmount);

							if (pointDiscountAmount > 0)
							{
								var pointRes = await _pointService.UpdatePointsAsync(new PointUpdateDTO
								{
									UserId = dto.UserId,
									ChangeAmount = -(int)pointDiscountAmount,
									Description = "訂單折抵",
									OrderNumber = orderNumber
								});

								if (!pointRes.IsSuccess) return (false, pointRes.Message, null);
							}
						}

						// --- D. 建立訂單主表 ---
						// 確保 StoreId 有效，如果 DTO 為 0 則從商品中抓取
						int effectiveStoreId = dto.StoreId;
						if (effectiveStoreId <= 0 && dto.Items.Any())
						{
							var firstProduct = await _context.Products.FindAsync(dto.Items[0].ProductId);
							if (firstProduct != null) effectiveStoreId = firstProduct.StoreId;
						}

						// 檢查賣場狀態
						var store = await _context.Stores.AsNoTracking().FirstOrDefaultAsync(s => s.Id == effectiveStoreId);
						if (store != null && store.StoreStatus == 2)
						{
							return (false, "該賣場目前休假中，暫時無法接受下單", null);
						}

						var order = new Order
						{
							OrderNumber = orderNumber,
							UserId = dto.UserId,
							StoreId = effectiveStoreId,
							TotalAmount = subtotal,
							ShippingFee = shippingFee,
							CouponId = dto.CouponId,
							DiscountAmount = couponDiscountAmount, // 優惠券折扣
							PointDiscount = (int)pointDiscountAmount, // 點數折扣
							FinalAmount = (subtotal + shippingFee) - couponDiscountAmount - pointDiscountAmount,
							Status = 0, // 0: 待付款
							RecipientName = dto.RecipientName,
							RecipientPhone = dto.RecipientPhone,
							RecipientAddress = dto.RecipientAddress,
							CreatedAt = DateTime.Now
						};

						_context.Orders.Add(order);
						await _context.SaveChangesAsync();

						// --- E. 鎖定優惠券 (狀態改為 3: 鎖定中) ---
						if (dto.CouponId.HasValue)
						{
							bool locked = await _couponService.LockCouponAsync(dto.UserId, dto.CouponId.Value, order.Id);
							if (!locked) return (false, "優惠券已被鎖定或無效", null);
						}

						// --- F. 建立訂單明細並計算分攤 (Pro-rating) ---
						foreach (var item in dto.Items)
						{
							decimal itemSubtotal = item.UnitPrice * item.Quantity;
							decimal? allocatedDiscount = null;

							if (subtotal > 0 && couponDiscountAmount > 0)
							{
								// 依小計比例分攤優惠券折扣
								allocatedDiscount = Math.Round((itemSubtotal / subtotal) * couponDiscountAmount, 2);
							}

							_context.OrderDetails.Add(new OrderDetail
							{
								OrderId = order.Id,
								ProductId = item.ProductId,
								// 如果 VariantId 為 0 或負數，代表無規格，設為 null 避免外鍵衝突
								VariantId = item.VariantId > 0 ? item.VariantId : null,
								Price = item.UnitPrice,
								Quantity = item.Quantity,
								ProductName = item.ProductName ?? "未知商品",
								VariantName = item.VariantName ?? "預設規格",
								SkuCode = "", // 補上空字串避免資料庫 Not Null 報錯
								CoverImage = "" // 補上空字串避免資料庫 Not Null 報錯
							});
						}

						// --- G. 建立金流紀錄 ---
						string merchantTradeNo = _paymentService.GenerateMerchantTradeNo(order);
						var paymentLog = new PaymentLog
						{
							OrderId = order.Id,
							MerchantTradeNo = merchantTradeNo,
							TradeNo = "", // 補上空字串避免資料庫 Not Null 報錯
							RtnMsg = "等待付款中", // 補上預設訊息
							PaymentType = "CreditCard", // 補上預設付款類型
							TradeAmt = order.FinalAmount,
							CreatedAt = DateTime.Now
						};

						_context.PaymentLogs.Add(paymentLog);

						await _context.SaveChangesAsync();
						await transaction.CommitAsync();

						return (true, "訂單已建立，請前往付款", orderNumber);
					}
					catch (Exception ex)
					{
						await transaction.RollbackAsync();
						// 取得最底層的錯誤訊息
						var msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
						return (false, $"結帳失敗: {msg}", null);
					}
				}
			});
		}
	}
}
