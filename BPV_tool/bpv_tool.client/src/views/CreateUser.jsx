import { useContext, useEffect, useState } from 'react';
import { AuthContext } from '../context/AuthContext.jsx';
import { useNavigate, Navigate } from 'react-router-dom';

export default function CreateUser() {
    const { user, token } = useContext(AuthContext);
    const [roles, setRoles] = useState([]);
    const [form, setForm] = useState({
        firstName: '',
        middleName: '',
        lastName: '',
        email: '',
        passwordHash: '',
        roleId: ''
    });
    const [error, setError] = useState('');
    const navigate = useNavigate();

    if (!user || user.role !== 'Admin') {
        return <Navigate to="/views/login" />;
    }

    useEffect(() => {
        const fetchRoles = async () => {
            try {
                const response = await fetch('/api/AppUsers/Roles', {
                    headers: {
                        'Content-Type': 'application/json',
                        'Authorization': `Bearer ${token}`
                    }
                });
                const data = await response.json();
                setRoles(data);
            } catch (err) {
                console.error('Error fetching roles', err);
            }
        };
        fetchRoles();
    }, [token]);

    const handleChange = (e) => {
        setForm({ ...form, [e.target.name]: e.target.value });
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        setError('');

        if (!form.roleId) {
            setError('Please select a role.');
            return;
        }

        try {
            const res = await fetch('/api/AppUsers/CreateUser', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    Authorization: `Bearer ${token}`
                },
                body: JSON.stringify(form)
            });

            if (res.ok) {
                navigate('/views/adminpanel');
            } else {
                const msg = await res.text();
                setError(msg || 'Failed to create user.');
            }
        } catch (err) {
            setError('An error occurred. Please try again.');
        }
    };

    return (
        <div className="container my-5">
            <h2>Create New User</h2>
            <form onSubmit={handleSubmit}>
                <div className="mb-3">
                    <label className="form-label">First Name</label>
                    <input type="text" name="firstName" className="form-control" value={form.firstName} onChange={handleChange} required />
                </div>
                <div className="mb-3">
                    <label className="form-label">Middle Name (optional)</label>
                    <input type="text" name="middleName" className="form-control" value={form.middleName} onChange={handleChange} />
                </div>
                <div className="mb-3">
                    <label className="form-label">Last Name</label>
                    <input type="text" name="lastName" className="form-control" value={form.lastName} onChange={handleChange} required />
                </div>
                <div className="mb-3">
                    <label className="form-label">Email</label>
                    <input type="email" name="email" className="form-control" value={form.email} onChange={handleChange} required />
                </div>
                <div className="mb-3">
                    <label className="form-label">Password</label>
                    <input type="password" name="passwordHash" className="form-control" value={form.passwordHash} onChange={handleChange} required />
                </div>
                <div className="mb-3">
                    <label className="form-label">Role</label>
                    <select name="roleId" className="form-select" value={form.roleId} onChange={handleChange} required>
                        <option value="">Select role</option>
                        {roles.map(role => (
                            <option key={role.id} value={role.id}>{role.roleName}</option>
                        ))}
                    </select>
                </div>
                {error && <div className="alert alert-danger">{error}</div>}
                <button type="submit" className="btn btn-primary">Create User</button>
            </form>
        </div>
    );
}


