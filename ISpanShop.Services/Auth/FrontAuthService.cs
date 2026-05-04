using ISpanShop.Models.DTOs.Auth;
using ISpanShop.Models.EfModels;
using ISpanShop.Common.Helpers;
using ISpanShop.Common.Enums;
using ISpanShop.Repositories.Members;
using ISpanShop.Services.Communication;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Net;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace ISpanShop.Services.Auth
{
    public class FrontAuthService : IFrontAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly ILoginHistoryRepository _loginHistoryRepository;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _config;
        private readonly HttpClient _httpClient;

        public FrontAuthService(
            IUserRepository userRepository, 
            ILoginHistoryRepository loginHistoryRepository,
            IEmailService emailService,
            IConfiguration config)
        {
            _userRepository = userRepository;
            _loginHistoryRepository = loginHistoryRepository;
            _emailService = emailService;
            _config = config;
            _httpClient = new HttpClient();
        }

        public async Task<FrontLoginResponseDto?> LoginAsync(FrontLoginRequestDto request, string ipAddress)
        {
            var user = await _userRepository.GetByEmailOrAccountAsync(request.Account);
            bool isPasswordCorrect = user != null &&
                !string.IsNullOrEmpty(user.Password) &&
                SecurityHelper.Verify(request.Password, user.Password);

            if (!isPasswordCorrect)
            {
                _loginHistoryRepository.Add(new ISpanShop.Models.DTOs.Members.LoginHistoryDto
                {
                    UserId = null,
                    AttemptedAccount = request.Account,
                    LoginTime = DateTime.Now,
                    Ipaddress = ipAddress,
                    IsSuccess = false
                });

                throw new Exception("帳號或密碼錯誤");
            }

            if (user!.IsConfirmed != true)
            {
                _loginHistoryRepository.Add(new ISpanShop.Models.DTOs.Members.LoginHistoryDto
                {
                    UserId = user.Id,
                    AttemptedAccount = request.Account,
                    LoginTime = DateTime.Now,
                    Ipaddress = ipAddress,
                    IsSuccess = false
                });

                throw new Exception("請先至信箱完成 Email 驗證");
            }

            _loginHistoryRepository.Add(new ISpanShop.Models.DTOs.Members.LoginHistoryDto
            {
                UserId = user.Id,
                AttemptedAccount = request.Account,
                LoginTime = DateTime.Now,
                Ipaddress = ipAddress,
                IsSuccess = true
            });

            return new FrontLoginResponseDto
            {
                Token = GenerateJwtToken(user),
                MemberId = user.Id,
                Email = user.Email,
                Account = user.Account,
                MemberName = user.MemberProfile?.FullName ?? user.Account,
                LevelName = user.MemberProfile?.Level?.LevelName ?? "一般會員",
                PointBalance = user.MemberProfile?.PointBalance ?? 0,
                IsSeller = user.MemberProfile?.IsSeller ?? false,
                IsBlacklisted = user.IsBlacklisted == true
            };
        }

        public async Task<bool> RegisterAsync(FrontRegisterRequestDto request)
        {
            if (await _userRepository.ExistsAsync(request.Email, request.Account)) throw new Exception("Email 或 帳號已存在");

            var confirmCode = await GenerateUniqueConfirmCodeAsync();
            var user = new User
            {
                Account = request.Account,
                Password = SecurityHelper.ToBCrypt(request.Password),
                Email = request.Email,
                RoleId = (int)RoleEnum.Member,
                CreatedAt = DateTime.Now,
                IsConfirmed = false,
                ConfirmCode = confirmCode
            };

            user.MemberProfile = new MemberProfile
            {
                FullName = request.FullName,
                PhoneNumber = request.PhoneNumber,
                LevelId = (int)MemberLevelEnum.Normal,
                UpdatedAt = DateTime.Now,
                IsSeller = false
            };

            await _userRepository.CreateAsync(user);
            await SendVerificationEmailAsync(user, confirmCode);
            return true;
        }

        public async Task<(bool IsSuccess, string Message)> VerifyEmailAsync(string confirmCode)
        {
            if (string.IsNullOrWhiteSpace(confirmCode))
            {
                return (false, "驗證連結無效");
            }

            var user = await _userRepository.GetPendingUserByConfirmCodeAsync(confirmCode);
            if (user == null)
            {
                return (false, "驗證連結無效或帳號已啟用");
            }

            var result = await _userRepository.ConfirmEmailAsync(user.Id);
            return result
                ? (true, "Email 驗證成功，請登入")
                : (false, "Email 驗證失敗，請稍後再試");
        }

        public async Task<OAuthResultDto> OAuthLoginAsync(string code, string redirectUri)
        {
            var googleUser = await GetGoogleUserAsync(code, redirectUri);
            
            var userByProvider = await _userRepository.FindByProviderAsync("Google", googleUser.ProviderId);
            if (userByProvider != null)
            {
                return new OAuthResultDto { Status = "Success", Token = GenerateJwtToken(userByProvider), Email = userByProvider.Email };
            }

            var userByEmail = await _userRepository.FindByEmailAsync(googleUser.Email);
            if (userByEmail != null)
            {
                return new OAuthResultDto 
                { 
                    Status = "MergeRequired", 
                    Email = googleUser.Email,
                    ProviderId = googleUser.ProviderId 
                };
            }

            var newUser = new User
            {
                // 生成匿名帳號名，格式為 G- 後接 Google ID 的後六位數 (例如 G-123456)
                Account = "G-" + (googleUser.ProviderId.Length > 6 ? googleUser.ProviderId.Substring(googleUser.ProviderId.Length - 6) : googleUser.ProviderId),
                Password = null,
                Email = googleUser.Email,
                Provider = "Google",
                ProviderId = googleUser.ProviderId,
                RoleId = (int)RoleEnum.Member,
                CreatedAt = DateTime.Now,
                IsConfirmed = true
            };

            newUser.MemberProfile = new MemberProfile
            {
                FullName = googleUser.DisplayName ?? googleUser.Email.Split('@')[0],
                LevelId = (int)MemberLevelEnum.Normal,
                UpdatedAt = DateTime.Now,
                IsSeller = false
            };

            await _userRepository.CreateAsync(newUser);
            return new OAuthResultDto { Status = "Success", Token = GenerateJwtToken(newUser), Email = newUser.Email };
        }

        public async Task<bool> BindOAuthAsync(int userId, string code, string redirectUri)
        {
            var googleUser = await GetGoogleUserAsync(code, redirectUri);
            
            var existingUser = await _userRepository.FindByProviderAsync("Google", googleUser.ProviderId);
            if (existingUser != null && existingUser.Id != userId) throw new Exception("此 Google 帳號已被其他會員綁定");

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) throw new Exception("找不到使用者");

            user.Provider = "Google";
            user.ProviderId = googleUser.ProviderId;
            
            await _userRepository.UpdatePasswordHashAsync(user.Id, user.Password);
            return true;
        }

        public async Task<FrontLoginResponseDto?> MergeOAuthAccountAsync(OAuthMergeDto dto, string ipAddress)
        {
            var existingUser = await _userRepository.GetByEmailOrAccountAsync(dto.Account);
            if (existingUser == null || !SecurityHelper.Verify(dto.Password, existingUser.Password)) throw new Exception("原帳號驗證失敗");

            existingUser.Provider = dto.Provider;
            existingUser.ProviderId = dto.OAuthProviderId;
            await _userRepository.UpdatePasswordHashAsync(existingUser.Id, existingUser.Password);

            return new FrontLoginResponseDto
            {
                Token = GenerateJwtToken(existingUser),
                MemberId = existingUser.Id,
                Email = existingUser.Email,
                Account = existingUser.Account,
                MemberName = existingUser.MemberProfile?.FullName ?? existingUser.Account
            };
        }

        public async Task<bool> UnbindOAuthAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) throw new Exception("找不到使用者");
            if (string.IsNullOrEmpty(user.Password)) throw new Exception("請先設定登入密碼後再解除綁定");

            user.Provider = null;
            user.ProviderId = null;
            await _userRepository.UpdatePasswordHashAsync(user.Id, user.Password);
            return true;
        }

        private async Task<OAuthCallbackDto> GetGoogleUserAsync(string code, string redirectUri)
        {
            var googleConfig = _config.GetSection("Google");
            var values = new Dictionary<string, string>
            {
                { "client_id", googleConfig["ClientId"] },
                { "client_secret", googleConfig["ClientSecret"] },
                { "code", code },
                { "grant_type", "authorization_code" },
                { "redirect_uri", redirectUri }
            };

            var response = await _httpClient.PostAsync("https://oauth2.googleapis.com/token", new FormUrlEncodedContent(values));
            var responseString = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode) throw new Exception("Google 驗證失敗");

            var json = JsonDocument.Parse(responseString);
            var idToken = json.RootElement.GetProperty("id_token").GetString();
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(idToken);
            
            return new OAuthCallbackDto
            {
                Provider = "Google",
                ProviderId = token.Claims.First(c => c.Type == "sub").Value,
                Email = token.Claims.First(c => c.Type == "email").Value,
                DisplayName = token.Claims.FirstOrDefault(c => c.Type == "name")?.Value
            };
        }

        private async Task<string> GenerateUniqueConfirmCodeAsync()
        {
            string code;
            do
            {
                code = Guid.NewGuid().ToString("N");
            }
            while (await _userRepository.ConfirmCodeExistsAsync(code));

            return code;
        }

        private async Task SendVerificationEmailAsync(User user, string confirmCode)
        {
            var frontendBaseUrl = _config["Frontend:BaseUrl"] ?? "http://localhost:5173";
            var verifyLink = $"{frontendBaseUrl.TrimEnd('/')}/verify-email?code={confirmCode}";
            var safeAccount = WebUtility.HtmlEncode(user.Account);
            var subject = "HowBuy好買 - 請完成 Email 驗證";
            var body = $@"
                <div style='font-family: sans-serif; padding: 20px; color: #333;'>
                    <h2>您好，{safeAccount}</h2>
                    <p>感謝您註冊 HowBuy好買。請點擊下方按鈕完成 Email 驗證並啟用帳號：</p>
                    <div style='margin: 30px 0;'>
                        <a href='{verifyLink}' style='background-color: #ee4d2d; color: white; padding: 12px 25px; text-decoration: none; border-radius: 4px; font-weight: bold;'>完成 Email 驗證</a>
                    </div>
                    <p>如果按鈕無法運作，請複製並貼上以下連結至瀏覽器：</p>
                    <p><a href='{verifyLink}'>{verifyLink}</a></p>
                    <hr style='border: none; border-top: 1px solid #eee; margin-top: 30px;'>
                    <p style='font-size: 12px; color: #999;'>如果您並未註冊 HowBuy好買，請忽略此電子郵件。</p>
                </div>";

            await _emailService.SendEmailAsync(user.Email, subject, body);
        }

        private string GenerateJwtToken(User user)
        {
            var jwtSettings = _config.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.MemberProfile?.FullName ?? user.Account),
                new Claim("RoleId", user.RoleId.ToString()),
                new Claim("IsBlacklisted", (user.IsBlacklisted == true).ToString())
            };

            var store = user.Stores?.FirstOrDefault();
            if (store != null) claims.Add(new Claim("StoreId", store.Id.ToString()));

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(double.Parse(jwtSettings["ExpireMinutes"]!)),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
