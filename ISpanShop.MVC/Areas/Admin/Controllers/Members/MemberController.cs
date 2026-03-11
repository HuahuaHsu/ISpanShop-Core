using ISpanShop.Models.DTOs.Common;
using ISpanShop.MVC.Areas.Admin.Models.Members;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ISpanShop.MVC.Areas.Admin.Controllers.Members
{
	[Area("Admin")]
	[Route("Admin/Members")]
	public class MemberController : Controller
	{
		// ===================================================================
		// Static mock data store — 跨 request 持久，模擬資料庫行為
		// 實際專案請替換為 DbContext / Repository 注入
		// ===================================================================
		private static readonly List<MemberItemVm> _mockStore = new List<MemberItemVm>
		{
			new MemberItemVm { UserId = 1, Account = "jackchan",    FullName = "陳大文", Email = "jackchan@example.com",  PhoneNumber = "0912-345-678", AvatarUrl = "https://i.pravatar.cc/150?img=12", IsSeller = false, LevelName = "金牌會員", PointBalance = 1200, IsBlacklisted = false, City = "台北市", Region = "大安區",  Street = "忠孝東路四段 100 號" },
			new MemberItemVm { UserId = 2, Account = "mary.lee",    FullName = "李小美", Email = "mary.lee@example.com",  PhoneNumber = "0923-456-789", AvatarUrl = "https://i.pravatar.cc/150?img=5",  IsSeller = true,  LevelName = "銀牌會員", PointBalance = 800,  IsBlacklisted = false, City = "新北市", Region = "板橋區",  Street = "文化路一段 25 號" },
			new MemberItemVm { UserId = 3, Account = "johnwang",    FullName = "王小明", Email = "johnwang@example.com",  PhoneNumber = "0934-567-890", AvatarUrl = "https://i.pravatar.cc/150?img=33", IsSeller = true,  LevelName = "金牌會員", PointBalance = 2500, IsBlacklisted = false, City = "台中市", Region = "西屯區",  Street = "台灣大道三段 888 號" },
			new MemberItemVm { UserId = 4, Account = "linda.liu",   FullName = "劉雅婷", Email = "linda.liu@example.com", PhoneNumber = "0945-678-901", AvatarUrl = "https://i.pravatar.cc/150?img=20", IsSeller = false, LevelName = "銅牌會員", PointBalance = 350,  IsBlacklisted = false, City = "高雄市", Region = "苓雅區",  Street = "四維三路 6 號" },
			new MemberItemVm { UserId = 5, Account = "kevin.hsu",   FullName = "許志明", Email = "kevin.hsu@example.com", PhoneNumber = "0956-789-012", AvatarUrl = "https://i.pravatar.cc/150?img=51", IsSeller = false, LevelName = "銀牌會員", PointBalance = 600,  IsBlacklisted = true,  City = "桃園市", Region = "中壢區",  Street = "中山路 200 號" },
			new MemberItemVm { UserId = 6, Account = "amy.chen",    FullName = "陳美玲", Email = "amy.chen@example.com",  PhoneNumber = "0967-890-123", AvatarUrl = "https://i.pravatar.cc/150?img=9",  IsSeller = true,  LevelName = "金牌會員", PointBalance = 3200, IsBlacklisted = false, City = "台北市", Region = "信義區",  Street = "松壽路 10 號" },
			new MemberItemVm { UserId = 7, Account = "david.chang", FullName = "張大衛", Email = "david.chang@example.com",PhoneNumber = "0978-901-234", AvatarUrl = "https://i.pravatar.cc/150?img=68", IsSeller = false, LevelName = "銅牌會員", PointBalance = 150,  IsBlacklisted = false, City = "台南市", Region = "永康區",  Street = "中正路 300 號" },
			new MemberItemVm { UserId = 8, Account = "sophia.wu",   FullName = "吳雅文", Email = "sophia.wu@example.com", PhoneNumber = "0989-012-345", AvatarUrl = "https://i.pravatar.cc/150?img=47", IsSeller = true,  LevelName = "銀牌會員", PointBalance = 950,  IsBlacklisted = false, City = "新北市", Region = "新店區",  Street = "北新路三段 88 號" }
		};

		// ===================================================================
		// Index — 會員列表
		// ===================================================================
		[HttpGet]
		public IActionResult Index(string keyword = "", int page = 1, int pageSize = 10)
		{
			var query = _mockStore.AsQueryable();

			if (!string.IsNullOrWhiteSpace(keyword))
			{
				query = query.Where(m =>
					m.Account.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
					m.FullName.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
					m.PhoneNumber.Contains(keyword, StringComparison.OrdinalIgnoreCase));
			}

			var totalCount = query.Count();
			var pagedMembers = query
				.Skip((page - 1) * pageSize)
				.Take(pageSize)
				.ToList();

			var viewModel = new MemberIndexVm
			{
				PagedResult = new PagedResult<MemberItemVm>
				{
					Data = pagedMembers,
					TotalCount = totalCount,
					CurrentPage = page,
					PageSize = pageSize
				},
				Keyword = keyword
			};

			return View("~/Areas/Admin/Views/Member/Index.cshtml", viewModel);
		}

		// ===================================================================
		// Edit GET — 完整編輯頁（fallback，非 Drawer 使用）
		// ===================================================================
		[HttpGet("Edit/{id}")]
		public IActionResult Edit(int id)
		{
			var member = _mockStore.FirstOrDefault(m => m.UserId == id);
			if (member == null) return NotFound();

			var vm = MapToEditVm(member);
			return View("~/Areas/Admin/Views/Member/Edit.cshtml", vm);
		}

		// ===================================================================
		// EditPartial GET — 回傳 Partial View 給 Side Drawer 使用
		// ===================================================================
		[HttpGet("EditPartial/{id}")]
		public IActionResult EditPartial(int id)
		{
			var member = _mockStore.FirstOrDefault(m => m.UserId == id);
			if (member == null) return NotFound();

			var vm = MapToEditVm(member);
			return PartialView("~/Areas/Admin/Views/Member/_EditPartial.cshtml", vm);
		}

		// ===================================================================
		// Edit POST — 儲存變更（同時支援一般 POST 及 AJAX POST）
		// ===================================================================
		[HttpPost("Edit/{id}")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, MemberEditVm model)
		{
			// 移除不在畫面上的欄位，避免不必要的驗證失敗
			ModelState.Remove("Account");
			ModelState.Remove("LevelName");
			ModelState.Remove("PointBalance");
			ModelState.Remove("AvatarFile");
			ModelState.Remove("AvatarUrl");
			ModelState.Remove("CityOptions");
			ModelState.Remove("RegionOptions");

			bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";

			if (id != model.UserId)
			{
				if (isAjax) return Json(new { success = false, message = "ID 不匹配，請重新操作" });
				return NotFound();
			}

			if (!ModelState.IsValid)
			{
				var errors = ModelState.Values
					.SelectMany(v => v.Errors)
					.Select(e => e.ErrorMessage)
					.ToList();

				if (isAjax) return Json(new { success = false, message = "驗證失敗：" + string.Join("、", errors) });

				TempData["Error"] = "表單驗證失敗：" + string.Join("、", errors);
				model.CityOptions = GetCityOptions();
				model.RegionOptions = GetRegionOptions(model.City ?? "台北市");
				return View("~/Areas/Admin/Views/Member/Edit.cshtml", model);
			}

			try
			{
				// 處理圖片上傳
				string? newAvatarUrl = null;
				if (model.AvatarFile != null && model.AvatarFile.Length > 0)
				{
					var allowedExt = new[] { ".jpg", ".jpeg", ".png", ".gif" };
					var ext = Path.GetExtension(model.AvatarFile.FileName).ToLowerInvariant();

					if (!allowedExt.Contains(ext))
					{
						var msg = "圖片格式不正確，只允許 jpg, jpeg, png, gif";
						if (isAjax) return Json(new { success = false, message = msg });
						TempData["Error"] = msg;
						model.CityOptions = GetCityOptions();
						model.RegionOptions = GetRegionOptions(model.City ?? "台北市");
						return View("~/Areas/Admin/Views/Member/Edit.cshtml", model);
					}

					if (model.AvatarFile.Length > 5 * 1024 * 1024)
					{
						var msg = "圖片檔案過大，不能超過 5MB";
						if (isAjax) return Json(new { success = false, message = msg });
						TempData["Error"] = msg;
						model.CityOptions = GetCityOptions();
						model.RegionOptions = GetRegionOptions(model.City ?? "台北市");
						return View("~/Areas/Admin/Views/Member/Edit.cshtml", model);
					}

					var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "avatars");
					Directory.CreateDirectory(uploadsFolder);
					var uniqueFileName = $"{Guid.NewGuid()}{ext}";
					var filePath = Path.Combine(uploadsFolder, uniqueFileName);
					using var fileStream = new FileStream(filePath, FileMode.Create);
					await model.AvatarFile.CopyToAsync(fileStream);
					newAvatarUrl = $"/images/avatars/{uniqueFileName}";
				}

				// ── 更新 static store（模擬資料庫 SaveChanges）──
				var member = _mockStore.FirstOrDefault(m => m.UserId == id);
				if (member == null)
				{
					if (isAjax) return Json(new { success = false, message = "找不到該會員" });
					return NotFound();
				}

				member.FullName      = model.FullName     ?? member.FullName;
				member.Email         = model.Email        ?? member.Email;
				member.PhoneNumber   = model.PhoneNumber  ?? member.PhoneNumber;
				member.IsBlacklisted = model.IsBlacklisted;
				member.City          = model.City         ?? member.City;
				member.Region        = model.Region       ?? member.Region;
				member.Street        = model.Street       ?? member.Street;

				if (!string.IsNullOrWhiteSpace(newAvatarUrl))
					member.AvatarUrl = newAvatarUrl;

				// ── 回應 ──
				if (isAjax)
				{
					return Json(new
					{
						success = true,
						message = $"會員「{member.FullName}」資料更新成功！",
						member = new
						{
							userId       = member.UserId,
							fullName     = member.FullName,
							email        = member.Email,
							phoneNumber  = member.PhoneNumber,
							isBlacklisted = member.IsBlacklisted,
							isSeller     = member.IsSeller,
							levelName    = member.LevelName,
							pointBalance = member.PointBalance,
							avatarUrl    = member.AvatarUrl,
							account      = member.Account
						}
					});
				}

				TempData["Success"] = $"會員「{member.FullName}」資料更新成功！";
				return RedirectToAction(nameof(Index));
			}
			catch (Exception ex)
			{
				if (isAjax) return Json(new { success = false, message = $"儲存失敗：{ex.Message}" });
				TempData["Error"] = $"儲存失敗：{ex.Message}";
				model.CityOptions = GetCityOptions();
				model.RegionOptions = GetRegionOptions(model.City ?? "台北市");
				return View("~/Areas/Admin/Views/Member/Edit.cshtml", model);
			}
		}

		// ===================================================================
		// GetRegions GET — 取得縣市對應的區域（AJAX 使用）
		// ===================================================================
		[HttpGet("GetRegions")]
		public IActionResult GetRegions(string city)
		{
			var options = GetRegionOptions(city ?? "");
			return Json(options.Select(r => new { value = r.Value, text = r.Text }));
		}

		// ===================================================================
		// 私有輔助方法
		// ===================================================================
		private MemberEditVm MapToEditVm(MemberItemVm member)
		{
			// 使用會員自己的縣市載入正確的區域列表
			var city = string.IsNullOrWhiteSpace(member.City) ? "台北市" : member.City;
			return new MemberEditVm
			{
				UserId        = member.UserId,
				Account       = member.Account,
				FullName      = member.FullName,
				Email         = member.Email,
				PhoneNumber   = member.PhoneNumber,
				AvatarUrl     = member.AvatarUrl,
				LevelName     = member.LevelName,
				PointBalance  = member.PointBalance,
				City          = member.City,
				Region        = member.Region,
				Street        = member.Street,
				IsBlacklisted = member.IsBlacklisted,
				IsSeller      = member.IsSeller,
				CityOptions   = GetCityOptions(),
				RegionOptions = GetRegionOptions(city)   // ← 依該會員縣市載入區域
			};
		}

		private List<SelectListItem> GetCityOptions()
		{
			return new List<SelectListItem>
			{
				new SelectListItem { Text = "請選擇縣市", Value = "" },
				new SelectListItem { Text = "台北市", Value = "台北市" },
				new SelectListItem { Text = "新北市", Value = "新北市" },
				new SelectListItem { Text = "桃園市", Value = "桃園市" },
				new SelectListItem { Text = "台中市", Value = "台中市" },
				new SelectListItem { Text = "台南市", Value = "台南市" },
				new SelectListItem { Text = "高雄市", Value = "高雄市" }
			};
		}

		private List<SelectListItem> GetRegionOptions(string city)
		{
			var regions = city switch
			{
				"台北市" => new[] { "中正區", "大同區", "中山區", "松山區", "大安區", "萬華區", "信義區", "士林區", "北投區", "內湖區", "南港區", "文山區" },
				"新北市" => new[] { "板橋區", "三重區", "中和區", "永和區", "新莊區", "新店區", "土城區", "蘆洲區", "樹林區", "汐止區" },
				"桃園市" => new[] { "桃園區", "中壢區", "平鎮區", "八德區", "楊梅區", "蘆竹區", "龜山區", "龍潭區", "大溪區" },
				"台中市" => new[] { "中區", "東區", "南區", "西區", "北區", "西屯區", "南屯區", "北屯區", "豐原區", "太平區" },
				"台南市" => new[] { "中西區", "東區", "南區", "北區", "安平區", "安南區", "永康區", "歸仁區", "新化區", "左鎮區" },
				"高雄市" => new[] { "新興區", "前金區", "苓雅區", "鹽埕區", "鼓山區", "旗津區", "前鎮區", "三民區", "左營區", "楠梓區" },
				_        => Array.Empty<string>()
			};

			var options = new List<SelectListItem> { new SelectListItem { Text = "請選擇區域", Value = "" } };
			options.AddRange(regions.Select(r => new SelectListItem { Text = r, Value = r }));
			return options;
		}
	}
}
