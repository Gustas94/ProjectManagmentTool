import { useState, useEffect } from "react";
import { useNavigate, useParams } from "react-router-dom";
import axios /*, { AxiosError } */ from "axios"; // Remove AxiosError if unused
import Navbar from "../components/Navbar";

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

  // For "Add Members" modal in Members tab:
  const [showMembersModal, setShowMembersModal] = useState(false);
  const [selectedUsers, setSelectedUsers] = useState<string[]>([]);
  const [currentMembersPage, setCurrentMembersPage] = useState<number>(1);
  const membersPerPage = 20;

  useEffect(() => {
    const fetchProjectDetails = async () => {
      try {
        const response = await axios.get(
          `http://localhost:5045/api/projects/${validProjectId}`,
          { headers: { Authorization: `Bearer ${localStorage.getItem("token")}` } }
        );
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
        const companyId = userResponse.data.companyID;
        if (companyId) {
          const compUsersResponse = await axios.get(
            `http://localhost:5045/api/users/company/${companyId}`,
            { headers: { Authorization: `Bearer ${localStorage.getItem("token")}` } }
          );
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
      <Navbar userInfo={{ firstName: "User", lastName: "", role: "Developer" }} />

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
            className={`px-3 py-1 rounded text-sm ${
              project.visibility === "private" ? "bg-red-500" : "bg-green-500"
            }`}
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
            className={`px-4 py-2 text-sm font-bold ${
              activeTab === tab ? "bg-green-600" : "hover:bg-gray-700"
            } rounded`}
            onClick={() => setActiveTab(tab)}
          >
            {tab.charAt(0).toUpperCase() + tab.slice(1)}
          </button>
        ))}
      </div>

      {/* Tab Content */}
      <div className="p-6">
        {activeTab === "tasks" && (
          <TasksTab
            projectId={validProjectId}
            tasks={tasks}
            setTasks={setTasks}
            members={members}
          />
        )}
        {activeTab === "members" && (
          <MembersTab
            members={members}
            companyUsers={companyUsers}
            selectedUsers={selectedUsers}
            setSelectedUsers={setSelectedUsers}
            currentMembersPage={currentMembersPage}
            setCurrentMembersPage={setCurrentMembersPage}
            membersPerPage={membersPerPage}
            handleAddUsersToProject={async () => {
              try {
                await Promise.all(
                  selectedUsers.map((userId: string) =>
                    axios.post(
                      "http://localhost:5045/api/users/project/add",
                      { userID: userId, projectID: parseInt(validProjectId, 10) },
                      { headers: { Authorization: `Bearer ${localStorage.getItem("token")}` } }
                    )
                  )
                );
                setShowMembersModal(false);
                setSelectedUsers([]);
                window.location.reload();
              } catch (error) {
                console.error("Error adding users to project:", error);
              }
            }}
            showMembersModal={showMembersModal}
            setShowMembersModal={setShowMembersModal}
          />
        )}
      </div>
    </div>
  );
};

// ================================
// TasksTab Component
// ================================
interface TasksTabProps {
  projectId: string;
  tasks: Task[];
  setTasks: (value: Task[] | ((prev: Task[]) => Task[])) => void;
  members: Member[];
}
const TasksTab = ({ projectId, tasks, setTasks, members }: TasksTabProps) => {
  const [showTaskModal, setShowTaskModal] = useState(false);
  const [currentTaskPage, setCurrentTaskPage] = useState<number>(1);
  const membersPerPageTasks = 20;

  const [newTask, setNewTask] = useState<Task>({
    taskID: 0,
    taskName: "",
    description: "",
    deadline: "",
    priority: "Medium",
    status: "To Do",
    assignedTo: [],
  });

  const createTask = async () => {
    if (!newTask.taskName || !newTask.deadline || newTask.assignedTo.length === 0) {
      alert("Task Name, Deadline, and at least one Assignee are required!");
      return;
    }
    try {
      const formattedDeadline = new Date(newTask.deadline).toISOString();
      const response = await axios.post(
        "http://localhost:5045/api/tasks/create",
        { ...newTask, deadline: formattedDeadline, projectID: projectId },
        { headers: { Authorization: `Bearer ${localStorage.getItem("token")}` } }
      );
      setTasks((prev: Task[]) => [
        ...prev,
        { ...newTask, taskID: response.data.taskId },
      ]);
      setNewTask({
        taskID: 0,
        taskName: "",
        description: "",
        deadline: "",
        priority: "Medium",
        status: "To Do",
        assignedTo: [],
      });
      setShowTaskModal(false);
    } catch (error) {
      console.error("Error creating task:", error);
    }
  };

  return (
    <div>
      <h2 className="text-xl font-bold">Tasks</h2>
      <button
        className="bg-green-600 px-4 py-2 rounded mt-2"
        onClick={() => setShowTaskModal(true)}
      >
        ➕ Create Task
      </button>

      {/* Task Creation Modal */}
      {showTaskModal && (
        <div className="fixed inset-0 flex items-center justify-center bg-black bg-opacity-50">
          <div className="bg-gray-800 p-6 rounded shadow-lg w-1/3">
            <h3 className="text-lg font-bold">Create Task</h3>
            <div className="mt-2">
              <input
                type="text"
                placeholder="Task Name"
                className="w-full p-2 bg-gray-700 rounded"
                value={newTask.taskName}
                onChange={(e) =>
                  setNewTask({ ...newTask, taskName: e.target.value })
                }
              />
            </div>
            <div className="mt-2">
              <input
                type="date"
                className="w-full p-2 bg-gray-700 rounded"
                value={newTask.deadline}
                onChange={(e) =>
                  setNewTask({ ...newTask, deadline: e.target.value })
                }
              />
            </div>
            <div className="mt-2">
              <label className="font-bold">Assign Members:</label>
              {members
                .slice(
                  (currentTaskPage - 1) * membersPerPageTasks,
                  currentTaskPage * membersPerPageTasks
                )
                .map((member: Member) => (
                  <div key={member.id} className="flex items-center gap-2">
                    <input
                      type="checkbox"
                      checked={newTask.assignedTo.includes(member.id)}
                      onChange={() =>
                        setNewTask((prev) => ({
                          ...prev,
                          assignedTo: prev.assignedTo.includes(member.id)
                            ? prev.assignedTo.filter((uid) => uid !== member.id)
                            : [...prev.assignedTo, member.id],
                        }))
                      }
                    />
                    <label>{`${member.firstName} ${member.lastName}`}</label>
                  </div>
                ))}
              <div className="flex justify-between mt-2">
                <button
                  onClick={() =>
                    setCurrentTaskPage((prev: number) => Math.max(prev - 1, 1))
                  }
                  className="bg-gray-600 px-2 py-1 rounded"
                >
                  ⬅ Prev
                </button>
                <button
                  onClick={() => setCurrentTaskPage((prev: number) => prev + 1)}
                  className="bg-gray-600 px-2 py-1 rounded"
                >
                  Next ➡
                </button>
              </div>
            </div>
            <div className="mt-4 flex gap-2">
              <button onClick={createTask} className="bg-green-600 px-4 py-2 rounded">
                Create Task
              </button>
              <button onClick={() => setShowTaskModal(false)} className="bg-red-600 px-4 py-2 rounded">
                Cancel
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Render existing tasks */}
      <div className="space-y-4 mt-4">
        {tasks.length === 0 ? (
          <p className="text-gray-400">No tasks yet.</p>
        ) : (
          tasks.map((task) => (
            <div key={task.taskID} className="p-3 bg-gray-700 rounded">
              <h3 className="font-bold">{task.taskName}</h3>
              <p className="text-sm text-gray-400">
                🗓 Deadline: {new Date(task.deadline).toLocaleDateString()}
              </p>
              <p className="text-sm text-gray-400">📌 Priority: {task.priority}</p>
              <p className="text-sm text-gray-400">🚦 Status: {task.status}</p>
            </div>
          ))
        )}
      </div>
    </div>
  );
};

// ================================
// MembersTab Component with "Add Members" Modal
// ================================
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

export default ProjectDetails;
