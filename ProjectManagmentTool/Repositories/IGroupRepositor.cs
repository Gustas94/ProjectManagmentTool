namespace ProjectManagmentTool.Repositories
{
    using ProjectManagmentTool.Data;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IGroupRepository
    {
        Task<List<Group>> GetAllGroupsByCompany(int? companyId);
        Task<Group?> GetGroupById(int groupId);
        Task AddGroup(Group group);
        Task AddGroupMembers(List<GroupMember> groupMembers);
        Task SaveChangesAsync();
    }

}
