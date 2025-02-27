using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectManagmentTool.Data
{
    public class UserProject
    {
        public string UserID { get; set; }
        public int ProjectID { get; set; }

        // Navigation properties
        public User User { get; set; }
        public Project Project { get; set; }
    }
}
