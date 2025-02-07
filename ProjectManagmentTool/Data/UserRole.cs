﻿namespace ProjectManagmentTool.Data
{
    public class UserRole
    {
        public int UserID { get; set; }
        public int RoleID { get; set; }
        public int ProjectID { get; set; }  // Optional, if you're using project-specific roles

        public User User { get; set; }
        public Role Role { get; set; }
        public Project Project { get; set; }  // If using Project-specific roles
    }
}
