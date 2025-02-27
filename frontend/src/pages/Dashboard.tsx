import { useState, useEffect, useRef } from "react";
import axios from "axios";
import Navbar from "../components/navbar";

const Dashboard = () => {
  const [userInfo, setUserInfo] = useState({ firstName: "User", lastName: "", role: "Loading..." });

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

  return (
    <div className="min-h-screen bg-gray-900 text-white">
      {/* Navbar */}
      <Navbar userInfo={userInfo} />

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
