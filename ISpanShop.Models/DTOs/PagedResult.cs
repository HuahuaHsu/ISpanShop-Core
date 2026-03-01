using System;
using System.Collections.Generic;

namespace ISpanShop.Models.DTOs
{
    /// <summary>
    /// 泛型分頁結果物件
    /// </summary>
    /// <typeparam name="T">資料型別</typeparam>
    public class PagedResult<T>
    {
        /// <summary>
        /// 當頁資料
        /// </summary>
        public List<T> Data { get; set; } = new List<T>();

        /// <summary>
        /// 總頁數
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// 目前頁碼
        /// </summary>
        public int CurrentPage { get; set; }

        /// <summary>
        /// 總筆數
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// 每頁筆數
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// 建立分頁結果
        /// </summary>
        public static PagedResult<T> Create(List<T> data, int totalCount, int pageNumber, int pageSize)
        {
            return new PagedResult<T>
            {
                Data = data,
                TotalCount = totalCount,
                CurrentPage = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };
        }
    }
}
