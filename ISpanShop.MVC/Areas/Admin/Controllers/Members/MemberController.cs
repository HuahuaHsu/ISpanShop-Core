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
		[HttpGet]
		public IActionResult Index(string keyword = "", int page = 1, int pageSize = 10)
		{
			// === 建立 Mock Data 供測試使用 ===
			var allMembers = new List<MemberItemVm>
			{
				new MemberItemVm
				{
					UserId = 1,
					Account = "jackchan",
					FullName = "陳大文",
					Email = "jackchan@example.com",
					PhoneNumber = "0912-345-678",
					AvatarUrl = "https://i.pravatar.cc/150?img=12",
					IsSeller = false,
					LevelName = "金牌會員",
					PointBalance = 1200,
					IsBlacklisted = false
				},
				new MemberItemVm
				{
					UserId = 2,
					Account = "mary.lee",
					FullName = "李小美",
					Email = "mary.lee@example.com",
					PhoneNumber = "0923-456-789",
					AvatarUrl = "https://i.pravatar.cc/150?img=5",
					IsSeller = true,
					LevelName = "銀牌會員",
					PointBalance = 800,
					IsBlacklisted = false
				},
				new MemberItemVm
				{
					UserId = 3,
					Account = "johnwang",
					FullName = "王小明",
					Email = "johnwang@example.com",
					PhoneNumber = "0934-567-890",
					AvatarUrl = "https://i.pravatar.cc/150?img=33",
					IsSeller = true,
					LevelName = "金牌會員",
					PointBalance = 2500,
					IsBlacklisted = false
				},
				new MemberItemVm
				{
					UserId = 4,
					Account = "linda.liu",
					FullName = "劉雅婷",
					Email = "linda.liu@example.com",
					PhoneNumber = "0945-678-901",
					AvatarUrl = "https://i.pravatar.cc/150?img=20",
					IsSeller = false,
					LevelName = "銅牌會員",
					PointBalance = 350,
					IsBlacklisted = false
				},
				new MemberItemVm
				{
					UserId = 5,
					Account = "kevin.hsu",
					FullName = "許志明",
					Email = "kevin.hsu@example.com",
					PhoneNumber = "0956-789-012",
					AvatarUrl = "https://i.pravatar.cc/150?img=51",
					IsSeller = false,
					LevelName = "銀牌會員",
					PointBalance = 600,
					IsBlacklisted = true
				},
				new MemberItemVm
				{
					UserId = 6,
					Account = "amy.chen",
					FullName = "陳美玲",
					Email = "amy.chen@example.com",
					PhoneNumber = "0967-890-123",
					AvatarUrl = "https://i.pravatar.cc/150?img=9",
					IsSeller = true,
					LevelName = "金牌會員",
					PointBalance = 3200,
					IsBlacklisted = false
				},
				new MemberItemVm
				{
					UserId = 7,
					Account = "david.chang",
					FullName = "張大衛",
					Email = "david.chang@example.com",
					PhoneNumber = "0978-901-234",
					AvatarUrl = "https://i.pravatar.cc/150?img=68",
					IsSeller = false,
					LevelName = "銅牌會員",
					PointBalance = 150,
					IsBlacklisted = false
				},
				new MemberItemVm
				{
					UserId = 8,
					Account = "sophia.wu",
					FullName = "吳雅文",
					Email = "sophia.wu@example.com",
					PhoneNumber = "0989-012-345",
					AvatarUrl = "https://i.pravatar.cc/150?img=47",
					IsSeller = true,
					LevelName = "銀牌會員",
					PointBalance = 950,
					IsBlacklisted = false
				}
			};

			// 關鍵字搜尋 (姓名、電話、帳號)
			var filteredMembers = allMembers.AsQueryable();
			if (!string.IsNullOrWhiteSpace(keyword))
			{
				filteredMembers = filteredMembers.Where(m =>
					m.Account.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
					m.FullName.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
					m.PhoneNumber.Contains(keyword, StringComparison.OrdinalIgnoreCase));
			}

			// 總筆數
			var totalCount = filteredMembers.Count();

			// 分頁
			var pagedMembers = filteredMembers
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

			// 明確指定使用 Member 資料夾 (單數)
			return View("~/Areas/Admin/Views/Member/Index.cshtml", viewModel);
		}

		[HttpGet("Edit/{id}")]
		public IActionResult Edit(int id)
		{
			// === 從 Mock Data 中查詢指定 ID 的會員 ===
			var allMembers = new List<MemberItemVm>
			{
				new MemberItemVm
				{
					UserId = 1,
					Account = "jackchan",
					FullName = "陳大文",
					Email = "jackchan@example.com",
					PhoneNumber = "0912-345-678",
					AvatarUrl = "https://i.pravatar.cc/150?img=12",
					IsSeller = false,
					LevelName = "金牌會員",
					PointBalance = 1200,
					IsBlacklisted = false
				},
				new MemberItemVm
				{
					UserId = 2,
					Account = "mary.lee",
					FullName = "李小美",
					Email = "mary.lee@example.com",
					PhoneNumber = "0923-456-789",
					AvatarUrl = "https://i.pravatar.cc/150?img=5",
					IsSeller = true,
					LevelName = "銀牌會員",
					PointBalance = 800,
					IsBlacklisted = false
				},
				new MemberItemVm
				{
					UserId = 3,
					Account = "johnwang",
					FullName = "王小明",
					Email = "johnwang@example.com",
					PhoneNumber = "0934-567-890",
					AvatarUrl = "https://i.pravatar.cc/150?img=33",
					IsSeller = true,
					LevelName = "金牌會員",
					PointBalance = 2500,
					IsBlacklisted = false
				},
				new MemberItemVm
				{
					UserId = 4,
					Account = "linda.liu",
					FullName = "劉雅婷",
					Email = "linda.liu@example.com",
					PhoneNumber = "0945-678-901",
					AvatarUrl = "https://i.pravatar.cc/150?img=20",
					IsSeller = false,
					LevelName = "銅牌會員",
					PointBalance = 350,
					IsBlacklisted = false
				},
				new MemberItemVm
				{
					UserId = 5,
					Account = "kevin.hsu",
					FullName = "許志明",
					Email = "kevin.hsu@example.com",
					PhoneNumber = "0956-789-012",
					AvatarUrl = "https://i.pravatar.cc/150?img=51",
					IsSeller = false,
					LevelName = "銀牌會員",
					PointBalance = 600,
					IsBlacklisted = true
				},
				new MemberItemVm
				{
					UserId = 6,
					Account = "amy.chen",
					FullName = "陳美玲",
					Email = "amy.chen@example.com",
					PhoneNumber = "0967-890-123",
					AvatarUrl = "https://i.pravatar.cc/150?img=9",
					IsSeller = true,
					LevelName = "金牌會員",
					PointBalance = 3200,
					IsBlacklisted = false
				},
				new MemberItemVm
				{
					UserId = 7,
					Account = "david.chang",
					FullName = "張大衛",
					Email = "david.chang@example.com",
					PhoneNumber = "0978-901-234",
					AvatarUrl = "https://i.pravatar.cc/150?img=68",
					IsSeller = false,
					LevelName = "銅牌會員",
					PointBalance = 150,
					IsBlacklisted = false
				},
				new MemberItemVm
				{
					UserId = 8,
					Account = "sophia.wu",
					FullName = "吳雅文",
					Email = "sophia.wu@example.com",
					PhoneNumber = "0989-012-345",
					AvatarUrl = "https://i.pravatar.cc/150?img=47",
					IsSeller = true,
					LevelName = "銀牌會員",
					PointBalance = 950,
					IsBlacklisted = false
				}
			};

			// ✅ 修正：使用傳入的 id 查詢指定會員（而不是永遠抓第一筆）
			var member = allMembers.FirstOrDefault(m => m.UserId == id);
			
			if (member == null)
			{
				return NotFound(); // 找不到該會員時返回 404
			}

			// 轉換為 EditVm
			var mockMember = new MemberEditVm
			{
				UserId = member.UserId,
				Account = member.Account,
				FullName = member.FullName,
				Email = member.Email,
				PhoneNumber = member.PhoneNumber,
				AvatarUrl = member.AvatarUrl,
				LevelName = member.LevelName,
				PointBalance = member.PointBalance,
				City = "台北市",  // Mock 資料預設值
				Region = "中正區",
				Street = "忠孝東路一段 100 號",
				IsBlacklisted = member.IsBlacklisted,
				IsSeller = member.IsSeller,
				CityOptions = GetCityOptions(),
				RegionOptions = GetRegionOptions("台北市")
			};

			// 實際開發時應從資料庫查詢
			// var user = await _context.Users.Include(...).FirstOrDefaultAsync(u => u.Id == id);
			// if (user == null) return NotFound();

			// 明確指定使用 Member 資料夾 (單數)
			return View("~/Areas/Admin/Views/Member/Edit.cshtml", mockMember);
		}

		[HttpPost("Edit/{id}")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, MemberEditVm model)
		{
			// ===================================================================
			// ✅ 步驟1：強制移除所有干擾驗證的項目
			// ===================================================================
			// 移除畫面上沒有提供輸入但模型卻要求必填的欄位
			ModelState.Remove("Account");           // 唯讀欄位（帳號不可變更）
			ModelState.Remove("LevelName");         // 唯讀欄位（會員等級由系統控制）
			ModelState.Remove("PointBalance");      // 唯讀欄位（點數由系統控制）
			ModelState.Remove("Password");          // 編輯頁面沒有密碼欄位
			ModelState.Remove("ConfirmPassword");   // 編輯頁面沒有確認密碼欄位
			ModelState.Remove("AvatarFile");        // 圖片上傳非必填
			ModelState.Remove("AvatarUrl");         // 圖片 URL 可為空
			ModelState.Remove("MemberLevel");       // 導覽屬性（若有）
			ModelState.Remove("CityOptions");       // 下拉選單選項
			ModelState.Remove("RegionOptions");     // 下拉選單選項
			ModelState.Remove("City");              // 地址選填
			ModelState.Remove("Region");            // 地址選填
			ModelState.Remove("Street");            // 地址選填

			// ✅ 步驟2：檢查 ID 是否匹配
			if (id != model.UserId)
			{
				TempData["Error"] = "ID 不匹配，請重新操作";
				return NotFound();
			}

			// ✅ 步驟3：驗證 ModelState（移除干擾項後再檢查）
			if (!ModelState.IsValid)
			{
				// 收集所有驗證錯誤
				var errors = ModelState.Values
					.SelectMany(v => v.Errors)
					.Select(e => e.ErrorMessage)
					.ToList();

				// 設定錯誤訊息到 TempData（不使用 JavaScript Alert）
				TempData["Error"] = "表單驗證失敗：" + string.Join("、", errors);

				// 重新填充下拉選單選項
				model.CityOptions = GetCityOptions();
				model.RegionOptions = GetRegionOptions(model.City ?? "台北市");

				// 返回編輯頁面，顯示錯誤訊息
				return View("~/Areas/Admin/Views/Member/Edit.cshtml", model);
			}

			try
			{
				// ===================================================================
				// ✅ 步驟4：處理圖片上傳（如果有上傳新圖片）
				// ===================================================================
				string? newAvatarUrl = null;

				if (model.AvatarFile != null && model.AvatarFile.Length > 0)
				{
					// 驗證檔案類型
					var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
					var fileExtension = Path.GetExtension(model.AvatarFile.FileName).ToLowerInvariant();

					if (!allowedExtensions.Contains(fileExtension))
					{
						TempData["Error"] = "圖片格式不正確，只允許 jpg, jpeg, png, gif 格式";
						model.CityOptions = GetCityOptions();
						model.RegionOptions = GetRegionOptions(model.City ?? "台北市");
						return View("~/Areas/Admin/Views/Member/Edit.cshtml", model);
					}

					// 驗證檔案大小 (限制 5MB)
					if (model.AvatarFile.Length > 5 * 1024 * 1024)
					{
						TempData["Error"] = "圖片檔案過大，不能超過 5MB";
						model.CityOptions = GetCityOptions();
						model.RegionOptions = GetRegionOptions(model.City ?? "台北市");
						return View("~/Areas/Admin/Views/Member/Edit.cshtml", model);
					}

					// 建立儲存目錄
					var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "avatars");
					if (!Directory.Exists(uploadsFolder))
					{
						Directory.CreateDirectory(uploadsFolder);
					}

					// 生成唯一檔名
					var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
					var filePath = Path.Combine(uploadsFolder, uniqueFileName);

					// 儲存檔案
					using (var fileStream = new FileStream(filePath, FileMode.Create))
					{
						await model.AvatarFile.CopyToAsync(fileStream);
					}

					// 記錄新的圖片 URL
					newAvatarUrl = $"/images/avatars/{uniqueFileName}";
				}

				// ===================================================================
				// ✅ 步驟5：「手動對照更新」- 從 Mock 資料中查詢並更新
				// ===================================================================
				// 注意：當前使用 Mock 資料，實際專案請改用資料庫查詢
				
				// Mock 資料列表（與 Index 和 GET Edit 保持一致）
				var allMembers = new List<MemberItemVm>
				{
					new MemberItemVm { UserId = 1, Account = "jackchan", FullName = "陳大文", Email = "jackchan@example.com", 
						PhoneNumber = "0912-345-678", AvatarUrl = "https://i.pravatar.cc/150?img=12", IsSeller = false, 
						LevelName = "金牌會員", PointBalance = 1200, IsBlacklisted = false },
					new MemberItemVm { UserId = 2, Account = "mary.lee", FullName = "李小美", Email = "mary.lee@example.com", 
						PhoneNumber = "0923-456-789", AvatarUrl = "https://i.pravatar.cc/150?img=5", IsSeller = true, 
						LevelName = "銀牌會員", PointBalance = 800, IsBlacklisted = false },
					new MemberItemVm { UserId = 3, Account = "johnwang", FullName = "王小明", Email = "johnwang@example.com", 
						PhoneNumber = "0934-567-890", AvatarUrl = "https://i.pravatar.cc/150?img=33", IsSeller = true, 
						LevelName = "金牌會員", PointBalance = 2500, IsBlacklisted = false },
					new MemberItemVm { UserId = 4, Account = "linda.liu", FullName = "劉雅婷", Email = "linda.liu@example.com", 
						PhoneNumber = "0945-678-901", AvatarUrl = "https://i.pravatar.cc/150?img=20", IsSeller = false, 
						LevelName = "銅牌會員", PointBalance = 350, IsBlacklisted = false },
					new MemberItemVm { UserId = 5, Account = "kevin.hsu", FullName = "許志明", Email = "kevin.hsu@example.com", 
						PhoneNumber = "0956-789-012", AvatarUrl = "https://i.pravatar.cc/150?img=51", IsSeller = false, 
						LevelName = "銀牌會員", PointBalance = 600, IsBlacklisted = true },
					new MemberItemVm { UserId = 6, Account = "amy.chen", FullName = "陳美玲", Email = "amy.chen@example.com", 
						PhoneNumber = "0967-890-123", AvatarUrl = "https://i.pravatar.cc/150?img=9", IsSeller = true, 
						LevelName = "金牌會員", PointBalance = 3200, IsBlacklisted = false },
					new MemberItemVm { UserId = 7, Account = "david.chang", FullName = "張大衛", Email = "david.chang@example.com", 
						PhoneNumber = "0978-901-234", AvatarUrl = "https://i.pravatar.cc/150?img=68", IsSeller = false, 
						LevelName = "銅牌會員", PointBalance = 150, IsBlacklisted = false },
					new MemberItemVm { UserId = 8, Account = "sophia.wu", FullName = "吳雅文", Email = "sophia.wu@example.com", 
						PhoneNumber = "0989-012-345", AvatarUrl = "https://i.pravatar.cc/150?img=47", IsSeller = true, 
						LevelName = "銀牌會員", PointBalance = 950, IsBlacklisted = false }
				};

				// 查詢要更新的會員
				var member = allMembers.FirstOrDefault(m => m.UserId == id);

				if (member == null)
				{
					TempData["Error"] = "找不到該會員";
					return NotFound();
				}

				// ===================================================================
				// ✅ 步驟6：手動對照更新 - 只更新畫面上有提供的欄位
				// ===================================================================
				// 只更新允許編輯的欄位
				member.FullName = model.FullName ?? member.FullName;
				member.Email = model.Email ?? member.Email;
				member.PhoneNumber = model.PhoneNumber ?? member.PhoneNumber;
				member.IsBlacklisted = model.IsBlacklisted; // 黑名單開關（重要）

				// 如果有上傳新圖片，更新頭像 URL
				if (!string.IsNullOrWhiteSpace(newAvatarUrl))
				{
					member.AvatarUrl = newAvatarUrl;
				}

				// 不要覆蓋以下唯讀欄位：
				// - Account (帳號不可變更)
				// - LevelName (會員等級由系統控制)
				// - PointBalance (點數由系統控制)
				// - IsSeller (賣家資格需另外審核)

				// ===================================================================
				// ✅ 步驟7：儲存變更（真實資料庫版本請啟用以下註解）
				// ===================================================================
				
				/* 
				// TODO: 真實資料庫版本
				// 1. 先從資料庫查詢
				var member = await _context.Members.FindAsync(id);
				
				if (member == null)
				{
					TempData["Error"] = "找不到該會員";
					return NotFound();
				}

				// 2. 手動對照更新（只更新允許編輯的欄位）
				member.FullName = model.FullName;
				member.Email = model.Email;
				member.PhoneNumber = model.PhoneNumber;
				member.IsBlacklisted = model.IsBlacklisted;

				// 3. 處理圖片（如果有上傳）
				if (!string.IsNullOrWhiteSpace(newAvatarUrl))
				{
					member.AvatarUrl = newAvatarUrl;
				}

				// 4. 執行儲存
				await _context.SaveChangesAsync();
				*/

				// ===================================================================
				// ✅ 步驟8：設定成功訊息並跳轉
				// ===================================================================
				TempData["Success"] = $"會員「{member.FullName}」資料更新成功！黑名單狀態：{(member.IsBlacklisted ? "已開啟" : "未開啟")}";

				// 務必跳轉回列表頁
				return RedirectToAction(nameof(Index));
			}
			catch (Exception ex)
			{
				// 發生例外時，顯示錯誤訊息（不使用 JavaScript Alert）
				TempData["Error"] = $"儲存失敗：{ex.Message}";

				model.CityOptions = GetCityOptions();
				model.RegionOptions = GetRegionOptions(model.City ?? "台北市");
				return View("~/Areas/Admin/Views/Member/Edit.cshtml", model);
			}
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
				_ => new[] { "" }
			};

			var options = new List<SelectListItem> { new SelectListItem { Text = "請選擇區域", Value = "" } };
			options.AddRange(regions.Select(r => new SelectListItem { Text = r, Value = r }));
			return options;
		}
	}
}