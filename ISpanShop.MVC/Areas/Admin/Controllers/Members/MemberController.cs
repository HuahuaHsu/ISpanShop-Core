using ISpanShop.Models.DTOs.Common;
using ISpanShop.Models.DTOs.Members;
using ISpanShop.MVC.Areas.Admin.Models.Members;
using ISpanShop.Services.Members;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Linq;
using System;
using ISpanShop.Models.EfModels;
using ISpanShop.MVC.Middleware;

namespace ISpanShop.MVC.Areas.Admin.Controllers.Members
{
	[Area("Admin")]
	[Route("Admin/Members")]
	[RequirePermission("member_manage")]
	public class MemberController : Controller
	{
		private readonly IMemberService _memberService;

		public MemberController(IMemberService memberService)
		{
			_memberService = memberService ?? throw new ArgumentNullException(nameof(memberService));
		}

		// ===================================================================
		// Index — 會員列表
		// ===================================================================
		[HttpGet]
		public IActionResult Index(string keyword = "", string status = "all", int? roleId = null, int? levelId = null, string sortColumn = "Id", string sortDirection = "asc", int page = 1, int pageSize = 10)
		{
			try
			{
				var criteria = new MemberCriteria
				{
					Keyword = keyword,
					Status = status == "all" ? "" : status,
					RoleId = roleId,
					LevelId = levelId,
					SortColumn = sortColumn,
					IsAscending = sortDirection == "asc",
					PageNumber = page,
					PageSize = pageSize
				};

				var pagedResult = _memberService.SearchPaged(criteria);
				var levels = _memberService.GetAllMembershipLevels();

				var viewModel = new MemberIndexVm
				{
					PagedResult = new PagedResult<MemberItemVm>
					{
						Data = pagedResult.Data.Select(m => new MemberItemVm
						{
							UserId = m.Id,
							Account = m.Account,
							FullName = m.FullName,
							Email = m.Email,
							PhoneNumber = m.PhoneNumber,
							AvatarUrl = m.AvatarUrl,
							IsSeller = m.IsSeller,
							LevelName = m.LevelName,
							TotalSpending = m.TotalSpending,
							PointBalance = m.PointBalance,
							IsBlacklisted = m.IsBlacklisted,
							City = m.City,
							Region = m.Region,
							Street = m.Address
						}).ToList(),
						TotalCount = pagedResult.TotalCount,
						CurrentPage = pagedResult.CurrentPage,
						PageSize = pagedResult.PageSize
					},
					Keyword = keyword,
					Status = status,
					RoleId = roleId,
					LevelId = levelId,
					SortColumn = sortColumn,
					SortDirection = sortDirection,
					MembershipLevels = levels.Select(l => new SelectListItem 
					{ 
						Value = l.Id.ToString(), 
						Text = l.LevelName,
						Selected = l.Id == levelId
					}).ToList()
				};

				return View("~/Areas/Admin/Views/Member/Index.cshtml", viewModel);
			}
			catch (Exception ex)
			{
				TempData["Error"] = $"載入會員列表失敗：{ex.Message}";
				return View("~/Areas/Admin/Views/Member/Index.cshtml", new MemberIndexVm
				{
					PagedResult = new PagedResult<MemberItemVm> { Data = new List<MemberItemVm>() }
				});
			}
		}

		// ===================================================================
		// Edit GET — 完整編輯頁（fallback，非 Drawer 使用）
		// ===================================================================
		[HttpGet("Edit/{id}")]
		public IActionResult Edit(int id)
		{
			var member = _memberService.GetMemberById(id);
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
			var member = _memberService.GetMemberById(id);
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
			ModelState.Remove("TotalSpending");
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
				// 處理圖片上傳 (這裡暫時維持原邏輯，或是移動到 Service)
				// 為了保持 Service 純粹，檔案處理可以留在 Controller 或專門的 FileService
				if (model.AvatarFile != null && model.AvatarFile.Length > 0)
				{
					// ... 原有的檔案處理邏輯 ...
					// 這裡簡化處理，實際專案中應該由 Service 處理 URL 儲存
				}

				// ── 更新資料庫 ──
				var dto = new MemberDto
				{
					Id = model.UserId,
					Email = model.Email,
					FullName = model.FullName,
					PhoneNumber = model.PhoneNumber,
					AvatarUrl = model.AvatarUrl, // 補上頭貼路徑
					IsBlacklisted = model.IsBlacklisted,
					IsSeller = model.IsSeller,
					City = model.City,
					Region = model.Region,
					Address = model.Street
				};

				_memberService.UpdateMemberProfile(dto);

				// ── 回應 ──
				if (isAjax)
				{
					var updatedMember = _memberService.GetMemberById(id);
					return Json(new
					{
						success = true,
						message = $"會員「{updatedMember.FullName}」資料更新成功！",
						member = new
						{
							userId = updatedMember.Id,
							fullName = updatedMember.FullName,
							email = updatedMember.Email,
							phoneNumber = updatedMember.PhoneNumber,
							isBlacklisted = updatedMember.IsBlacklisted,
							isSeller = updatedMember.IsSeller,
							levelName = updatedMember.LevelName,
							pointBalance = updatedMember.PointBalance,
							avatarUrl = updatedMember.AvatarUrl,
							account = updatedMember.Account
						}
					});
				}

				TempData["Success"] = "會員資料更新成功！";
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
		private MemberEditVm MapToEditVm(MemberDto member)
		{
			// 使用會員自己的縣市載入正確的區域列表
			var city = string.IsNullOrWhiteSpace(member.City) ? "台北市" : member.City;
			return new MemberEditVm
			{
				UserId        = member.Id,
				Account       = member.Account,
				FullName      = member.FullName,
				Email         = member.Email,
				PhoneNumber   = member.PhoneNumber,
				AvatarUrl     = member.AvatarUrl,
				LevelName     = member.LevelName,
				TotalSpending = member.TotalSpending,
				PointBalance  = member.PointBalance,
				City          = member.City,
				Region        = member.Region,
				Street        = member.Address,
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
