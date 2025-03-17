using System;
using System.Collections.Generic;

namespace ProjectManagmentTool.Data
{
    public class Company
    {
        public int CompanyID { get; set; }
        public string CompanyName { get; set; }
        // This property stores the User Id of the CEO.
        public string CEOID { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int IndustryId { get; set; }
        public Industry Industry { get; set; }

        // One-to-many: all users belonging to this company.
        public ICollection<User> Users { get; set; }
        // One-to-one: the designated CEO.
        public User CEO { get; set; }

        public ICollection<Group> Groups { get; set; } = new List<Group>();
    }
}
