using ProjectManagmentTool.Data;
using System.Text.Json.Serialization; // Required for [JsonIgnore]

public class Project
{
    public int ProjectID { get; set; }
    public string ProjectName { get; set; }
    public string Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int CompanyID { get; set; }
    public string ProjectManagerID { get; set; }
    public string Visibility { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Prevents Swagger from requiring full objects
    [JsonIgnore]
    public Company Company { get; set; }

    [JsonIgnore]
    public User ProjectManager { get; set; }

    public ICollection<UserProject> UserProjects { get; set; } = new List<UserProject>();
}
