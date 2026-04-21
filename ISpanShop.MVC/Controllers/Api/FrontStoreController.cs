using ISpanShop.Models.DTOs.Stores;
using ISpanShop.Services.Stores;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;

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
        /// 上傳賣場 Logo
        /// </summary>
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

                var status = await _storeService.GetStoreStatusAsync(userId);
                return Ok(new { status });
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
        public async Task<IActionResult> GetDashboardData()
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

                var data = await _storeService.GetDashboardDataAsync(userId);
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
    }
}
