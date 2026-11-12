using Microsoft.EntityFrameworkCore;

namespace DemoMVC.Helpers
{
    // =========================
    // Bài 12: Phân trang
    // Chỉ lấy đúng số dòng của trang hiện tại từ CSDL (Skip/Take → OFFSET ... FETCH NEXT)
    // =========================
    public class PaginatedList<T> : List<T>
    {
        public static readonly int[] PageSizes = { 5, 10, 20, 50 };
        public const int DefaultPageSize = 10;

        public int PageIndex { get; private set; }
        public int PageSize { get; private set; }
        public int TotalCount { get; private set; }
        public string? Keyword { get; private set; }

        public int TotalPages => Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));
        public bool HasPreviousPage => PageIndex > 1;
        public bool HasNextPage => PageIndex < TotalPages;
        public int FirstItem => TotalCount == 0 ? 0 : (PageIndex - 1) * PageSize + 1;
        public int LastItem => Math.Min(PageIndex * PageSize, TotalCount);

        public PaginatedList(List<T> items, int count, int pageIndex, int pageSize, string? keyword)
        {
            TotalCount = count;
            PageIndex = pageIndex;
            PageSize = pageSize;
            Keyword = keyword;
            AddRange(items);
        }

        // source cần OrderBy trước để thứ tự các trang ổn định
        public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int pageIndex, int pageSize, string? keyword = null)
        {
            if (!PageSizes.Contains(pageSize))
            {
                pageSize = DefaultPageSize;
            }

            var count = await source.CountAsync();
            var tongSoTrang = Math.Max(1, (int)Math.Ceiling(count / (double)pageSize));

            // Ví dụ xoá dòng cuối cùng của trang cuối → lùi về trang trước đó
            pageIndex = Math.Clamp(pageIndex, 1, tongSoTrang);

            var items = await source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
            return new PaginatedList<T>(items, count, pageIndex, pageSize, keyword);
        }
    }
}
