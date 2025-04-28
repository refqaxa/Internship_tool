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
import CreateUser from './views/CreateUser.jsx';

function App() {

    return (
        <>
            <Navbar />

            <Routes>
                <Route path="/" element={
                    <div className="container text-center my-4">
                        <h1>Welcome to the best internship tool web app</h1>
                    </div>
                } />
                <Route path="/views/adminpanel" element={<AdminPanel />} />
                <Route path="/views/Teacherdashboard" element={<TeacherDashboard />} />
                <Route path="/views/Studentdashboard" element={<StudentDashboard />} />
                <Route path="/views/logbook" element={<Logbook />} />
                <Route path="/views/createuser" element={<CreateUser />} />
                <Route path="/views/login" element={<Login />} />
            </Routes>

            <Footer />
        </>
    );

}

export default App;
