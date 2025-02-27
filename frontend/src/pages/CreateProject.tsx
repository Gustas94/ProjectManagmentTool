import { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import axios from "axios";
import Navbar from "../components/navbar";

const CreateProject = () => {
    const navigate = useNavigate();
    const [userInfo, setUserInfo] = useState({ firstName: "", lastName: "", role: "", companyID: null });
    const [users, setUsers] = useState<{ id: string; firstName: string; lastName: string }[]>([]);
    const [formData, setFormData] = useState({
        projectName: "",
        description: "",
        startDate: "",
        endDate: "",
        projectManagerID: "",
        visibility: "public",
    });

    // Fetch user data and company users
    useEffect(() => {
        const fetchUserInfo = async () => {
          try {
            const response = await axios.get("http://localhost:5045/api/users/me", {
              headers: { Authorization: `Bearer ${localStorage.getItem("token")}` },
              withCredentials: true,
            });
      
            setUserInfo(response.data);
      
            // ✅ Fetch company users only if companyID is valid
            if (response.data.companyID) {
              const usersResponse = await axios.get(`http://localhost:5045/api/users/company/${response.data.companyID}`, {
                headers: { Authorization: `Bearer ${localStorage.getItem("token")}` },
                withCredentials: true,
              });
      
              setUsers(usersResponse.data);
            }
          } catch (error) {
            console.error("Error fetching user data:", error);
          }
        };
      

        fetchUserInfo();
    }, []);

    // Handle form input changes
    const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement>) => {
        setFormData({ ...formData, [e.target.name]: e.target.value });
    };

    // Handle form submission
    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        try {
            await axios.post(
                "http://localhost:5045/api/projects/create",
                {
                    projectName: formData.projectName,
                    description: formData.description,
                    startDate: formData.startDate,
                    endDate: formData.endDate,
                    visibility: formData.visibility,
                    companyID: userInfo.companyID,  // ✅ Send as integer
                    projectManagerID: formData.projectManagerID,  // ✅ Send as string
                },
                {
                    headers: { Authorization: `Bearer ${localStorage.getItem("token")}` },
                }
            );
            navigate("/projects"); // Redirect to Projects page after creation
        } catch (error) {
            console.error("Error creating project:", error);
        }
    };

    return (
        <div className="min-h-screen bg-gray-900 text-white">
            <Navbar userInfo={userInfo} />

            <div className="max-w-3xl mx-auto mt-10 bg-slate-800 p-6 rounded-lg shadow-lg">
                <h1 className="text-2xl font-bold mb-4">Create New Project</h1>
                <form onSubmit={handleSubmit} className="space-y-4">
                    <div>
                        <label className="block text-gray-400">Project Name</label>
                        <input
                            type="text"
                            name="projectName"
                            value={formData.projectName}
                            onChange={handleChange}
                            className="w-full p-2 bg-gray-700 rounded"
                            required
                        />
                    </div>

                    <div>
                        <label className="block text-gray-400">Description</label>
                        <textarea
                            name="description"
                            value={formData.description}
                            onChange={handleChange}
                            className="w-full p-2 bg-gray-700 rounded"
                            required
                        />
                    </div>

                    <div className="flex gap-4">
                        <div className="flex-1">
                            <label className="block text-gray-400">Start Date</label>
                            <input
                                type="date"
                                name="startDate"
                                value={formData.startDate}
                                onChange={handleChange}
                                className="w-full p-2 bg-gray-700 rounded"
                                required
                            />
                        </div>
                        <div className="flex-1">
                            <label className="block text-gray-400">End Date</label>
                            <input
                                type="date"
                                name="endDate"
                                value={formData.endDate}
                                onChange={handleChange}
                                className="w-full p-2 bg-gray-700 rounded"
                                required
                            />
                        </div>
                    </div>

                    <div>
                        <label className="block text-gray-400">Project Manager</label>
                        <select
                            name="projectManagerID"
                            value={formData.projectManagerID}
                            onChange={handleChange}
                            className="w-full p-2 bg-gray-700 rounded"
                            required
                        >
                            <option value="">Select a Manager</option>
                            {users.map((user) => (
                                <option key={user.id} value={user.id}>
                                    {user.firstName} {user.lastName}
                                </option>
                            ))}
                        </select>
                    </div>

                    <div>
                        <label className="block text-gray-400">Visibility</label>
                        <select
                            name="visibility"
                            value={formData.visibility}
                            onChange={handleChange}
                            className="w-full p-2 bg-gray-700 rounded"
                        >
                            <option value="public">Public</option>
                            <option value="private">Private</option>
                        </select>
                    </div>

                    <button type="submit" className="w-full bg-green-600 p-2 rounded hover:bg-green-700">
                        Create Project
                    </button>
                </form>
            </div>
        </div>
    );
};

export default CreateProject;
