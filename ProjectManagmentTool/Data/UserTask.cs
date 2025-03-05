using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectManagmentTool.Data
{
    public class UserTask
    {
        public string UserID { get; set; }
        public int TaskID { get; set; }

        public User User { get; set; }
        public ProjectTask Task { get; set; }
    }
}
