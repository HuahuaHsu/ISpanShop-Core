using Microsoft.AspNetCore.Mvc;

namespace ISpanShop.MVC.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class TestApiController : ControllerBase
	{
		[HttpGet]
		public IActionResult Get()
		{
			// 回傳一段簡單的 JSON 資料
			return Ok(new
			{
				success = true,
				message = "恭喜！CORS 設定大成功，前後端連線無障礙！"
			});
		}
	}
}