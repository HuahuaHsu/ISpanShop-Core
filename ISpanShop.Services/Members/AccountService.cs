using ISpanShop.Common.Helpers;
using ISpanShop.Models.DTOs.Auth;
using ISpanShop.Models.EfModels;
using ISpanShop.Repositories.Members;
using ISpanShop.Services.Communication;

namespace ISpanShop.Services.Members
{
	public class AccountService : IAccountService
	{
		private readonly IUserRepository _userRepository;
		private readonly IPasswordResetTokenRepository _tokenRepository;
		private readonly IEmailService _emailService;

		public AccountService(
			IUserRepository userRepository, 
			IPasswordResetTokenRepository tokenRepository,
			IEmailService emailService)
		{
			_userRepository = userRepository;
			_tokenRepository = tokenRepository;
			_emailService = emailService;
		}

		public async Task<(bool IsSuccess, string Message)> ChangePasswordAsync(ChangePasswordDto dto)
		{
			// 1. 取得使用者
			var user = await _userRepository.GetByIdAsync(dto.UserId);
			if (user == null)
			{
				return (false, "找不到該使用者");
			}

			// 2. 驗證舊密碼
			if (!SecurityHelper.Verify(dto.OldPassword, user.Password))
			{
				return (false, "舊密碼錯誤");
			}

			// 3. 確認新密碼與確認密碼一致
			if (dto.NewPassword != dto.ConfirmPassword)
			{
				return (false, "兩次輸入的新密碼不一致");
			}

			// 4. 確認新舊密碼不同
			if (dto.OldPassword == dto.NewPassword)
			{
				return (false, "新密碼不能與舊密碼相同");
			}

			// 5. 更新密碼
			var newHash = SecurityHelper.ToBCrypt(dto.NewPassword);
			var result = await _userRepository.UpdatePasswordHashAsync(dto.UserId, newHash);

			return result ? (true, "密碼已成功變更") : (false, "密碼更新失敗");
		}

		public async Task<(bool IsSuccess, string Message)> ForgotPasswordAsync(ForgotPasswordDto dto)
		{
			// 1. 用 Email 查使用者
			var user = await _userRepository.GetByEmailOrAccountAsync(dto.Email);
			if (user == null)
			{
				// 為了安全，即使 Email 不存在也回傳成功訊息，避免被探測 Email
				return (true, "重設密碼信件已發送 (若該 Email 已註冊)");
			}

			// 2. 清除該使用者舊有的 Token
			await _tokenRepository.DeleteByUserIdAsync(user.Id);

			// 3. 產生新 Token
			var token = Guid.NewGuid().ToString("N");
			var resetToken = new PasswordResetToken
			{
				UserId = user.Id,
				Token = token,
				ExpiryDate = DateTime.Now.AddMinutes(30),
				IsUsed = false,
				CreatedAt = DateTime.Now
			};

			// 4. 儲存 Token
			await _tokenRepository.CreateAsync(resetToken);

			// 5. 發送郵件
			try
			{
				var resetLink = $"http://localhost:5173/reset-password?token={token}";
				var subject = "HowBuy好買 - 重設您的密碼";
				var body = $@"
                    <div style='font-family: sans-serif; padding: 20px; color: #333;'>
                        <h2>您好，{user.Account}</h2>
                        <p>我們收到了重設您 HowBuy好買 帳號密碼的請求。</p>
                        <p>請點擊下方按鈕以重設密碼（此連結將在 30 分鐘後失效）：</p>
                        <div style='margin: 30px 0;'>
                            <a href='{resetLink}' style='background-color: #ee4d2d; color: white; padding: 12px 25px; text-decoration: none; border-radius: 4px; font-weight: bold;'>重設密碼</a>
                        </div>
                        <p>如果按鈕無法運作，請複製並貼上以下連結至瀏覽器：</p>
                        <p><a href='{resetLink}'>{resetLink}</a></p>
                        <hr style='border: none; border-top: 1px solid #eee; margin-top: 30px;'>
                        <p style='font-size: 12px; color: #999;'>如果您並未要求重設密碼，請忽略此電子郵件。您的帳號安全無虞。</p>
                    </div>";

				await _emailService.SendEmailAsync(user.Email, subject, body);
				return (true, "重設密碼信件已發送，請檢查您的信箱");
			}
			catch (Exception ex)
			{
				// 郵件發送失敗但不中斷流程 (用於本地測試)
				// 可以在開發者主控台查看 Token
				Console.WriteLine($"[錯誤] 郵件發送失敗: {ex.Message}");
				Console.WriteLine($"[本地測試] 重設連結: http://localhost:5173/reset-password?token={token}");
				
				return (false, $"郵件發送失敗，請確認郵件伺服器設定。錯誤原因: {ex.Message}");
			}
		}

		public async Task<(bool IsSuccess, string Message)> ResetPasswordAsync(ResetPasswordDto dto)
		{
			// 1. 確認新密碼一致
			if (dto.NewPassword != dto.ConfirmPassword)
			{
				return (false, "兩次輸入的新密碼不一致");
			}

			// 2. 驗證 Token
			var tokenRecord = await _tokenRepository.GetByTokenAsync(dto.Token);
			if (tokenRecord == null || tokenRecord.IsUsed || tokenRecord.ExpiryDate < DateTime.Now)
			{
				return (false, "效驗碼無效或已過期");
			}

			// 3. 用 Token 內的 UserId 取得使用者
			var user = await _userRepository.GetByIdAsync(tokenRecord.UserId);
			if (user == null)
			{
				return (false, "找不到該使用者");
			}

			// 4. 更新密碼
			var newHash = SecurityHelper.ToBCrypt(dto.NewPassword);
			var result = await _userRepository.UpdatePasswordHashAsync(user.Id, newHash);

			if (result)
			{
				// 5. 標記 Token 已使用
				await _tokenRepository.DeleteByUserIdAsync(user.Id);
				return (true, "密碼重設成功，請使用新密碼登入");
			}

			return (false, "密碼更新失敗");
		}
	}
}