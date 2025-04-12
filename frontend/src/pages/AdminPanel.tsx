import React, { useState, useEffect } from "react";
import axios from "axios";
import Navbar from "../components/Navbar";
import { useNavigate } from "react-router-dom";

// ----------------------------
// TypeScript Interfaces
// ----------------------------
interface UserInfo {
    id: string;
    firstName: string;
    lastName: string;
    email: string;
    role: string;
    companyID: number;
}

interface Role {
    id: string;
    name: string;
    companyID: number | null;
    isCompanyRole: boolean;
}

interface Permission {
    permissionID: number;
    permissionName: string;
}

// ----------------------------
// PermissionEditingTab Component
// ----------------------------
interface PermissionEditingTabProps {
    newPermissionName: string;
    setNewPermissionName: (name: string) => void;
    newPermissionDescription: string;
    setNewPermissionDescription: (desc: string) => void;
    selectedRoleForPermission: string;
    setSelectedRoleForPermission: (role: string) => void;
    roles: Role[];
    permissions: Permission[];
    token: string | null;
    refreshPermissions: () => void;
}

const PermissionEditingTab = ({
    newPermissionName,
    setNewPermissionName,
    newPermissionDescription,
    setNewPermissionDescription,
    selectedRoleForPermission,
    setSelectedRoleForPermission,
    roles,
    permissions,
    token,
    refreshPermissions,
}: PermissionEditingTabProps) => {
    const [modalRoleId, setModalRoleId] = useState<string | null>(null);
    const [modalRoleName, setModalRoleName] = useState<string>("");
    const [rolePermissions, setRolePermissions] = useState<number[]>([]);
    console.debug("[DEBUG] Rendering existing permissions list", permissions);
    const createPermission = async () => {
        try {
            await axios.post(
                "http://localhost:5045/api/admin/permissions",
                { name: newPermissionName, description: newPermissionDescription },
                { headers: { Authorization: `Bearer ${token}` } }
            );
            alert("Permission created successfully");
            refreshPermissions();
            setNewPermissionName("");
            setNewPermissionDescription("");
        } catch (error) {
            console.error("Error creating permission:", error);
            alert("Failed to create permission");
        }
    };

    const openPermissionModal = async (roleId: string, roleName: string) => {
        setModalRoleId(roleId);
        setModalRoleName(roleName);
        try {
            const res = await axios.get<number[]>(
                `http://localhost:5045/api/admin/roles/${roleId}/permissions`,
                { headers: { Authorization: `Bearer ${token}` } }
            );
            setRolePermissions(res.data);
        } catch (err) {
            console.error("Failed to fetch role permissions:", err);
        }
    };

    const handlePermissionToggle = async (permId: number) => {
        const hasPermission = rolePermissions.includes(permId);
        try {
            let updatedPermissions: number[];

            if (hasPermission) {
                // Remove the permission
                updatedPermissions = rolePermissions.filter((id) => id !== permId);
            } else {
                // Add the permission
                updatedPermissions = [...rolePermissions, permId];
            }

            // Send the updated list to the backend using PUT
            await axios.put(
                `http://localhost:5045/api/admin/roles/${modalRoleId}/permissions`,
                { permissionIds: updatedPermissions },
                { headers: { Authorization: `Bearer ${token}` } }
            );

            // Update the local state
            setRolePermissions(updatedPermissions);
        } catch (err) {
            console.error("Failed to toggle permission:", err);
            alert("Failed to update permission");
        }
    };

    const assignPermissionToRole = async () => {
        console.debug(
            "[DEBUG] Assigning permission:",
            newPermissionName,
            "to role:",
            selectedRoleForPermission
        );
        try {
            await axios.post(
                `http://localhost:5045/api/admin/roles/${selectedRoleForPermission}/permissions`,
                { permissionName: newPermissionName },
                { headers: { Authorization: `Bearer ${token}` } }
            );
            alert("Permission assigned to role successfully");
        } catch (error) {
            console.error("Error assigning permission:", error);
            alert("Failed to assign permission");
        }
    };

    return (
        <div>
            <h2 className="text-xl font-bold mb-4">Permission Editing</h2>

            {/* CREATE PERMISSION */}
            <div className="mb-6">
                <h3 className="text-lg font-semibold mb-2">Create New Permission</h3>
                <input
                    type="text"
                    className="w-full p-2 bg-gray-700 rounded mb-2"
                    placeholder="Permission Name"
                    value={newPermissionName}
                    onChange={(e) => setNewPermissionName(e.target.value)}
                />
                <input
                    type="text"
                    className="w-full p-2 bg-gray-700 rounded mb-2"
                    placeholder="Permission Description"
                    value={newPermissionDescription}
                    onChange={(e) => setNewPermissionDescription(e.target.value)}
                />
                <button
                    className="bg-green-600 hover:bg-green-700 px-4 py-2 rounded"
                    onClick={createPermission}
                >
                    Create Permission
                </button>
            </div>

            {/* PERMISSION CHECKLIST MODAL */}
            <div className="mb-6">
                <h3 className="text-lg font-semibold mb-2">Assign Permissions</h3>
                <ul className="space-y-2">
                    {roles.map((role) => (
                        <li key={role.id} className="flex justify-between items-center bg-gray-800 p-2 rounded">
                            <span>{role.name}</span>
                            <button
                                className="bg-blue-600 hover:bg-blue-700 px-3 py-1 rounded text-sm"
                                onClick={() => openPermissionModal(role.id, role.name)}
                            >
                                Edit Permissions
                            </button>
                        </li>
                    ))}
                </ul>
            </div>

            {modalRoleId && (
                <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
                    <div className="bg-gray-900 p-6 rounded w-[400px] max-w-full">
                        <h3 className="text-xl font-bold mb-4">Permissions for {modalRoleName}</h3>
                        <div className="max-h-64 overflow-y-auto mb-4">
                            {permissions?.length > 0 && permissions.map((perm, index) => (
                                <label key={perm.permissionID ?? `perm-check-${index}`} className="block mb-2">
                                    <input
                                        type="checkbox"
                                        className="mr-2"
                                        checked={rolePermissions.includes(perm.permissionID)}
                                        onChange={() => handlePermissionToggle(perm.permissionID)}
                                    />
                                    {perm.permissionName}
                                </label>
                            ))}

                        </div>
                        <button
                            className="bg-gray-600 hover:bg-gray-700 px-4 py-2 rounded"
                            onClick={() => setModalRoleId(null)}
                        >
                            Close
                        </button>
                    </div>
                </div>
            )}
            <div>
                <h3 className="text-lg font-semibold mb-2">Existing Permissions</h3>
                <ul className="list-disc pl-5">
                    {permissions.map((perm, index) => (
                        <li key={perm.permissionID ?? `perm-${index}`}>{perm.permissionName}</li>
                    ))}
                </ul>
            </div>
        </div>
    );
};

// ----------------------------
// AdminPanel Component
// ----------------------------
const AdminPanel = () => {
    // Current active admin tab: roleManagement, userManagement, permissionEditing.
    const [activeTab, setActiveTab] = useState("roleManagement");

    // Data state for users, roles, permissions, and current user info.
    const [users, setUsers] = useState<UserInfo[]>([]);
    const [roles, setRoles] = useState<Role[]>([]);
    const [permissions, setPermissions] = useState<Permission[]>([]);
    const [currentUser, setCurrentUser] = useState<UserInfo | null>(null);
    const [loading, setLoading] = useState<boolean>(true);

    // State for permission editing form.
    const [newPermissionName, setNewPermissionName] = useState("");
    const [newPermissionDescription, setNewPermissionDescription] = useState("");
    const [selectedRoleForPermission, setSelectedRoleForPermission] = useState("");

    const token = localStorage.getItem("token");
    const navigate = useNavigate();

    // Fetch current user info
    useEffect(() => {
        const fetchCurrentUser = async () => {
            console.debug("[DEBUG] Fetching current user info from /api/users/me");
            try {
                const response = await axios.get("http://localhost:5045/api/users/me", {
                    headers: { Authorization: `Bearer ${token}` },
                });
                console.debug("[DEBUG] Received current user data:", response.data);
                setCurrentUser(response.data);
            } catch (error) {
                console.error("Error fetching current user info:", error);
            }
        };
        fetchCurrentUser();
    }, [token]);

    // Fetch admin-related data on component mount.
    useEffect(() => {
        const fetchAdminData = async () => {
            console.debug("[DEBUG] Fetching admin data: users and roles");
            try {
                const [usersResponse, rolesResponse] = await Promise.all([
                    axios.get<UserInfo[]>("http://localhost:5045/api/admin/users", {
                        headers: { Authorization: `Bearer ${token}` },
                    }),
                    axios.get<Role[]>("http://localhost:5045/api/admin/roles", {
                        headers: { Authorization: `Bearer ${token}` },
                    }),
                ]);
                setUsers(usersResponse.data);
                setRoles(rolesResponse.data);
            } catch (error: any) {
                if (error.response?.status === 403) {
                    console.warn("[WARN] User is not authorized to access admin data");
                    navigate("/unauthorized");
                } else {
                    console.error("Error fetching admin data:", error);
                }
            } finally {
                setLoading(false);
            }
        };
    
        fetchAdminData();
    }, [token, navigate]);

    useEffect(() => {
        const fetchPermissions = async () => {
            console.debug("[DEBUG] Fetching permissions from /api/admin/permissions");
            try {
                const response = await axios.get<Permission[]>("http://localhost:5045/api/admin/permissions", {
                    headers: { Authorization: `Bearer ${token}` },
                });
                console.debug("[DEBUG] Fetched permissions:", response.data);
                setPermissions(response.data);
            } catch (error) {
                console.error("Error fetching permissions:", error);
            }
        };

        fetchPermissions();
    }, [token]);


    // A helper function to refresh permissions data.
    const refreshPermissions = async () => {
        try {
            const response = await axios.get<Permission[]>("http://localhost:5045/api/admin/permissions", {
                headers: { Authorization: `Bearer ${token}` },
            });
            console.debug("[DEBUG] Refreshed permissions:", response.data);
            setPermissions(response.data);
        } catch (error) {
            console.error("Error refreshing permissions:", error);
        }
    };

    // ----------------------------
    // Inline Tab Components for Role & User Management
    // (They are not causing focus issues so we keep them inline)
    // ----------------------------
    const RoleManagementTab = () => {
        const [editingRoleId, setEditingRoleId] = useState<string | null>(null);
        const [editedRoleName, setEditedRoleName] = useState<string>("");
        const [editedIsCompanyRole, setEditedIsCompanyRole] = useState<boolean>(false);
        const [editedCompanyID, setEditedCompanyID] = useState<number | null>(null);

        const handleEditClick = (role: Role) => {
            console.debug("[DEBUG] Start editing role:", role);
            setEditingRoleId(role.id);
            setEditedRoleName(role.name);
            setEditedIsCompanyRole(role.isCompanyRole);
            setEditedCompanyID(role.companyID);
        };

        const handleCancelClick = () => {
            console.debug("[DEBUG] Cancel editing role");
            setEditingRoleId(null);
        };

        const handleSaveClick = async (roleId: string) => {
            console.debug("[DEBUG] Saving changes for role:", roleId, {
                name: editedRoleName,
                isCompanyRole: editedIsCompanyRole,
                companyID: editedCompanyID ?? currentUser?.companyID,
            });
            try {
                const companyIDToSend = editedCompanyID ?? currentUser?.companyID;
                await axios.put(
                    `http://localhost:5045/api/admin/roles/${roleId}`,
                    {
                        name: editedRoleName,
                        isCompanyRole: editedIsCompanyRole,
                        companyID: companyIDToSend,
                    },
                    { headers: { Authorization: `Bearer ${token}` } }
                );
                alert("Role updated successfully");

                const rolesResponse = await axios.get<Role[]>("http://localhost:5045/api/admin/roles", {
                    headers: { Authorization: `Bearer ${token}` },
                });
                console.debug("[DEBUG] Refreshed roles after update:", rolesResponse.data);
                setRoles(rolesResponse.data);
                setEditingRoleId(null);
            } catch (error) {
                console.error("Error updating role:", error);
                alert("Failed to update role");
            }
        };

        return (
            <div>
                <h2 className="text-xl font-bold mb-4">Role Management</h2>
                <table className="mx-auto min-w-full bg-gray-800 border border-gray-700 text-center">
                    <thead>
                        <tr>
                            <th className="py-2 px-4 border-b">Role ID</th>
                            <th className="py-2 px-4 border-b">Role Name</th>
                            <th className="py-2 px-4 border-b">Company Role</th>
                            <th className="py-2 px-4 border-b">Actions</th>
                        </tr>
                    </thead>
                    <tbody>
                        {roles.map((role) => (
                            <tr key={role.id}>
                                <td className="py-2 px-4 border-b">{role.id}</td>
                                <td className="py-2 px-4 border-b">
                                    {editingRoleId === role.id ? (
                                        <input
                                            type="text"
                                            value={editedRoleName}
                                            className="bg-gray-700 p-1 rounded"
                                            onChange={(e) => setEditedRoleName(e.target.value)}
                                        />
                                    ) : (
                                        role.name
                                    )}
                                </td>
                                <td className="py-2 px-4 border-b">
                                    {editingRoleId === role.id ? (
                                        <select
                                            value={editedIsCompanyRole ? "Yes" : "No"}
                                            onChange={(e) => setEditedIsCompanyRole(e.target.value === "Yes")}
                                            className="bg-gray-700 p-1 rounded"
                                        >
                                            <option value="Yes">Yes</option>
                                            <option value="No">No</option>
                                        </select>
                                    ) : (
                                        role.isCompanyRole ? "Yes" : "No"
                                    )}
                                </td>
                                <td className="py-2 px-4 border-b">
                                    {editingRoleId === role.id ? (
                                        <>
                                            <button
                                                className="bg-blue-600 hover:bg-blue-700 px-2 py-1 rounded mr-2"
                                                onClick={() => handleSaveClick(role.id)}
                                            >
                                                Save
                                            </button>
                                            <button
                                                className="bg-gray-600 hover:bg-gray-700 px-2 py-1 rounded"
                                                onClick={handleCancelClick}
                                            >
                                                Cancel
                                            </button>
                                        </>
                                    ) : (
                                        <button
                                            className="bg-green-600 hover:bg-green-700 px-2 py-1 rounded"
                                            onClick={() => handleEditClick(role)}
                                        >
                                            Edit
                                        </button>
                                    )}
                                </td>
                            </tr>
                        ))}
                    </tbody>
                </table>
            </div>
        );
    };

    const UserManagementTab = () => {
        const [editingUserId, setEditingUserId] = useState<string | null>(null);
        const [selectedUserRole, setSelectedUserRole] = useState<string>("");
        const banUser = async (userId: string) => {
            console.debug("[DEBUG] Banning user with ID:", userId);
            try {
                await axios.delete(`http://localhost:5045/api/admin/users/${userId}/ban`, {
                    headers: { Authorization: `Bearer ${token}` },
                });
                alert("User banned successfully");
                const response = await axios.get<UserInfo[]>("http://localhost:5045/api/admin/users", {
                    headers: { Authorization: `Bearer ${token}` },
                });
                console.debug("[DEBUG] Refreshed users after banning:", response.data);
                setUsers(response.data);
            } catch (error) {
                console.error("Error banning user:", error);
                alert("Failed to ban user");
            }
        };

        const updateUserRole = async (userId: string) => {
            try {
                await axios.put(
                    `http://localhost:5045/api/admin/users/${userId}/role`,
                    { newRole: selectedUserRole },
                    { headers: { Authorization: `Bearer ${token}` } }
                );
                alert("User role updated successfully");

                const response = await axios.get<UserInfo[]>("http://localhost:5045/api/admin/users", {
                    headers: { Authorization: `Bearer ${token}` },
                });
                setUsers(response.data);
                setEditingUserId(null);
            } catch (error) {
                console.error("Error updating user role:", error);
                alert("Failed to update role");
            }
        };

        return (
            <div>
                <h2 className="text-xl font-bold mb-4">User Management</h2>
                <table className="min-w-full bg-gray-800 border border-gray-700 text-center">
                    <thead>
                        <tr>
                            <th className="py-2 px-4 border-b">Name</th>
                            <th className="py-2 px-4 border-b">Email</th>
                            <th className="py-2 px-4 border-b">Role</th>
                            <th className="py-2 px-4 border-b">Actions</th>
                        </tr>
                    </thead>
                    <tbody>
                        {users.map((user) => (
                            <tr key={user.id}>
                                <td className="py-2 px-4 border-b">
                                    {user.firstName} {user.lastName}
                                </td>
                                <td className="py-2 px-4 border-b">{user.email}</td>
                                <td className="py-2 px-4 border-b">
                                    {editingUserId === user.id ? (
                                        <select
                                            value={selectedUserRole}
                                            onChange={(e) => setSelectedUserRole(e.target.value)}
                                            className="bg-gray-700 p-1 rounded"
                                        >
                                            {roles.map((role) => (
                                                <option key={role.id} value={role.name}>{role.name}</option>
                                            ))}
                                        </select>
                                    ) : (
                                        user.role
                                    )}
                                </td>
                                <td className="py-2 px-4 border-b">
                                    {editingUserId === user.id ? (
                                        <>
                                            <button
                                                className="bg-blue-600 hover:bg-blue-700 px-2 py-1 rounded mr-2"
                                                onClick={() => updateUserRole(user.id)}
                                            >
                                                Save
                                            </button>
                                            <button
                                                className="bg-gray-600 hover:bg-gray-700 px-2 py-1 rounded"
                                                onClick={() => setEditingUserId(null)}
                                            >
                                                Cancel
                                            </button>
                                        </>
                                    ) : (
                                        <>
                                            <button
                                                className="bg-green-600 hover:bg-green-700 px-2 py-1 rounded mr-2"
                                                onClick={() => {
                                                    setEditingUserId(user.id);
                                                    setSelectedUserRole(user.role);
                                                }}
                                            >
                                                Change Role
                                            </button>
                                            <button
                                                className="bg-red-600 hover:bg-red-700 px-2 py-1 rounded"
                                                onClick={() => banUser(user.id)}
                                            >
                                                Ban
                                            </button>
                                        </>
                                    )}
                                </td>

                            </tr>
                        ))}
                    </tbody>
                </table>
            </div>
        );
    };

    if (loading) {
        return (
            <div className="min-h-screen bg-gray-900 text-white flex items-center justify-center">
                Loading admin panel...
            </div>
        );
    }

    return (
        <div className="min-h-screen bg-gray-900 text-white">
            {/* Navbar */}
            <Navbar />

            {/* Admin Panel Header */}
            <div className="p-6 bg-slate-800 border-b border-gray-700">
                <h1 className="text-2xl font-bold">Admin Panel</h1>
            </div>

            {/* Tab Navigation */}
            <div className="flex justify-center bg-gray-800 border-b border-gray-700 p-2">
                {["roleManagement", "userManagement", "permissionEditing"].map((tab) => (
                    <button
                        key={tab}
                        className={`px-4 py-2 text-sm font-bold ${activeTab === tab ? "bg-green-600" : "hover:bg-gray-700"} rounded`}
                        onClick={() => {
                            console.debug("[DEBUG] Changing active tab to:", tab);
                            setActiveTab(tab);
                        }}
                    >
                        {tab === "roleManagement"
                            ? "Role Management"
                            : tab === "userManagement"
                                ? "User Management"
                                : "Permission Editing"}
                    </button>
                ))}
            </div>

            {/* Tab Content */}
            <div className="p-6">
                {activeTab === "roleManagement" && <RoleManagementTab />}
                {activeTab === "userManagement" && <UserManagementTab />}
                {activeTab === "permissionEditing" && (
                    <PermissionEditingTab
                        newPermissionName={newPermissionName}
                        setNewPermissionName={setNewPermissionName}
                        newPermissionDescription={newPermissionDescription}
                        setNewPermissionDescription={setNewPermissionDescription}
                        selectedRoleForPermission={selectedRoleForPermission}
                        setSelectedRoleForPermission={setSelectedRoleForPermission}
                        roles={roles}
                        permissions={permissions}
                        token={token}
                        refreshPermissions={refreshPermissions}
                    />
                )}
            </div>
        </div>
    );
};

export default AdminPanel;
