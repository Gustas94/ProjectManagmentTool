import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import axios from 'axios';

const Login = () => {
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [showPassword, setShowPassword] = useState(false);
    const [error, setError] = useState('');
    const navigate = useNavigate();

    const handleLogin = async (e: React.FormEvent) => {
        e.preventDefault();
        setError('');

        try {
            const response = await axios.post('http://localhost:5198/api/auth/login', { email, password });
            localStorage.setItem('token', response.data.token);
            navigate('/dashboard');
        } catch (err) {
            setError('Invalid email or password');
        }
    };

    return (
        <div className="flex justify-center items-center min-h-screen border-gray-500">
            <div className="bg-slate-800 p-8 rounded-lg shadow-lg w-full max-w-sm border border-slate-800">
                <h2 className="text-2xl font-bold text-white mb-6 text-center">Login</h2>
                {error && <p className="text-red-500 text-sm text-center mb-2">{error}</p>}
                
                <form onSubmit={handleLogin} className="flex flex-col gap-4">
                    <div>
                        <label className="block text-gray-400 text-sm font-medium mb-1">Email</label>
                        <input 
                            type="email" 
                            className="w-full p-3 rounded bg-slate-800 text-white border border-gray-700 focus:outline-none focus:ring-2 focus:ring-blue-500" 
                            placeholder="Enter your email" 
                            value={email} 
                            onChange={(e) => setEmail(e.target.value)} 
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
                                {showPassword ? (
                                    <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth={1.5} stroke="currentColor" className="w-5 h-5">
                                        <path strokeLinecap="round" strokeLinejoin="round" d="M3.98 8.223A10.477 10.477 0 001.934 12C3.226 16.338 7.244 19.5 12 19.5c.993 0 1.953-.138 2.863-.395M6.228 6.228A10.45 10.45 0 0112 4.5c4.756 0 8.773 3.162 10.065 7.498a10.523 10.523 0 01-4.293 5.774M6.228 6.228L3 3m3.228 3.228l3.65 3.65m7.894 7.894L21 21m-3.228-3.228l-3.65-3.65m0 0a3 3 0 10-4.243-4.243m4.242 4.242L9.88 9.88"/>
                                    </svg>
                                ) : (
                                    <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth={1.5} stroke="currentColor" className="w-5 h-5">
                                        <path strokeLinecap="round" strokeLinejoin="round" d="M2.036 12.322a1.012 1.012 0 010-.639C3.423 7.51 7.36 4.5 12 4.5c4.638 0 8.573 3.007 9.963 7.178.07.207.07.431 0 .639C20.577 16.49 16.64 19.5 12 19.5c-4.638 0-8.573-3.007-9.963-7.178z"/>
                                        <path strokeLinecap="round" strokeLinejoin="round" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"/>
                                    </svg>
                                )}
                            </button>
                        </div>
                    </div>
                    <button 
                        type="submit" 
                        className="w-full bg-blue-700 hover:bg-blue-800 text-white p-3 rounded font-bold transition"
                    >
                        Sign in
                    </button>
                </form>

                <div className="mt-4 text-center">
                    <p className="text-gray-400">
                        Don't have an account?  
                        <button 
                            onClick={() => navigate('/register')} 
                            className="text-blue-600 hover:text-blue-700 ml-1 underline"
                        >
                            Register here
                        </button>
                    </p>
                </div>
                <div className="mt-4 text-center">
                    <p className="text-gray-400">
                        Want to create a company?  
                        <button 
                            onClick={() => navigate('/create-company')} 
                            className="text-blue-600 hover:text-blue-700 ml-1 underline"
                        >
                            Create it here
                        </button>
                    </p>
                </div>
            </div>
        </div>
    );
};

export default Login;
