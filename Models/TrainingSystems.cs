namespace DemoMVC.Models
{
    // Danh mục hệ đào tạo: mã 2 chữ số là phần thứ 2 của mã sinh viên.
    // 21 = Đại học chính quy theo đề bài, các mã còn lại là ví dụ.
    public static class TrainingSystems
    {
        public static readonly IReadOnlyDictionary<string, string> All = new Dictionary<string, string>
        {
            ["21"] = "Đại học chính quy",
            ["22"] = "Đại học liên thông",
            ["23"] = "Văn bằng 2",
        };

        public static bool IsValid(string? code) => code != null && All.ContainsKey(code);

        public static string GetName(string code) => All.TryGetValue(code, out var name) ? name : code;
    }
}
