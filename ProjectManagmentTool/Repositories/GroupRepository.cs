namespace ProjectManagmentTool.Repositories
{
    using Microsoft.EntityFrameworkCore;
    using ProjectManagmentTool.Data;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class GroupRepository : IGroupRepository
    {
        private readonly ApplicationDbContext _context;

        public GroupRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int?> GetCompanyIdByUserId(string userId)
        {
            var user = await _context.Users
                .Where(u => u.Id == userId)
                .Select(u => u.CompanyID)
                .FirstOrDefaultAsync();
            return user;
        }

        public async Task<List<Group>> GetAllGroupsByCompanyId(int? companyId) // 🔥 Fix: Correct method name
        {
            return await _context.Groups
                .Where(g => g.CompanyID == companyId)
                .Include(g => g.GroupLead)
                .ToListAsync();
        }

        public async Task<Group?> GetGroupById(int groupId)
        {
            return await _context.Groups
                .Include(g => g.GroupLead)
                .Include(g => g.ProjectGroups).ThenInclude(pg => pg.Project)
                .Include(g => g.Tasks)
                .FirstOrDefaultAsync(g => g.GroupID == groupId);
        }

        public async Task AddGroup(Group group)
        {
            _context.Groups.Add(group);
        }

        public async Task AddGroupMembers(List<GroupMember> groupMembers)
        {
            _context.GroupMembers.AddRange(groupMembers);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
