namespace ProjectManagmentTool.Data
{
    public class RolePermission
    {
        public string RoleID { get; set; }
        public int PermissionID { get; set; }

        public Role Role { get; set; }
        public Permission Permission { get; set; }
    }
}
