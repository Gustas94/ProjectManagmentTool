import { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import axios from "axios";
import Navbar from "../components/Navbar";
import { usePermission } from "../hooks/usePermission";

interface Project {
  projectID: number;
  projectName: string;
  description: string;
  startDate: string;
  endDate: string;
  visibility: string;
  projectManagerName: string;
  membersCount: number;
  createdAt: string;
  updatedAt: string;
  totalTasks?: number;
  completedTasks?: number;
}

const Projects = () => {
  const navigate = useNavigate();
  const [userInfo, setUserInfo] = useState({ firstName: "User", lastName: "", role: "Loading..." });
  const [projects, setProjects] = useState<Project[]>([]);
  const [viewType, setViewType] = useState("grid");
  const [searchQuery, setSearchQuery] = useState("");
  const [filter, setFilter] = useState("all");
  const { hasPermission } = usePermission();

  // Fetch user data and projects
  useEffect(() => {
    const fetchProjects = async () => {
      try {
        const response = await axios.get("http://localhost:5045/api/projects/all", {
          headers: { Authorization: `Bearer ${localStorage.getItem("token")}` },
        });

        const projectsWithProgress = await Promise.all(
          response.data.map(async (project: Project) => {
            try {
              const progressRes = await axios.get(`http://localhost:5045/api/tasks/project/${project.projectID}/progress`, {
                headers: { Authorization: `Bearer ${localStorage.getItem("token")}` },
              });

              return {
                ...project,
                totalTasks: progressRes.data.totalTasks,
                completedTasks: progressRes.data.completedTasks,
              };
            } catch (error) {
              console.error(`Error fetching progress for project ${project.projectID}`, error);
              return project; // fallback if progress fetch fails
            }
          })
        );

        setProjects(projectsWithProgress);
      } catch (error) {
        console.error("Error fetching projects:", error);
      }
    };

    fetchProjects();
  }, []);


  // Filter projects based on search & status
  const filteredProjects = projects.filter((project) =>
    (filter === "all" || project.visibility.toLowerCase() === filter) &&
    project.projectName.toLowerCase().includes(searchQuery.toLowerCase())
  );

  return (
    <div className="min-h-screen bg-gray-900 text-white">
      {/* Navbar */}
      <Navbar />

      {/* Search, Filter & Create Project Button */}
      <div className="p-4 bg-slate-800 border-b border-gray-700 flex flex-wrap justify-between items-center gap-2">
        <input
          type="text"
          placeholder="Search projects..."
          className="w-full sm:w-1/3 p-2 bg-gray-700 text-white rounded focus:outline-none"
          value={searchQuery}
          onChange={(e) => setSearchQuery(e.target.value)}
        />
        <select
          className="w-full sm:w-auto p-2 bg-gray-700 text-white rounded"
          value={filter}
          onChange={(e) => setFilter(e.target.value)}
        >
          <option value="all">All</option>
          <option value="public">Public</option>
          <option value="private">Private</option>
        </select>
        <button
          className="w-full sm:w-auto bg-gray-700 p-2 rounded hover:bg-gray-600"
          onClick={() => setViewType(viewType === "grid" ? "list" : "grid")}
        >
          {viewType === "grid" ? "🔲 Grid View" : "📋 List View"}
        </button>
        <div>
          {hasPermission("CREATE_PROJECT") && (
            <button
              className="w-full sm:w-auto bg-green-600 px-4 py-2 rounded hover:bg-green-700"
              onClick={() => navigate("/projects/create-project")}
            >
              ➕ Create Project
            </button>
          )}
        </div>
      </div>

      {/* Project List/Grid View */}
      <div className={`p-6 ${viewType === "grid" ? "grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-6" : "flex flex-col gap-4"}`}>
        {filteredProjects.map((project) => (
          <div
            key={project.projectID}
            className="bg-slate-800 p-4 rounded shadow-lg border border-gray-700 hover:bg-slate-700 cursor-pointer transition"
            onClick={() => navigate(`/projects/${project.projectID}`)}
          >
            <h3 className="text-lg font-bold">{project.projectName}</h3>
            <p className="text-sm text-gray-400">
              📅 {new Date(project.startDate).toLocaleDateString()} - {new Date(project.endDate).toLocaleDateString()}
            </p>
            <p className="text-sm text-gray-400">👤 Manager: {project.projectManagerName}</p>
            <p className="text-sm text-gray-400">👥 Members: {project.membersCount}</p>
            <div className="mt-2">
              <p className="text-sm text-gray-400">
                ✅ Progress:{" "}
                {project.totalTasks ? `${Math.round((project.completedTasks ?? 0) / project.totalTasks * 100)}%` : "N/A"}
              </p>
              <div className="w-full bg-gray-700 h-2 rounded">
                <div
                  className="h-2 rounded bg-blue-500"
                  style={{
                    width: project.totalTasks
                      ? `${(project.completedTasks ?? 0) / project.totalTasks * 100}%`
                      : "0%",
                  }}
                />
              </div>
            </div>
            <div className="flex gap-2 mt-2 flex-wrap">
              <button className="bg-blue-600 px-3 py-1 rounded text-sm hover:bg-blue-700">📌 Tasks</button>
              <button className="bg-gray-600 px-3 py-1 rounded text-sm hover:bg-gray-700">💬 Discussions</button>
              <button className="bg-gray-600 px-3 py-1 rounded text-sm hover:bg-gray-700">📂 Files</button>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
};

export default Projects;
