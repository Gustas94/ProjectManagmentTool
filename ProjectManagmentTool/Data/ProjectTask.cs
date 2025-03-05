using System;

namespace ProjectManagmentTool.Data
{
    public class ProjectTask
    {
        public int TaskID { get; set; }  // ✅ Ensure this is a primary key
        public string TaskName { get; set; }
        public string Description { get; set; }
        public DateTime Deadline { get; set; }
        public int ProjectID { get; set; }
        public int? GroupID { get; set; } // Nullable for now
        public string AssignedTo { get; set; }
        public string Status { get; set; } // To Do, In Progress, Completed
        public string Priority { get; set; } // Urgent, Medium, Low
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // ✅ Define relationships
        public Project Project { get; set; }
        public Group Group { get; set; }
        public User AssignedUser { get; set; }
    }
}
