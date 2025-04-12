import { useContext } from "react";
import { UserContext } from "../contexts/UserContext";

export function usePermission() {
  const { user } = useContext(UserContext);

  const hasPermission = (permission: string): boolean => {
    return user?.permissions?.includes(permission) ?? false;
  };

  return { hasPermission };
}
