using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectManagmentTool.Data
{
    public class UserProject
    {
        public string UserID { get; set; }
        public int ProjectID { get; set; }
        public User User { get; set; }
        public Project Project { get; set; }
        public int? GroupID { get; set; }
        [ForeignKey("GroupID")]
        public Group Group { get; set; }
    }
}