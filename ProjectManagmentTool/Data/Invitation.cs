namespace ProjectManagmentTool.Data
{
    public class Invitation
    {
        public int InvitationID { get; set; }
        public string InviteCode { get; set; } = Guid.NewGuid().ToString(); // Automatically generated unique code
        public string InvitationLink => $"http://localhost:5173/register?invite={InviteCode}"; // Dynamically generated link
        public string? Email { get; set; }
        public string? RoleID { get; set; }
        public int CompanyID { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Role Role { get; set; }
        public Company Company { get; set; }
    }
}
