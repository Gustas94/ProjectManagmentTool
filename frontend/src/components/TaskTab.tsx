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

export interface Group {
  groupID: number;
  groupName: string;
}

export interface ExtendedTask extends Task {
  assignedUserIDs: string[];
  assignedGroupIDs: number[];
}

interface TasksTabProps {
  projectId: string;
  tasks: Task[];
  setTasks: (value: Task[] | ((prev: Task[]) => Task[])) => void;
  members: Member[];
}

interface SubmittedSearch {
  searchTerm: string;
  statusFilter: string;
  priorityFilter: string;
  sortBy: string;
  sortOrder: string;
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
  const [availableGroups, setAvailableGroups] = useState<Group[]>([]);
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

  // States for search, filter, sort values (as entered by the user)
  const [searchTerm, setSearchTerm] = useState("");
  const [statusFilter, setStatusFilter] = useState("");
  const [priorityFilter, setPriorityFilter] = useState("");
  const [sortBy, setSortBy] = useState("taskName");
  const [sortOrder, setSortOrder] = useState("asc");
  const [page, setPage] = useState(1);
  const pageSize = 20;
  const [totalTasks, setTotalTasks] = useState(0);

  // This state stores the submitted search/filter/sort values.
  const [submittedSearch, setSubmittedSearch] = useState<SubmittedSearch>({
    searchTerm: "",
    statusFilter: "",
    priorityFilter: "",
    sortBy: "taskName",
    sortOrder: "asc",
  });

  // Fetch available groups for assignment
  useEffect(() => {
    const fetchGroups = async () => {
      try {
        const response = await axios.get("http://localhost:5045/api/groups/all", {
          headers: { Authorization: `Bearer ${localStorage.getItem("token")}` },
        });
        console.debug("Available groups fetched:", response.data);
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

  // Helper function to map group IDs to group names
  const getGroupNames = (groupIDs: number[]) => {
    return groupIDs
      .map((id) => {
        const group = availableGroups.find((g) => g.groupID === id);
        return group ? group.groupName : id;
      })
      .join(", ");
  };

  // Create Task function remains the same.
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
      // Convert deadline and projectId to proper formats
      const formattedDeadline = new Date(newTask.deadline).toISOString();
      const numericProjectId = parseInt(projectId, 10);

      console.debug("Creating task with data:", {
        ...newTask,
        deadline: formattedDeadline,
        ProjectID: numericProjectId,
      });
      const response = await axios.post(
        "http://localhost:5045/api/tasks/create",
        {
          ...newTask,
          deadline: formattedDeadline,
          ProjectID: numericProjectId, // using uppercase property if required by API DTO
        },
        { headers: { Authorization: `Bearer ${localStorage.getItem("token")}` } }
      );
      console.debug("Task creation response:", response.data);

      const createdTask = response.data.task;
      console.debug("Created task object:", createdTask);

      // Update tasks list using the correct property name from createdTask.
      setTasks((prev: Task[]) => [
        ...prev,
        {
          ...newTask,
          taskID: createdTask.taskID || createdTask.TaskID,
        } as ExtendedTask,
      ]);

      // Reset newTask state
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
      // Refresh filtered tasks after creation.
      fetchTasks();
    } catch (error) {
      console.error("Error creating task:", error);
    }
  };

  // Function to call API and fetch tasks using the submitted search/filter/sort values.
  const fetchTasks = async () => {
    try {
      const params = {
        searchTerm: submittedSearch.searchTerm,
        status: submittedSearch.statusFilter,
        priority: submittedSearch.priorityFilter,
        sortBy: submittedSearch.sortBy,
        sortOrder: submittedSearch.sortOrder,
        page: page.toString(),
        pageSize: pageSize.toString(),
      };
      const queryStr = new URLSearchParams(params).toString();
      const response = await axios.get(
        `http://localhost:5045/api/tasks/project/${projectId}/search?${queryStr}`,
        { headers: { Authorization: `Bearer ${localStorage.getItem("token")}` } }
      );
      console.debug("Filtered tasks:", response.data);
      setTasks(response.data.tasks);
      setTotalTasks(response.data.totalTasks);
    } catch (error) {
      console.error("Error fetching tasks:", error);
    }
  };

  // On component mount and whenever the submitted search values or page changes, fetch tasks.
  useEffect(() => {
    fetchTasks();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [submittedSearch, page]);

  // Handler for Search button click.
  const handleSearchClick = () => {
    // Update the submitted search values and reset page number to 1.
    setSubmittedSearch({
      searchTerm,
      statusFilter,
      priorityFilter,
      sortBy,
      sortOrder,
    });
    setPage(1);
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

      {/* Search / Filter / Sort UI */}
      <div className="mt-4 p-4 bg-gray-800 rounded">
        <h3 className="text-lg font-bold text-white mb-2">Search / Filter Tasks</h3>
        <div className="flex flex-wrap gap-4 items-end">
          <input
            type="text"
            className="p-2 border rounded bg-gray-700 text-white"
            placeholder="Search tasks..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
          />
          <select
            className="p-2 border rounded bg-gray-700 text-white"
            value={statusFilter}
            onChange={(e) => setStatusFilter(e.target.value)}
          >
            <option value="">All Status</option>
            <option value="To Do">To Do</option>
            <option value="In Progress">In Progress</option>
            <option value="Testing">Testing</option>
            <option value="Completed">Completed</option>
          </select>
          <select
            className="p-2 border rounded bg-gray-700 text-white"
            value={priorityFilter}
            onChange={(e) => setPriorityFilter(e.target.value)}
          >
            <option value="">All Priority</option>
            <option value="High">High</option>
            <option value="Medium">Medium</option>
            <option value="Low">Low</option>
          </select>
          <select
            className="p-2 border rounded bg-gray-700 text-white"
            value={sortBy}
            onChange={(e) => setSortBy(e.target.value)}
          >
            <option value="taskName">Name</option>
            <option value="deadline">Deadline</option>
            <option value="priority">Priority</option>
          </select>
          <select
            className="p-2 border rounded bg-gray-700 text-white"
            value={sortOrder}
            onChange={(e) => setSortOrder(e.target.value)}
          >
            <option value="asc">Ascending</option>
            <option value="desc">Descending</option>
          </select>
          {/* Search button */}
          <button
            onClick={handleSearchClick}
            className="bg-blue-600 text-white px-4 py-2 rounded hover:bg-blue-500"
          >
            Search
          </button>
        </div>
      </div>

      {/* Tasks List */}
      <div className="mt-4">
        {tasks.length === 0 ? (
          <p className="text-gray-400">No tasks yet.</p>
        ) : (
          <div className="grid grid-cols-5 gap-4">
            {tasks.map((taskItem) => {
              // Cast taskItem to ExtendedTask if possible
              const extTask = taskItem as ExtendedTask;
              // Use fallback: if taskItem.taskID is undefined, try taskItem.TaskID
              const id = taskItem.taskID || (taskItem as any).TaskID;
              return (
                <div
                  key={id}
                  className="p-3 bg-gray-700 rounded cursor-pointer hover:bg-gray-600"
                  onClick={() => {
                    const taskId = taskItem.taskID || (taskItem as any).TaskID;
                    console.debug("Navigating to task detail for:", taskItem, "Using ID:", taskId);
                    navigate(`/tasks/${taskId}`);
                  }}
                >
                  <h3 className="font-bold text-white">{taskItem.taskName}</h3>
                  <p className="text-sm text-gray-400">
                    🗓 Deadline: {new Date(taskItem.deadline).toLocaleDateString()}
                  </p>
                  <p className="text-sm text-gray-400">📌 Priority: {taskItem.priority}</p>
                  <p className="text-sm text-gray-400">🚦 Status: {taskItem.status}</p>
                  {extTask.assignedGroupIDs && extTask.assignedGroupIDs.length > 0 && (
                    <p className="text-sm text-gray-400">
                      Groups: {getGroupNames(extTask.assignedGroupIDs)}
                    </p>
                  )}
                </div>
              );
            })}
          </div>
        )}
      </div>

      {/* Pagination controls for tasks list */}
      {tasks.length > 0 && (
        <div className="flex justify-between mt-4">
          <button
            onClick={() => setPage((prev) => Math.max(prev - 1, 1))}
            disabled={page <= 1}
            className="bg-gray-600 px-2 py-1 rounded hover:bg-gray-500 text-white"
          >
            Prev
          </button>
          <span className="text-gray-300">
            Page {page} of {Math.ceil(totalTasks / pageSize)}
          </span>
          <button
            onClick={() => setPage((prev) => prev + 1)}
            disabled={page >= Math.ceil(totalTasks / pageSize)}
            className="bg-gray-600 px-2 py-1 rounded hover:bg-gray-500 text-white"
          >
            Next
          </button>
        </div>
      )}

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

            {/* Description Field */}
            <div className="mb-4">
              <label className="block text-sm font-semibold mb-1 text-gray-200">
                Description
              </label>
              <textarea
                className="w-full p-2 rounded bg-gray-700 text-white focus:outline-none"
                rows={3}
                value={newTask.description}
                onChange={(e) => setNewTask({ ...newTask, description: e.target.value })}
              />
            </div>

            {/* Status Field */}
            <div className="mb-4">
              <label className="block text-sm font-semibold mb-1 text-gray-200">
                Status
              </label>
              <select
                className="w-full p-2 rounded bg-gray-700 text-white focus:outline-none"
                value={newTask.status}
                onChange={(e) => setNewTask({ ...newTask, status: e.target.value })}
              >
                <option value="To Do">To Do</option>
                <option value="In Progress">In Progress</option>
                <option value="Testing">Testing</option>
                <option value="Completed">Completed</option>
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
    </div>
  );
};

export default TasksTab;
