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
            // 🔹 Use relative URL (proxy will forward this to backend)
            const response = await axios.post('/api/auth/login', { email, password });
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
                                {showPassword ? "🙈" : "👁️"}
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
            </div>
        </div>
    );
};

export default Login;
