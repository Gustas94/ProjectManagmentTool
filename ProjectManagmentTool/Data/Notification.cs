namespace ProjectManagmentTool.Data
{
    public class Notification
    {
        public int NotificationID { get; set; }
        public int UserID { get; set; }
        public string Message { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }

        public User User { get; set; }
    }
}
