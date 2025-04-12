import { useNavigate } from "react-router-dom";

export default function Unauthorized() {
    const navigate = useNavigate();

    return (
        <div className="min-h-screen bg-gray-900 text-white flex items-center justify-center px-4">
            <div className="text-center max-w-md">
                <div className="text-6xl text-yellow-400 mb-4">⚠️</div>
                <h1 className="text-4xl font-bold mb-2">403 - Access Denied</h1>
                <p className="text-lg mb-6">You do not have permission to view this page.</p>
                <button
                    className="bg-blue-600 hover:bg-blue-700 px-5 py-2 rounded text-white font-semibold transition"
                    onClick={() => navigate("/dashboard")}
                >
                    Go Back Home
                </button>
            </div>
        </div>
    );
}
