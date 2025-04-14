import { useEffect, useState } from 'react';
import './App.css';
import Navbar from './components/navbar.jsx';
import Footer from './components/footer.jsx';
import login from './login.jsx'

function App() {

    return (
        <>
          <Navbar />
          <div className="container my-4">
            <h1>Welcome naar de beste BPV tool web app</h1>
          </div>
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

//// Login usage:
//const response = await fetch('/api/AppUsers/Login', {
//    method: 'POST',
//    headers: { 'Content-Type': 'application/json' },
//    body: JSON.stringify({ email: 'email', password: '' })
//});
//const data = await response.json();
//console.log(user);
//// Handle login (in a real-world scenario, you would store a JWT token for further requests)

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

