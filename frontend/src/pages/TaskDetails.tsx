import { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import axios from "axios";
import Navbar from "../components/Navbar";

interface GroupAssignedUser {
    id: string;
    firstName: string;
    lastName: string;
    groupName: string;
}

interface Task {
    taskID: number;
    taskName: string;
    description: string;
    deadline: string;
    priority: string;
    status: string;
    projectName: string;
    projectID: number;
    individualAssignedUsers: { id: string; firstName: string; lastName: string }[];
    groupAssignedUsers: GroupAssignedUser[];
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

    // All project members/groups
    const [projectMembers, setProjectMembers] = useState<Member[]>([]);
    const [projectGroups, setProjectGroups] = useState<Group[]>([]);

    // Selected for assignment
    const [selectedUsers, setSelectedUsers] = useState<string[]>([]);
    const [selectedGroups, setSelectedGroups] = useState<number[]>([]);

    // Editable task fields
    const [taskName, setTaskName] = useState("");
    const [deadline, setDeadline] = useState("");
    const [priority, setPriority] = useState("Medium");

    // Modal toggle
    const [showAssignModal, setShowAssignModal] = useState(false);

    useEffect(() => {
        if (taskId) {
            fetchTaskDetails(taskId);
        }
    }, [taskId]);

    const fetchTaskDetails = async (id: string) => {
        try {
            const response = await axios.get(
                `http://localhost:5045/api/tasks/${id}`,
                {
                    headers: { Authorization: `Bearer ${localStorage.getItem("token")}` },
                }
            );
            const taskData = response.data;
            setTask(taskData);
            setTaskName(taskData.taskName);

            // Convert deadline to "YYYY-MM-DD"
            if (taskData.deadline) {
                const parsedDate = new Date(taskData.deadline);
                const formattedDate = parsedDate.toISOString().split("T")[0];
                setDeadline(formattedDate);
            } else {
                setDeadline("");
            }

            setPriority(taskData.priority);

            if (taskData.projectID) {
                fetchProjectMembers(taskData.projectID);
                fetchProjectGroups(taskData.projectID);
            } else {
                console.error("Error: Task is missing projectID");
            }
        } catch (error) {
            console.error("Error fetching task details:", error);
            navigate("/projects");
        }
        setLoading(false);
    };

    const fetchProjectMembers = async (projectId: number) => {
        try {
            const response = await axios.get(
                `http://localhost:5045/api/users/project/${projectId}`,
                {
                    headers: { Authorization: `Bearer ${localStorage.getItem("token")}` },
                }
            );
            setProjectMembers(response.data);
        } catch (error) {
            console.error("Error fetching project members:", error);
        }
    };

    const fetchProjectGroups = async (projectId: number) => {
        try {
            const response = await axios.get(
                `http://localhost:5045/api/projects/${projectId}/groups`,
                {
                    headers: { Authorization: `Bearer ${localStorage.getItem("token")}` },
                }
            );
            setProjectGroups(response.data);
        } catch (error) {
            console.error("Error fetching project groups:", error);
        }
    };

    const handleUserCheckboxChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const { value, checked } = e.target;
        if (checked) {
            setSelectedUsers((prev) => [...prev, value]);
        } else {
            setSelectedUsers((prev) => prev.filter((id) => id !== value));
        }
    };

    const handleGroupCheckboxChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const { value, checked } = e.target;
        const groupId = parseInt(value, 10);
        if (checked) {
            setSelectedGroups((prev) => [...prev, groupId]);
        } else {
            setSelectedGroups((prev) => prev.filter((id) => id !== groupId));
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
                {
                    headers: { Authorization: `Bearer ${localStorage.getItem("token")}` },
                }
            );
            // Refresh
            fetchTaskDetails(taskId!);
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
                {
                    headers: { Authorization: `Bearer ${localStorage.getItem("token")}` },
                }
            );
            fetchTaskDetails(taskId!);
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
        return (
            <div className="min-h-screen bg-gray-900 text-white flex items-center justify-center">
                Loading...
            </div>
        );
    }

    if (!task) {
        return (
            <div className="min-h-screen bg-gray-900 text-white flex items-center justify-center">
                Task not found.
            </div>
        );
    }

    return (
        <div className="min-h-screen bg-gray-900 text-white">
            <Navbar userInfo={{ firstName: "User", lastName: "", role: "Developer" }} />

            {/* TOP SECTION */}
            <div className="p-6 bg-slate-800 border-b border-gray-700">
                <h1 className="text-2xl font-bold">
                    {task.projectName} / {task.taskName}
                </h1>

                {/* Editable Fields */}
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

                {/* Action Buttons */}
                <div className="flex items-center space-x-2 mt-4">
                    <button onClick={updateTask} className="bg-blue-600 px-4 py-2 rounded">
                        Update Task
                    </button>
                    <button onClick={deleteTask} className="bg-red-600 px-4 py-2 rounded">
                        Delete Task
                    </button>
                    <button
                        onClick={() => setShowAssignModal(true)}
                        className="bg-green-600 px-4 py-2 rounded"
                    >
                        ➕ Assign Members/Groups
                    </button>
                </div>
            </div>

            {/* BOTTOM SECTION: Display currently assigned users/groups */}
            <div className="p-6">
                <h3 className="text-xl font-bold">Assigned Users</h3>
                <ul className="list-disc pl-5">
                    {task.individualAssignedUsers.map(user => (
                        <li key={user.id}>{user.firstName} {user.lastName}</li>
                    ))}
                    {task.groupAssignedUsers.map(user => (
                        <li key={user.id}>
                            {user.firstName} {user.lastName} ({user.groupName})
                        </li>
                    ))}
                </ul>

                <h3 className="text-xl font-bold mt-6">Assigned Groups</h3>
                <ul className="list-disc pl-5">
                    {task.assignedGroups.map(group => (
                        <li key={group.groupID}>{group.groupName}</li>
                    ))}
                </ul>
            </div>

            {/* Single Assign Modal Block */}
            {showAssignModal && (
                <div className="fixed inset-0 flex items-center justify-center bg-black bg-opacity-50">
                    <div className="bg-gray-800 p-6 rounded shadow-lg w-1/3">
                        <h3 className="text-lg font-bold">Add Members to Project</h3>

                        {/* Bottom section: Display assigned users/groups */}
                        <div className="p-6">
                            <h3 className="text-xl font-bold">Assigned Users</h3>
                            <ul className="list-disc pl-5">
                                {task.individualAssignedUsers.map(user => (
                                    <li key={user.id}>{user.firstName} {user.lastName}</li>
                                ))}
                                {task.groupAssignedUsers.map(user => (
                                    <li key={user.id}>{user.firstName} {user.lastName} ({user.groupName})</li>
                                ))}
                            </ul>

                            <h3 className="text-xl font-bold mt-6">Assigned Groups</h3>
                            <ul className="list-disc pl-5">
                                {task.assignedGroups.map(group => (
                                    <li key={group.groupID}>{group.groupName}</li>
                                ))}
                            </ul>
                        </div>

                        {/* Modal Buttons */}
                        <div className="flex justify-end mt-4">
                            <button
                                onClick={assignUsersAndGroups}
                                className="bg-green-600 px-4 py-2 rounded mr-2"
                            >
                                Add Selected
                            </button>
                            <button
                                onClick={() => setShowAssignModal(false)}
                                className="bg-red-600 px-4 py-2 rounded"
                            >
                                Cancel
                            </button>
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
};

export default TaskDetails;
