using System.Collections.Generic;

namespace MyNavicat.Api.Models.Common
{
    /// <summary>
    /// 分頁結果類別
    /// </summary>
    /// <typeparam name="T">資料型別</typeparam>
    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new List<T>();
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public long TotalCount { get; set; }
        public int TotalPages => PageSize > 0 ? (int)System.Math.Ceiling((double)TotalCount / PageSize) : 0;

        public PagedResult()
        {
        }

        public PagedResult(List<T> items, int pageIndex, int pageSize, long totalCount)
        {
            Items = items;
            PageIndex = pageIndex;
            PageSize = pageSize;
            TotalCount = totalCount;
        }
    }
}
