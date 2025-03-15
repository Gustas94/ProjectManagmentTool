namespace ProjectManagmentTool.Data
{
    public class TaskGroup
    {
        public int TaskID { get; set; } // Foreign key to Task
        public ProjectTask Task { get; set; } // Navigation property to Task

        public int GroupID { get; set; } // Foreign key to Group
        public Group Group { get; set; } // Navigation property to Group
    }
}