import { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import axios from "axios";
import Navbar from "../components/Navbar";

interface Group {
  groupID: number;
  groupName: string;
  description: string;
  groupLeadName: string;
  createdAt: string;
  updatedAt: string;
  companyID: number;
}

interface CompanyUser {
  id: string;
  firstName: string;
  lastName: string;
}

const Groups = () => {
  const navigate = useNavigate();
  const [userInfo, setUserInfo] = useState({ firstName: "User", lastName: "", role: "Loading...", companyID: 0 });
  const [groups, setGroups] = useState<Group[]>([]);
  const [companyUsers, setCompanyUsers] = useState<CompanyUser[]>([]);
  const [showGroupModal, setShowGroupModal] = useState(false);

  // State for new group creation
  const [newGroup, setNewGroup] = useState<{
    groupName: string;
    description: string;
    groupLeadID: string;
    groupMemberIDs: string[];
  }>({
    groupName: "",
    description: "",
    groupLeadID: "",
    groupMemberIDs: []
  });

  useEffect(() => {
    const fetchUserInfo = async () => {
      try {
        const response = await axios.get("http://localhost:5045/api/users/me", {
          headers: { Authorization: `Bearer ${localStorage.getItem("token")}` }
        });
        console.log("User info response:", response.data); // ✅ Debug log
        setUserInfo(response.data);
  
        // Fetch company users once userInfo is set
        if (response.data.companyID) {
          const compResponse = await axios.get(
            `http://localhost:5045/api/users/company/${response.data.companyID}`,
            { headers: { Authorization: `Bearer ${localStorage.getItem("token")}` } }
          );
          setCompanyUsers(compResponse.data);
        }
      } catch (error) {
        console.error("Failed to fetch user info or company users:", error);
      }
    };
  
    fetchUserInfo();
  }, []);
  
  useEffect(() => {
    if (userInfo.companyID) {
      const fetchGroups = async () => {
        try {
          const response = await axios.get(`http://localhost:5045/api/groups/all`, {
            headers: { Authorization: `Bearer ${localStorage.getItem("token")}` }
          });
  
          // 🔥 FIX: Ensure `userInfo.companyID` is available
          console.log("Fetching groups for company:", userInfo.companyID);
          const filteredGroups = response.data.filter((group: Group) => group.companyID === userInfo.companyID);
  
          setGroups(filteredGroups);
        } catch (error) {
          console.error("Error fetching groups:", error);
        }
      };
  
      fetchGroups();
    }
  }, [userInfo.companyID]); // 🔥 FIX: Run fetchGroups only when `userInfo.companyID` changes
  

  const toggleMemberSelection = (userId: string) => {
    setNewGroup((prev) => {
      const alreadySelected = prev.groupMemberIDs.includes(userId);
      const updatedMembers = alreadySelected
        ? prev.groupMemberIDs.filter((id) => id !== userId)
        : [...prev.groupMemberIDs, userId];
      // If the currently selected team lead is no longer in the selected members, clear it.
      const updatedLead = updatedMembers.includes(prev.groupLeadID) ? prev.groupLeadID : "";
      return { ...prev, groupMemberIDs: updatedMembers, groupLeadID: updatedLead };
    });
  };

  const createGroup = async () => {
    if (!newGroup.groupName) {
      alert("Group Name is required.");
      return;
    }
    if (newGroup.groupMemberIDs.length === 0) {
      alert("Please select at least one group member.");
      return;
    }
    if (!newGroup.groupLeadID) {
      alert("Please select a team lead from the selected members.");
      return;
    }
    try {
      const response = await axios.post(
        "http://localhost:5045/api/groups/create",
        newGroup,
        { headers: { Authorization: `Bearer ${localStorage.getItem("token")}` } }
      );
      // Update groups list with the new group (you might want to refetch instead)
      setGroups([...groups, {
        groupID: response.data.groupID,
        groupName: newGroup.groupName,
        description: newGroup.description,
        groupLeadName: companyUsers.find(u => u.id === newGroup.groupLeadID)
          ? companyUsers.find(u => u.id === newGroup.groupLeadID)!.firstName + " " + companyUsers.find(u => u.id === newGroup.groupLeadID)!.lastName
          : "",
        createdAt: new Date().toISOString(),
        updatedAt: new Date().toISOString(),
        companyID: userInfo.companyID
      }]);
      // Reset the modal state
      setNewGroup({ groupName: "", description: "", groupLeadID: "", groupMemberIDs: [] });
      setShowGroupModal(false);
    } catch (error) {
      console.error("Error creating group:", error);
    }
  };

  return (
    <div className="min-h-screen bg-gray-900 text-white">
      <Navbar userInfo={userInfo} />

      {/* Header */}
      <div className="p-4 bg-slate-800 border-b border-gray-700 flex justify-between items-center">
        <h1 className="text-2xl font-bold">Groups</h1>
        <button
          className="bg-green-600 px-4 py-2 rounded hover:bg-green-700"
          onClick={() => setShowGroupModal(true)}
        >
          ➕ Create Group
        </button>
      </div>

      {/* Groups List */}
      <div className="p-6 grid grid-cols-3 gap-6">
        {groups.map((group) => (
          <div
            key={group.groupID}
            className="bg-slate-800 p-4 rounded shadow-lg border border-gray-700 hover:bg-slate-700 cursor-pointer transition"
            onClick={() => navigate(`/groups/${group.groupID}`)} // Navigate to group details
          >
            <h3 className="text-lg font-bold">{group.groupName}</h3>
            <p className="text-sm text-gray-400">{group.description}</p>
            <p className="text-sm text-gray-400">Lead: {group.groupLeadName}</p>
          </div>
        ))}
      </div>

      {/* Create Group Modal */}
      {showGroupModal && (
        <div className="fixed inset-0 flex items-center justify-center bg-black bg-opacity-50">
          <div className="bg-gray-800 p-6 rounded shadow-lg w-1/2">
            <h3 className="text-lg font-bold">Create Group</h3>
            <div className="mt-2">
              <input
                type="text"
                placeholder="Group Name"
                className="w-full p-2 bg-gray-700 rounded"
                value={newGroup.groupName}
                onChange={(e) => setNewGroup({ ...newGroup, groupName: e.target.value })}
              />
            </div>
            <div className="mt-2">
              <textarea
                placeholder="Description"
                className="w-full p-2 bg-gray-700 rounded"
                value={newGroup.description}
                onChange={(e) => setNewGroup({ ...newGroup, description: e.target.value })}
              ></textarea>
            </div>
            {/* Company Users Selection */}
            <div className="mt-2">
              <p className="font-bold mb-1">Select Group Members:</p>
              <div className="max-h-40 overflow-y-auto border border-gray-600 p-2 rounded">
                {companyUsers.map((user) => (
                  <div key={user.id} className="flex items-center gap-2">
                    <input
                      type="checkbox"
                      checked={newGroup.groupMemberIDs.includes(user.id)}
                      onChange={() => toggleMemberSelection(user.id)}
                    />
                    <span>{user.firstName} {user.lastName}</span>
                  </div>
                ))}
              </div>
            </div>
            {/* Team Lead Dropdown */}
            <div className="mt-2">
              <p className="font-bold mb-1">Select Team Lead:</p>
              <select
                className="w-full p-2 bg-gray-700 rounded"
                value={newGroup.groupLeadID}
                onChange={(e) => setNewGroup({ ...newGroup, groupLeadID: e.target.value })}
                disabled={newGroup.groupMemberIDs.length === 0}
              >
                <option value="">-- Select Team Lead --</option>
                {companyUsers
                  .filter((user) => newGroup.groupMemberIDs.includes(user.id))
                  .map((user) => (
                    <option key={user.id} value={user.id}>
                      {user.firstName} {user.lastName}
                    </option>
                  ))}
              </select>
            </div>
            <div className="mt-4 flex gap-2">
              <button onClick={createGroup} className="bg-green-600 px-4 py-2 rounded">
                Create Group
              </button>
              <button onClick={() => setShowGroupModal(false)} className="bg-red-600 px-4 py-2 rounded">
                Cancel
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default Groups;
