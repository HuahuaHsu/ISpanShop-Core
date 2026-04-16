using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
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

        public IEnumerable<StoreDto> GetAllStores(
            string? keyword,
            string verifyStatus,
            string blockStatus,
            int? storeStatusFilter,
            string sortColumn,
            string sortDirection,
            int page,
            int pageSize,
            out int totalCount
        )
        {
            var result = new List<StoreDto>();
            totalCount = 0;
            var connectionString = _context.Database.GetDbConnection().ConnectionString;

            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();

                // 1. 條件過濾與總筆數
                string whereClause = @"
                    WHERE 1 = 1
                      AND (@Keyword IS NULL
                           OR S.StoreName LIKE '%' + @Keyword + '%'
                           OR U.Account   LIKE '%' + @Keyword + '%')
                      AND (@VerifyStatus = 'all'
                           OR (@VerifyStatus = 'verified'   AND S.IsVerified = 1)
                           OR (@VerifyStatus = 'unverified' AND S.IsVerified = 0))
                      AND (@BlockStatus = 'all'
                           OR (@BlockStatus = 'normal'  AND U.IsBlacklisted = 0)
                           OR (@BlockStatus = 'blocked' AND U.IsBlacklisted = 1))
                      AND (@StoreStatusFilter IS NULL OR S.StoreStatus = @StoreStatusFilter)";

                string countSql = $@"
                    SELECT COUNT(DISTINCT S.Id)
                    FROM   Stores S
                    JOIN   Users U ON S.UserId = U.Id
                    {whereClause}";

                using (var cmdCount = new SqlCommand(countSql, conn))
                {
                    cmdCount.Parameters.AddWithValue("@Keyword", (object)keyword ?? DBNull.Value);
                    cmdCount.Parameters.AddWithValue("@VerifyStatus", verifyStatus);
                    cmdCount.Parameters.AddWithValue("@BlockStatus", blockStatus);
                    cmdCount.Parameters.AddWithValue("@StoreStatusFilter", (object)storeStatusFilter ?? DBNull.Value);
                    totalCount = (int)cmdCount.ExecuteScalar();
                }

                // 2. 分頁資料查詢
                string dataSql = $@"
                    SELECT S.Id            AS StoreId,
                           S.UserId,
                           U.Account       AS OwnerAccount,
                           S.StoreName,
                           S.Description,
                           S.IsVerified,
                           U.IsBlacklisted,
                           S.StoreStatus,
                           S.CreatedAt,
                           (SELECT COUNT(*) FROM Products P WHERE P.StoreId = S.Id AND P.IsDeleted = 0) AS ProductCount
                    FROM   Stores S
                    JOIN   Users U   ON S.UserId = U.Id
                    {whereClause}
                    ORDER BY 
                        CASE WHEN @SortColumn = 'StoreName'  AND @SortDirection = 'asc'  THEN S.StoreName END ASC,
                        CASE WHEN @SortColumn = 'StoreName'  AND @SortDirection = 'desc' THEN S.StoreName END DESC,
                        CASE WHEN @SortColumn = 'CreatedAt'  AND @SortDirection = 'asc'  THEN S.CreatedAt END ASC,
                        CASE WHEN @SortColumn = 'CreatedAt'  AND @SortDirection = 'desc' THEN S.CreatedAt END DESC,
                        CASE WHEN @SortColumn = 'StoreId'    AND @SortDirection = 'asc'  THEN S.Id END ASC,
                        CASE WHEN @SortColumn = 'StoreId'    AND @SortDirection = 'desc' THEN S.Id END DESC
                    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

                using (var cmd = new SqlCommand(dataSql, conn))
                {
                    cmd.Parameters.AddWithValue("@Keyword", (object)keyword ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@VerifyStatus", verifyStatus);
                    cmd.Parameters.AddWithValue("@BlockStatus", blockStatus);
                    cmd.Parameters.AddWithValue("@StoreStatusFilter", (object)storeStatusFilter ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@SortColumn", sortColumn);
                    cmd.Parameters.AddWithValue("@SortDirection", sortDirection);
                    cmd.Parameters.AddWithValue("@Offset", (page - 1) * pageSize);
                    cmd.Parameters.AddWithValue("@PageSize", pageSize);

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
								Description = reader.IsDBNull("Description")
					? null : reader.GetString("Description"),
								IsVerified = reader.GetBoolean("IsVerified"),
								IsBlacklisted = reader.GetBoolean("IsBlacklisted"),
								StoreStatus = Convert.ToInt32(reader["StoreStatus"]), 
								CreatedAt = reader.IsDBNull("CreatedAt")
					? (DateTime?)null : reader.GetDateTime("CreatedAt"),
								ProductCount = Convert.ToInt32(reader["ProductCount"])
							});
                        }
                    }
                }
            }
            return result;
        }

        public (int Total, int Verified, int Blocked) GetStoreStats()
        {
            int total = 0, verified = 0, blocked = 0;
            var connectionString = _context.Database.GetDbConnection().ConnectionString;

            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string sql = @"
                    SELECT COUNT(*) AS Total,
                           SUM(CASE WHEN IsVerified = 1 THEN 1 ELSE 0 END) AS Verified,
                           (SELECT COUNT(*) FROM Users U JOIN Stores S ON U.Id = S.UserId WHERE U.IsBlacklisted = 1) AS Blocked
                    FROM Stores S";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            total = reader.GetInt32("Total");
                            verified = reader.IsDBNull("Verified") ? 0 : reader.GetInt32("Verified");
                            blocked = reader.GetInt32("Blocked");
                        }
                    }
                }
            }
            return (total, verified, blocked);
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
                           S.StoreStatus,
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
                                Description = reader.IsDBNull("Description")
                         ? null : reader.GetString("Description"),
                                IsVerified = reader.GetBoolean("IsVerified"),
                                IsBlacklisted = reader.GetBoolean("IsBlacklisted"),
                                StoreStatus = Convert.ToInt32(reader["StoreStatus"]),
                                CreatedAt = reader.IsDBNull("CreatedAt")
                         ? (DateTime?)null : reader.GetDateTime("CreatedAt"),
                                ProductCount = Convert.ToInt32(reader["ProductCount"]),
                                ActiveProductCount = Convert.ToInt32(reader["ActiveProductCount"]),
                                TotalSales = Convert.ToInt32(reader["TotalSales"])
                            };
                        }
                    }
                }
            }
            return null;
        }

        public async Task<Store?> GetStoreByUserIdAsync(int userId)
        {
            return await _context.Stores
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.UserId == userId);
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

        public bool UpdateStoreStatus(int storeId, int status)
        {
            var connectionString = _context.Database.GetDbConnection().ConnectionString;

            using (var conn = new SqlConnection(connectionString))
            {
                string sql = @"
                    UPDATE Stores
                    SET    StoreStatus = @StoreStatus
                    WHERE  Id = @StoreId";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@StoreStatus", status);
                    cmd.Parameters.AddWithValue("@StoreId", storeId);
                    conn.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

		public bool ToggleBlacklist(int userId, bool isBlacklisted)
		{
			throw new NotImplementedException();
		}
	}
}
