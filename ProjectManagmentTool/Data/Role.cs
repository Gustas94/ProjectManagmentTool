namespace ProjectManagmentTool.Data
{
    public class Role
    {
        public int RoleID { get; set; }
        public string RoleName { get; set; }
        public int CompanyID { get; set; }
        public bool IsCompanyRole { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public Company Company { get; set; }

        public ICollection<RolePermission> RolePermissions { get; set; }
    }
}
