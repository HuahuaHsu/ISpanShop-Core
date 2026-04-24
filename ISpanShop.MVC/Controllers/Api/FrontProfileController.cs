using ISpanShop.Common.Helpers;
using ISpanShop.Models.DTOs.Auth;
using ISpanShop.Models.DTOs.Members;
using ISpanShop.Services.Members;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ISpanShop.MVC.Controllers.Api
{
    [ApiController]
    [Route("api/front/profile")]
    [Authorize(AuthenticationSchemes = "FrontendJwt")]
    public class FrontProfileController : ControllerBase
    {
        private readonly IMemberService _memberService;
        private readonly IAccountService _accountService;
        private readonly IWebHostEnvironment _environment;

        public FrontProfileController(IMemberService memberService, IAccountService accountService, IWebHostEnvironment environment)
        {
            _memberService = memberService;
            _accountService = accountService;
            _environment = environment;
        }

        /// <summary>
        /// 上傳大頭照
        /// </summary>
        [HttpPost("upload")]
        public async Task<IActionResult> UploadAvatar(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "請選擇檔案" });

            try
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                var extension = Path.GetExtension(file.FileName).ToLower();
                if (!allowedExtensions.Contains(extension))
                    return BadRequest(new { message = "僅支援 JPG, JPEG, PNG 格式" });

                var uploadDir = Path.Combine(_environment.WebRootPath, "uploads", "avatars");
                if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

                var fileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadDir, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var url = $"/uploads/avatars/{fileName}";
                return Ok(new { url });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"上傳失敗: {ex.Message}" });
            }
        }

        /// <summary>
        /// 取得個人詳細資料 (從 Token 取得身分)
        /// </summary>
        [HttpGet]
        public IActionResult GetProfile()
        {
            try
            {
                var userId = User.GetUserId();
                if (userId == null) return Unauthorized(new { message = "未授權" });

                var profile = _memberService.GetMemberById(userId.Value);
                if (profile == null) return NotFound(new { message = "找不到該會員" });

                return Ok(profile);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// 更新個人資料 (從 Token 取得身分)
        /// </summary>
        [HttpPut]
        public IActionResult UpdateProfile([FromBody] UpdateMemberProfileDto dto)
        {
            try
            {
                var userId = User.GetUserId();
                if (userId == null) return Unauthorized(new { message = "未授權" });

                // 強制將 ID 設定為 Token 中的 ID，確保安全性
                dto.Id = userId.Value;

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                _memberService.UpdateMemberProfile(dto);
                return Ok(new { message = "更新成功" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
