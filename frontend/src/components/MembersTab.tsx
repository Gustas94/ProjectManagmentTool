interface MembersTabProps {
    members: Member[];
    companyUsers: Member[];
    selectedUsers: string[];
    setSelectedUsers: (users: string[]) => void;
    currentMembersPage: number;
    setCurrentMembersPage: (page: number | ((prev: number) => number)) => void;
    membersPerPage: number;
    handleAddUsersToProject: () => Promise<void>;
    showMembersModal: boolean;
    setShowMembersModal: (show: boolean) => void;
  }
  
  interface Member {
    id: string;
    firstName: string;
    lastName: string;
  }
  
  const MembersTab = ({
    members,
    companyUsers,
    selectedUsers,
    setSelectedUsers,
    currentMembersPage,
    setCurrentMembersPage,
    membersPerPage,
    handleAddUsersToProject,
    showMembersModal,
    setShowMembersModal,
  }: MembersTabProps) => {
    return (
      <div>
        <h2 className="text-xl font-bold">Project Members</h2>
        <button
          className="bg-green-600 px-4 py-2 rounded mt-2"
          onClick={() => setShowMembersModal(true)}
        >
          ➕ Add Members
        </button>
        <ul className="mt-4">
          {members.length === 0 ? (
            <p className="text-gray-400">No members yet.</p>
          ) : (
            members.map((member) => (
              <li key={member.id}>
                {member.firstName} {member.lastName}
              </li>
            ))
          )}
        </ul>
  
        {/* Add Members Modal */}
        {showMembersModal && (
          <div className="fixed inset-0 flex items-center justify-center bg-black bg-opacity-50">
            <div className="bg-gray-800 p-6 rounded shadow-lg w-1/3">
              <h3 className="text-lg font-bold">Add Members to Project</h3>
              {companyUsers
                .slice(
                  (currentMembersPage - 1) * membersPerPage,
                  currentMembersPage * membersPerPage
                )
                .map((user: Member) => (
                  <div key={user.id} className="flex items-center gap-2">
                    <input
                      type="checkbox"
                      checked={selectedUsers.includes(user.id)}
                      onChange={() => {
                        if (selectedUsers.includes(user.id)) {
                          setSelectedUsers(selectedUsers.filter((uid) => uid !== user.id));
                        } else {
                          setSelectedUsers([...selectedUsers, user.id]);
                        }
                      }}
                    />
                    <label>{`${user.firstName} ${user.lastName}`}</label>
                  </div>
                ))}
              <div className="flex justify-between mt-2">
                <button
                  onClick={() =>
                    setCurrentMembersPage((prev: number) => Math.max(prev - 1, 1))
                  }
                  className="bg-gray-600 px-2 py-1 rounded"
                >
                  ⬅ Prev
                </button>
                <button
                  onClick={() => setCurrentMembersPage((prev: number) => prev + 1)}
                  className="bg-gray-600 px-2 py-1 rounded"
                >
                  Next ➡
                </button>
              </div>
              <div className="mt-4 flex gap-2">
                <button onClick={handleAddUsersToProject} className="bg-green-600 px-4 py-2 rounded">
                  ✅ Add Selected
                </button>
                <button onClick={() => setShowMembersModal(false)} className="bg-red-600 px-4 py-2 rounded">
                  ❌ Cancel
                </button>
              </div>
            </div>
          </div>
        )}
      </div>
    );
  };


  export default MembersTab;