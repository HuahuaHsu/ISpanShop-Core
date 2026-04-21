using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ISpanShop.MVC.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class UploadController : ControllerBase
    {
        private readonly string _uploadPath;

        public UploadController()
        {
            // 設定上傳路徑為 wwwroot/uploads
            _uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            if (!Directory.Exists(_uploadPath))
            {
                Directory.CreateDirectory(_uploadPath);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Upload(List<IFormFile> files)
        {
            if (files == null || files.Count == 0) return BadRequest("沒有選擇檔案");

            var urls = new List<string>();

            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    // 隨機檔名防止重複
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    var filePath = Path.Combine(_uploadPath, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    // 回傳相對路徑
                    urls.Add("/uploads/" + fileName);
                }
            }

            return Ok(new { urls });
        }
    }
}
