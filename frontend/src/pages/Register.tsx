import { useState, useEffect } from "react";
import { useNavigate, useSearchParams } from "react-router-dom";
import axios from "axios";

const Register = () => {
  const [searchParams] = useSearchParams();
  const inviteCode = searchParams.get("invite"); // ✅ Get invite code from URL
  const [formData, setFormData] = useState({
    firstName: "",
    lastName: "",
    email: "",
    password: "",
    confirmPassword: "",
  });
  const [error, setError] = useState<string | null>(null);
  const navigate = useNavigate();

  useEffect(() => {
    if (inviteCode) {
      console.log("Registering with invite:", inviteCode);
    }
  }, [inviteCode]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (formData.password !== formData.confirmPassword) {
      setError("Passwords do not match.");
      return;
    }

    try {
      await axios.post("/api/invitations/accept", {
        inviteCode, // ✅ Send invitation code
        ...formData,
      });

      navigate("/dashboard"); // ✅ Redirect after successful registration
    } catch (error) {
      setError("Registration failed. Please try again.");
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-gray-900 text-white">
      <form onSubmit={handleSubmit} className="p-6 bg-gray-800 rounded">
        <h2 className="text-2xl font-bold mb-4">Register</h2>

        {inviteCode && (
          <p className="text-green-400">You're joining via an invitation link.</p>
        )}

        <input type="text" placeholder="First Name" value={formData.firstName}
          onChange={(e) => setFormData({ ...formData, firstName: e.target.value })}
          className="w-full p-2 mt-2 bg-gray-700 rounded" required
        />
        <input type="text" placeholder="Last Name" value={formData.lastName}
          onChange={(e) => setFormData({ ...formData, lastName: e.target.value })}
          className="w-full p-2 mt-2 bg-gray-700 rounded" required
        />
        <input type="email" placeholder="Email" value={formData.email}
          onChange={(e) => setFormData({ ...formData, email: e.target.value })}
          className="w-full p-2 mt-2 bg-gray-700 rounded" required
        />
        <input type="password" placeholder="Password" value={formData.password}
          onChange={(e) => setFormData({ ...formData, password: e.target.value })}
          className="w-full p-2 mt-2 bg-gray-700 rounded" required
        />
        <input type="password" placeholder="Confirm Password" value={formData.confirmPassword}
          onChange={(e) => setFormData({ ...formData, confirmPassword: e.target.value })}
          className="w-full p-2 mt-2 bg-gray-700 rounded" required
        />

        {error && <p className="text-red-400">{error}</p>}

        <button type="submit" className="mt-4 bg-green-600 px-4 py-2 rounded">
          Register
        </button>
      </form>
    </div>
  );
};

export default Register;
