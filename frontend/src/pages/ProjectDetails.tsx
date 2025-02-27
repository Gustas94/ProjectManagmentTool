import { useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import Navbar from "../components/navbar";

const ProjectDetails = () => {
  const navigate = useNavigate();
  const { projectId } = useParams(); // Get project ID from URL params
  const [activeTab, setActiveTab] = useState("overview");

  // Example static project data (Replace with API data)
  const project = {
    projectID: projectId,
    projectName: "Website Redesign",
    description: "Revamping our company website for a modern look.",
    startDate: "2025-02-01",
    endDate: "2025-06-15",
    visibility: "public",
    projectManager: "Alice Johnson",
    membersCount: 12,
    progress: 60,
    status: "Active",
  };

  return (
    <div className="min-h-screen bg-gray-900 text-white">
      {/* Navbar */}
      <Navbar userInfo={{ firstName: "User", lastName: "", role: "Developer" }} />

      {/* Project Overview */}
      <div className="p-6 bg-slate-800 border-b border-gray-700">
        <h1 className="text-2xl font-bold">{project.projectName}</h1>
        <p className="text-gray-400">{project.description}</p>
        <div className="flex gap-4 mt-4">
          <span className="bg-gray-700 px-3 py-1 rounded text-sm">📅 {project.startDate} - {project.endDate}</span>
          <span className={`px-3 py-1 rounded text-sm ${project.status === "Active" ? "bg-green-600" : "bg-red-600"}`}>
            {project.status}
          </span>
          <span className={`px-3 py-1 rounded text-sm ${project.visibility === "private" ? "bg-red-500" : "bg-green-500"}`}>
            {project.visibility === "private" ? "🔒 Private" : "🌍 Public"}
          </span>
        </div>
        <p className="mt-2 text-gray-400">👤 Project Manager: {project.projectManager}</p>
        <p className="text-gray-400">👥 Members: {project.membersCount}</p>
        {/* Progress Bar */}
        <div className="mt-4 bg-gray-700 h-3 rounded">
          <div className="h-3 bg-blue-500 rounded" style={{ width: `${project.progress}%` }}></div>
        </div>
      </div>

      {/* Tab Navigation */}
      <div className="flex justify-center bg-gray-800 border-b border-gray-700 p-2">
        {["overview", "tasks", "groups", "discussion", "timeline", "members"].map((tab) => (
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
        {activeTab === "overview" && <OverviewTab project={project} />}
        {activeTab === "tasks" && <TasksTab />}
        {activeTab === "groups" && <GroupsTab />}
        {activeTab === "discussion" && <DiscussionTab />}
        {activeTab === "timeline" && <TimelineTab />}
        {activeTab === "members" && <MembersTab />}
      </div>
    </div>
  );
};

// ✅ Overview Tab
const OverviewTab = ({ project }: { project: any }) => (
  <div>
    <h2 className="text-xl font-bold">Project Overview</h2>
    <p className="text-gray-400">{project.description}</p>
    <p className="text-gray-400 mt-2">📅 Start Date: {project.startDate}</p>
    <p className="text-gray-400">⏳ End Date: {project.endDate}</p>
    <p className="text-gray-400">👤 Manager: {project.projectManager}</p>
  </div>
);

// ✅ Tasks Tab (Static Placeholder)
const TasksTab = () => (
  <div>
    <h2 className="text-xl font-bold">Tasks</h2>
    <p className="text-gray-400">List of tasks will be displayed here.</p>
  </div>
);

// ✅ Groups Tab (Static Placeholder)
const GroupsTab = () => (
  <div>
    <h2 className="text-xl font-bold">Groups</h2>
    <p className="text-gray-400">Groups associated with the project will be shown here.</p>
  </div>
);

// ✅ Discussion Tab (Static Placeholder)
const DiscussionTab = () => (
  <div>
    <h2 className="text-xl font-bold">Discussions</h2>
    <p className="text-gray-400">A message board for team communication.</p>
  </div>
);

// ✅ Timeline Tab (Static Placeholder)
const TimelineTab = () => (
  <div>
    <h2 className="text-xl font-bold">Timeline</h2>
    <p className="text-gray-400">Project roadmap and key milestones.</p>
  </div>
);

// ✅ Members & Permissions Tab (Static Placeholder)
const MembersTab = () => (
  <div>
    <h2 className="text-xl font-bold">Members & Permissions</h2>
    <p className="text-gray-400">List of all project members and their roles.</p>
  </div>
);

export default ProjectDetails;
