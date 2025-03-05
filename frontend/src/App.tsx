import { Routes, Route } from 'react-router-dom';
import Login from './pages/Login';
import CreateCompany from './pages/CreateCompany';
import Dashboard from './pages/Dashboard.tsx';
import Projects from './pages/Projects.tsx';
import CreateProject from './pages/CreateProject.tsx';
import ProjectDetails from './pages/ProjectDetails.tsx';
import Register from './pages/Register.tsx';

function App() {
  return (
      <Routes>
        <Route path="/" element={<Login />} />
        <Route path="/login" element={<Login />} />
        <Route path="/register" element={<Register />} />
        <Route path="/create-company" element={<CreateCompany />} />
        <Route path="/dashboard" element={<Dashboard />} />
        <Route path="/projects" element={<Projects />} />
        <Route path="projects/create-project" element={<CreateProject />} />
        <Route path="/projects/:projectId" element={<ProjectDetails />} />
      </Routes>
  );
}

export default App;
