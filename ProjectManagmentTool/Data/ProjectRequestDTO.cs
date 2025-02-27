namespace ProjectManagmentTool.Data
{
    public class ProjectRequestDTO
    {
        public string ProjectName { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int CompanyID { get; set; }  // ✅ Only IDs
        public string ProjectManagerID { get; set; }  // ✅ Only IDs
        public string Visibility { get; set; }
    }

}
