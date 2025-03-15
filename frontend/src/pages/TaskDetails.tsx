import { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import axios from "axios";
import Navbar from "../components/Navbar";

interface Task {
    taskID: number;
    taskName: string;
    description: string;
    deadline: string;
    priority: string;
    status: string;
    projectName: string;
    assignedUsers: { id: string; firstName: string; lastName: string }[];
    assignedGroups: { groupID: number; groupName: string }[];
}

interface Member {
    id: string;
    firstName: string;
    lastName: string;
}

interface Group {
    groupID: number;
    groupName: string;
}

const TaskDetails = () => {
    const navigate = useNavigate();
    const { taskId } = useParams<{ taskId: string }>();
    const [task, setTask] = useState<Task | null>(null);
    const [loading, setLoading] = useState(true);
    const [projectMembers, setProjectMembers] = useState<Member[]>([]);
    const [projectGroups, setProjectGroups] = useState<Group[]>([]);
    const [selectedUsers, setSelectedUsers] = useState<string[]>([]);
    const [selectedGroups, setSelectedGroups] = useState<number[]>([]);
    const [taskName, setTaskName] = useState("");
    const [deadline, setDeadline] = useState("");
    const [priority, setPriority] = useState("Medium");
    const [showAssignModal, setShowAssignModal] = useState(false);

    useEffect(() => {
        fetchTaskDetails();
    }, [taskId]);

    const fetchTaskDetails = async () => {
        try {
            const response = await axios.get(`http://localhost:5045/api/tasks/${taskId}`, {
                headers: { Authorization: `Bearer ${localStorage.getItem("token")}` },
            });

            const taskData = response.data;
            setTask(taskData);
            setTaskName(taskData.taskName);
            setDeadline(taskData.deadline);
            setPriority(taskData.priority);

            if (taskData.projectId) {
                fetchProjectMembers(taskData.projectId);
                fetchProjectGroups(taskData.projectId);
            } else {
                console.error("Error: Task is missing projectId");
            }

        } catch (error) {
            console.error("Error fetching task details:", error);
            navigate("/projects");
        }
        setLoading(false);
    };

    const fetchProjectMembers = async (projectId: string) => {
        try {
            const response = await axios.get(`http://localhost:5045/api/users/project/${projectId}`, {
                headers: { Authorization: `Bearer ${localStorage.getItem("token")}` },
            });
            setProjectMembers(response.data);
        } catch (error) {
            console.error("Error fetching project members:", error);
        }
    };

    const fetchProjectGroups = async (projectId: string) => {
        try {
            const response = await axios.get(`http://localhost:5045/api/projects/${projectId}/groups`, {
                headers: { Authorization: `Bearer ${localStorage.getItem("token")}` },
            });
            setProjectGroups(response.data);
        } catch (error) {
            console.error("Error fetching project groups:", error);
        }
    };

    const assignUsersAndGroups = async () => {
        try {
            await axios.post(
                `http://localhost:5045/api/tasks/${taskId}/assign`,
                {
                    assignedUserIDs: selectedUsers,
                    assignedGroupIDs: selectedGroups,
                },
                { headers: { Authorization: `Bearer ${localStorage.getItem("token")}` } }
            );
            fetchTaskDetails();
            setShowAssignModal(false);
        } catch (error) {
            console.error("Error assigning users/groups:", error);
        }
    };

    const updateTask = async () => {
        try {
            await axios.put(
                `http://localhost:5045/api/tasks/${taskId}`,
                {
                    taskName,
                    deadline,
                    priority,
                },
                { headers: { Authorization: `Bearer ${localStorage.getItem("token")}` } }
            );
            fetchTaskDetails();
        } catch (error) {
            console.error("Error updating task:", error);
        }
    };

    const deleteTask = async () => {
        try {
            await axios.delete(`http://localhost:5045/api/tasks/${taskId}`, {
                headers: { Authorization: `Bearer ${localStorage.getItem("token")}` },
            });
            navigate("/projects");
        } catch (error) {
            console.error("Error deleting task:", error);
        }
    };

    if (loading) {
        return <div className="min-h-screen bg-gray-900 text-white flex items-center justify-center">Loading...</div>;
    }

    if (!task) {
        return <div className="min-h-screen bg-gray-900 text-white flex items-center justify-center">Task not found.</div>;
    }

    return (
        <div className="min-h-screen bg-gray-900 text-white">
            <Navbar userInfo={{ firstName: "User", lastName: "", role: "Developer" }} />

            <div className="p-6 bg-slate-800 border-b border-gray-700">
                <h1 className="text-2xl font-bold">{task.projectName} / {task.taskName}</h1>

                <div className="mt-4">
                    <label className="block text-gray-300">Task Name</label>
                    <input
                        type="text"
                        className="w-full p-2 bg-gray-700 text-white rounded"
                        value={taskName}
                        onChange={(e) => setTaskName(e.target.value)}
                    />
                </div>

                <div className="mt-4">
                    <label className="block text-gray-300">Deadline</label>
                    <input
                        type="date"
                        className="w-full p-2 bg-gray-700 text-white rounded"
                        value={deadline}
                        onChange={(e) => setDeadline(e.target.value)}
                    />
                </div>

                <div className="mt-4">
                    <label className="block text-gray-300">Priority</label>
                    <select
                        className="w-full p-2 bg-gray-700 text-white rounded"
                        value={priority}
                        onChange={(e) => setPriority(e.target.value)}
                    >
                        <option value="High">High</option>
                        <option value="Medium">Medium</option>
                        <option value="Low">Low</option>
                    </select>
                </div>

                <button onClick={updateTask} className="bg-blue-600 px-4 py-2 rounded mt-4">
                    Update Task
                </button>
                <button onClick={deleteTask} className="bg-red-600 px-4 py-2 rounded mt-4 ml-2">
                    Delete Task
                </button>
            </div>

            <div className="p-6">
                <button
                    onClick={() => setShowAssignModal(true)}
                    className="bg-green-600 px-4 py-2 rounded mt-2"
                >
                    ➕ Assign Members/Groups
                </button>
            </div>

            {/* Assign Modal */}
            {showAssignModal && (
                <div className="fixed inset-0 flex items-center justify-center bg-black bg-opacity-50">
                    <div className="bg-gray-800 p-6 rounded shadow-lg w-1/3">
                        <h3 className="text-lg font-bold">Assign Users & Groups</h3>

                        <label className="block mt-2">Select Users</label>
                        <select multiple className="w-full p-2 bg-gray-700 text-white rounded" onChange={(e) =>
                            setSelectedUsers([...e.target.selectedOptions].map((o) => o.value))}>
                            {projectMembers.map((member) => (
                                <option key={member.id} value={member.id}>
                                    {member.firstName} {member.lastName}
                                </option>
                            ))}
                        </select>

                        <label className="block mt-2">Select Groups</label>
                        <select multiple className="w-full p-2 bg-gray-700 text-white rounded" onChange={(e) =>
                            setSelectedGroups([...e.target.selectedOptions].map((o) => parseInt(o.value)))}>
                            {projectGroups.map((group) => (
                                <option key={group.groupID} value={group.groupID}>
                                    {group.groupName}
                                </option>
                            ))}
                        </select>

                        <button onClick={assignUsersAndGroups} className="bg-green-600 px-4 py-2 rounded mt-4">
                            ✅ Assign
                        </button>
                    </div>
                </div>
            )}
        </div>
    );
};

export default TaskDetails;
