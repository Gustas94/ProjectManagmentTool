namespace ProjectManagmentTool.Data
{
    public class Permission
    {
        public int PermissionID { get; set; }
        public string PermissionName { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public ICollection<RolePermission> RolePermissions { get; set; }
    }
}
