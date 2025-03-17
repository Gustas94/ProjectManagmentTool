using Microsoft.AspNetCore.Identity;
using System;

namespace ProjectManagmentTool.Data
{
    public class User : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        // This foreign key is for the company membership (many users can share one company)
        public int? CompanyID { get; set; }
        public string RoleID { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation property for company membership
        public Company Company { get; set; }
        // Navigation property for when this user is the CEO
        public Company CEOCompany { get; set; }
        public Role Role { get; set; }
    }
}
