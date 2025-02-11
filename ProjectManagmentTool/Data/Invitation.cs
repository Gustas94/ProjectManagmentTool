namespace ProjectManagmentTool.Data
{
    public class Invitation
    {
        public int InvitationID { get; set; }
        public string InvitationLink { get; set; }
        public string Email { get; set; }
        public string RoleID { get; set; }
        public int CompanyID { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; }

        public Role Role { get; set; }
        public Company Company { get; set; }
    }
}
