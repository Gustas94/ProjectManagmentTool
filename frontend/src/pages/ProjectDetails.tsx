import { useState, useEffect } from "react";
import { useNavigate, useParams } from "react-router-dom";
import axios from "axios";
import Navbar from "../components/Navbar";
import GroupsTab from "../components/GroupsTab";
import TaskTab from "../components/TaskTab";
import { usePermission } from "../hooks/usePermission";

// ================================
// TypeScript Interfaces
// ================================
interface Task {
  taskID: number;
  taskName: string;
  description: string;
  deadline: string;
  priority: string;
  status: string;
  assignedTo: string[]; // Array of user IDs
}

interface Member {
  id: string;
  firstName: string;
  lastName: string;
  assignedVia: string[];
}

// ================================
// Main Component: ProjectDetails
// ================================
const ProjectDetails = () => {
  const navigate = useNavigate();
  const { projectId } = useParams<{ projectId: string }>();
  const validProjectId: string = projectId ?? "";
  const [activeTab, setActiveTab] = useState("tasks");
  const [project, setProject] = useState<any>(null);
  const [tasks, setTasks] = useState<Task[]>([]);
  const [members, setMembers] = useState<Member[]>([]);
  const [companyUsers, setCompanyUsers] = useState<Member[]>([]);
  const [loading, setLoading] = useState(true);

  // For modals
  const [showAssignModal, setShowAssignModal] = useState(false);
  const [showCreateTaskModal, setShowCreateTaskModal] = useState(false);

  // Members loading state
  const [membersLoading] = useState(false);
  // Selected members for assignment (for project assignment, not used in create task)
  const [selectedMembers, setSelectedMembers] = useState<string[]>([]);

  // New task form state
  const [newTaskName, setNewTaskName] = useState("");
  const [newTaskDescription, setNewTaskDescription] = useState("");
  const [newTaskDeadline, setNewTaskDeadline] = useState("");
  const [newTaskPriority, setNewTaskPriority] = useState("Medium");
  const [newTaskStatus, setNewTaskStatus] = useState("To Do");
  // If you want to assign users or groups during creation, add those states as needed:
  const [newTaskAssignedUsers, setNewTaskAssignedUsers] = useState<string[]>([]);
  const [newTaskAssignedGroups, setNewTaskAssignedGroups] = useState<number[]>([]);

  const { hasPermission } = usePermission();

  useEffect(() => {
    const fetchProjectDetails = async () => {
      try {
        const response = await axios.get(
          `http://localhost:5045/api/projects/${validProjectId}`,
          { headers: { Authorization: `Bearer ${localStorage.getItem("token")}` } }
        );
        console.debug("Project details fetched:", response.data);
        setProject(response.data);
      } catch (error) {
        console.error("Error fetching project details:", error);
        navigate("/projects");
      }
    };

    const fetchTasks = async () => {
      try {
        const response = await axios.get(
          `http://localhost:5045/api/tasks/project/${validProjectId}`,
          { headers: { Authorization: `Bearer ${localStorage.getItem("token")}` } }
        );
        console.debug("Tasks fetched:", response.data);
        setTasks(response.data);
      } catch (error) {
        console.error("Error fetching tasks:", error);
      }
    };

    const fetchProjectMembers = async () => {
      try {
        const response = await axios.get(
          `http://localhost:5045/api/users/project/${validProjectId}`,
          { headers: { Authorization: `Bearer ${localStorage.getItem("token")}` } }
        );
        console.debug("Project members fetched:", response.data);
        setMembers(response.data);
      } catch (error: any) {
        if (error.response?.status === 404) {
          console.warn("No members found for this project.");
          setMembers([]);
        } else {
          console.error("Error fetching project members:", error);
        }
      }
    };

    const fetchCompanyUsers = async () => {
      try {
        const userResponse = await axios.get("http://localhost:5045/api/users/me", {
          headers: { Authorization: `Bearer ${localStorage.getItem("token")}` },
        });
        console.debug("User info fetched for company users:", userResponse.data);
        const companyId = userResponse.data.companyID;
        if (companyId) {
          const compUsersResponse = await axios.get(
            `http://localhost:5045/api/users/company/${companyId}`,
            { headers: { Authorization: `Bearer ${localStorage.getItem("token")}` } }
          );
          console.debug("Company users fetched:", compUsersResponse.data);
          setCompanyUsers(compUsersResponse.data);
        }
      } catch (error) {
        console.error("Error fetching company users:", error);
      }
    };

    Promise.all([
      fetchProjectDetails(),
      fetchTasks(),
      fetchProjectMembers(),
      fetchCompanyUsers(),
    ]).then(() => setLoading(false));
  }, [validProjectId, navigate]);

  const assignMembers = async () => {
    try {
      await Promise.all(
        selectedMembers.map((userId) =>
          axios.post(
            "http://localhost:5045/api/users/project/add",
            { userID: userId, projectID: parseInt(validProjectId, 10) },
            { headers: { Authorization: `Bearer ${localStorage.getItem("token")}` } }
          )
        )
      );
      alert("Members assigned successfully!");
      setSelectedMembers([]);
      setShowAssignModal(false);

      // Refresh members
      const refreshed = await axios.get(
        `http://localhost:5045/api/users/project/${validProjectId}`,
        { headers: { Authorization: `Bearer ${localStorage.getItem("token")}` } }
      );
      console.debug("Members refreshed:", refreshed.data);
      setMembers(refreshed.data);
    } catch (err) {
      console.error("Error assigning members", err);
      alert("Failed to assign members.");
    }
  };

  // Function to create a new task using the new form fields
  const createTask = async () => {
    try {
      const newTaskObject = {
        TaskName: newTaskName,
        Description: newTaskDescription,
        Deadline: newTaskDeadline,
        Priority: newTaskPriority,
        Status: newTaskStatus,
        ProjectID: parseInt(validProjectId, 10),
        AssignedUserIDs: newTaskAssignedUsers,
        AssignedGroupIDs: newTaskAssignedGroups,
      };
      console.debug("Creating new task with data:", newTaskObject);
      const response = await axios.post("http://localhost:5045/api/tasks/create", newTaskObject, {
        headers: { Authorization: `Bearer ${localStorage.getItem("token")}` },
      });
      console.debug("New task creation response:", response.data);
      alert("Task created successfully!");
      // Optionally update tasks list (if your API returns the created task)
      setTasks((prev) => [...prev, response.data.task]);
      // Reset new task form fields
      setNewTaskName("");
      setNewTaskDescription("");
      setNewTaskDeadline("");
      setNewTaskPriority("Medium");
      setNewTaskStatus("To Do");
      setNewTaskAssignedUsers([]);
      setNewTaskAssignedGroups([]);
      setShowCreateTaskModal(false);
    } catch (error) {
      console.error("Error creating task:", error);
      alert("Failed to create task.");
    }
  };

  if (loading) {
    return (
      <div className="min-h-screen bg-gray-900 text-white flex items-center justify-center">
        Loading...
      </div>
    );
  }
  if (!project) {
    return (
      <div className="min-h-screen bg-gray-900 text-white flex items-center justify-center">
        Project not found.
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-900 text-white">
      <Navbar />

      {/* Project Overview */}
      <div className="p-6 bg-slate-800 border-b border-gray-700">
        <h1 className="text-2xl font-bold">{project.projectName}</h1>
        <p className="text-gray-400">{project.description}</p>
        <div className="flex gap-4 mt-4">
          <span className="bg-gray-700 px-3 py-1 rounded text-sm">
            📅 {new Date(project.startDate).toLocaleDateString()} -{" "}
            {new Date(project.endDate).toLocaleDateString()}
          </span>
          <span
            className={`px-3 py-1 rounded text-sm ${project.visibility === "private" ? "bg-red-500" : "bg-green-500"}`}
          >
            {project.visibility === "private" ? "🔒 Private" : "🌍 Public"}
          </span>
        </div>
        <p className="mt-2 text-gray-400">👤 Project Manager: {project.projectManagerName}</p>
        <p className="text-gray-400">👥 Members: {project.membersCount}</p>
      </div>

      {/* Tab Navigation */}
      <div className="flex justify-center bg-gray-800 border-b border-gray-700 p-2">
        {["tasks", "groups", "discussion", "timeline", "members"].map((tab) => (
          <button
            key={tab}
            className={`px-4 py-2 text-sm font-bold ${activeTab === tab ? "bg-green-600" : "hover:bg-gray-700"} rounded`}
            onClick={() => setActiveTab(tab)}
          >
            {tab.charAt(0).toUpperCase() + tab.slice(1)}
          </button>
        ))}
      </div>

      {/* Tab Content */}
      <div className="p-6">
        {activeTab === "tasks" && (
          <>
            {/* Existing tasks list rendered via TaskTab */}
            <TaskTab
              projectId={validProjectId}
              tasks={tasks}
              setTasks={setTasks}
              members={members}
            />
          </>
        )}
        {activeTab === "groups" && (
          <GroupsTab
            projectId={validProjectId}
            onGroupChange={() => {
              axios
                .get(`http://localhost:5045/api/users/project/${validProjectId}`, {
                  headers: { Authorization: `Bearer ${localStorage.getItem("token")}` },
                })
                .then((res) => {
                  console.debug("Members after group update:", res.data);
                  setMembers(res.data);
                })
                .catch((err) => console.error("Error refreshing members:", err));
            }}
          />
        )}
        {activeTab === "members" && (
          <div className="mt-4">
            <div className="flex items-center justify-between mb-4">
              <h3 className="text-xl font-semibold">Group Members</h3>
              {hasPermission("ASSIGN_MEMBERS") && (
              <button
                className="bg-green-600 hover:bg-green-700 px-4 py-2 rounded"
                onClick={() => setShowAssignModal(true)}
              >
                ➕ Assign Members
              </button>
              )}
            </div>

            {membersLoading ? (
              <p>Loading members...</p>
            ) : members.length === 0 ? (
              <p className="text-gray-400">No members assigned to this group.</p>
            ) : (
              <div className="grid grid-cols-2 md:grid-cols-3 gap-4">
                {members.map((member) => (
                  <div
                    key={member.id}
                    className="bg-gray-700 p-3 rounded shadow text-white border border-gray-600"
                  >
                    <p className="font-medium">
                      {member.firstName} {member.lastName}
                    </p>
                    <p className="text-sm text-gray-400">
                      {member.assignedVia.length > 0
                        ? member.assignedVia.join(", ")
                        : "Direct"}
                    </p>
                  </div>
                ))}
              </div>
            )}

            {showAssignModal && (
              <div className="fixed inset-0 z-50 bg-black bg-opacity-50 flex items-center justify-center">
                <div className="bg-gray-800 rounded-lg p-6 w-full max-w-md border border-gray-600">
                  <h2 className="text-xl font-bold text-white mb-4">Assign New Members</h2>
                  <div className="max-h-60 overflow-y-auto grid grid-cols-1 gap-2 text-white">
                    {companyUsers
                      .filter((user) => !members.some((m) => m.id === user.id))
                      .map((user) => (
                        <label key={user.id} className="flex items-center gap-2">
                          <input
                            type="checkbox"
                            value={user.id}
                            checked={selectedMembers.includes(user.id)}
                            onChange={(e) => {
                              const isChecked = e.target.checked;
                              setSelectedMembers((prev) =>
                                isChecked ? [...prev, user.id] : prev.filter((id) => id !== user.id)
                              );
                            }}
                          />
                          {user.firstName} {user.lastName}
                        </label>
                      ))}
                  </div>
                  <div className="flex justify-end gap-2 mt-4">
                    <button
                      className="bg-gray-600 hover:bg-gray-700 px-4 py-2 rounded"
                      onClick={() => setShowAssignModal(false)}
                    >
                      Cancel
                    </button>
                    <button
                      className="bg-green-600 hover:bg-green-700 px-4 py-2 rounded"
                      onClick={assignMembers}
                      disabled={selectedMembers.length === 0}
                    >
                      Assign Selected
                    </button>
                  </div>
                </div>
              </div>
            )}
          </div>
        )}
      </div>

      {/* Create Task Modal */}
      {showCreateTaskModal && (
        <div className="fixed inset-0 z-50 bg-black bg-opacity-50 flex items-center justify-center">
          <div className="bg-gray-800 rounded-lg p-6 w-full max-w-md border border-gray-600">
            <h2 className="text-xl font-bold text-white mb-4">Create New Task</h2>
            <div className="mb-4">
              <label className="block text-gray-300 mb-1">Task Name</label>
              <input
                type="text"
                className="w-full p-2 bg-gray-700 text-white rounded"
                value={newTaskName}
                onChange={(e) => setNewTaskName(e.target.value)}
              />
            </div>
            <div className="mb-4">
              <label className="block text-gray-300 mb-1">Description</label>
              <textarea
                className="w-full p-2 bg-gray-700 text-white rounded"
                rows={3}
                value={newTaskDescription}
                onChange={(e) => setNewTaskDescription(e.target.value)}
              ></textarea>
            </div>
            <div className="mb-4">
              <label className="block text-gray-300 mb-1">Deadline</label>
              <input
                type="date"
                className="w-full p-2 bg-gray-700 text-white rounded"
                value={newTaskDeadline}
                onChange={(e) => setNewTaskDeadline(e.target.value)}
              />
            </div>
            <div className="mb-4">
              <label className="block text-gray-300 mb-1">Priority</label>
              <select
                className="w-full p-2 bg-gray-700 text-white rounded"
                value={newTaskPriority}
                onChange={(e) => setNewTaskPriority(e.target.value)}
              >
                <option value="High">High</option>
                <option value="Medium">Medium</option>
                <option value="Low">Low</option>
              </select>
            </div>
            <div className="mb-4">
              <label className="block text-gray-300 mb-1">Status</label>
              <select
                className="w-full p-2 bg-gray-700 text-white rounded"
                value={newTaskStatus}
                onChange={(e) => setNewTaskStatus(e.target.value)}
              >
                <option value="To Do">To Do</option>
                <option value="In Progress">In Progress</option>
                <option value="Testing">Testing</option>
                <option value="Completed">Completed</option>
              </select>
            </div>
            <div className="flex justify-end gap-2 mt-4">
              <button
                className="bg-gray-600 hover:bg-gray-700 px-4 py-2 rounded"
                onClick={() => setShowCreateTaskModal(false)}
              >
                Cancel
              </button>
              <button
                className="bg-green-600 hover:bg-green-700 px-4 py-2 rounded"
                onClick={createTask}
              >
                Create Task
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default ProjectDetails;
