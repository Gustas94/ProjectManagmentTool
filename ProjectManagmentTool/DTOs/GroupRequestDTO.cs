namespace ProjectManagmentTool.DTOs
{
    public class GroupRequestDTO
    {
        public string GroupName { get; set; }
        public string Description { get; set; }
        // The selected team lead (must be one of the selected group members)
        public string GroupLeadID { get; set; }
        // List of selected company user IDs to add as group members
        public List<string> GroupMemberIDs { get; set; }
    }
}
