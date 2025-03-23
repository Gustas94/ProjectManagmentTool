namespace ProjectManagmentTool.DTOs
{
    public class AssignMembersRequestDTO
    {
        // The project from which you want to assign members
        public int ProjectID { get; set; }
        // List of user IDs to assign to the group
        public List<string> UserIDs { get; set; }
    }
}
