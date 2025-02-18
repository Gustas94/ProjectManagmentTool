import { useState } from "react";
import { useNavigate } from "react-router-dom";

const Dashboard = () => {
  const navigate = useNavigate();
  const [darkMode, setDarkMode] = useState(false);
  const [dropdownOpen, setDropdownOpen] = useState(false);

  return (
    <div className={`min-h-screen ${darkMode ? "bg-gray-900 text-white" : "bg-gray-100 text-gray-900"}`}>
      {/* Navbar */}
      <nav className="bg-slate-800 p-4 flex justify-between items-center shadow-lg">
        <div className="flex items-center gap-6">
          <span className="text-xl font-bold text-white">Company Logo</span>
          <button className="text-gray-300 hover:text-white">Home</button>
          <button className="text-gray-300 hover:text-white">Projects</button>
          <button className="text-gray-300 hover:text-white">Groups</button>
          <button className="text-gray-300 hover:text-white">Admin Panel</button>
        </div>
        <div className="flex items-center gap-4">
          <button className="text-gray-300 hover:text-white">🔔</button>
          <div className="relative">
            <button
              className="w-10 h-10 bg-blue-600 rounded-full flex items-center justify-center text-white font-bold"
              onClick={() => setDropdownOpen(!dropdownOpen)}
            >
              JD
            </button>
            {/* Dropdown Menu */}
            {dropdownOpen && (
              <div className="absolute right-0 mt-2 w-48 bg-white text-gray-900 rounded shadow-lg">
                <div className="p-3 border-b text-center font-bold">John Doe</div>
                <div className="p-3 border-b text-center text-gray-600">Admin</div>
                <button className="w-full text-left px-4 py-2 hover:bg-gray-200">Profile</button>
                <button className="w-full text-left px-4 py-2 hover:bg-gray-200">Settings</button>
                <button className="w-full text-left px-4 py-2 hover:bg-gray-200" onClick={() => setDarkMode(!darkMode)}>
                  {darkMode ? "Light Mode" : "Dark Mode"}
                </button>
                <button className="w-full text-left px-4 py-2 text-red-600 hover:bg-gray-200" onClick={() => navigate("/")}>Log Out</button>
              </div>
            )}
          </div>
        </div>
      </nav>

      {/* Main Layout */}
      <div className="flex p-6 gap-6">
        {/* Left Side - Task List */}
        <div className="w-2/3 bg-white p-4 rounded shadow-lg overflow-y-auto max-h-[70vh]">
          <h2 className="text-xl font-bold mb-4">Your Tasks</h2>
          <div className="space-y-4">
            {/* Example Task */}
            <div className="bg-gray-200 p-4 rounded">
              <h3 className="font-bold">Task Name</h3>
              <p className="text-sm">Deadline: March 5, 2025</p>
              <p className="text-sm">Project: Project Alpha | Group: Dev Team</p>
              <p className="text-sm">Role: Developer</p>
              <button className="text-blue-600 hover:underline">View Messages</button>
            </div>
          </div>
        </div>

        {/* Right Side - Task & Goal Progress */}
        <div className="w-1/3 space-y-4">
          <div className="bg-white p-4 rounded shadow-lg">
            <h2 className="text-xl font-bold mb-2">Progress Overview</h2>
            <p>📊 Progress to All Goals: 75%</p>
            <p>✅ Completed Tasks: 12</p>
            <p>❌ Incomplete Tasks: 3</p>
            <p>⏳ Overdue Tasks: 1</p>
          </div>
          <div className="bg-white p-4 rounded shadow-lg">
            <h2 className="text-xl font-bold mb-2">Recent Activity</h2>
            <p>✔ John completed "Fix UI Bug"</p>
            <p>🔄 Jane changed role to "Manager"</p>
          </div>
        </div>
      </div>
    </div>
  );
};

export default Dashboard;
