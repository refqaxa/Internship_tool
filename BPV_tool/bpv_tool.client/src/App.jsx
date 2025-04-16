import { useEffect, useState } from 'react';
import './App.css';
import Navbar from './components/navbar.jsx';
import Footer from './components/footer.jsx';

import { Routes, Route } from 'react-router-dom';
import Login from './views/Login';
import TeacherDashboard from './views/Teacherdashboard';
import StudentDashboard from './views/Studentdashboard';
import AdminPanel from './views/AdminPanel';
import Logbook from './views/Logbook';

function App() {

    return (
        <>
            <Navbar />

            <Routes>
                <Route path="/" element={
                    <div className="container my-4">
                        <h1>Welcome naar de beste BPV tool web app</h1>
                    </div>
                } />
                <Route path="/views/adminpanel" element={<AdminPanel />} />
                <Route path="/views/Teacherdashboard" element={<TeacherDashboard />} />
                <Route path="/views/Studentdashboard" element={<StudentDashboard />} />
                <Route path="/views/logbook" element={<Logbook />} />
                <Route path="/views/login" element={<Login />} />
            </Routes>
            
          <Footer />
        </>
    );
    
}


export default App;



//// Update user usage:
//await fetch('/api/appusers/create-user', {
//    method: 'POST',
//    headers: { 'Content-Type': 'application/json' },
//    body: JSON.stringify({
//        firstName: 'John',
//        lastName: 'Doe',
//        email: 'john@example.com',
//        passwordHash: 'John@123',
//        roleId: 'role-guid-here'
//    })
//});

//// Roles dropdown
//function RoleDropdown({ onChange }) {
//    const [roles, setRoles] = useState([]);

//    useEffect(() => {
//        fetch("/api/appusers/roles")
//            .then(res => res.json())
//            .then(data => setRoles(data))
//            .catch(err => console.error("Failed to fetch roles", err));
//    }, []);

//    return (
//        <select onChange={e => onChange(e.target.value)}>
//            <option value="">-- Select Role --</option>
//            {roles.map(role => (
//                <option key={role.id} value={role.id}>
//                    {role.roleName}
//                </option>
//            ))}
//        </select>
//    );
//}

