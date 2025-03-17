using ProjectManagmentTool.Data;

namespace ProjectManagmentTool.Factories
{
    using System;
    using System.Collections.Generic;

    public static class GroupFactory
    {
        public static Group CreateGroup(string groupName, string description, string groupLeadID, int? companyID)
        {
            return new Group
            {
                GroupName = groupName,
                Description = description,
                GroupLeadID = groupLeadID,
                CompanyID = companyID,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public static List<GroupMember> CreateGroupMembers(int groupId, List<string> memberIds)
        {
            var members = new List<GroupMember>();
            foreach (var memberId in memberIds)
            {
                members.Add(new GroupMember { GroupID = groupId, UserID = memberId });
            }
            return members;
        }
    }
}

