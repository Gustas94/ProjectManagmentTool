import { useState, useEffect } from "react";
import { useNavigate, useSearchParams } from "react-router-dom";
import axios, { AxiosError } from "axios";

const Register = () => {
  const [searchParams] = useSearchParams();
  const inviteCode = searchParams.get("invite"); // Get invite code from URL
  const [formData, setFormData] = useState({
    firstName: "",
    lastName: "",
    email: "",
    password: "",
    confirmPassword: "",
  });
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const navigate = useNavigate();

  // Validate invite code when component mounts (if present)
  useEffect(() => {
    if (inviteCode) {
      axios
        .get(`/api/invitations/validate?inviteCode=${inviteCode}`)
        .then(() => console.log("Invite code is valid"))
        .catch(() => setError("Invalid or expired invite link."));
    }
  }, [inviteCode]);

  // Clear error on input change
  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setFormData({ ...formData, [e.target.name]: e.target.value });
    setError(null);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setIsSubmitting(true);

    if (formData.password !== formData.confirmPassword) {
      setError("Passwords do not match.");
      setIsSubmitting(false);
      return;
    }

    try {
      if (inviteCode) {
        // Registration via invitation
        await axios.post("/api/invitations/accept", {
          inviteCode, // Send the invite code
          ...formData,
        });
      } else {
        // Default registration (if no invite is present)
        await axios.post("/api/auth/register", {
          ...formData,
        });
      }
      navigate("/dashboard");
    } catch (err) {
      const axiosError = err as AxiosError<{ errors?: any; message?: string }>;
      if (axiosError.response?.data?.errors) {
        setError(Object.values(axiosError.response.data.errors).join(" "));
      } else if (axiosError.response?.data?.message) {
        setError(axiosError.response.data.message);
      } else {
        setError("Registration failed. Please try again.");
      }
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-gray-900 text-white">
      <form onSubmit={handleSubmit} className="p-6 bg-gray-800 rounded">
        <h2 className="text-2xl font-bold mb-4">Register</h2>

        {inviteCode && (
          <p className="text-green-400">
            You're joining via an invitation link.
          </p>
        )}

        {error && <p className="text-red-400 mt-2">{error}</p>}

        <input
          type="text"
          name="firstName"
          placeholder="First Name"
          value={formData.firstName}
          onChange={handleChange}
          className="w-full p-2 mt-2 bg-gray-700 rounded"
          required
        />
        <input
          type="text"
          name="lastName"
          placeholder="Last Name"
          value={formData.lastName}
          onChange={handleChange}
          className="w-full p-2 mt-2 bg-gray-700 rounded"
          required
        />
        <input
          type="email"
          name="email"
          placeholder="Email"
          value={formData.email}
          onChange={handleChange}
          className="w-full p-2 mt-2 bg-gray-700 rounded"
          required
        />
        <input
          type="password"
          name="password"
          placeholder="Password"
          value={formData.password}
          onChange={handleChange}
          className="w-full p-2 mt-2 bg-gray-700 rounded"
          required
        />
        <input
          type="password"
          name="confirmPassword"
          placeholder="Confirm Password"
          value={formData.confirmPassword}
          onChange={handleChange}
          className="w-full p-2 mt-2 bg-gray-700 rounded"
          required
        />

        <button
          type="submit"
          disabled={isSubmitting}
          className={`mt-4 bg-green-600 px-4 py-2 rounded ${
            isSubmitting ? "opacity-50 cursor-not-allowed" : ""
          }`}
        >
          {isSubmitting ? "Registering..." : "Register"}
        </button>
      </form>
    </div>
  );
};

export default Register;
