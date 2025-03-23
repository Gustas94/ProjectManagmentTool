namespace ProjectManagmentTool.DTOs
{
    public class TaskDTO
    {
        public int TaskID { get; set; }
        public string TaskName { get; set; }
        public string Description { get; set; }
        public string Deadline { get; set; }
        public string Priority { get; set; }
        public string Status { get; set; }
    }
}
