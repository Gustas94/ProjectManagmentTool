namespace ProjectManagmentTool.Repositories
{
    using ProjectManagmentTool.Data;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IGroupRepository
    {
        Task<int?> GetCompanyIdByUserId(string userId);
        Task<List<Group>> GetAllGroupsByCompanyId(int? companyId);
        Task<Group?> GetGroupById(int groupId);
        Task AddGroup(Group group);
        Task AddGroupMembers(List<GroupMember> groupMembers);
        Task SaveChangesAsync();
    }
}
