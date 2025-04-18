import { useState, useEffect, useContext } from 'react';
import { AuthContext } from '../context/AuthContext.jsx';
import { toast } from 'react-toastify';

export default function CreateUserModal({ show, onClose, onUserCreated }) {
    const { token } = useContext(AuthContext);
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

    useEffect(() => {
        if (!show) return;
        fetch('/api/AppUsers/Roles', {
            headers: { Authorization: `Bearer ${token}` }
        })
            .then(res => res.json())
            .then(setRoles)
            .catch(() => toast.error('Failed to load roles'));
    }, [show, token]);

    const handleChange = (e) => {
        setForm({ ...form, [e.target.name]: e.target.value });
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
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
                toast.success('User created!');
                onClose();
                onUserCreated(); // refresh users
            } else {
                const msg = await res.text();
                console.log(res)
                setError(msg || 'Failed to create user.');
            }
        } catch {
            setError('An error occurred.');
        }
    };

    return (
        <div className={`modal ${show ? 'd-block show' : ''}`} tabIndex="-1" style={{ backgroundColor: 'rgba(0,0,0,0.5)' }}>
            <div className="modal-dialog modal-lg">
                <div className="modal-content">
                    <form onSubmit={handleSubmit}>
                        <div className="modal-header">
                            <h5 className="modal-title">Create New User</h5>
                            <button type="button" className="btn-close" onClick={onClose}></button>
                        </div>
                        <div className="modal-body">
                            <div className="row g-3">
                                <div className="col-md-6">
                                    <input name="firstName" value={form.firstName} onChange={handleChange} required className="form-control" placeholder="First Name" />
                                </div>
                                <div className="col-md-6">
                                    <input name="middleName" value={form.middleName} onChange={handleChange} className="form-control" placeholder="Middle Name (optional)" />
                                </div>
                                <div className="col-md-6">
                                    <input name="lastName" value={form.lastName} onChange={handleChange} required className="form-control" placeholder="Last Name" />
                                </div>
                                <div className="col-md-6">
                                    <input type="email" name="email" value={form.email} onChange={handleChange} required className="form-control" placeholder="Email" />
                                </div>
                                <div className="col-md-6">
                                    <input type="password" name="password" value={form.password} onChange={handleChange} required className="form-control" placeholder="Password" />
                                </div>
                                <div className="col-md-6">
                                    <select name="roleId" value={form.roleId} onChange={handleChange} required className="form-select">
                                        <option value="">Select Role</option>
                                        {roles.map(role => (
                                            <option key={role.id} value={role.id}>{role.roleName}</option>
                                        ))}
                                    </select>
                                </div>
                            </div>
                            {error && <div className="alert alert-danger mt-3">{error}</div>}
                        </div>
                        <div className="modal-footer">
                            <button type="button" className="btn btn-secondary" onClick={onClose}>Cancel</button>
                            <button type="submit" className="btn btn-primary">Create</button>
                        </div>
                    </form>
                </div>
            </div>
        </div>
    );
}
