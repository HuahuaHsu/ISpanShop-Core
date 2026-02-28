using Microsoft.AspNetCore.Mvc;
using ISpanShop.Models.DTOs;
using ISpanShop.Services;
using ISpanShop.Services.Interfaces;

namespace ISpanShop.WebAPI.Controllers
{
    /// <summary>
    /// 商品 API 控制器 - 提供商品相關的 RESTful API 端點
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsApiController : ControllerBase
    {
        private readonly IProductService _productService;

        /// <summary>
        /// 建構子 - 注入 ProductService
        /// </summary>
        /// <param name="productService">商品 Service</param>
        public ProductsApiController(IProductService productService)
        {
            _productService = productService;
        }

        /// <summary>
        /// 建立新商品
        /// </summary>
        /// <param name="dto">商品建立 DTO (使用 FormData 以支持圖片上傳)</param>
        /// <returns>建立成功訊息</returns>
        [HttpPost]
        public IActionResult CreateProduct([FromForm] ProductCreateDto dto)
        {
            _productService.CreateProduct(dto);
            return Ok(new { message = "商品建立成功" });
        }
    }
}
