namespace ProjectManagmentTool.Data
{
    public class Discussion
    {
        public int DiscussionID { get; set; }
        public int ProjectID { get; set; }
        public int GroupID { get; set; }
        public int UserID { get; set; }
        public string Message { get; set; }
        public DateTime CreatedAt { get; set; }

        public Project Project { get; set; }
        public Group Group { get; set; }
        public User User { get; set; }
    }
}
