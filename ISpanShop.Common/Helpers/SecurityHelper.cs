using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BCrypt.Net;

namespace ISpanShop.Common.Helpers
{
	/// <summary>
	/// 提供安全性相關的輔助方法，例如密碼雜湊
	/// </summary>
	public static class SecurityHelper
	{
		/// <summary>
		/// 將明文密碼進行 BCrypt 雜湊處理 (包含 Salt)
		/// </summary>
		/// <param name="password">使用者輸入的明文密碼</param>
		/// <returns>雜湊後字串</returns>
		/// <exception cref="ArgumentNullException">當密碼為空或 null 時拋出異常</exception>
		public static string ToBCrypt(string password)
		{
			// 1. 基本檢查：密碼不能是空的
			if (string.IsNullOrEmpty(password))
			{
				throw new ArgumentNullException(nameof(password), "密碼不能為空");
			}

			// 2. 使用 BCrypt 進行雜湊 (會自動產生 Salt 並包含在回傳字串中)
			// Work Factor 預設通常是 11，適合目前的硬體效能
			return BCrypt.Net.BCrypt.HashPassword(password);
		}

		/// <summary>
		/// 驗證使用者輸入的密碼是否與資料庫中的雜湊密碼相符
		/// </summary>
		/// <param name="password">使用者登入時輸入的明文密碼</param>
		/// <param name="hashedPassword">從資料庫取出的雜湊密碼字串</param>
		/// <returns>密碼正確回傳 true，否則回傳 false</returns>
		public static bool Verify(string password, string hashedPassword)
		{
			// 1. 基本檢查：如果有任何一個是空的，直接視為驗證失敗
			if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hashedPassword))
			{
				return false;
			}

			// 2. 進行比對 (BCrypt 會自動從 hashedPassword 提取 Salt 來運算)
			return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
		}
	}
}

