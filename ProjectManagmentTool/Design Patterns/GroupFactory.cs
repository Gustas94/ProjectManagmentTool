using ProjectManagmentTool.Data;

namespace ProjectManagmentTool.Design_Patterns
{
    public static class GroupFactory
    {
        public static Group CreateGroup(string groupName, string description, string groupLeadID, int? companyID)
        {
            return new Group
            {
                GroupName = groupName,
                Description = description,
                GroupLeadID = groupLeadID,
                CompanyID = companyID, // Ensure correct type
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public static GroupMember CreateGroupMember(int groupId, string userId)
        {
            return new GroupMember
            {
                GroupID = groupId,
                UserID = userId
            };
        }
    }
}
