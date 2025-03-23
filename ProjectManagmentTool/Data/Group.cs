using ProjectManagmentTool.Data;
using System.ComponentModel.DataAnnotations.Schema;

public class Group
{
    public int GroupID { get; set; }
    public string GroupName { get; set; }
    public string Description { get; set; }
    public string GroupLeadID { get; set; }
    public int? CompanyID { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // ✅ Add these two
    public string Status { get; set; } = "Active"; // or "Paused", "Completed"
    public string CurrentProgress { get; set; } = "Not Started"; // or custom values

    public Company Company { get; set; }
    public User GroupLead { get; set; }

    public int? ProjectID { get; set; }
    [ForeignKey("ProjectID")]
    public Project Project { get; set; }

    public ICollection<ProjectGroup> ProjectGroups { get; set; } = new List<ProjectGroup>();
    public ICollection<GroupMember> GroupMembers { get; set; } = new List<GroupMember>();
    public ICollection<ProjectTask> Tasks { get; set; } = new List<ProjectTask>();
    public ICollection<TaskGroup> TaskGroups { get; set; } = new List<TaskGroup>();
}
