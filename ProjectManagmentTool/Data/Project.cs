namespace ProjectManagmentTool.Data
{
    public class Project
    {
        public int ProjectID { get; set; }
        public string ProjectName { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int CompanyID { get; set; }
        public string ProjectManagerID { get; set; }
        public string Visibility { get; set; } // Public or Private
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public Company Company { get; set; }
        public User ProjectManager { get; set; }
    }
}
