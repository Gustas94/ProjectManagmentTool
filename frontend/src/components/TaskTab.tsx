import { useState, useEffect } from "react";
import axios from "axios";
import { useNavigate } from "react-router-dom";

// Define types if not imported from elsewhere
export interface Task {
    taskID: number;
    taskName: string;
    description: string;
    deadline: string;
    priority: string;
    status: string;
    assignedTo: string[]; // legacy, if needed
}

export interface Member {
    id: string;
    firstName: string;
    lastName: string;
}

interface TasksTabProps {
    projectId: string;
    tasks: Task[];
    setTasks: (value: Task[] | ((prev: Task[]) => Task[])) => void;
    members: Member[];
}

export interface ExtendedTask extends Task {
    // New fields for assignment by users and groups
    assignedUserIDs: string[];
    assignedGroupIDs: number[];
}

const TasksTab = ({ projectId, tasks, setTasks, members }: TasksTabProps) => {
    const navigate = useNavigate();
    const [showTaskModal, setShowTaskModal] = useState(false);

    // Pagination states for Members
    const [currentMemberPage, setCurrentMemberPage] = useState<number>(1);
    const membersPerPage = 20;
    const totalMemberPages = Math.ceil(members.length / membersPerPage);

    // Pagination states for Groups
    const [currentGroupPage, setCurrentGroupPage] = useState<number>(1);
    const groupsPerPage = 20;
    const [availableGroups, setAvailableGroups] = useState<{ groupID: number; groupName: string }[]>([]);
    const totalGroupPages = Math.ceil(availableGroups.length / groupsPerPage);

    // The new/extended task object that includes both user & group assignment
    const [newTask, setNewTask] = useState<ExtendedTask>({
        taskID: 0,
        taskName: "",
        description: "",
        deadline: "",
        priority: "Medium",
        status: "To Do",
        assignedTo: [],
        assignedUserIDs: [],
        assignedGroupIDs: [],
    });

    // Fetch available groups for assignment
    useEffect(() => {
        const fetchGroups = async () => {
            try {
                const response = await axios.get("http://localhost:5045/api/groups/all", {
                    headers: { Authorization: `Bearer ${localStorage.getItem("token")}` },
                });
                setAvailableGroups(
                    response.data.map((g: any) => ({
                        groupID: g.groupID,
                        groupName: g.groupName,
                    }))
                );
            } catch (error) {
                console.error("Error fetching groups:", error);
            }
        };
        fetchGroups();
    }, []);

    // Slice members and groups based on the current page
    const visibleMembers = members.slice(
        (currentMemberPage - 1) * membersPerPage,
        currentMemberPage * membersPerPage
    );
    const visibleGroups = availableGroups.slice(
        (currentGroupPage - 1) * groupsPerPage,
        currentGroupPage * groupsPerPage
    );

    const createTask = async () => {
        if (
            !newTask.taskName ||
            !newTask.deadline ||
            (newTask.assignedUserIDs.length === 0 && newTask.assignedGroupIDs.length === 0)
        ) {
            alert("Task Name, Deadline, and at least one assignee (individual or group) are required!");
            return;
        }
        try {
            const formattedDeadline = new Date(newTask.deadline).toISOString();
            const response = await axios.post(
                "http://localhost:5045/api/tasks/create",
                {
                    ...newTask,
                    deadline: formattedDeadline,
                    projectID: projectId,
                },
                { headers: { Authorization: `Bearer ${localStorage.getItem("token")}` } }
            );

            setTasks((prev: Task[]) => [
                ...prev,
                { ...newTask, taskID: response.data.taskId } as Task,
            ]);

            setNewTask({
                taskID: 0,
                taskName: "",
                description: "",
                deadline: "",
                priority: "Medium",
                status: "To Do",
                assignedTo: [],
                assignedUserIDs: [],
                assignedGroupIDs: [],
            });

            setShowTaskModal(false);
        } catch (error) {
            console.error("Error creating task:", error);
        }
    };

    return (
        <div>
            <h2 className="text-xl font-bold">Tasks</h2>
            <button
                className="bg-green-600 px-4 py-2 rounded mt-2"
                onClick={() => setShowTaskModal(true)}
            >
                ➕ Create Task
            </button>

            {/* Task Creation Modal */}
            {showTaskModal && (
                <div className="fixed inset-0 flex items-center justify-center bg-black bg-opacity-50 z-50">
                    <div className="bg-gray-800 w-full max-w-2xl mx-4 rounded shadow-lg p-6 space-y-6">
                        <h3 className="text-xl font-bold text-white">Create Task</h3>

                        {/* Task Name & Deadline */}
                        <div className="flex flex-col space-y-4 md:flex-row md:space-y-0 md:space-x-4">
                            {/* Task Name */}
                            <div className="flex-1">
                                <label className="block text-sm font-semibold mb-1 text-gray-200">
                                    Task Name
                                </label>
                                <input
                                    type="text"
                                    placeholder="e.g., Implement new feature"
                                    className="w-full p-2 rounded bg-gray-700 text-white focus:outline-none"
                                    value={newTask.taskName}
                                    onChange={(e) => setNewTask({ ...newTask, taskName: e.target.value })}
                                />
                            </div>

                            {/* Deadline */}
                            <div className="flex-1">
                                <label className="block text-sm font-semibold mb-1 text-gray-200">
                                    Deadline
                                </label>
                                <input
                                    type="date"
                                    className="w-full p-2 rounded bg-gray-700 text-white focus:outline-none"
                                    value={newTask.deadline}
                                    onChange={(e) => setNewTask({ ...newTask, deadline: e.target.value })}
                                />
                            </div>
                        </div>

                        {/* Priority Selection */}
                        <div className="mb-4">
                            <label className="block text-sm font-semibold mb-1 text-gray-200">
                                Priority
                            </label>
                            <select
                                className="w-full p-2 rounded bg-gray-700 text-white focus:outline-none"
                                value={newTask.priority}
                                onChange={(e) => setNewTask({ ...newTask, priority: e.target.value })}
                            >
                                <option value="High">High</option>
                                <option value="Medium">Medium</option>
                                <option value="Low">Low</option>
                            </select>
                        </div>

                        {/* Two-column layout for Individual Members and Groups */}
                        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                            {/* Individual Members */}
                            <div>
                                <label className="block text-sm font-semibold mb-1 text-gray-200">
                                    Assign Individual Members
                                </label>
                                <div className="border border-gray-600 rounded p-2 max-h-40 overflow-y-auto">
                                    {visibleMembers.map((member: Member) => (
                                        <div key={member.id} className="flex items-center gap-2 mb-1">
                                            <input
                                                type="checkbox"
                                                className="form-checkbox h-4 w-4 text-green-500"
                                                checked={newTask.assignedUserIDs.includes(member.id)}
                                                onChange={() =>
                                                    setNewTask((prev) => ({
                                                        ...prev,
                                                        assignedUserIDs: prev.assignedUserIDs.includes(member.id)
                                                            ? prev.assignedUserIDs.filter((uid) => uid !== member.id)
                                                            : [...prev.assignedUserIDs, member.id],
                                                    }))
                                                }
                                            />
                                            <label className="text-gray-200">
                                                {member.firstName} {member.lastName}
                                            </label>
                                        </div>
                                    ))}
                                </div>
                                {/* Prev/Next Buttons for Members */}
                                <div className="flex justify-between mt-2">
                                    <button
                                        onClick={() => setCurrentMemberPage((prev: number) => Math.max(prev - 1, 1))}
                                        className="bg-gray-600 text-white px-2 py-1 rounded hover:bg-gray-500"
                                        disabled={currentMemberPage <= 1}
                                    >
                                        ⬅ Prev
                                    </button>
                                    <button
                                        onClick={() => setCurrentMemberPage((prev: number) => prev + 1)}
                                        className="bg-gray-600 text-white px-2 py-1 rounded hover:bg-gray-500"
                                        disabled={currentMemberPage >= totalMemberPages}
                                    >
                                        Next ➡
                                    </button>
                                </div>
                            </div>

                            {/* Groups */}
                            <div>
                                <label className="block text-sm font-semibold mb-1 text-gray-200">
                                    Assign Groups
                                </label>
                                <div className="border border-gray-600 rounded p-2 max-h-40 overflow-y-auto">
                                    {visibleGroups.map((group) => (
                                        <div key={group.groupID} className="flex items-center gap-2 mb-1">
                                            <input
                                                type="checkbox"
                                                className="form-checkbox h-4 w-4 text-green-500"
                                                checked={newTask.assignedGroupIDs.includes(group.groupID)}
                                                onChange={() =>
                                                    setNewTask((prev) => ({
                                                        ...prev,
                                                        assignedGroupIDs: prev.assignedGroupIDs.includes(group.groupID)
                                                            ? prev.assignedGroupIDs.filter((gid) => gid !== group.groupID)
                                                            : [...prev.assignedGroupIDs, group.groupID],
                                                    }))
                                                }
                                            />
                                            <label className="text-gray-200">{group.groupName}</label>
                                        </div>
                                    ))}
                                </div>
                                {/* Prev/Next Buttons for Groups */}
                                <div className="flex justify-between mt-2">
                                    <button
                                        onClick={() => setCurrentGroupPage((prev: number) => Math.max(prev - 1, 1))}
                                        className="bg-gray-600 text-white px-2 py-1 rounded hover:bg-gray-500"
                                        disabled={currentGroupPage <= 1}
                                    >
                                        ⬅ Prev
                                    </button>
                                    <button
                                        onClick={() => setCurrentGroupPage((prev: number) => prev + 1)}
                                        className="bg-gray-600 text-white px-2 py-1 rounded hover:bg-gray-500"
                                        disabled={currentGroupPage >= totalGroupPages}
                                    >
                                        Next ➡
                                    </button>
                                </div>
                            </div>
                        </div>

                        {/* Action Buttons */}
                        <div className="flex justify-end space-x-2">
                            <button
                                onClick={() => setShowTaskModal(false)}
                                className="bg-red-600 text-white px-4 py-2 rounded hover:bg-red-500"
                            >
                                Cancel
                            </button>
                            <button
                                onClick={createTask}
                                className="bg-green-600 text-white px-4 py-2 rounded hover:bg-green-500"
                            >
                                Create Task
                            </button>
                        </div>
                    </div>
                </div>
            )}

            {/* Render existing tasks */}
            <div className="mt-4">
                {tasks.length === 0 ? (
                    <p className="text-gray-400">No tasks yet.</p>
                ) : (
                    <div className="grid grid-cols-5 gap-4">
                        {tasks.map((task) => (
                            <div
                                key={task.taskID}
                                className="p-3 bg-gray-700 rounded cursor-pointer hover:bg-gray-600"
                                onClick={() => navigate(`/tasks/${task.taskID}`)} // ✅ Navigation
                            >
                                <h3 className="font-bold text-white">{task.taskName}</h3>
                                <p className="text-sm text-gray-400">
                                    🗓 Deadline: {new Date(task.deadline).toLocaleDateString()}
                                </p>
                                <p className="text-sm text-gray-400">📌 Priority: {task.priority}</p>
                                <p className="text-sm text-gray-400">🚦 Status: {task.status}</p>
                            </div>
                        ))}
                    </div>
                )}
            </div>
        </div>
    );
};


export default TasksTab;
