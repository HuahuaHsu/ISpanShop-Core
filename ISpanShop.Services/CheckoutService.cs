using ISpanShop.Models.DTOs;
using ISpanShop.Models.EfModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISpanShop.Services
{
	public class CheckoutService
	{
		private readonly ISpanShopDBContext _context;
		private readonly PointService _pointService;
		private readonly PaymentService _paymentService;

		public CheckoutService(ISpanShopDBContext context, PointService pointService, PaymentService paymentService)
		{
			_context = context;
			_pointService = pointService;
			_paymentService = paymentService;
		}

		public async Task<(bool IsSuccess, string Message, string OrderNumber)> CreateOrderAsync(CheckoutRequestDTO dto)
		{
			// 1. 使用 Transaction 確保訂單與點數異動的一致性
			using (var transaction = await _context.Database.BeginTransactionAsync())
			{
				try
				{
					// --- A. 計算原始總額 ---
					decimal subtotal = dto.Items.Sum(x => x.UnitPrice * x.Quantity);
					decimal shippingFee = 60; // 假設運費固定
					decimal discountAmount = 0;

					// --- B. 處理點數折抵邏輯 ---
					if (dto.UsePoints)
					{
						int balance = await _pointService.GetBalanceAsync(dto.UserId);
						// 假設 1 點抵 1 元，且最多折抵訂單金額
						discountAmount = Math.Min(balance, subtotal);

						if (discountAmount > 0)
						{
							var pointRes = await _pointService.UpdatePointsAsync(new PointUpdateDTO
							{
								UserId = dto.UserId,
								ChangeAmount = -(int)discountAmount,
								Description = "訂單折抵",
								OrderNumber = "Pending" // 稍後更新
							});

							if (!pointRes.IsSuccess) return (false, pointRes.Message, null);
						}
					}

					// --- C. 建立訂單主表 ---
					var orderNumber = DateTime.Now.ToString("yyyyMMddHHmmss") + dto.UserId.ToString().PadLeft(4, '0');
					var order = new Order
					{
						OrderNumber = orderNumber,
						UserId = dto.UserId,
						StoreId = dto.StoreId,
						TotalAmount = subtotal,
						ShippingFee = shippingFee,
						PointDiscount = (int)discountAmount,
						FinalAmount = (subtotal + shippingFee) - discountAmount,
						Status = 0, // 0: 待付款
						RecipientName = dto.RecipientName,
						RecipientPhone = dto.RecipientPhone,
						RecipientAddress = dto.RecipientAddress,
						CreatedAt = DateTime.Now
					};

					_context.Orders.Add(order);
					await _context.SaveChangesAsync(); // 取得 Order.Id

					// --- D. 建立訂單明細 ---
					foreach (var item in dto.Items)
					{
						_context.OrderDetails.Add(new OrderDetail
						{
							OrderId = order.Id,
							ProductId = item.ProductId,
							VariantId = item.VariantId,
							Price = item.UnitPrice,
							Quantity = item.Quantity
							// 其他欄位如 ProductName 可從 DB 抓取補上
						});
					}
					// --- E. 預先建立金流紀錄 (PaymentLog) ---
					// 這裡產生的 MerchantTradeNo 必須跟傳給綠界的一模一樣
					string merchantTradeNo = _paymentService.GenerateMerchantTradeNo();

					var paymentLog = new PaymentLog
					{
						OrderId = order.Id,
						MerchantTradeNo = merchantTradeNo,
						TradeAmt = order.FinalAmount,
						CreatedAt = DateTime.Now,
						// 其他欄位如 TradeNo, PaymentDate 等到回傳時再補填
					};

					_context.PaymentLogs.Add(paymentLog);

					// 為了讓後續流程拿得到這個單號，我們可以把它包在 Result 回傳
					await _context.SaveChangesAsync();
					await transaction.CommitAsync();

					return (true, "訂單已建立", merchantTradeNo); // 改回傳 MerchantTradeNo

					await _context.SaveChangesAsync();
					await transaction.CommitAsync();

					return (true, "訂單已建立，請前往付款", orderNumber);
				}
				catch (Exception ex)
				{
					await transaction.RollbackAsync();
					return (false, $"結帳失敗: {ex.Message}", null);
				}
			}
		}
	}
}
