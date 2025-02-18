import { useState, useEffect, useRef } from "react";
import { useNavigate } from "react-router-dom";
import axios from "axios";

const Dashboard = () => {
  const navigate = useNavigate();
  const [dropdownOpen, setDropdownOpen] = useState(false);
  const [userInfo, setUserInfo] = useState({ firstName: "User", lastName: "", role: "Loading..." });
  const dropdownRef = useRef<HTMLDivElement>(null);

  // Fetch user data (name & role)
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

  return (
    <div className="min-h-screen bg-gray-900 text-white">
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

      {/* Main Layout */}
      <div className="flex p-6 gap-6">
        <div className="w-2/3 bg-slate-800 p-4 rounded shadow-lg overflow-y-auto max-h-[70vh] border border-slate-700">
          <h2 className="text-xl font-bold mb-4">Your Tasks</h2>
          <div className="space-y-4">
            <div className="bg-gray-700 p-4 rounded border border-gray-600">
              <h3 className="font-bold text-white">Task Name</h3>
              <p className="text-sm text-gray-300">📅 Deadline: March 5, 2025</p>
              <p className="text-sm text-gray-300">🚀 Project: Project Alpha | 🏢 Group: Dev Team</p>
              <p className="text-sm text-gray-300">👨‍💻 Role: {userInfo.role}</p>
              <button className="text-blue-400 hover:underline">💬 View Messages</button>
            </div>
          </div>
        </div>

        <div className="w-1/3 space-y-4">
          <div className="bg-slate-800 p-4 rounded shadow-lg border border-slate-700">
            <h2 className="text-xl font-bold mb-2">📊 Progress Overview</h2>
            <p>✅ Completed Tasks: <span className="text-green-400">12</span></p>
            <p>❌ Incomplete Tasks: <span className="text-red-400">3</span></p>
            <p>⏳ Overdue Tasks: <span className="text-yellow-400">1</span></p>
          </div>
          <div className="bg-slate-800 p-4 rounded shadow-lg border border-slate-700">
            <h2 className="text-xl font-bold mb-2">🔄 Recent Activity</h2>
            <p>✔ <span className="text-green-400">John</span> completed "Fix UI Bug"</p>
            <p>🔄 <span className="text-blue-400">Jane</span> changed role to "Manager"</p>
          </div>
        </div>
      </div>
    </div>
  );
};

export default Dashboard;
