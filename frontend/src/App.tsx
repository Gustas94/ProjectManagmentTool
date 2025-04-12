import { Routes, Route } from 'react-router-dom';
import Login from './pages/Login';
import CreateCompany from './pages/CreateCompany';
import Dashboard from './pages/Dashboard';
import Projects from './pages/Projects';
import CreateProject from './pages/CreateProject';
import ProjectDetails from './pages/ProjectDetails';
import Register from './pages/Register';
import Groups from './pages/Groups';
import GroupDetails from "./pages/GroupDetails";
import TaskDetails from './pages/TaskDetails';
import AdminPanel from './pages/AdminPanel';
import { UserProvider } from './contexts/UserContext';  // Import the provider
import Unauthorized from "./pages/Unauthorized";

function App() {
  return (
    <UserProvider>
      <Routes>
        <Route path="/" element={<Login />} />
        <Route path="/login" element={<Login />} />
        <Route path="/register" element={<Register />} />
        <Route path="/create-company" element={<CreateCompany />} />
        <Route path="/dashboard" element={<Dashboard />} />
        <Route path="/projects" element={<Projects />} />
        <Route path="/projects/create-project" element={<CreateProject />} />
        <Route path="/projects/:projectId" element={<ProjectDetails />} />
        <Route path="/groups" element={<Groups />} />
        <Route path="/groups/:groupId" element={<GroupDetails />} />
        <Route path="/tasks/:taskId" element={<TaskDetails />} />
        <Route path="/admin" element={<AdminPanel />} />
        <Route path="/unauthorized" element={<Unauthorized />} />
      </Routes>
    </UserProvider>
  );
}

export default App;
