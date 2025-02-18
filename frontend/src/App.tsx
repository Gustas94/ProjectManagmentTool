import { Routes, Route } from 'react-router-dom';
import Login from './pages/Login';
import CreateCompany from './pages/CreateCompany';
import Dashboard from './pages/Dashboard.tsx';

function App() {
  return (
      <Routes>
        <Route path="/" element={<Login />} />
        <Route path="/login" element={<Login />} />
        <Route path="/create-company" element={<CreateCompany />} />
        <Route path="/dashboard" element={<Dashboard />} />
      </Routes>
  );
}

export default App;
