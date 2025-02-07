namespace ProjectManagmentTool.Data
{
    public class Company
    {
        public int CompanyID { get; set; }
        public string CompanyName { get; set; }
        public string Industry { get; set; }
        public int CEOID { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public User CEO { get; set; }
    }
}
