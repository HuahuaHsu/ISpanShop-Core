using ISpanShop.Common.Enums;
using ISpanShop.Models.DTOs.Stores;
using ISpanShop.Services.Stores;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
using ISpanShop.Models.DTOs.Orders;

namespace ISpanShop.MVC.Controllers.Api
{
    [ApiController]
    [Route("api/front/store")]
    [Authorize(AuthenticationSchemes = "FrontendJwt")]
    public class FrontStoreController : ControllerBase
    {
        private readonly IFrontStoreService _storeService;

        public FrontStoreController(IFrontStoreService storeService)
        {
            _storeService = storeService;
        }

        /// <summary>
        /// 取得賣場訂單列表 (支援分頁)
        /// </summary>
        [HttpGet("orders")]
        public async Task<IActionResult> GetSellerOrders([FromQuery] OrderStatus? status = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string keyword = null)
        {
            try
            {
                var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdStr, out int userId)) return Unauthorized();

                var pagedResult = await _storeService.GetSellerOrdersAsync(userId, status, page, pageSize, keyword);
                return Ok(pagedResult);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// 取得賣家視角的訂單詳情
        /// </summary>
        [HttpGet("orders/{orderId}")]
        public async Task<IActionResult> GetSellerOrderDetail(long orderId)
        {
            try
            {
                var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdStr, out int userId)) return Unauthorized();

                var detail = await _storeService.GetSellerOrderDetailAsync(userId, orderId);
                return Ok(detail);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// 更新訂單狀態 (例如出貨)
        /// </summary>
        [HttpPut("orders/{orderId}/status")]
        public async Task<IActionResult> UpdateOrderStatus(long orderId, [FromBody] UpdateStatusRequest request)
        {
            try
            {
                var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdStr, out int userId)) return Unauthorized();

                var success = await _storeService.UpdateOrderStatusAsync(userId, orderId, request.Status);
                return success ? Ok(new { message = "狀態更新成功" }) : BadRequest(new { message = "更新失敗" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        public class UpdateStatusRequest
        {
            public OrderStatus Status { get; set; }
        }

        /// <summary>
        /// 上傳賣場 Logo...
        [HttpPost("upload-logo")]
        public async Task<IActionResult> UploadLogo(IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest(new { message = "請選擇檔案" });

            try
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "stores");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var url = $"/uploads/stores/{fileName}";
                return Ok(new { url });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// 申請成為賣家
        /// </summary>
        [HttpPost("apply")]
        public async Task<IActionResult> ApplyStore([FromBody] StoreApplyRequestDto dto)
        {
            try
            {
                var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdStr, out int userId))
                {
                    return Unauthorized(new { message = "無效的使用者身分" });
                }

                var success = await _storeService.ApplyStoreAsync(userId, dto);
                return success ? Ok(new { message = "申請已送出，請靜候管理員審核" }) : BadRequest(new { message = "申請失敗" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// 取得賣場申請狀態
        /// </summary>
        [HttpGet("status")]
        public async Task<IActionResult> GetStoreStatus()
        {
            try
            {
                var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdStr, out int userId))
                {
                    return Unauthorized(new { message = "無效的使用者身分" });
                }

                var (status, isBanned) = await _storeService.GetStoreStatusDetailAsync(userId);
                return Ok(new { status, isBanned });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// 取得賣場儀表板數據 (僅限已通過審核的賣家)
        /// </summary>
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboardData([FromQuery] int days = 7)
        {
            try
            {
                var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdStr, out int userId))
                {
                    return Unauthorized(new { message = "無效的使用者身分" });
                }

                // 檢查是否為賣家
                var status = await _storeService.GetStoreStatusAsync(userId);
                if (status != "Approved")
                {
                    return Forbid("您的賣場尚未通過審核或已被停權");
                }

                var data = await _storeService.GetDashboardDataAsync(userId, days);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// 取得賣場資訊 (用於編輯頁面)
        /// </summary>
        [HttpGet("profile")]
        public async Task<IActionResult> GetStoreInfo()
        {
            try
            {
                var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdStr, out int userId)) return Unauthorized();

                var info = await _storeService.GetStoreInfoAsync(userId);
                return Ok(info);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// 更新賣場資訊
        /// </summary>
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateStoreInfo([FromBody] UpdateStoreInfoRequestDto dto)
        {
            try
            {
                var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdStr, out int userId)) return Unauthorized();

                var success = await _storeService.UpdateStoreInfoAsync(userId, dto);
                return success ? Ok(new { message = "賣場資訊已更新" }) : BadRequest(new { message = "更新失敗" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// 取得待出貨訂單數量
        /// </summary>
        [HttpGet("pending-orders")]
        public async Task<IActionResult> GetPendingOrders()
        {
            try
            {
                var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdStr, out int userId)) return Unauthorized();

                var count = await _storeService.GetPendingOrdersCountAsync(userId);
                return Ok(new { count });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// 取得賣場退貨申請列表
        /// </summary>
        [HttpGet("returns")]
        public async Task<IActionResult> GetSellerReturns([FromQuery] bool? isProcessed = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdStr, out int userId)) return Unauthorized();

                var result = await _storeService.GetSellerReturnsAsync(userId, isProcessed, page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// 取得退貨申請詳情
        /// </summary>
        [HttpGet("returns/{orderId}")]
        public async Task<IActionResult> GetSellerReturnDetail(long orderId)
        {
            try
            {
                var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdStr, out int userId)) return Unauthorized();

                var result = await _storeService.GetSellerReturnDetailAsync(userId, orderId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// 審核退貨申請
        /// </summary>
        [HttpPost("returns/{orderId}/review")]
        public async Task<IActionResult> ReviewReturnRequest(long orderId, [FromBody] ReviewReturnRequestDto dto)
        {
            try
            {
                var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdStr, out int userId)) return Unauthorized();

                var success = await _storeService.ReviewReturnRequestAsync(userId, orderId, dto);
                return success ? Ok(new { message = "審核結果已提交" }) : BadRequest(new { message = "審核提交失敗" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// 賣家回覆評價
        /// </summary>
        [HttpPost("reviews/reply")]
        public async Task<IActionResult> ReplyToReview([FromBody] SellerReplyDto dto)
        {
            try
            {
                var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdStr, out int userId)) return Unauthorized();

                var success = await _storeService.ReplyToReviewAsync(userId, dto);
                return success ? Ok(new { message = "回覆成功" }) : BadRequest(new { message = "回覆失敗" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
