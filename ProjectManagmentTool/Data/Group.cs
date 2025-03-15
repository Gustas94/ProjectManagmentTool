using ProjectManagmentTool.Data;
using System.ComponentModel.DataAnnotations.Schema;

public class Group
{
    public int GroupID { get; set; }
    public string GroupName { get; set; }
    public string Description { get; set; }
    public string GroupLeadID { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public User GroupLead { get; set; }

    // Reference to the project this group belongs to
    public int? ProjectID { get; set; }
    [ForeignKey("ProjectID")]
    public Project Project { get; set; }

    // Many-to-Many Relationship (Projects ↔ Groups)
    public ICollection<ProjectGroup> ProjectGroups { get; set; } = new List<ProjectGroup>();

    // Many-to-Many Relationship (Groups ↔ Users)
    public ICollection<GroupMember> GroupMembers { get; set; } = new List<GroupMember>();

    // Tasks associated with this group
    public virtual ICollection<ProjectTask> Tasks { get; set; } = new List<ProjectTask>();
}