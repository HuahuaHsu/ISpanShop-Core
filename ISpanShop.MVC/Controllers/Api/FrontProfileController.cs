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
        /// 更新個人資料 (使用專用 DTO)
        /// </summary>
        [HttpPut("{id}")]
        public IActionResult UpdateProfile(int id, [FromBody] UpdateMemberProfileDto dto)
        {
            try
            {
                if (id != dto.Id) 
                    return BadRequest(new { message = "會員 ID 不符" });

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
