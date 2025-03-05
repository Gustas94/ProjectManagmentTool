import { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import axios from "axios";
import Navbar from "../components/Navbar";

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
}


const Projects = () => {
  const navigate = useNavigate();
  const [userInfo, setUserInfo] = useState({ firstName: "User", lastName: "", role: "Loading..." });
  const [projects, setProjects] = useState<Project[]>([]);
  const [viewType, setViewType] = useState("grid");
  const [searchQuery, setSearchQuery] = useState("");
  const [filter, setFilter] = useState("all");

  // Fetch user data and projects
  useEffect(() => {
    const fetchUserInfo = async () => {
      try {
        const response = await axios.get("http://localhost:5045/api/users/me", {
          headers: { Authorization: `Bearer ${localStorage.getItem("token")}` },
        });
        setUserInfo(response.data);
      } catch (error) {
        console.error("Failed to fetch user info:", error);
      }
    };

    const fetchProjects = async () => {
      try {
        const response = await axios.get("http://localhost:5045/api/projects/all", {
          headers: { Authorization: `Bearer ${localStorage.getItem("token")}` },
        });
        setProjects(response.data);
      } catch (error) {
        console.error("Error fetching projects:", error);
      }
    };

    fetchUserInfo();
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
      <Navbar userInfo={userInfo} />

      {/* Search, Filter & Create Project Button */}
      <div className="p-4 bg-slate-800 border-b border-gray-700 flex justify-between items-center">
        <input
          type="text"
          placeholder="Search projects..."
          className="w-1/3 p-2 bg-gray-700 text-white rounded focus:outline-none"
          value={searchQuery}
          onChange={(e) => setSearchQuery(e.target.value)}
        />
        <select
          className="p-2 bg-gray-700 text-white rounded"
          value={filter}
          onChange={(e) => setFilter(e.target.value)}
        >
          <option value="all">All</option>
          <option value="public">Public</option>
          <option value="private">Private</option>
        </select>
        <button
          className="bg-gray-700 p-2 rounded hover:bg-gray-600"
          onClick={() => setViewType(viewType === "grid" ? "list" : "grid")}
        >
          {viewType === "grid" ? "🔲 Grid View" : "📋 List View"}
        </button>
        {/* Create Project Button */}
        <button
          className="bg-green-600 px-4 py-2 rounded hover:bg-green-700"
          onClick={() => navigate("/projects/create-project")}
        >
          ➕ Create Project
        </button>
      </div>

      {/* Project List/Grid View */}
      <div className={`p-6 ${viewType === "grid" ? "grid grid-cols-3 gap-6" : "flex flex-col gap-4"}`}>
        {filteredProjects.map((project) => (
          <div
            key={project.projectID}
            className="bg-slate-800 p-4 rounded shadow-lg border border-gray-700 hover:bg-slate-700 cursor-pointer transition"
            onClick={() => navigate(`/projects/${project.projectID}`)}
          >
            <h3 className="text-lg font-bold">{project.projectName}</h3>
            <p className="text-sm text-gray-400">📅 {new Date(project.startDate).toLocaleDateString()} - {new Date(project.endDate).toLocaleDateString()}</p>
            <p className="text-sm text-gray-400">👤 Manager: {project.projectManagerName}</p>
            <p className="text-sm text-gray-400">👥 Members: {project.membersCount}</p>
            <div className="mt-2 bg-gray-700 h-2 rounded">
              <div
                className={`h-2 rounded ${
                  project.visibility === "private" ? "bg-red-500" : "bg-green-500"
                }`}
                style={{ width: "100%" }}
              ></div>
            </div>
            <div className="flex gap-2 mt-2">
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