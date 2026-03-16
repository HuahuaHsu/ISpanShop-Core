using ISpanShop.Models.DTOs.Admins;
using ISpanShop.Repositories.Admins;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace ISpanShop.Repositories.Admins
{
	/// <summary>
	/// 管理員資料存取實現 - 使用 ADO.NET
	/// </summary>
	public class AdminRepository : IAdminRepository
	{
		private readonly string _connectionString;

		public AdminRepository(IConfiguration configuration)
		{
			_connectionString = configuration.GetConnectionString("DefaultConnection")
				?? throw new ArgumentNullException(nameof(configuration), "DefaultConnection 未設定");
		}

		// ==========================================
		// 以下是你原本就寫好的方法
		// ==========================================

		public AdminDto? GetAdminById(int adminId)
		{
			try
			{
				using (SqlConnection conn = new SqlConnection(_connectionString))
				{
					conn.Open();
					string query = @"
						SELECT 
							A.Id AS UserId,
							A.Account,
							A.Email,
							A.RoleId,
							R.RoleName,
							A.AdminLevelId,
							AL.LevelName AS AdminLevelName,
							A.IsBlacklisted,
							A.IsFirstLogin,
							A.CreatedAt,
							A.UpdatedAt
						FROM Users A
						JOIN Roles R ON A.RoleId = R.Id
						LEFT JOIN AdminLevels AL ON A.AdminLevelId = AL.Id
						WHERE A.Id = @AdminId
						  AND R.RoleName IN ('Admin', 'SuperAdmin')";

					using (SqlCommand cmd = new SqlCommand(query, conn))
					{
						cmd.Parameters.AddWithValue("@AdminId", adminId);

						using (SqlDataReader reader = cmd.ExecuteReader())
						{
							if (reader.Read())
							{
								return new AdminDto
								{
									UserId = (int)reader["UserId"],
									Account = reader["Account"]?.ToString() ?? "",
									Email = reader["Email"]?.ToString() ?? "",
									RoleId = (int)reader["RoleId"],
									RoleName = reader["RoleName"]?.ToString() ?? "",
									AdminLevelId = reader["AdminLevelId"] != DBNull.Value ? (int?)reader["AdminLevelId"] : null,
									AdminLevelName = reader["AdminLevelName"]?.ToString() ?? "",
									IsBlacklisted = reader["IsBlacklisted"] != DBNull.Value && (bool)reader["IsBlacklisted"],
									IsFirstLogin = reader["IsFirstLogin"] != DBNull.Value && (bool)reader["IsFirstLogin"],
									CreatedAt = reader["CreatedAt"] != DBNull.Value ? (DateTime)reader["CreatedAt"] : DateTime.MinValue,
									UpdatedAt = reader["UpdatedAt"] != DBNull.Value ? (DateTime?)reader["UpdatedAt"] : null
								};
							}
							return null;
						}
					}
				}
			}
			catch (SqlException ex)
			{
				throw new InvalidOperationException("查詢管理員失敗", ex);
			}
		}

		/// <summary>
		/// 取得所有管理員資訊（僅限 Admin 和 SuperAdmin 角色）
		/// </summary>
		public IEnumerable<AdminDto> GetAllAdmins(AdminCriteria criteria = null)
		{
			var admins = new List<AdminDto>();
			try
			{
				using (SqlConnection conn = new SqlConnection(_connectionString))
				{
					conn.Open();
					string query = @"
						SELECT 
							A.Id AS UserId,
							A.Account,
							A.Email,
							A.RoleId,
							R.RoleName,
							A.AdminLevelId,
							AL.LevelName AS AdminLevelName,
							A.IsBlacklisted,
							A.IsFirstLogin,
							A.CreatedAt,
							A.UpdatedAt
						FROM Users A
						JOIN Roles R ON A.RoleId = R.Id
						LEFT JOIN AdminLevels AL ON A.AdminLevelId = AL.Id
						WHERE R.RoleName IN ('Admin', 'SuperAdmin')";

					// 篩選條件
					if (criteria != null)
					{
						if (!string.IsNullOrEmpty(criteria.Keyword))
						{
							query += " AND (A.Account LIKE @keyword OR A.Email LIKE @keyword)";
						}

						if (criteria.AdminLevelId.HasValue)
						{
							query += " AND A.AdminLevelId = @adminLevelId";
						}

						if (!string.IsNullOrEmpty(criteria.Status))
						{
							switch (criteria.Status)
							{
								case "active":
									query += " AND A.IsBlacklisted = 0";
									break;
								case "blocked":
									query += " AND A.IsBlacklisted = 1";
									break;
								case "firstLogin":
									query += " AND A.IsFirstLogin = 1";
									break;
							}
						}

						// 排序
						string sortCol = criteria.SortColumn switch
						{
							"Account" => "A.Account",
							"Email" => "A.Email",
							"AdminLevelName" => "AL.LevelName",
							"IsBlacklisted" => "A.IsBlacklisted",
							"CreatedAt" => "A.CreatedAt",
							_ => "A.Id"
						};
						query += $" ORDER BY {sortCol} {(criteria.IsAscending ? "ASC" : "DESC")}";
					}
					else
					{
						query += " ORDER BY A.CreatedAt DESC";
					}

					using (SqlCommand cmd = new SqlCommand(query, conn))
					{
						if (criteria != null)
						{
							if (!string.IsNullOrEmpty(criteria.Keyword))
								cmd.Parameters.AddWithValue("@keyword", $"%{criteria.Keyword}%");
							if (criteria.AdminLevelId.HasValue)
								cmd.Parameters.AddWithValue("@adminLevelId", criteria.AdminLevelId.Value);
						}

						using (SqlDataReader reader = cmd.ExecuteReader())
						{
							while (reader.Read())
							{
								admins.Add(new AdminDto
								{
									UserId = (int)reader["UserId"],
									Account = reader["Account"]?.ToString() ?? "",
									Email = reader["Email"]?.ToString() ?? "",
									RoleId = (int)reader["RoleId"],
									RoleName = reader["RoleName"]?.ToString() ?? "",
									AdminLevelId = reader["AdminLevelId"] != DBNull.Value ? (int?)reader["AdminLevelId"] : null,
									AdminLevelName = reader["AdminLevelName"]?.ToString() ?? "",
									IsBlacklisted = reader["IsBlacklisted"] != DBNull.Value && (bool)reader["IsBlacklisted"],
									IsFirstLogin = reader["IsFirstLogin"] != DBNull.Value && (bool)reader["IsFirstLogin"],
									CreatedAt = reader["CreatedAt"] != DBNull.Value ? (DateTime)reader["CreatedAt"] : DateTime.MinValue,
									UpdatedAt = reader["UpdatedAt"] != DBNull.Value ? (DateTime?)reader["UpdatedAt"] : null
								});
							}
						}
					}
				}
			}
			catch (SqlException ex)
			{
				throw new InvalidOperationException("資料庫查詢失敗", ex);
			}
			return admins;
		}

		public IEnumerable<AdminDto> SearchPaged(AdminCriteria criteria, out int totalCount)
		{
			var admins = new List<AdminDto>();
			totalCount = 0;
			try
			{
				using (SqlConnection conn = new SqlConnection(_connectionString))
				{
					conn.Open();

					// 1. 取得總筆數
					string countQuery = @"
						SELECT COUNT(1)
						FROM Users A
						JOIN Roles R ON A.RoleId = R.Id
						LEFT JOIN AdminLevels AL ON A.AdminLevelId = AL.Id
						WHERE R.RoleName IN ('Admin', 'SuperAdmin')";

					if (!string.IsNullOrEmpty(criteria.Keyword))
						countQuery += " AND (A.Account LIKE @keyword OR A.Email LIKE @keyword)";

					if (criteria.AdminLevelId.HasValue)
						countQuery += " AND A.AdminLevelId = @adminLevelId";

					if (!string.IsNullOrEmpty(criteria.Status))
					{
						switch (criteria.Status)
						{
							case "active": countQuery += " AND A.IsBlacklisted = 0"; break;
							case "blocked": countQuery += " AND A.IsBlacklisted = 1"; break;
							case "firstLogin": countQuery += " AND A.IsFirstLogin = 1"; break;
						}
					}

					using (SqlCommand countCmd = new SqlCommand(countQuery, conn))
					{
						if (!string.IsNullOrEmpty(criteria.Keyword))
							countCmd.Parameters.AddWithValue("@keyword", $"%{criteria.Keyword}%");
						if (criteria.AdminLevelId.HasValue)
							countCmd.Parameters.AddWithValue("@adminLevelId", criteria.AdminLevelId.Value);

						totalCount = (int)countCmd.ExecuteScalar();
					}

					// 2. 取得分頁資料
					string query = @"
						SELECT 
							A.Id AS UserId,
							A.Account,
							A.Email,
							A.RoleId,
							R.RoleName,
							A.AdminLevelId,
							AL.LevelName AS AdminLevelName,
							A.IsBlacklisted,
							A.IsFirstLogin,
							A.CreatedAt,
							A.UpdatedAt
						FROM Users A
						JOIN Roles R ON A.RoleId = R.Id
						LEFT JOIN AdminLevels AL ON A.AdminLevelId = AL.Id
						WHERE R.RoleName IN ('Admin', 'SuperAdmin')";

					if (!string.IsNullOrEmpty(criteria.Keyword))
						query += " AND (A.Account LIKE @keyword OR A.Email LIKE @keyword)";

					if (criteria.AdminLevelId.HasValue)
						query += " AND A.AdminLevelId = @adminLevelId";

					if (!string.IsNullOrEmpty(criteria.Status))
					{
						switch (criteria.Status)
						{
							case "active": query += " AND A.IsBlacklisted = 0"; break;
							case "blocked": query += " AND A.IsBlacklisted = 1"; break;
							case "firstLogin": query += " AND A.IsFirstLogin = 1"; break;
						}
					}

					// 排序
					string sortCol = criteria.SortColumn switch
					{
						"Account" => "A.Account",
						"Email" => "A.Email",
						"AdminLevelName" => "AL.LevelName",
						"IsBlacklisted" => "A.IsBlacklisted",
						"CreatedAt" => "A.CreatedAt",
						_ => "A.Id"
					};
					query += $" ORDER BY {sortCol} {(criteria.IsAscending ? "ASC" : "DESC")}";

					// 分頁
					query += " OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

					using (SqlCommand cmd = new SqlCommand(query, conn))
					{
						if (!string.IsNullOrEmpty(criteria.Keyword))
							cmd.Parameters.AddWithValue("@keyword", $"%{criteria.Keyword}%");
						if (criteria.AdminLevelId.HasValue)
							cmd.Parameters.AddWithValue("@adminLevelId", criteria.AdminLevelId.Value);

						cmd.Parameters.AddWithValue("@Offset", (criteria.PageNumber - 1) * criteria.PageSize);
						cmd.Parameters.AddWithValue("@PageSize", criteria.PageSize);

						using (SqlDataReader reader = cmd.ExecuteReader())
						{
							while (reader.Read())
							{
								admins.Add(new AdminDto
								{
									UserId = (int)reader["UserId"],
									Account = reader["Account"]?.ToString() ?? "",
									Email = reader["Email"]?.ToString() ?? "",
									RoleId = (int)reader["RoleId"],
									RoleName = reader["RoleName"]?.ToString() ?? "",
									AdminLevelId = reader["AdminLevelId"] != DBNull.Value ? (int?)reader["AdminLevelId"] : null,
									AdminLevelName = reader["AdminLevelName"]?.ToString() ?? "",
									IsBlacklisted = reader["IsBlacklisted"] != DBNull.Value && (bool)reader["IsBlacklisted"],
									IsFirstLogin = reader["IsFirstLogin"] != DBNull.Value && (bool)reader["IsFirstLogin"],
									CreatedAt = reader["CreatedAt"] != DBNull.Value ? (DateTime)reader["CreatedAt"] : DateTime.MinValue,
									UpdatedAt = reader["UpdatedAt"] != DBNull.Value ? (DateTime?)reader["UpdatedAt"] : null
								});
							}
						}
					}
				}
			}
			catch (SqlException ex)
			{
				throw new InvalidOperationException("資料庫分頁查詢失敗", ex);
			}
			return admins;
		}

		/// <summary>
		/// 更新管理員的角色
		/// </summary>
		public bool UpdateAdminRole(int adminId, int roleId)
		{
			try
			{
				using (SqlConnection conn = new SqlConnection(_connectionString))
				{
					conn.Open();
					string query = @"
						UPDATE Users 
						SET RoleId = @RoleId, 
							UpdatedAt = GETDATE()
						WHERE Id = @AdminId 
							AND RoleId IN (SELECT Id FROM Roles WHERE RoleName IN ('Admin', 'SuperAdmin'))";

					using (SqlCommand cmd = new SqlCommand(query, conn))
					{
						cmd.Parameters.AddWithValue("@AdminId", adminId);
						cmd.Parameters.AddWithValue("@RoleId", roleId);

						int rowsAffected = cmd.ExecuteNonQuery();
						return rowsAffected > 0;
					}
				}
			}
			catch (SqlException ex)
			{
				throw new InvalidOperationException("更新管理員角色失敗", ex);
			}
		}

		// ==========================================
		// 以下是剛才幫你補齊的 8 個規格方法
		// ==========================================

		/// <summary>
		/// 取得可選擇的管理員層級（排除超級管理員）
		/// </summary>
		public IEnumerable<AdminLevelDto> GetSelectableAdminLevels()
		{
			var levels = new List<AdminLevelDto>();
			try
			{
				using (SqlConnection conn = new SqlConnection(_connectionString))
				{
					conn.Open();
					string query = @"
						SELECT Id AS AdminLevelId, LevelName, Description
						FROM AdminLevels
						WHERE Id != 1
						ORDER BY Id";

					using (SqlCommand cmd = new SqlCommand(query, conn))
					{
						using (SqlDataReader reader = cmd.ExecuteReader())
						{
							while (reader.Read())
							{
								levels.Add(new AdminLevelDto
								{
									AdminLevelId = (int)reader["AdminLevelId"],
									LevelName = reader["LevelName"]?.ToString() ?? "",
									Description = reader["Description"]?.ToString() ?? ""
								});
							}
						}
					}
				}
			}
			catch (SqlException ex)
			{
				throw new InvalidOperationException("查詢管理員層級失敗", ex);
			}
			return levels;
		}

		/// <summary>
		/// 透過帳號取得管理員
		/// </summary>
		public AdminDto? GetAdminByAccount(string account)
		{
			try
			{
				using (SqlConnection conn = new SqlConnection(_connectionString))
				{
					conn.Open();
					string query = @"
						SELECT U.Id AS UserId,
							   U.Account,
							   U.Email,
							   U.Password AS PasswordHash,
							   U.RoleId,
							   R.RoleName,
							   U.AdminLevelId,
							   AL.LevelName AS AdminLevelName,
							   U.IsBlacklisted,
							   U.IsFirstLogin
						FROM Users U
						JOIN Roles R ON U.RoleId = R.Id
						LEFT JOIN AdminLevels AL ON U.AdminLevelId = AL.Id
						WHERE U.Account = @Account";

					using (SqlCommand cmd = new SqlCommand(query, conn))
					{
						cmd.Parameters.AddWithValue("@Account", account);
						using (SqlDataReader reader = cmd.ExecuteReader())
						{
							if (reader.Read())
							{
								return new AdminDto
								{
									UserId = (int)reader["UserId"],
									Account = reader["Account"]?.ToString() ?? "",
									Email = reader["Email"]?.ToString() ?? "",
									PasswordHash = reader["PasswordHash"]?.ToString() ?? "",
									RoleId = (int)reader["RoleId"],
									RoleName = reader["RoleName"]?.ToString() ?? "",
									AdminLevelId = reader["AdminLevelId"] != DBNull.Value ? (int?)reader["AdminLevelId"] : null,
									AdminLevelName = reader["AdminLevelName"]?.ToString() ?? "",
									IsBlacklisted = reader["IsBlacklisted"] != DBNull.Value && (bool)reader["IsBlacklisted"],
									IsFirstLogin = reader["IsFirstLogin"] != DBNull.Value && (bool)reader["IsFirstLogin"]
								};
							}
							return null;
						}
					}
				}
			}
			catch (SqlException ex)
			{
				throw new InvalidOperationException("透過帳號查詢管理員失敗", ex);
			}
		}

		/// <summary>
		/// 取得下一個管理員序列號
		/// </summary>
		public int GetNextAdminSequence()
		{
			try
			{
				using (SqlConnection conn = new SqlConnection(_connectionString))
				{
					conn.Open();
					string query = "SELECT ISNULL(MAX(Id), 0) + 1 FROM Users";
					using (SqlCommand cmd = new SqlCommand(query, conn))
					{
						return (int)cmd.ExecuteScalar();
					}
				}
			}
			catch (SqlException ex)
			{
				throw new InvalidOperationException("取得序列號失敗", ex);
			}
		}

		/// <summary>
		/// 新增管理員
		/// </summary>
		public bool CreateAdmin(string account, string email, string passwordHash, int roleId, int adminLevelId)
		{
			try
			{
				using (SqlConnection conn = new SqlConnection(_connectionString))
				{
					conn.Open();
					string query = @"
						INSERT INTO Users
							(RoleId, Account, Password, Email,
							 AdminLevelId, IsConfirmed,
							 IsBlacklisted, IsFirstLogin, CreatedAt)
						VALUES
							(@RoleId, @Account, @PasswordHash, @Email,
							 @AdminLevelId, 1,
							 0, 1, GETDATE())";

					using (SqlCommand cmd = new SqlCommand(query, conn))
					{
						cmd.Parameters.AddWithValue("@RoleId", roleId);
						cmd.Parameters.AddWithValue("@Account", account);
						cmd.Parameters.AddWithValue("@PasswordHash", passwordHash);
						cmd.Parameters.AddWithValue("@Email", email);
						cmd.Parameters.AddWithValue("@AdminLevelId", adminLevelId);

						int rowsAffected = cmd.ExecuteNonQuery();
						return rowsAffected > 0;
					}
				}
			}
			catch (SqlException ex)
			{
				throw new InvalidOperationException("新增管理員失敗", ex);
			}
		}

		/// <summary>
		/// 停用管理員 (軟刪除)
		/// </summary>
		public bool DeactivateAdmin(int userId)
		{
			try
			{
				using (SqlConnection conn = new SqlConnection(_connectionString))
				{
					conn.Open();
					string query = "UPDATE Users SET IsBlacklisted = 1, UpdatedAt = GETDATE() WHERE Id = @UserId";
					using (SqlCommand cmd = new SqlCommand(query, conn))
					{
						cmd.Parameters.AddWithValue("@UserId", userId);
						return cmd.ExecuteNonQuery() > 0;
					}
				}
			}
			catch (SqlException ex)
			{
				throw new InvalidOperationException("停用管理員失敗", ex);
			}
		}

		/// <summary>
		/// 檢查帳號是否存在
		/// </summary>
		public bool IsAccountExists(string account)
		{
			try
			{
				using (SqlConnection conn = new SqlConnection(_connectionString))
				{
					conn.Open();
					string query = "SELECT COUNT(1) FROM Users WHERE Account = @Account";
					using (SqlCommand cmd = new SqlCommand(query, conn))
					{
						cmd.Parameters.AddWithValue("@Account", account);
						int count = (int)cmd.ExecuteScalar();
						return count > 0;
					}
				}
			}
			catch (SqlException ex)
			{
				throw new InvalidOperationException("檢查帳號失敗", ex);
			}
		}

		/// <summary>
		/// 變更密碼
		/// </summary>
		public bool ChangePassword(int userId, string newPasswordHash)
		{
			try
			{
				using (SqlConnection conn = new SqlConnection(_connectionString))
				{
					conn.Open();
					string query = "UPDATE Users SET Password = @PasswordHash, UpdatedAt = GETDATE() WHERE Id = @UserId";
					using (SqlCommand cmd = new SqlCommand(query, conn))
					{
						cmd.Parameters.AddWithValue("@UserId", userId);
						cmd.Parameters.AddWithValue("@PasswordHash", newPasswordHash);
						return cmd.ExecuteNonQuery() > 0;
					}
				}
			}
			catch (SqlException ex)
			{
				throw new InvalidOperationException("變更密碼失敗", ex);
			}
		}

		/// <summary>
		/// 設定為完成首次登入
		/// </summary>
		public bool SetFirstLoginComplete(int userId)
		{
			try
			{
				using (SqlConnection conn = new SqlConnection(_connectionString))
				{
					conn.Open();
					string query = "UPDATE Users SET IsFirstLogin = 0, UpdatedAt = GETDATE() WHERE Id = @UserId";
					using (SqlCommand cmd = new SqlCommand(query, conn))
					{
						cmd.Parameters.AddWithValue("@UserId", userId);
						return cmd.ExecuteNonQuery() > 0;
					}
				}
			}
			catch (SqlException ex)
			{
				throw new InvalidOperationException("更新首次登入狀態失敗", ex);
			}
		}

		/// <summary>
		/// 取得超級管理員的數量
		/// </summary>
		public int GetSuperAdminCount()
		{
			try
			{
				using (SqlConnection conn = new SqlConnection(_connectionString))
				{
					conn.Open();
					string query = @"
						SELECT COUNT(1) FROM Users U
						JOIN AdminLevels AL ON U.AdminLevelId = AL.Id
						WHERE AL.Id = 1 AND U.IsBlacklisted = 0";

					using (SqlCommand cmd = new SqlCommand(query, conn))
					{
						return (int)cmd.ExecuteScalar();
					}
				}
			}
			catch (SqlException ex)
			{
				throw new InvalidOperationException("查詢超級管理員數量失敗", ex);
			}
		}

		/// <summary>取得指定身分的所有權限</summary>
		public IEnumerable<PermissionDto> GetPermissionsByAdminLevel(int adminLevelId)
		{
			var permissions = new List<PermissionDto>();
			try
			{
				using (SqlConnection conn = new SqlConnection(_connectionString))
				{
					conn.Open();
					string query = @"
                SELECT P.Id AS PermissionId, P.PermissionKey,
                       P.DisplayName, P.Description
                FROM   AdminLevelPermissions ALP
                JOIN   Permissions P ON ALP.PermissionId = P.Id
                WHERE  ALP.AdminLevelId = @AdminLevelId";

					using (SqlCommand cmd = new SqlCommand(query, conn))
					{
						cmd.Parameters.AddWithValue("@AdminLevelId", adminLevelId);
						using (SqlDataReader reader = cmd.ExecuteReader())
						{
							while (reader.Read())
							{
								permissions.Add(new PermissionDto
								{
									PermissionId = (int)reader["PermissionId"],
									PermissionKey = reader["PermissionKey"]?.ToString() ?? "",
									DisplayName = reader["DisplayName"]?.ToString() ?? "",
									Description = reader["Description"]?.ToString() ?? ""
								});
							}
						}
					}
				}
			}
			catch (SqlException ex)
			{
				throw new InvalidOperationException("查詢身分權限失敗", ex);
			}
			return permissions;
		}

		/// <summary>取得所有管理員身分（不含 DefaultPermissions，由 Service 補入）</summary>
		public IEnumerable<AdminLevelDto> GetAllAdminLevels()
		{
			var levels = new List<AdminLevelDto>();
			try
			{
				using (SqlConnection conn = new SqlConnection(_connectionString))
				{
					conn.Open();
					string query = @"
                SELECT Id AS AdminLevelId, LevelName, Description
                FROM   AdminLevels
                ORDER BY Id";

					using (SqlCommand cmd = new SqlCommand(query, conn))
					{
						using (SqlDataReader reader = cmd.ExecuteReader())
						{
							while (reader.Read())
							{
								levels.Add(new AdminLevelDto
								{
									AdminLevelId = (int)reader["AdminLevelId"],
									LevelName = reader["LevelName"]?.ToString() ?? "",
									Description = reader["Description"]?.ToString() ?? ""
								});
							}
						}
					}
				}
			}
			catch (SqlException ex)
			{
				throw new InvalidOperationException("查詢所有身分失敗", ex);
			}
			return levels;
		}

		/// <summary>更新管理員的身分等級與黑名單狀態</summary>
		public bool UpdateAdminLevel(int userId, int adminLevelId, bool isBlacklisted)
		{
			try
			{
				using (SqlConnection conn = new SqlConnection(_connectionString))
				{
					conn.Open();
					string query = @"
                UPDATE Users
                SET    AdminLevelId = @AdminLevelId,
                       IsBlacklisted = @IsBlacklisted,
                       UpdatedAt    = GETDATE()
                WHERE  Id = @UserId";

					using (SqlCommand cmd = new SqlCommand(query, conn))
					{
						cmd.Parameters.AddWithValue("@UserId", userId);
						cmd.Parameters.AddWithValue("@AdminLevelId", adminLevelId);
						cmd.Parameters.AddWithValue("@IsBlacklisted", isBlacklisted);
						return cmd.ExecuteNonQuery() > 0;
					}
				}
			}
			catch (SqlException ex)
			{
				throw new InvalidOperationException("更新管理員身分與狀態失敗", ex);
			}
		}

		/// <summary>新增管理員身分（含預設權限）</summary>
		public bool CreateAdminLevel(AdminLevelCreateDto dto)
		{
			try
			{
				using (SqlConnection conn = new SqlConnection(_connectionString))
				{
					conn.Open();
					using (SqlTransaction tx = conn.BeginTransaction())
					{
						try
						{
							// 1. 新增身分，取得新 ID
							string insertLevel = @"
                        INSERT INTO AdminLevels (LevelName, Description)
                        VALUES (@LevelName, @Description);
                        SELECT SCOPE_IDENTITY();";

							int newLevelId;
							using (SqlCommand cmd = new SqlCommand(insertLevel, conn, tx))
							{
								cmd.Parameters.AddWithValue("@LevelName", dto.LevelName);
								cmd.Parameters.AddWithValue("@Description", dto.Description ?? "");
								newLevelId = Convert.ToInt32(cmd.ExecuteScalar());
							}

							// 2. 逐筆新增權限對應
							if (dto.PermissionIds != null && dto.PermissionIds.Count > 0)
							{
								string insertPerm = @"
                            INSERT INTO AdminLevelPermissions (AdminLevelId, PermissionId)
                            VALUES (@AdminLevelId, @PermissionId)";

								foreach (int permId in dto.PermissionIds)
								{
									using (SqlCommand cmd = new SqlCommand(insertPerm, conn, tx))
									{
										cmd.Parameters.AddWithValue("@AdminLevelId", newLevelId);
										cmd.Parameters.AddWithValue("@PermissionId", permId);
										cmd.ExecuteNonQuery();
									}
								}
							}

							tx.Commit();
							return true;
						}
						catch
						{
							tx.Rollback();
							throw;
						}
					}
				}
			}
			catch (SqlException ex)
			{
				throw new InvalidOperationException("新增身分失敗", ex);
			}
		}

		/// <summary>覆蓋式更新管理員身分設定</summary>
		public bool UpdateAdminLevelConfig(AdminLevelUpdateDto dto)
		{
			try
			{
				using (SqlConnection conn = new SqlConnection(_connectionString))
				{
					conn.Open();
					using (SqlTransaction tx = conn.BeginTransaction())
					{
						try
						{
							// 1. 更新身分名稱與描述
							string updateLevel = @"
                        UPDATE AdminLevels
                        SET    LevelName   = @LevelName,
                               Description = @Description
                        WHERE  Id = @AdminLevelId";

							using (SqlCommand cmd = new SqlCommand(updateLevel, conn, tx))
							{
								cmd.Parameters.AddWithValue("@AdminLevelId", dto.AdminLevelId);
								cmd.Parameters.AddWithValue("@LevelName", dto.LevelName);
								cmd.Parameters.AddWithValue("@Description", dto.Description ?? "");
								cmd.ExecuteNonQuery();
							}

							// 2. 刪除舊權限
							string deletePerm = @"
                        DELETE FROM AdminLevelPermissions
                        WHERE  AdminLevelId = @AdminLevelId";

							using (SqlCommand cmd = new SqlCommand(deletePerm, conn, tx))
							{
								cmd.Parameters.AddWithValue("@AdminLevelId", dto.AdminLevelId);
								cmd.ExecuteNonQuery();
							}

							// 3. 逐筆新增新權限
							if (dto.PermissionIds != null && dto.PermissionIds.Count > 0)
							{
								string insertPerm = @"
                            INSERT INTO AdminLevelPermissions (AdminLevelId, PermissionId)
                            VALUES (@AdminLevelId, @PermissionId)";

								foreach (int permId in dto.PermissionIds)
								{
									using (SqlCommand cmd = new SqlCommand(insertPerm, conn, tx))
									{
										cmd.Parameters.AddWithValue("@AdminLevelId", dto.AdminLevelId);
										cmd.Parameters.AddWithValue("@PermissionId", permId);
										cmd.ExecuteNonQuery();
									}
								}
							}

							tx.Commit();
							return true;
						}
						catch
						{
							tx.Rollback();
							throw;
						}
					}
				}
			}
			catch (SqlException ex)
			{
				throw new InvalidOperationException("更新身分設定失敗", ex);
			}
		}

		/// <summary>刪除管理員身分（含關聯權限）</summary>
		public bool DeleteAdminLevel(int adminLevelId)
		{
			try
			{
				using (SqlConnection conn = new SqlConnection(_connectionString))
				{
					conn.Open();
					using (SqlTransaction tx = conn.BeginTransaction())
					{
						try
						{
							// 1. 刪除關聯權限
							string deletePerm = @"
                        DELETE FROM AdminLevelPermissions
                        WHERE  AdminLevelId = @AdminLevelId";

							using (SqlCommand cmd = new SqlCommand(deletePerm, conn, tx))
							{
								cmd.Parameters.AddWithValue("@AdminLevelId", adminLevelId);
								cmd.ExecuteNonQuery();
							}

							// 2. 刪除身分
							string deleteLevel = @"
                        DELETE FROM AdminLevels
                        WHERE  Id = @AdminLevelId";

							using (SqlCommand cmd = new SqlCommand(deleteLevel, conn, tx))
							{
								cmd.Parameters.AddWithValue("@AdminLevelId", adminLevelId);
								cmd.ExecuteNonQuery();
							}

							tx.Commit();
							return true;
						}
						catch
						{
							tx.Rollback();
							throw;
						}
					}
				}
			}
			catch (SqlException ex)
			{
				throw new InvalidOperationException("刪除身分失敗", ex);
			}
		}

		/// <summary>確認是否有啟用中的管理員綁定此身分</summary>
		public bool HasAdminsBoundToLevel(int adminLevelId)
		{
			try
			{
				using (SqlConnection conn = new SqlConnection(_connectionString))
				{
					conn.Open();
					string query = @"
                SELECT COUNT(1) FROM Users
                WHERE  AdminLevelId  = @AdminLevelId
                  AND  IsBlacklisted = 0";

					using (SqlCommand cmd = new SqlCommand(query, conn))
					{
						cmd.Parameters.AddWithValue("@AdminLevelId", adminLevelId);
						return (int)cmd.ExecuteScalar() > 0;
					}
				}
			}
			catch (SqlException ex)
			{
				throw new InvalidOperationException("查詢身分綁定人數失敗", ex);
			}
		}

		/// <summary>取得所有可用權限</summary>
		public IEnumerable<PermissionDto> GetAllPermissions()
		{
			var permissions = new List<PermissionDto>();
			try
			{
				using (SqlConnection conn = new SqlConnection(_connectionString))
				{
					conn.Open();
					string query = @"
                SELECT Id AS PermissionId, PermissionKey,
                       DisplayName, Description
                FROM   Permissions
                ORDER BY Id";

					using (SqlCommand cmd = new SqlCommand(query, conn))
					{
						using (SqlDataReader reader = cmd.ExecuteReader())
						{
							while (reader.Read())
							{
								permissions.Add(new PermissionDto
								{
									PermissionId = (int)reader["PermissionId"],
									PermissionKey = reader["PermissionKey"]?.ToString() ?? "",
									DisplayName = reader["DisplayName"]?.ToString() ?? "",
									Description = reader["Description"]?.ToString() ?? ""
								});
							}
						}
					}
				}
			}
			catch (SqlException ex)
			{
				throw new InvalidOperationException("查詢所有權限失敗", ex);
			}
			return permissions;
		}

		/// <summary>重設密碼並強制下次登入修改</summary>
		public bool ResetPassword(int userId, string passwordHash)
		{
			try
			{
				using (SqlConnection conn = new SqlConnection(_connectionString))
				{
					conn.Open();
					string query = @"
						UPDATE Users
						SET Password = @PasswordHash,
							IsFirstLogin = 1,
							UpdatedAt = GETDATE()
						WHERE Id = @UserId";

					using (SqlCommand cmd = new SqlCommand(query, conn))
					{
						cmd.Parameters.AddWithValue("@UserId", userId);
						cmd.Parameters.AddWithValue("@PasswordHash", passwordHash);
						return cmd.ExecuteNonQuery() > 0;
					}
				}
			}
			catch (SqlException ex)
			{
				throw new InvalidOperationException("重設管理員密碼失敗", ex);
			}
		}

		public AdminLoginClaimsDto GetAdminWithPermissions(int adminId)
		{
			AdminLoginClaimsDto adminPermission = null;

			try
			{
				using (SqlConnection conn = new SqlConnection(_connectionString))
				{
					conn.Open();
					string query = @"
                SELECT A.Id          AS AdminId,
                       A.AdminLevelId AS LevelId,
                       AL.LevelName,
                       P.PermissionKey
                FROM   Users A
                JOIN   AdminLevels AL
                           ON A.AdminLevelId = AL.Id
                JOIN   AdminLevelPermissions ALP
                           ON AL.Id = ALP.AdminLevelId
                JOIN   Permissions P
                           ON ALP.PermissionId = P.Id
                WHERE  A.Id = @AdminId";

					using (SqlCommand cmd = new SqlCommand(query, conn))
					{
						cmd.Parameters.AddWithValue("@AdminId", adminId);
						using (SqlDataReader reader = cmd.ExecuteReader())
						{
							while (reader.Read())
							{
								if (adminPermission == null)
								{
									adminPermission = new AdminLoginClaimsDto  // ← 這行
									{
										AdminId = (int)reader["AdminId"],
										LevelId = (int)reader["LevelId"],
										LevelName = reader["LevelName"].ToString(),
										PermissionKeys = new List<string>()
									};
								}

								string permissionKey = reader["PermissionKey"]?.ToString();
								if (!string.IsNullOrEmpty(permissionKey))
								{
									adminPermission.PermissionKeys.Add(permissionKey);
								}
							}
						}
					}
				}
			}
			catch (SqlException ex)
			{
				throw new InvalidOperationException("取得管理員權限清單失敗", ex);
			}

			return adminPermission;
		}

		
	}
}