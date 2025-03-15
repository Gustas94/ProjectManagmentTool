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

    if (loading) {
        return <div className="min-h-screen bg-gray-900 text-white flex items-center justify-center">Loading group details...</div>;
    }

    if (!group) {
        return <div className="min-h-screen bg-gray-900 text-white flex items-center justify-center">Group not found.</div>;
    }

    return (
        <div className="min-h-screen bg-gray-900 text-white">
            <Navbar userInfo={{ firstName: "User", lastName: "", role: "CEO" }} />
            <div className="p-6 bg-slate-800 border-b border-gray-700">
                <button className="text-gray-400 mb-4 hover:text-white" onClick={() => navigate("/groups")}>
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
                        <p><span className="font-semibold">Assigned Lead:</span> {group.groupLeadName}</p>
                    </div>
                    <div>
                        <p><span className="font-semibold">Current Progress:</span> {group.currentProgress || "N/A"}</p>
                        <p><span className="font-semibold">Status:</span>
                            <span className={`ml-2 px-2 py-1 rounded ${group.status === "Active"
                                    ? "bg-green-600"
                                    : group.status === "Paused"
                                        ? "bg-yellow-600"
                                        : "bg-red-600"
                                }`}>
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
                                    <div key={task.taskID} className="p-4 bg-gray-700 rounded shadow-lg">
                                        <h3 className="text-lg font-bold">{task.taskName}</h3>
                                        <p className="text-sm text-gray-400">🗓 Deadline: {new Date(task.deadline).toLocaleDateString()}</p>
                                        <p className="text-sm text-gray-400">📌 Priority: {task.priority}</p>
                                        <p className="text-sm text-gray-400">🚦 Status: {task.status}</p>
                                    </div>
                                ))}
                            </div>
                        )}
                    </div>
                )}
                {activeTab === "discussions" && (
                    <div>
                        <h2 className="text-xl font-bold mb-4">Discussions</h2>
                        <p>Group discussions will be displayed here.</p>
                    </div>
                )}
                {activeTab === "members" && (
                    <div>
                        <h2 className="text-xl font-bold mb-4">Group Members</h2>
                        <p>List of group members will be displayed here.</p>
                    </div>
                )}
                {activeTab === "timeline" && (
                    <div>
                        <h2 className="text-xl font-bold mb-4">Timeline</h2>
                        <p>Group timeline and milestones will be displayed here.</p>
                    </div>
                )}
            </div>
        </div>
    );
};

export default GroupDetails;
