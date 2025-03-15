namespace ProjectManagmentTool.Data
{
    public class GroupMember
    {
        public int GroupID { get; set; }
        public Group Group { get; set; }

        public string UserID { get; set; }
        public User User { get; set; }
    }
}
