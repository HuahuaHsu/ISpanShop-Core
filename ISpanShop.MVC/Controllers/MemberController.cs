using ISpanShop.Models.Dtos;
using ISpanShop.Models.DTOs;
using ISpanShop.Models.EfModels;
using ISpanShop.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ISpanShop.Models.Dtos
{
	public class MemberController : Controller
	{
		private readonly MemberService _memberService;

		public MemberController(MemberService memberService)
		{
			_memberService = memberService;
		}

		// 列表頁 (支援搜尋)
		// URL 可能長這樣: /Members?keyword=jack&status=blocked
		public IActionResult Index(string keyword, string status)
		{
			// 保存搜尋條件，讓 View 的搜尋框可以顯示剛剛輸入的字
			ViewBag.Keyword = keyword;
			ViewBag.Status = status;

			var members = _memberService.Search(keyword, status);
			return View(members);
		}

		// 編輯/管理狀態頁 (GET)
		public IActionResult Edit(int id)
		{
			var member = _memberService.GetMemberById(id);
			if (member == null)
			{
				return NotFound();
			}
			return View(member);
		}

		// 編輯/管理狀態頁 (POST)
		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult Edit(MemberDto memberDto)
		{
			// 這裡不需要 ModelState.IsValid 驗證 Account/Email 必填
			// 因為我們只更新狀態，而且前端那些欄位是 Readonly

			try
			{
				_memberService.UpdateMemberStatus(memberDto);

				// 更新成功，跳轉回列表
				return RedirectToAction(nameof(Index));
			}
			catch (Exception ex)
			{
				// 如果出錯 (例如找不到人)，顯示錯誤訊息
				ModelState.AddModelError("", ex.Message);
				return View(memberDto);
			}
		}
	}
}