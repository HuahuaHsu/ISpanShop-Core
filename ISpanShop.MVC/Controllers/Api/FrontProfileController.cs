using ISpanShop.Models.DTOs.Members;
using ISpanShop.Services.Members;
using Microsoft.AspNetCore.Mvc;

namespace ISpanShop.MVC.Controllers.Api
{
    [ApiController]
    [Route("api/front/profile")]
    public class FrontProfileController : ControllerBase
    {
        private readonly IMemberService _memberService;

        public FrontProfileController(IMemberService memberService)
        {
            _memberService = memberService;
        }

        /// <summary>
        /// 取得個人詳細資料
        /// </summary>
        [HttpGet("{id}")]
        public IActionResult GetProfile(int id)
        {
            try
            {
                var profile = _memberService.GetMemberById(id);
                if (profile == null) return NotFound(new { message = "找不到該會員" });

                return Ok(profile);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// 更新個人資料
        /// </summary>
        [HttpPut("{id}")]
        public IActionResult UpdateProfile(int id, [FromBody] MemberDto dto)
        {
            try
            {
                // 如果 Body 中的 Id 為 0，自動補上路由中的 ID
                if (dto.Id == 0) dto.Id = id;

                if (id != dto.Id) 
                    return BadRequest(new { message = $"ID 不符: 路由={id}, 資料={dto.Id}" });

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
