import { useState, useEffect } from 'react';
import axios, { AxiosError } from 'axios';
import { useNavigate } from 'react-router-dom';

interface Industry {
    id: number;
    name: string;
}

const CreateCompany = () => {
    const [companyName, setCompanyName] = useState('');
    const [industryId, setIndustryId] = useState<number>(0);
    const [industryOptions, setIndustryOptions] = useState<Industry[]>([]);
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

    useEffect(() => {
        const fetchIndustries = async () => {
            try {
                const response = await axios.get('/api/industry');
                setIndustryOptions(response.data);
            } catch (error) {
                console.error("Failed to fetch industries:", error);
            }
        };
        fetchIndustries();
    }, []);

    const handleCreateCompany = async (e: React.FormEvent) => {
        e.preventDefault();

        if (isSubmitting) return;
        setIsSubmitting(true);
        setError('');
        setSuccess('');

        if (password !== confirmPassword) {
            setError('Passwords do not match');
            setIsSubmitting(false);
            return;
        }
        if (industryId === 0) {
            setError('Please select an industry.');
            setIsSubmitting(false);
            return;
        }

        try {
            const response = await axios.post('/api/company/create', {
                companyName,
                industryId,
                email,
                firstName,
                lastName,
                password
            });

            if (response.data.token) {
                localStorage.setItem('token', response.data.token);
                axios.defaults.headers.common['Authorization'] = `Bearer ${response.data.token}`;
            }
            setSuccess('Company and CEO account created successfully!');
            navigate('/dashboard');
        } catch (err: unknown) {
            try {
                const axiosError = err as AxiosError<{ message?: string; errors?: any }>;
                let errorMessage = '';
                if (axiosError.response?.data?.errors) {
                    const errorsData = axiosError.response.data.errors;
                    if (Array.isArray(errorsData)) {
                        errorMessage = errorsData.join(', ');
                    } else if (typeof errorsData === 'object') {
                        for (const key in errorsData) {
                            if (Object.prototype.hasOwnProperty.call(errorsData, key)) {
                                const errorVal = errorsData[key];
                                if (Array.isArray(errorVal)) {
                                    if (key.toLowerCase().includes("password")) {
                                        errorMessage += "Password error: " + errorVal.join(', ') + ". ";
                                    } else if (key.toLowerCase().includes("email") || key.toLowerCase().includes("username")) {
                                        errorMessage += "Account error: " + errorVal.join(', ') + ". ";
                                    } else {
                                        errorMessage += errorVal.join(', ') + ". ";
                                    }
                                } else {
                                    errorMessage += errorVal + ". ";
                                }
                            }
                        }
                    }
                } else if (axiosError.response?.data?.message) {
                    errorMessage = axiosError.response.data.message;
                } else {
                    errorMessage = 'Failed to create company or user';
                }
                setError(errorMessage);
            } catch (processingError) {
                setError('An unexpected error occurred.');
            }
        } finally {
            setIsSubmitting(false);
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
                        <select
                            className="w-full p-3 rounded bg-gray-800 text-white border border-gray-700 focus:outline-none focus:ring-2 focus:ring-blue-500"
                            value={industryId}
                            onChange={(e) => setIndustryId(Number(e.target.value))}
                        >
                            <option value={0}>Select an industry</option>
                            {industryOptions.map(option => (
                                <option key={option.id} value={option.id}>
                                    {option.name}
                                </option>
                            ))}
                        </select>
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
