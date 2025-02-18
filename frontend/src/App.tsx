import { Routes, Route } from 'react-router-dom';
import Login from './pages/Login';
import CreateCompany from './pages/CreateCompany';
import Dashboard from './pages/Dashboard.tsx';
import Projects from './pages/Projects.tsx';

function App() {
  return (
      <Routes>
        <Route path="/" element={<Login />} />
        <Route path="/login" element={<Login />} />
        <Route path="/create-company" element={<CreateCompany />} />
        <Route path="/dashboard" element={<Dashboard />} />
        <Route path="/projects" element={<Projects />} />
      </Routes>
  );
}

export default App;
