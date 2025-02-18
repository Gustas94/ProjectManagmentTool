import { useState, useEffect, useRef } from "react";
import { useNavigate } from "react-router-dom";
import axios from "axios";

const Projects = () => {
  const navigate = useNavigate();
  const [dropdownOpen, setDropdownOpen] = useState(false);
  const [userInfo, setUserInfo] = useState({ firstName: "User", lastName: "", role: "Loading..." });
  const dropdownRef = useRef<HTMLDivElement>(null);
  const [viewType, setViewType] = useState("grid");
  const [searchQuery, setSearchQuery] = useState("");
  const [filter, setFilter] = useState("all");

  // Fetch user data
  useEffect(() => {
    const fetchUserInfo = async () => {
      try {
        const response = await axios.get("/api/dashboard/user-info", {
          headers: { Authorization: `Bearer ${localStorage.getItem("token")}` }
        });
        setUserInfo(response.data);
      } catch (error) {
        console.error("Failed to fetch user info:", error);
      }
    };
    fetchUserInfo();
  }, []);

  // Close dropdown when clicking outside
  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (dropdownRef.current && !dropdownRef.current.contains(event.target as Node)) {
        setDropdownOpen(false);
      }
    };
    document.addEventListener("mousedown", handleClickOutside);
    return () => document.removeEventListener("mousedown", handleClickOutside);
  }, []);

  // Example static project data (Replace with API data)
  const projects = [
    { id: 1, name: "Website Redesign", status: "Active", startDate: "2025-02-01", deadline: "2025-06-15", manager: "Alice Johnson", members: 12, progress: 60 },
    { id: 2, name: "Mobile App Development", status: "Paused", startDate: "2025-01-10", deadline: "2025-08-30", manager: "Michael Smith", members: 8, progress: 35 },
    { id: 3, name: "AI Research Project", status: "Completed", startDate: "2024-05-01", deadline: "2024-12-01", manager: "Dr. Emily Carter", members: 20, progress: 100 },
  ];

  const filteredProjects = projects.filter((project) =>
    (filter === "all" || project.status.toLowerCase() === filter) &&
    project.name.toLowerCase().includes(searchQuery.toLowerCase())
  );

  return (
    <div className="min-h-screen bg-gray-900 text-white">
      {/* Navbar */}
      <nav className="bg-slate-800 p-4 flex justify-between items-center shadow-lg">
        <div className="flex items-center gap-6">
          <span className="text-xl font-bold text-white">Company Logo</span>
          <button className="text-gray-300 hover:text-white" onClick={() => navigate("/dashboard")}>Home</button>
          <button className="text-gray-300 hover:text-white" onClick={() => navigate("/projects")}>Projects</button>
          <button className="text-gray-300 hover:text-white">Groups</button>
          <button className="text-gray-300 hover:text-white">Admin Panel</button>
        </div>
        <div className="flex items-center gap-4">
          <button className="text-gray-300 hover:text-white">🔔</button>
          <div className="relative" ref={dropdownRef}>
            <button
              className="w-10 h-10 bg-blue-600 rounded-full flex items-center justify-center text-white font-bold"
              onClick={() => setDropdownOpen(!dropdownOpen)}
            >
              {userInfo.firstName.charAt(0)}
              {userInfo.lastName.charAt(0)}
            </button>
            {dropdownOpen && (
              <div className="absolute right-0 mt-2 w-48 bg-slate-800 text-white rounded shadow-lg">
                <div className="p-3 border-b border-gray-700 text-center font-bold">
                  {userInfo.firstName} {userInfo.lastName}
                </div>
                <div className="p-3 border-b border-gray-700 text-center text-gray-400">{userInfo.role}</div>
                <button className="w-full text-left px-4 py-2 hover:bg-gray-700">Profile</button>
                <button className="w-full text-left px-4 py-2 hover:bg-gray-700">Settings</button>
                <button
                  className="w-full text-left px-4 py-2 text-red-400 hover:bg-gray-700"
                  onClick={() => {
                    localStorage.removeItem("token");
                    navigate("/");
                  }}
                >
                  Log Out
                </button>
              </div>
            )}
          </div>
        </div>
      </nav>

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
          <option value="active">Active</option>
          <option value="paused">Paused</option>
          <option value="completed">Completed</option>
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
          onClick={() => navigate("/projects/create")}
        >
          ➕ Create Project
        </button>
      </div>

      {/* Project List/Grid View */}
      <div className={`p-6 ${viewType === "grid" ? "grid grid-cols-3 gap-6" : "flex flex-col gap-4"}`}>
        {filteredProjects.map((project) => (
          <div
            key={project.id}
            className="bg-slate-800 p-4 rounded shadow-lg border border-gray-700 hover:bg-slate-700 cursor-pointer transition"
            onClick={() => navigate(`/projects/${project.id}`)}
          >
            <h3 className="text-lg font-bold">{project.name}</h3>
            <p className="text-sm text-gray-400">📅 {project.startDate} - {project.deadline}</p>
            <p className="text-sm text-gray-400">👤 Manager: {project.manager}</p>
            <p className="text-sm text-gray-400">👥 Members: {project.members}</p>
            <div className="mt-2 bg-gray-700 h-2 rounded">
              <div
                className={`h-2 rounded ${
                  project.progress < 50 ? "bg-red-500" : project.progress < 80 ? "bg-yellow-500" : "bg-green-500"
                }`}
                style={{ width: `${project.progress}%` }}
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
