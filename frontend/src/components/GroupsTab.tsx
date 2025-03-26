import { useState, useEffect } from "react";
import axios from "axios";

interface AssignedGroup {
  groupID: number;
  groupName: string;
  description: string;
  groupLeadName: string;
}

interface Group {
  groupID: number;
  groupName: string;
  description: string;
  groupLeadName: string;
  createdAt: string;
  updatedAt: string;
}

interface GroupsTabProps {
  projectId: string;
  onGroupChange?: () => void
}

const GroupsTab = ({ projectId, onGroupChange }: GroupsTabProps) => {
  const [assignedGroups, setAssignedGroups] = useState<AssignedGroup[]>([]);
  const [allGroups, setAllGroups] = useState<Group[]>([]);
  const [selectedGroupID, setSelectedGroupID] = useState<number | null>(null);
  const [loading, setLoading] = useState(true);

  // Fetch assigned groups for the project
  const fetchAssignedGroups = async () => {
    try {
      const response = await axios.get(
        `http://localhost:5045/api/projects/${projectId}/groups`,
        { headers: { Authorization: `Bearer ${localStorage.getItem("token")}` } }
      );
      setAssignedGroups(response.data);
    } catch (error) {
      console.error("Error fetching assigned groups:", error);
    }
  };

  // Fetch all groups
  const fetchAllGroups = async () => {
    try {
      const response = await axios.get("http://localhost:5045/api/groups/all", {
        headers: { Authorization: `Bearer ${localStorage.getItem("token")}` }
      });
      setAllGroups(response.data);
    } catch (error) {
      console.error("Error fetching all groups:", error);
    }
  };

  useEffect(() => {
    const fetchData = async () => {
      await Promise.all([fetchAssignedGroups(), fetchAllGroups()]);
      setLoading(false);
    };
    fetchData();
  }, [projectId]);

  // Compute available groups (all groups that are not yet assigned)
  const availableGroups = allGroups.filter(
    (g) => !assignedGroups.some((ag) => ag.groupID === g.groupID)
  );

  const assignGroup = async () => {
    if (selectedGroupID === null) {
      alert("Please select a group to assign.");
      return;
    }
    try {
      await axios.post(
        `http://localhost:5045/api/projects/${projectId}/assign-group`,
        { groupID: selectedGroupID },
        { headers: { Authorization: `Bearer ${localStorage.getItem("token")}` } }
      );
      // Refresh assigned groups and reset selection
      await fetchAssignedGroups();
      setSelectedGroupID(null);
    } catch (error) {
      console.error("Error assigning group:", error);
    }
  };

  const removeGroup = async (groupId: number) => {
    const confirmRemove = confirm("Are you sure you want to remove this group from the project?");
    if (!confirmRemove) return;

    try {
      await axios.delete(
        `http://localhost:5045/api/projects/${projectId}/remove-group/${groupId}`,
        {
          headers: {
            Authorization: `Bearer ${localStorage.getItem("token")}`,
          },
        }
      );
      await fetchAssignedGroups(); // refresh list
      if (onGroupChange) onGroupChange(); // notify parent to refresh members
    } catch (error) {
      console.error("Error removing group:", error);
      alert("Failed to remove group from project.");
    }
  };

  if (loading) {
    return <div>Loading groups...</div>;
  }

  return (
    <div>
      <h2 className="text-xl font-bold">Assigned Groups</h2>
      {assignedGroups.length === 0 ? (
        <p className="text-gray-400">No groups assigned to this project.</p>
      ) : (
        <div className="grid grid-cols-2 gap-4 mt-4">
          {assignedGroups.map((group) => (
            <div key={group.groupID} className="bg-slate-800 p-4 rounded shadow border border-gray-700">
              <h3 className="font-bold">{group.groupName}</h3>
              <p className="text-sm text-gray-400">{group.description}</p>
              <p className="text-sm text-gray-400">Team Lead: {group.groupLeadName}</p>
              <button
                className="mt-2 text-red-400 hover:text-red-600 text-sm underline"
                onClick={() => removeGroup(group.groupID)}
              >
                ❌ Remove Group
              </button>
            </div>
          ))}
        </div>
      )}
      <div className="mt-6">
        <h2 className="text-xl font-bold">Assign a New Group</h2>
        {availableGroups.length === 0 ? (
          <p className="text-gray-400 mt-2">No available groups to assign.</p>
        ) : (
          <div className="mt-2 flex items-center gap-4">
            <select
              className="p-2 bg-gray-700 rounded"
              value={selectedGroupID ?? ""}
              onChange={(e) => setSelectedGroupID(Number(e.target.value))}
            >
              <option value="">-- Select Group --</option>
              {availableGroups.map((group) => (
                <option key={group.groupID} value={group.groupID}>
                  {group.groupName}
                </option>
              ))}
            </select>
            <button
              className="bg-green-600 px-4 py-2 rounded hover:bg-green-700"
              onClick={assignGroup}
            >
              Assign Group
            </button>
          </div>
        )}
      </div>
    </div>
  );
};

export default GroupsTab;
