using ISpanShop.Models.DTOs;
using ISpanShop.Repositories.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace ISpanShop.Repositories
{
	/// <summary>
	/// 管理員角色資料存取實現 - 使用 ADO.NET
	/// </summary>
	public class AdminRoleRepository : IAdminRoleRepository
	{
		private readonly string _connectionString;

		public AdminRoleRepository(IConfiguration configuration)
		{
			_connectionString = configuration.GetConnectionString("DefaultConnection")
				?? throw new ArgumentNullException(nameof(configuration), "DefaultConnection 未設定");
		}

		/// <summary>
		/// 取得所有管理員列表
		/// </summary>
		public IEnumerable<AdminPermissionDto> GetAllPermissions()
		{
			var permissions = new List<AdminPermissionDto>();
			try
			{
				using (SqlConnection conn = new SqlConnection(_connectionString))
				{
					conn.Open();
					string query = @"
                SELECT Id AS PermissionId, PermissionKey, Description
                FROM Permissions
                ORDER BY Id";

					using (SqlCommand cmd = new SqlCommand(query, conn))
					using (SqlDataReader reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							permissions.Add(new AdminPermissionDto
							{
								PermissionId = (int)reader["PermissionId"],
								PermissionKey = reader["PermissionKey"]?.ToString()!,
								Description = reader["Description"]?.ToString()
							});
						}
					}
				}
			}
			catch (SqlException ex)
			{
				throw new InvalidOperationException("資料庫查詢失敗", ex);
			}
			return permissions;
		}

		
	}
}
