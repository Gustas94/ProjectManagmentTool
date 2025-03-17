using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using ProjectManagmentTool.Data;

public class ProjectTask
{
    [Key]
    public int TaskID { get; set; }
    public string TaskName { get; set; }
    public string Description { get; set; }
    public DateTime Deadline { get; set; }
    public string Status { get; set; }
    public string Priority { get; set; }
    public int ProjectID { get; set; }
    [ForeignKey("ProjectID")]
    public Project Project { get; set; }
    public int? GroupID { get; set; }
    [ForeignKey("GroupID")]
    public Group Group { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public ICollection<TaskGroup> TaskGroups { get; set; } = new List<TaskGroup>();
}