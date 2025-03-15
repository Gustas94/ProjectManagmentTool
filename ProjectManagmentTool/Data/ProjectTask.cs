using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

public class ProjectTask
{
    [Key]
    public int TaskID { get; set; }

    public string TaskName { get; set; }
    public string Description { get; set; }
    public DateTime Deadline { get; set; }
    public string Status { get; set; }
    public string Priority { get; set; }

    // Foreign key to Project
    public int ProjectID { get; set; }
    [ForeignKey("ProjectID")]
    public Project Project { get; set; }

    // Foreign key to Group (if applicable)
    public int? GroupID { get; set; } // Make GroupID nullable
    [ForeignKey("GroupID")]
    public Group Group { get; set; }

    // Timestamps
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}