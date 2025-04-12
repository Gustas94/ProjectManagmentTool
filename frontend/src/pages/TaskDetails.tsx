import { useEffect, useState, useContext } from "react";
import { useNavigate, useParams } from "react-router-dom";
import axios from "axios";
import Navbar from "../components/Navbar";
import { UserContext } from "../contexts/UserContext"; // Adjust the path as needed

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

// Combined type includes a flag if the user was assigned directly.
type CombinedAssignedUser = {
    id: string;
    firstName: string;
    lastName: string;
    groups: string[];
    directly?: boolean;
};

const TaskDetails = () => {
    const navigate = useNavigate();
    const { taskId } = useParams<{ taskId: string }>();
    const [task, setTask] = useState<Task | null>(null);
    const [loading, setLoading] = useState(true);

    // All project members/groups
    const [projectMembers, setProjectMembers] = useState<Member[]>([]);
    const [projectGroups, setProjectGroups] = useState<Group[]>([]);

    // Selected for assignment (used in modal)
    const [selectedUsers, setSelectedUsers] = useState<string[]>([]);
    const [selectedGroups, setSelectedGroups] = useState<number[]>([]);

    // Editable task fields
    const [taskName, setTaskName] = useState("");
    const [description, setDescription] = useState("");
    const [deadline, setDeadline] = useState("");
    const [priority, setPriority] = useState("Medium");
    const [status, setStatus] = useState("To Do");

    // Modal toggle
    const [showAssignModal, setShowAssignModal] = useState(false);

    const { user: currentUser } = useContext(UserContext);

    // Get the current user from context
    const { user } = useContext(UserContext);

    useEffect(() => {
        if (taskId) {
            fetchTaskDetails(taskId);
        }
    }, [taskId]);

    const fetchTaskDetails = async (id: string) => {
        try {
            const response = await axios.get(`http://localhost:5045/api/tasks/${id}`, {
                headers: { Authorization: `Bearer ${localStorage.getItem("token")}` },
            });
            const taskData = response.data;
            setTask(taskData);
            setTaskName(taskData.taskName);
            setDescription(taskData.description);
            setStatus(taskData.status);

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

    const markAsCompleted = async () => {
        try {
            await axios.put(
                `http://localhost:5045/api/tasks/${taskId}`,
                {
                    taskName,
                    description,
                    deadline,
                    priority,
                    status: "Completed",
                    projectID: task?.projectID,
                    assignedUserIDs: task?.individualAssignedUsers?.map(user => user.id) || [],
                    assignedGroupIDs: task?.assignedGroups?.map(group => group.groupID) || []
                },
                {
                    headers: { Authorization: `Bearer ${localStorage.getItem("token")}` },
                }
            );
            fetchTaskDetails(taskId!);
        } catch (error) {
            console.error("Failed to mark task as completed", error);
        }
    };

    const fetchProjectMembers = async (projectId: number) => {
        try {
            const response = await axios.get(`http://localhost:5045/api/users/project/${projectId}`, {
                headers: { Authorization: `Bearer ${localStorage.getItem("token")}` },
            });
            setProjectMembers(response.data);
        } catch (error) {
            console.error("Error fetching project members:", error);
        }
    };

    const fetchProjectGroups = async (projectId: number) => {
        try {
            const response = await axios.get(`http://localhost:5045/api/projects/${projectId}/groups`, {
                headers: { Authorization: `Bearer ${localStorage.getItem("token")}` },
            });
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
            fetchTaskDetails(taskId!);
            setSelectedUsers([]);
            setSelectedGroups([]);
            setShowAssignModal(false);
        } catch (error) {
            console.error("Error assigning users/groups:", error);
        }
    };

    const removeUserFromTask = async (userId: string) => {
        try {
            await axios.delete(`http://localhost:5045/api/tasks/${taskId}/assign/user/${userId}`, {
                headers: { Authorization: `Bearer ${localStorage.getItem("token")}` },
            });
            fetchTaskDetails(taskId!);
        } catch (error) {
            console.error("Error removing direct user assignment:", error);
        }
    };

    const removeGroupFromTask = async (groupId: number) => {
        try {
            await axios.delete(`http://localhost:5045/api/tasks/${taskId}/assign/group/${groupId}`, {
                headers: { Authorization: `Bearer ${localStorage.getItem("token")}` },
            });
            fetchTaskDetails(taskId!);
        } catch (error) {
            console.error("Error removing group from task:", error);
        }
    };

    const updateTask = async () => {
        try {
            await axios.put(
                `http://localhost:5045/api/tasks/${taskId}`,
                {
                    taskName,
                    description,
                    deadline,
                    priority,
                    status,
                    projectID: task?.projectID,
                    assignedUserIDs: task?.individualAssignedUsers?.map(user => user.id) || [],
                    assignedGroupIDs: task?.assignedGroups?.map(group => group.groupID) || []
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
        if (window.confirm("Are you sure you want to delete this task? This action cannot be undone.")) {
            try {
                await axios.delete(`http://localhost:5045/api/tasks/${taskId}`, {
                    headers: { Authorization: `Bearer ${localStorage.getItem("token")}` },
                });
                navigate("/projects");
            } catch (error) {
                console.error("Error deleting task:", error);
            }
        }
    };

    const goToProjectDetails = () => {
        if (task?.projectID) {
            navigate(`/projects/${task.projectID}`);
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

    // Build combined assigned users: those assigned directly are marked with directly=true,
    // then merge in groupAssignedUsers.
    const combined: { [id: string]: CombinedAssignedUser } = {};
    task.individualAssignedUsers.forEach((user) => {
        combined[user.id] = {
            id: user.id,
            firstName: user.firstName,
            lastName: user.lastName,
            groups: [],
            directly: true,
        };
    });
    task.groupAssignedUsers.forEach((user) => {
        if (combined[user.id]) {
            if (!combined[user.id].groups.includes(user.groupName)) {
                combined[user.id].groups.push(user.groupName);
            }
        } else {
            combined[user.id] = {
                id: user.id,
                firstName: user.firstName,
                lastName: user.lastName,
                groups: [user.groupName],
            };
        }
    });
    const combinedAssignedUsers: CombinedAssignedUser[] = Object.values(combined);

    return (
        <div className="min-h-screen bg-gray-900 text-white">
            <Navbar />

            {/* Header Section */}
            <div className="p-6 bg-slate-800 border-b border-gray-700">
                <h1 className="text-2xl font-bold">
                    <span onClick={goToProjectDetails} className="cursor-pointer hover:text-blue-400">
                        {task.projectName}
                    </span>{" "}
                    / {task.taskName}
                </h1>
                <div className="mt-4 grid grid-cols-1 md:grid-cols-3 gap-4">
                    <div>
                        <label className="block text-gray-300">Task Name</label>
                        <input
                            type="text"
                            className="w-full p-2 bg-gray-700 text-white rounded"
                            value={taskName}
                            onChange={(e) => setTaskName(e.target.value)}
                            disabled={!user?.permissions?.includes("EDIT_TASK")}
                        />
                    </div>
                    <div>
                        <label className="block text-gray-300">Deadline</label>
                        <input
                            type="date"
                            className="w-full p-2 bg-gray-700 text-white rounded"
                            value={deadline}
                            onChange={(e) => setDeadline(e.target.value)}
                            disabled={!user?.permissions?.includes("EDIT_TASK")}
                        />
                    </div>
                    <div>
                        <label className="block text-gray-300">Priority</label>
                        <select
                            className="w-full p-2 bg-gray-700 text-white rounded"
                            value={priority}
                            onChange={(e) => setPriority(e.target.value)}
                            disabled={!user?.permissions?.includes("EDIT_TASK")}
                        >
                            <option value="High">High</option>
                            <option value="Medium">Medium</option>
                            <option value="Low">Low</option>
                        </select>
                    </div>
                </div>
                {/* Row for Description and Status */}
                <div className="mt-4 grid grid-cols-1 md:grid-cols-2 gap-4">
                    <div>
                        <label className="block text-gray-300">Description</label>
                        <textarea
                            className="w-full p-2 bg-gray-700 text-white rounded resize-none"
                            rows={3}
                            value={description}
                            onChange={(e) => setDescription(e.target.value)}
                            disabled={!user?.permissions?.includes("EDIT_TASK")}
                        ></textarea>
                    </div>
                    <div>
                        <label className="block text-gray-300">Status</label>
                        <select
                            className="w-full p-2 bg-gray-700 text-white rounded"
                            value={status}
                            onChange={(e) => setStatus(e.target.value)}
                            disabled={!user?.permissions?.includes("EDIT_TASK")}
                        >
                            <option value="To Do">To Do</option>
                            <option value="In Progress">In Progress</option>
                            <option value="Testing">Testing</option>
                            <option value="Completed">Completed</option>
                        </select>
                    </div>
                </div>
                {/* Action Buttons - Render based on permissions */}
                <div className="flex items-center space-x-2 mt-4">
                    {user && user.permissions?.includes("EDIT_TASK") && (
                        <button onClick={updateTask} className="bg-blue-600 hover:bg-blue-700 px-4 py-2 rounded">
                            Update Task
                        </button>
                    )}
                    {user && user.permissions?.includes("DELETE_TASK") && (
                        <button onClick={deleteTask} className="bg-red-600 hover:bg-red-700 px-4 py-2 rounded">
                            Delete Task
                        </button>
                    )}
                    {user && user.permissions?.includes("ASSIGN_TASK") && (
                        <button onClick={() => setShowAssignModal(true)} className="bg-green-600 hover:bg-green-700 px-4 py-2 rounded">
                            ➕ Assign Members/Groups
                        </button>
                    )}
                    {user && user.permissions?.includes("COMPLETE_TASK") && status !== "Completed" && (
                        <button
                            onClick={markAsCompleted}
                            className="bg-purple-600 hover:bg-purple-700 px-4 py-2 rounded"
                        >
                            ✅ Mark as Completed
                        </button>
                    )}
                </div>
            </div>

            {/* Assigned Users & Groups */}
            <div className="p-6 grid grid-cols-1 md:grid-cols-2 gap-6">
                <div>
                    <h3 className="text-xl font-bold mb-2">Assigned Users</h3>
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                        {combinedAssignedUsers.map((assignedUser) => {
                            const labelParts: string[] = [];
                            if (assignedUser.directly) {
                                labelParts.push("Directly");
                            }
                            if (assignedUser.groups && assignedUser.groups.length > 0) {
                                assignedUser.groups.forEach((group) => {
                                    labelParts.push(group);
                                });
                            }
                            const finalLabel = labelParts.join(", ");
                            return (
                                <div key={assignedUser.id} className="bg-gray-700 p-4 rounded shadow">
                                    <div className="flex justify-between items-center">
                                        <p className="font-bold">
                                            {assignedUser.firstName} {assignedUser.lastName}
                                        </p>
                                        {/* Use currentUser from context (assume you have it via useContext(UserContext)) */}
                                        {assignedUser.directly && currentUser && currentUser.permissions?.includes("ASSIGN_TASK") && (
                                            <button
                                                onClick={() => removeUserFromTask(assignedUser.id)}
                                                className="bg-red-600 hover:bg-red-700 px-2 py-1 rounded text-xs"
                                            >
                                                Remove Direct
                                            </button>
                                        )}
                                    </div>
                                    {labelParts.length > 0 && (
                                        <p className="text-sm text-gray-300">({finalLabel})</p>
                                    )}
                                </div>
                            );
                        })}

                    </div>
                </div>
                <div>
                    <h3 className="text-xl font-bold mb-2">Assigned Groups</h3>
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                        {task.assignedGroups.map((group) => (
                            <div key={group.groupID} className="bg-gray-700 p-4 rounded shadow flex justify-between items-center">
                                <p className="font-bold">{group.groupName}</p>
                                {user && user.permissions?.includes("ASSIGN_TASK") && (
                                    <button
                                        onClick={() => removeGroupFromTask(group.groupID)}
                                        className="bg-red-600 hover:bg-red-700 px-2 py-1 rounded text-xs"
                                    >
                                        Remove
                                    </button>
                                )}
                            </div>
                        ))}
                    </div>
                </div>
            </div>

            {/* Assignment Modal */}
            {showAssignModal && (
                <div className="fixed inset-0 z-50 bg-black bg-opacity-50 flex items-center justify-center">
                    <div className="bg-gray-800 rounded-lg p-6 w-full max-w-md border border-gray-600">
                        <h2 className="text-xl font-bold text-white mb-4">Assign Users & Groups</h2>
                        <div className="mb-4">
                            <h3 className="text-lg font-semibold text-white">Select Users</h3>
                            <div className="max-h-40 overflow-y-auto mt-2 grid grid-cols-1 gap-2">
                                {projectMembers.map((member) => (
                                    <label key={member.id} className="flex items-center gap-2 bg-gray-700 p-2 rounded">
                                        <input
                                            type="checkbox"
                                            value={member.id}
                                            checked={selectedUsers.includes(member.id)}
                                            onChange={handleUserCheckboxChange}
                                        />
                                        <span>
                                            {member.firstName} {member.lastName}
                                        </span>
                                    </label>
                                ))}
                            </div>
                        </div>
                        <div className="mb-4">
                            <h3 className="text-lg font-semibold text-white">Select Groups</h3>
                            <div className="max-h-40 overflow-y-auto mt-2 grid grid-cols-1 gap-2">
                                {projectGroups.map((group) => (
                                    <label key={group.groupID} className="flex items-center gap-2 bg-gray-700 p-2 rounded">
                                        <input
                                            type="checkbox"
                                            value={group.groupID}
                                            checked={selectedGroups.includes(group.groupID)}
                                            onChange={handleGroupCheckboxChange}
                                        />
                                        <span>{group.groupName}</span>
                                    </label>
                                ))}
                            </div>
                        </div>
                        <div className="flex justify-end gap-2 mt-4">
                            <button onClick={() => setShowAssignModal(false)} className="bg-gray-600 hover:bg-gray-700 px-4 py-2 rounded">
                                Cancel
                            </button>
                            <button onClick={assignUsersAndGroups} className="bg-green-600 hover:bg-green-700 px-4 py-2 rounded">
                                Add Selected
                            </button>
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
};

export default TaskDetails;
