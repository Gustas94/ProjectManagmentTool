namespace ProjectManagmentTool.Data
{
    public class User
    {
        public int UserID { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PasswordHash { get; set; }
        public int CompanyID { get; set; }
        public int RoleID { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public Company Company { get; set; }
        public Role Role { get; set; }
    }
}
