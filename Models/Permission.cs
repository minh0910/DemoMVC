namespace DemoMVC.Models
{
    public class Permission
    {
        public int Id { get; set; }

        public string Controller { get; set; } = string.Empty;

        public string Action { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public string Key => $"{Controller}.{Action}";

        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}
