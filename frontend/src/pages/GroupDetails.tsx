import { useState, useEffect } from "react";
import { useParams, useNavigate } from "react-router-dom";
import axios from "axios";
import Navbar from "../components/Navbar";

interface Task {
    taskID: number;
    taskName: string;
    description: string;
    deadline: string;
    priority: string;
    status: string;
}

interface Member {
    id: string;
    firstName: string;
    lastName: string;
}

interface GroupDetailsData {
    groupID: number;
    groupName: string;
    description: string;
    projectAffiliation: string[];
    groupLeadName: string;
    currentProgress: string;
    status: string;
    tasks: Task[];
}

const GroupDetails = () => {
    const { groupId } = useParams<{ groupId: string }>();
    const navigate = useNavigate();
    const [group, setGroup] = useState<GroupDetailsData | null>(null);
    const [loading, setLoading] = useState(true);
    const [activeTab, setActiveTab] = useState("tasks");

    // State for dynamic tab data (e.g., members)
    const [members, setMembers] = useState<Member[]>([]);
    const [membersLoading, setMembersLoading] = useState(false);

    const [companyUsers, setCompanyUsers] = useState<Member[]>([]);
    const [selectedMembers, setSelectedMembers] = useState<string[]>([]);

    // Fetch basic group details on mount
    useEffect(() => {
        const fetchGroupDetails = async () => {
            try {
                const response = await axios.get(
                    `http://localhost:5045/api/groups/${groupId}`,
                    { headers: { Authorization: `Bearer ${localStorage.getItem("token")}` } }
                );
                setGroup(response.data);
            } catch (error: any) {
                console.error("Error fetching group details:", error);
                if (error.response && error.response.status === 404) {
                    navigate("/groups");
                }
            } finally {
                setLoading(false);
            }
        };
        fetchGroupDetails();
    }, [groupId, navigate]);

    // Dynamically fetch group members when "members" tab is active
    useEffect(() => {
        if (activeTab === "members") {
            const fetchMembers = async () => {
                setMembersLoading(true);
                try {
                    const response = await axios.get(
                        `http://localhost:5045/api/groups/${groupId}/members`,
                        { headers: { Authorization: `Bearer ${localStorage.getItem("token")}` } }
                    );
                    setMembers(response.data);
                } catch (error: any) {
                    console.error("Error fetching group members:", error);
                } finally {
                    setMembersLoading(false);
                }
            };
            fetchMembers();
        }
    }, [activeTab, groupId]);

    const removeMember = async (userId: string) => {
        const confirmRemove = window.confirm("Are you sure you want to remove this member from the group?");
        if (!confirmRemove) return;

        try {
            await axios.delete(
                `http://localhost:5045/api/groups/${groupId}/members/${userId}`,
                {
                    headers: { Authorization: `Bearer ${localStorage.getItem("token")}` },
                }
            );

            setMembers((prev) => prev.filter((m) => m.id !== userId));
            alert("Member removed successfully.");
        } catch (err) {
            console.error("Error removing member:", err);
            alert("Failed to remove member.");
        }
    };

    useEffect(() => {
        const fetchCompanyUsers = async () => {
            try {
                const userInfo = await axios.get("http://localhost:5045/api/users/me", {
                    headers: { Authorization: `Bearer ${localStorage.getItem("token")}` },
                });
                const companyId = userInfo.data.companyID;

                const usersRes = await axios.get(
                    `http://localhost:5045/api/users/company/${companyId}`,
                    { headers: { Authorization: `Bearer ${localStorage.getItem("token")}` } }
                );

                setCompanyUsers(usersRes.data);
            } catch (err) {
                console.error("Failed to fetch company users", err);
            }
        };

        fetchCompanyUsers();
    }, []);


    const assignMembers = async () => {
        try {
            await axios.post(
                `http://localhost:5045/api/groups/${groupId}/assign-members`,
                { memberIDs: selectedMembers },
                { headers: { Authorization: `Bearer ${localStorage.getItem("token")}` } }
            );
            alert("Members assigned successfully!");
            setSelectedMembers([]);

            //Refresh members list directly:
            const refreshed = await axios.get(
                `http://localhost:5045/api/groups/${groupId}/members`,
                { headers: { Authorization: `Bearer ${localStorage.getItem("token")}` } }
            );
            setMembers(refreshed.data);

        } catch (err) {
            console.error("Error assigning members", err);
            alert("Failed to assign members.");
        }
    };

    if (loading) {
        return (
            <div className="min-h-screen bg-gray-900 text-white flex items-center justify-center">
                Loading group details...
            </div>
        );
    }

    if (!group) {
        return (
            <div className="min-h-screen bg-gray-900 text-white flex items-center justify-center">
                Group not found.
            </div>
        );
    }

    return (
        <div className="min-h-screen bg-gray-900 text-white">
            <Navbar />
            <div className="p-6 bg-slate-800 border-b border-gray-700">
                <button
                    className="text-gray-400 mb-4 hover:text-white"
                    onClick={() => navigate("/groups")}
                >
                    &larr; Back to Groups
                </button>
                <h1 className="text-3xl font-bold">{group.groupName}</h1>
                <p className="mt-2">{group.description}</p>
                <div className="mt-4 grid grid-cols-2 gap-4">
                    <div>
                        <p>
                            <span className="font-semibold">Project Affiliation:</span>{" "}
                            {group.projectAffiliation.length > 0
                                ? group.projectAffiliation.join(", ")
                                : "N/A"}
                        </p>
                        <p>
                            <span className="font-semibold">Assigned Lead:</span>{" "}
                            {group.groupLeadName}
                        </p>
                    </div>
                    <div>
                        <p>
                            <span className="font-semibold">Current Progress:</span>{" "}
                            {group.currentProgress || "N/A"}
                        </p>
                        <p>
                            <span className="font-semibold">Status:</span>
                            <span
                                className={`ml-2 px-2 py-1 rounded ${group.status === "Active"
                                    ? "bg-green-600"
                                    : group.status === "Paused"
                                        ? "bg-yellow-600"
                                        : "bg-red-600"
                                    }`}
                            >
                                {group.status}
                            </span>
                        </p>
                    </div>
                </div>
            </div>

            {/* Tab Navigation */}
            <div className="flex justify-center bg-gray-800 p-4">
                {["tasks", "discussions", "members", "timeline"].map((tab) => (
                    <button
                        key={tab}
                        className={`px-4 py-2 mx-2 rounded ${activeTab === tab ? "bg-green-600" : "bg-gray-700 hover:bg-gray-600"
                            }`}
                        onClick={() => setActiveTab(tab)}
                    >
                        {tab.charAt(0).toUpperCase() + tab.slice(1)}
                    </button>
                ))}
            </div>

            {/* Tab Content */}
            <div className="p-6">
                {activeTab === "tasks" && (
                    <div>
                        <h2 className="text-xl font-bold mb-4">Group Tasks</h2>
                        {group.tasks.length === 0 ? (
                            <p>No tasks assigned to this group.</p>
                        ) : (
                            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
                                {group.tasks.map((task) => (
                                    <div
                                        key={task.taskID}
                                        className="p-4 bg-gray-700 rounded shadow-lg hover:bg-gray-600 cursor-pointer transition"
                                        onClick={() => navigate(`/tasks/${task.taskID}`)}
                                    >
                                        <h3 className="text-lg font-bold">{task.taskName}</h3>
                                        <p className="text-sm text-gray-400">
                                            🗓 Deadline: {new Date(task.deadline).toLocaleDateString()}
                                        </p>
                                        <p className="text-sm text-gray-400">
                                            📌 Priority: {task.priority}
                                        </p>
                                        <p className="text-sm text-gray-400">
                                            🚦 Status: {task.status}
                                        </p>
                                    </div>
                                ))}
                            </div>
                        )}
                    </div>
                )}
                {activeTab === "discussions" && (
                    <div>
                        <h2 className="text-xl font-bold mb-4">Discussions</h2>
                        {/* Fetch and display discussions dynamically if available */}
                        <p>Group discussions will be displayed here.</p>
                    </div>
                )}
                {activeTab === "members" && (
                    <div className="mt-4 space-y-10">
                        {/* 🧑‍🤝‍🧑 Show Current Members */}
                        <div>
                            <h3 className="text-xl font-semibold mb-4 border-b border-gray-600 pb-2">
                                Current Members:
                            </h3>
                            {membersLoading ? (
                                <p>Loading members...</p>
                            ) : members.length === 0 ? (
                                <p className="text-gray-400">No members assigned to this group.</p>
                            ) : (
                                <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4">
                                    {members.map((member) => (
                                        <div
                                            key={member.id}
                                            className="bg-gray-800 p-3 rounded-lg shadow hover:shadow-lg transition"
                                        >
                                            <p className="text-white font-medium">
                                                {member.firstName} {member.lastName}
                                            </p>
                                            <button
                                                onClick={() => removeMember(member.id)}
                                                className="text-red-400 hover:text-red-600 text-sm"
                                            >
                                                ❌ Remove
                                            </button>
                                        </div>
                                    ))}
                                </div>
                            )}
                        </div>

                        {/* ➕ Assign New Members */}
                        <div>
                            <h3 className="text-xl font-semibold mb-4 border-b border-gray-600 pb-2">
                                Assign New Members:
                            </h3>
                            <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-3 max-h-72 overflow-y-auto">
                                {companyUsers
                                    .filter(
                                        (user) => !members.some((member) => member.id === user.id)
                                    )
                                    .map((user) => (
                                        <label
                                            key={user.id}
                                            className="flex items-center gap-2 bg-gray-800 px-3 py-2 rounded hover:bg-gray-700 cursor-pointer"
                                        >
                                            <input
                                                type="checkbox"
                                                value={user.id}
                                                checked={selectedMembers.includes(user.id)}
                                                onChange={(e) => {
                                                    const isChecked = e.target.checked;
                                                    setSelectedMembers((prev) =>
                                                        isChecked
                                                            ? [...prev, user.id]
                                                            : prev.filter((id) => id !== user.id)
                                                    );
                                                }}
                                            />
                                            <span className="text-white">
                                                {user.firstName} {user.lastName}
                                            </span>
                                        </label>
                                    ))}
                            </div>
                            <button
                                className="mt-6 bg-green-600 px-6 py-2 rounded hover:bg-green-700"
                                onClick={assignMembers}
                                disabled={selectedMembers.length === 0}
                            >
                                ➕ Assign Selected
                            </button>
                        </div>
                    </div>
                )}
                {activeTab === "timeline" && (
                    <div>
                        <h2 className="text-xl font-bold mb-4">Timeline</h2>
                        {/* Fetch and display timeline data dynamically if available */}
                        <p>Group timeline and milestones will be displayed here.</p>
                    </div>
                )}
            </div>
        </div>
    );
};

export default GroupDetails;
