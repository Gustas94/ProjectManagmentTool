using Microsoft.AspNetCore.Identity;
using System;

namespace ProjectManagmentTool.Data
{
    public class User : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int? CompanyID { get; set; }
        public string RoleID { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public Company Company { get; set; }
        public Company CEOCompany { get; set; }
        public Role Role { get; set; }
    }
}
