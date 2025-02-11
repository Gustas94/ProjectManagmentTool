namespace ProjectManagmentTool.Data
{
    public class Group
    {
        public int GroupID { get; set; }
        public string GroupName { get; set; }
        public string Description { get; set; }
        public int ProjectID { get; set; }
        public string GroupLeadID { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public Project Project { get; set; }
        public User GroupLead { get; set; }
    }
}
