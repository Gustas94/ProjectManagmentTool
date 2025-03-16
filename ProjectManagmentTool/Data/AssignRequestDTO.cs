namespace ProjectManagmentTool.Data
{
    public class AssignRequestDTO
    {
        // List of individually assigned user IDs
        public List<string> AssignedUserIDs { get; set; }

        // List of group IDs to assign to the task
        public List<int> AssignedGroupIDs { get; set; }
    }
}
