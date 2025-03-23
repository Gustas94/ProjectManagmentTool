using System.Collections.Generic;

namespace ProjectManagmentTool.DTOs
{
    public class GroupDetailsDTO
    {
        public int GroupID { get; set; }
        public string GroupName { get; set; }
        public string Description { get; set; }
        public List<string> ProjectAffiliation { get; set; }
        public string GroupLeadName { get; set; }
        public string CurrentProgress { get; set; }
        public string Status { get; set; }
        public List<TaskDTO> Tasks { get; set; }
        public List<UserDTO> Members { get; set; }
    }
}