using ISpanShop.Common.Helpers;
using ISpanShop.Models.DTOs.Members;
using ISpanShop.Services.Members;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ISpanShop.MVC.Controllers.Api
{
    [ApiController]
    [Route("api/member/addresses")]
    [Authorize(AuthenticationSchemes = "FrontendJwt")]
    public class FrontAddressController : ControllerBase
    {
        private readonly IAddressService _addressService;

        public FrontAddressController(IAddressService addressService)
        {
            _addressService = addressService;
        }

        /// <summary>
        /// 取得目前登入使用者的所有收件地址
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetList()
        {
            var userId = User.GetUserId();
            if (!userId.HasValue) return Unauthorized();

            var addresses = await _addressService.GetAddressListAsync(userId.Value);
            return Ok(addresses);
        }

        /// <summary>
        /// 新增收件地址
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateAddressDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = User.GetUserId();
            if (!userId.HasValue) return Unauthorized();

            var result = await _addressService.CreateAddressAsync(userId.Value, dto);
            return Ok(result);
        }

        /// <summary>
        /// 更新收件地址
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateAddressDto dto)
        {
            if (id != dto.Id) return BadRequest(new { message = "ID 不符" });
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = User.GetUserId();
            if (!userId.HasValue) return Unauthorized();

            var success = await _addressService.UpdateAddressAsync(userId.Value, dto);
            if (!success) return NotFound(new { message = "找不到該地址或無權限修改" });

            return Ok(new { message = "更新成功" });
        }

        /// <summary>
        /// 刪除收件地址
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.GetUserId();
            if (!userId.HasValue) return Unauthorized();

            var success = await _addressService.DeleteAddressAsync(userId.Value, id);
            if (!success) return NotFound(new { message = "找不到該地址或無權限刪除" });

            return Ok(new { message = "刪除成功" });
        }

        /// <summary>
        /// 設定為預設地址
        /// </summary>
        [HttpPatch("{id}/default")]
        public async Task<IActionResult> SetDefault(int id)
        {
            var userId = User.GetUserId();
            if (!userId.HasValue) return Unauthorized();

            var success = await _addressService.SetDefaultAddressAsync(userId.Value, id);
            if (!success) return NotFound(new { message = "找不到該地址或無權限設定" });

            return Ok(new { message = "設定成功" });
        }
    }
}
