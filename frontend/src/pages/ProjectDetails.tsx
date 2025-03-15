import { useState, useEffect } from "react";
import { useNavigate, useParams } from "react-router-dom";
import axios /*, { AxiosError } */ from "axios"; // Remove AxiosError if unused
import Navbar from "../components/Navbar";
import GroupsTab from "../components/GroupsTab";
import TaskTab from "../components/TaskTab";
import MembersTab from "../components/MembersTab";

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
            className={`px-3 py-1 rounded text-sm ${project.visibility === "private" ? "bg-red-500" : "bg-green-500"
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
            className={`px-4 py-2 text-sm font-bold ${activeTab === tab ? "bg-green-600" : "hover:bg-gray-700"
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
          <TaskTab
            projectId={validProjectId}
            tasks={tasks}
            setTasks={setTasks}
            members={members}
          />
        )}
        {activeTab === "groups" && (
          // Render the GroupsTab component and pass the current project ID.
          <GroupsTab projectId={validProjectId} />
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

export default ProjectDetails;
