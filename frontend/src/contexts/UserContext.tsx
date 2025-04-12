import React, { createContext, useState, useEffect, ReactNode } from "react";
import axios from "axios";

interface UserInfo {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  role: string;
  companyID?: number | null;
  permissions?: string[];
}

interface UserContextType {
  user: UserInfo | null;
  setUser: (user: UserInfo | null) => void;
  fetchUser: () => Promise<void>;
}

export const UserContext = createContext<UserContextType>({
  user: null,
  setUser: () => {},
  fetchUser: async () => {},
});

interface UserProviderProps {
  children: ReactNode;
}

export const UserProvider: React.FC<UserProviderProps> = ({ children }) => {
  const [user, setUser] = useState<UserInfo | null>(null);
  const token = localStorage.getItem("token");

  const fetchUser = async () => {
    if (!token) return;

    try {
      const userResponse = await axios.get("http://localhost:5045/api/users/me", {
        headers: { Authorization: `Bearer ${token}` },
      });

      const userData = userResponse.data;

      if (!userData?.id) {
        console.warn("User data missing ID.");
        setUser(userData);
        return;
      }

      // ✅ Set user from /me which already includes permissions
      setUser(userData);
    } catch (error) {
      console.error("Error fetching user:", error);
    }
  };

  useEffect(() => {
    fetchUser(); // Fetch on load
  }, [token]);

  return (
    <UserContext.Provider value={{ user, setUser, fetchUser }}>
      {children}
    </UserContext.Provider>
  );
};
