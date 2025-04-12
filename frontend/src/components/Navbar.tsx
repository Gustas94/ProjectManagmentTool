import { useState, useEffect, useRef, useContext } from "react";
import { useNavigate } from "react-router-dom";
import { UserContext } from "../contexts/UserContext"; // Adjust the import path as needed
import { usePermission } from "../hooks/usePermission";

const Navbar: React.FC = () => {
  const { user, setUser } = useContext(UserContext);
  console.log("User from context:", user);
  console.log("Permissions:", user?.permissions);
  const navigate = useNavigate();
  const [isDropdownOpen, setDropdownOpen] = useState(false);
  const dropdownRef = useRef<HTMLDivElement>(null);
  const { hasPermission } = usePermission();

  // Handle clicks outside the dropdown to close it
  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (dropdownRef.current && !dropdownRef.current.contains(event.target as Node)) {
        setDropdownOpen(false);
      }
    };
    if (isDropdownOpen) {
      document.addEventListener("mousedown", handleClickOutside);
    }
    return () => {
      document.removeEventListener("mousedown", handleClickOutside);
    };
  }, [isDropdownOpen]);

  return (
    <nav className="bg-slate-800 p-4 flex justify-between items-center shadow-lg relative">
      {/* Left Section - Navigation */}
      <div className="flex items-center gap-6">
        <span className="text-xl font-bold text-white">Company Logo</span>
        <button className="text-gray-300 hover:text-white" onClick={() => navigate("/dashboard")}>
          Home
        </button>
        <button className="text-gray-300 hover:text-white" onClick={() => navigate("/projects")}>
          Projects
        </button>
        <button className="text-gray-300 hover:text-white" onClick={() => navigate("/groups")}>
          Groups
        </button>
        {/* Conditionally render Admin Panel if the user has a specific permission */} 
        {hasPermission("CREATE_PROJECT") && (
          <button className="text-gray-300 hover:text-white" onClick={() => navigate("/admin")}>
            Admin Panel
          </button>
        )}
      </div>

      {/* Right Section - Notifications & Profile */}
      <div className="flex items-center gap-4 relative">
        <button className="text-gray-300 hover:text-white">🔔</button>

        {/* Profile Dropdown */}
        <div ref={dropdownRef} className="relative">
          <button
            className="w-10 h-10 bg-blue-600 rounded-full flex items-center justify-center text-white font-bold"
            onClick={() => setDropdownOpen(!isDropdownOpen)}
          >
            {user ? user.firstName.charAt(0) : "U"}
            {user ? user.lastName.charAt(0) : ""}
          </button>

          {isDropdownOpen && (
            <div className="absolute right-0 mt-2 w-48 bg-gray-800 border border-gray-600 rounded-lg shadow-lg z-50">
              <div className="px-4 py-3 text-white border-b border-gray-600">
                <p className="font-semibold text-center">
                  {user ? `${user.firstName} ${user.lastName}` : "User"}
                </p>
              </div>
              <div className="px-4 py-3 text-white border-b border-gray-600">
                <p className="text-gray-400 text-sm text-center">{user ? user.role : ""}</p>
              </div>
              <button
                className="w-full text-left px-4 py-2 text-gray-300 hover:bg-gray-700 hover:text-white flex justify-center"
                onClick={() => navigate("/profile")}
              >
                Profile
              </button>
              <button
                className="w-full text-left px-4 py-2 text-gray-300 hover:bg-gray-700 hover:text-white flex justify-center"
                onClick={() => {
                  localStorage.removeItem("token");
                  setUser(null);
                  navigate("/login");
                }}
              >
                Logout
              </button>
            </div>
          )}
        </div>
      </div>
    </nav>
  );
};

export default Navbar;
