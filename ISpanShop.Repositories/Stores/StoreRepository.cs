using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ISpanShop.Models.DTOs.Stores;
using ISpanShop.Models.EfModels;

namespace ISpanShop.Repositories.Stores
{
    public class StoreRepository : IStoreRepository
    {
        private readonly ISpanShopDBContext _context;

        public StoreRepository(ISpanShopDBContext context)
        {
            _context = context;
        }

        public IEnumerable<StoreDto> GetAllStores()
        {
            var result = new List<StoreDto>();
            var connectionString = _context.Database.GetDbConnection().ConnectionString;

            using (var conn = new SqlConnection(connectionString))
            {
                // 優化：採用子查詢分別計算，提升對大表的查詢效能
                string sql = @"
                    SELECT S.Id            AS StoreId,
                           S.UserId,
                           U.Account       AS OwnerAccount,
                           S.StoreName,
                           S.Description,
                           S.IsVerified,
                           U.IsBlacklisted,
                           S.CreatedAt,
                           (SELECT COUNT(*) FROM Products P WHERE P.StoreId = S.Id AND P.IsDeleted = 0) AS ProductCount
                    FROM   Stores S
                    JOIN   Users U   ON S.UserId = U.Id
                    ORDER BY S.CreatedAt DESC";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Add(new StoreDto
                            {
                                StoreId = reader.GetInt32("StoreId"),
                                UserId = reader.GetInt32("UserId"),
                                OwnerAccount = reader.GetString("OwnerAccount"),
                                StoreName = reader.GetString("StoreName"),
                                Description = reader.IsDBNull("Description") ? null : reader.GetString("Description"),
                                IsVerified = reader.GetBoolean("IsVerified"),
                                IsBlacklisted = reader.GetBoolean("IsBlacklisted"),
                                CreatedAt = reader.IsDBNull("CreatedAt") ? (DateTime?)null : reader.GetDateTime("CreatedAt"),
                                ProductCount = reader.GetInt32("ProductCount")
                            });
                        }
                    }
                }
            }
            return result;
        }

        public StoreDetailDto? GetStoreById(int storeId)
        {
            var connectionString = _context.Database.GetDbConnection().ConnectionString;

            using (var conn = new SqlConnection(connectionString))
            {
                string sql = @"
                    SELECT S.Id              AS StoreId,
                           S.UserId,
                           U.Account         AS OwnerAccount,
                           S.StoreName,
                           S.Description,
                           S.IsVerified,
                           U.IsBlacklisted,
                           S.CreatedAt,
                           (SELECT COUNT(*) FROM Products P WHERE P.StoreId = S.Id AND P.IsDeleted = 0) AS ProductCount,
                           (SELECT COUNT(*) FROM Products P WHERE P.StoreId = S.Id AND P.IsDeleted = 0 AND P.Status = 1) AS ActiveProductCount,
                           ISNULL((SELECT SUM(TotalSales) FROM Products P WHERE P.StoreId = S.Id AND P.IsDeleted = 0), 0) AS TotalSales
                    FROM   Stores S
                    JOIN   Users U   ON S.UserId = U.Id
                    WHERE  S.Id = @StoreId";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@StoreId", storeId);
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new StoreDetailDto
                            {
                                StoreId = reader.GetInt32("StoreId"),
                                UserId = reader.GetInt32("UserId"),
                                OwnerAccount = reader.GetString("OwnerAccount"),
                                StoreName = reader.GetString("StoreName"),
                                Description = reader.IsDBNull("Description") ? null : reader.GetString("Description"),
                                IsVerified = reader.GetBoolean("IsVerified"),
                                IsBlacklisted = reader.GetBoolean("IsBlacklisted"),
                                CreatedAt = reader.IsDBNull("CreatedAt") ? (DateTime?)null : reader.GetDateTime("CreatedAt"),
                                ProductCount = reader.GetInt32("ProductCount"),
                                ActiveProductCount = reader.GetInt32("ActiveProductCount"),
                                TotalSales = reader.GetInt32("TotalSales")
                            };
                        }
                    }
                }
            }
            return null;
        }

        public bool ToggleVerified(int storeId, bool isVerified)
        {
            var connectionString = _context.Database.GetDbConnection().ConnectionString;

            using (var conn = new SqlConnection(connectionString))
            {
                string sql = @"
                    UPDATE Stores
                    SET    IsVerified = @IsVerified
                    WHERE  Id = @StoreId";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@IsVerified", isVerified);
                    cmd.Parameters.AddWithValue("@StoreId", storeId);
                    conn.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public bool ToggleBlacklist(int userId, bool isBlacklisted)
        {
            var connectionString = _context.Database.GetDbConnection().ConnectionString;

            using (var conn = new SqlConnection(connectionString))
            {
                string sql = @"
                    UPDATE Users
                    SET    IsBlacklisted = @IsBlacklisted,
                           UpdatedAt = GETDATE()
                    WHERE  Id = @UserId";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@IsBlacklisted", isBlacklisted);
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    conn.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }
    }
}
