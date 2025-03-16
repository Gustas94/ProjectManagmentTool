import { useState } from 'react';
import axios, { AxiosError } from 'axios';
import { useNavigate } from 'react-router-dom';

const CreateCompany = () => {
    const [companyName, setCompanyName] = useState('');
    const [industry, setIndustry] = useState('');
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [confirmPassword, setConfirmPassword] = useState('');
    const [firstName, setFirstName] = useState('');
    const [lastName, setLastName] = useState('');
    const [showPassword, setShowPassword] = useState(false);
    const [showConfirmPassword, setShowConfirmPassword] = useState(false);
    const [error, setError] = useState('');
    const [success, setSuccess] = useState('');
    const navigate = useNavigate();
    const [isSubmitting, setIsSubmitting] = useState(false);

    const handleCreateCompany = async (e: React.FormEvent) => {
        e.preventDefault();
        
        if (isSubmitting) return; // Prevent multiple clicks
        setIsSubmitting(true);  // Disable further submissions
    
        setError('');
        setSuccess('');
    
        if (password !== confirmPassword) {
            setError('Passwords do not match');
            setIsSubmitting(false); // Allow retry
            return;
        }
    
        try {
            // 🔹 Step 1: Register the CEO User
            const userResponse = await axios.post('/api/auth/register', {
                email,
                password,
                firstName,
                lastName
            });
    
            const userId = userResponse.data.userId; // Get CEO ID from API response
    
            // 🔹 Step 2: Create the Company with CEO ID
            await axios.post('/api/company/create', {
                companyName,
                industry,
                email, // CEO's email
                firstName,
                lastName,
                password
            });
    
            setSuccess('Company and CEO account created successfully!');
            navigate('/dashboard');
        } catch (err: unknown) {
            const error = err as AxiosError<{ message?: string; errors?: Record<string, string[]> }>;
            console.error("Error:", error.response);
    
            if (error.response?.data?.errors) {
                const messages = Object.values(error.response.data.errors).flat().join(', ');
                setError(messages);
            } else {
                setError(error.response?.data?.message || 'Failed to create company or user');
            }
        } finally {
            setIsSubmitting(false); // Allow retry after error
        }
    };

    return (
        <div className="flex justify-center items-center min-h-screen border-gray-500">
            <div className="bg-slate-800 p-8 rounded-lg shadow-lg w-full max-w-sm border border-slate-800">
                <h2 className="text-2xl font-bold text-white mb-6 text-center">Create Company</h2>
                {error && <p className="text-red-500 text-sm text-center mb-2">{error}</p>}
                {success && <p className="text-green-500 text-sm text-center mb-2">{success}</p>}

                <form onSubmit={handleCreateCompany} className="flex flex-col gap-4">
                    <div>
                        <label className="block text-gray-400 text-sm font-medium mb-1">Company Name</label>
                        <input
                            type="text"
                            className="w-full p-3 rounded bg-gray-800 text-white border border-gray-700 focus:outline-none focus:ring-2 focus:ring-blue-500"
                            placeholder="Enter company name"
                            value={companyName}
                            onChange={(e) => setCompanyName(e.target.value)}
                        />
                    </div>
                    <div>
                        <label className="block text-gray-400 text-sm font-medium mb-1">Industry</label>
                        <input
                            type="text"
                            className="w-full p-3 rounded bg-gray-800 text-white border border-gray-700 focus:outline-none focus:ring-2 focus:ring-blue-500"
                            placeholder="Enter industry"
                            value={industry}
                            onChange={(e) => setIndustry(e.target.value)}
                        />
                    </div>
                    <div>
                        <label className="block text-gray-400 text-sm font-medium mb-1">Email</label>
                        <input
                            type="email"
                            className="w-full p-3 rounded bg-gray-800 text-white border border-gray-700 focus:outline-none focus:ring-2 focus:ring-blue-500"
                            placeholder="Enter your email"
                            value={email}
                            onChange={(e) => setEmail(e.target.value)}
                        />
                    </div>
                    <div>
                        <label className="block text-gray-400 text-sm font-medium mb-1">First Name</label>
                        <input
                            type="text"
                            className="w-full p-3 rounded bg-gray-800 text-white border border-gray-700 focus:outline-none focus:ring-2 focus:ring-blue-500"
                            placeholder="Enter your first name"
                            value={firstName}
                            onChange={(e) => setFirstName(e.target.value)}
                        />
                    </div>
                    <div>
                        <label className="block text-gray-400 text-sm font-medium mb-1">Last Name</label>
                        <input
                            type="text"
                            className="w-full p-3 rounded bg-gray-800 text-white border border-gray-700 focus:outline-none focus:ring-2 focus:ring-blue-500"
                            placeholder="Enter your last name"
                            value={lastName}
                            onChange={(e) => setLastName(e.target.value)}
                        />
                    </div>
                    <div>
                        <label className="block text-gray-400 text-sm font-medium mb-1">Password</label>
                        <div className="relative">
                            <input
                                type={showPassword ? "text" : "password"}
                                className="w-full p-3 rounded bg-gray-800 text-white border border-gray-700 focus:outline-none focus:ring-2 focus:ring-blue-500 pr-12"
                                placeholder="Enter your password"
                                value={password}
                                onChange={(e) => setPassword(e.target.value)}
                            />
                            <button
                                type="button"
                                onClick={() => setShowPassword(!showPassword)}
                                className="absolute inset-y-0 right-3 flex items-center text-gray-400 hover:text-gray-300"
                            >
                                {showPassword ? "👁️" : "🔒"}
                            </button>
                        </div>
                    </div>
                    <div>
                        <label className="block text-gray-400 text-sm font-medium mb-1">Confirm Password</label>
                        <div className="relative">
                            <input
                                type={showConfirmPassword ? "text" : "password"}
                                className="w-full p-3 rounded bg-gray-800 text-white border border-gray-700 focus:outline-none focus:ring-2 focus:ring-blue-500 pr-12"
                                placeholder="Confirm password"
                                value={confirmPassword}
                                onChange={(e) => setConfirmPassword(e.target.value)}
                            />
                            <button
                                type="button"
                                onClick={() => setShowConfirmPassword(!showConfirmPassword)}
                                className="absolute inset-y-0 right-3 flex items-center text-gray-400 hover:text-gray-300"
                            >
                                {showConfirmPassword ? "👁️" : "🔒"}
                            </button>
                        </div>
                    </div>
                    <button type="submit" className="w-full bg-blue-700 hover:bg-blue-800 text-white p-3 rounded font-bold transition">
                        Create Company
                    </button>
                </form>
                <div className="mt-4 text-center">
                    <p className="text-gray-400">
                        Already have an account?  
                        <button 
                            onClick={() => navigate('/')} 
                            className="text-blue-600 hover:text-blue-700 ml-1 underline"
                        >
                            Sign in here
                        </button>
                    </p>
                </div>
            </div>
        </div>
    );
};

export default CreateCompany;
