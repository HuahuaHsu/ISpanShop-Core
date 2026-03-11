using ISpanShop.Models.DTOs;
using ISpanShop.Repositories.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace ISpanShop.Repositories
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

		public AdminDto? GetAdminById(int adminId)
		{
			try
			{
				using (SqlConnection conn = new SqlConnection(_connectionString))
				{
					conn.Open();
					string query = @"
                SELECT 
                    A.Id AS AdminId,
                    A.Account,
                    A.Email,
                    A.RoleId,
                    R.RoleName,
                    A.CreatedAt,
                    A.UpdatedAt
                FROM Users A
                JOIN Roles R ON A.RoleId = R.Id
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
									AdminId = (int)reader["AdminId"],
									Account = reader["Account"]?.ToString(),
									Email = reader["Email"]?.ToString(),
									RoleId = (int)reader["RoleId"],
									RoleName = reader["RoleName"]?.ToString(),
									CreatedAt = reader["CreatedAt"] != DBNull.Value
													? (DateTime?)reader["CreatedAt"] : null,
									UpdatedAt = reader["UpdatedAt"] != DBNull.Value
													? (DateTime?)reader["UpdatedAt"] : null
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
		public IEnumerable<AdminDto> GetAllAdmins()
		{
			var admins = new List<AdminDto>();

			try
			{
				using (SqlConnection conn = new SqlConnection(_connectionString))
				{
					conn.Open();

					string query = @"
						SELECT 
							A.Id AS AdminId,
							A.Account,
							A.Email,
							A.RoleId,
							R.RoleName,
							A.CreatedAt,
							A.UpdatedAt
						FROM Users A
						JOIN Roles R ON A.RoleId = R.Id
						WHERE R.RoleName IN ('Admin', 'SuperAdmin')
						ORDER BY A.CreatedAt DESC";

					using (SqlCommand cmd = new SqlCommand(query, conn))
					{
						using (SqlDataReader reader = cmd.ExecuteReader())
						{
							while (reader.Read())
							{
								admins.Add(new AdminDto
								{
									AdminId = (int)reader["AdminId"],
									Account = reader["Account"]?.ToString(),
									Email = reader["Email"]?.ToString(),
									RoleId = (int)reader["RoleId"],
									RoleName = reader["RoleName"]?.ToString(),
									CreatedAt = reader["CreatedAt"] != DBNull.Value ? (DateTime?)reader["CreatedAt"] : null,
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

		
	}
}
