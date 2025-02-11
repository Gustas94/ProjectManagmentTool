using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace ProjectManagmentTool.Data
{
    public class Role : IdentityRole
    {
        public int? CompanyID { get; set; }
        public bool IsCompanyRole { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public Company Company { get; set; }
        public ICollection<RolePermission> RolePermissions { get; set; }
    }
}
