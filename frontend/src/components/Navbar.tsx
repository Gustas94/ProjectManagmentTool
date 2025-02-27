import { useState, useEffect, useRef } from "react";
import { useNavigate } from "react-router-dom";

interface UserInfo {
  firstName: string;
  lastName: string;
  role: string;
  companyID?: number | null;
}

interface NavbarProps {
  userInfo: UserInfo;
}

const Navbar: React.FC<NavbarProps> = ({ userInfo }) => {
  const navigate = useNavigate();
  const [isDropdownOpen, setDropdownOpen] = useState(false);
  const dropdownRef = useRef<HTMLDivElement>(null);

  // Handle outside clicks to close the dropdown
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
        <button className="text-gray-300 hover:text-white">Groups</button>
        <button className="text-gray-300 hover:text-white">Admin Panel</button>
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
            {userInfo.firstName?.charAt(0)}
            {userInfo.lastName?.charAt(0)}
          </button>

          {/* Dropdown Menu */}
          {isDropdownOpen && (
            <div className="absolute right-0 mt-2 w-48 bg-gray-800 border border-gray-600 rounded-lg shadow-lg z-50">
              <div className="px-4 py-3 text-white border-b border-gray-600">
                <p className="font-semibold text-center">{userInfo.firstName} {userInfo.lastName}</p>
              </div>
              <div className="px-4 py-3 text-white border-b border-gray-600">
                <p className="text-gray-400 text-sm text-center">{userInfo.role}</p>
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
