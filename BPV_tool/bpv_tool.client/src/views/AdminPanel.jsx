import { useContext, useEffect, useState } from 'react';
import { AuthContext } from '../context/AuthContext.jsx';
import { Navigate } from 'react-router-dom';
import axios from 'axios';
import { toast } from 'react-toastify';
import CreateUserModal from '../components/CreateUserModal.jsx';


export default function AdminPanel() {
    const { user, token } = useContext(AuthContext);
    const [users, setUsers] = useState([]);
    const [roles, setRoles] = useState([]);
    const [loading, setLoading] = useState(true);
    const [showCreateModal, setShowCreateModal] = useState(false);


    useEffect(() => {
        if (user && user.role === 'Admin') {
            fetchData();
        }
    }, [user]);

    const fetchData = async () => {
        try {
            const [usersRes, rolesRes] = await Promise.all([
                axios.get('/api/AppUsers/AllUsers', {
                    headers: { Authorization: `Bearer ${token}` },
                }),
                axios.get('/api/AppUsers/Roles'),
            ]);

            setUsers(usersRes.data);
            setRoles(rolesRes.data);
        } catch (err) {
            toast.error('Failed to fetch users or roles');
        } finally {
            setLoading(false);
        }
    };

    const handleRoleChange = async (userId, newRoleId) => {
        try {
            await axios.put(`/api/AppUsers/UpdateRole/${userId}`, newRoleId, {
                headers: { Authorization: `Bearer ${token}` },
            });
            toast.success('Role updated');
            fetchData();
        } catch (err) {
            toast.error('Failed to update role');
        }
    };

    const handleDelete = async (userId) => {
        const confirmDelete = window.confirm('This user might have uploaded files, logs, or approvals. Are you sure you want to delete?');
        if (!confirmDelete) return;

        try {
            await axios.delete(`/api/AppUsers/${userId}`, {
                headers: { Authorization: `Bearer ${token}` },
            });
            toast.success('User deleted');
            fetchData();
        } catch (err) {
            toast.error('Failed to delete user');
        }
    };

    if (!user || user.role !== 'Admin') {
        return <Navigate to="/views/login" />;
    }

    return (
        <div className="container my-5">
            <CreateUserModal
                show={showCreateModal}
                onClose={() => setShowCreateModal(false)}
                token={token}
                onUserCreated={fetchData}
            />

            <div className="d-flex justify-content-between align-items-center mb-4">
                <h2>Admin Panel</h2>
                <button className="btn btn-success" onClick={() => setShowCreateModal(true)}>
                    Nieuwe gebruiker toevoegen
                </button>
            </div>

            {loading ? (
                <p>Loading users...</p>
            ) : users.length === 0 ? (
                <div className="alert alert-info">No users found.</div>
            ) : (
                <table className="table table-bordered table-striped">
                    <thead>
                        <tr>
                            <th>Full Name</th>
                            <th>Email</th>
                            <th>Role</th>
                            <th>Actions</th>
                        </tr>
                    </thead>
                    <tbody>
                        {users.map((userObj) => (
                            <tr key={userObj.id}>
                                <td>{userObj.fullName}</td>
                                <td>{userObj.email}</td>
                                <td>
                                    <select
                                        className="form-select"
                                        value={roles.find(r => r.roleName === userObj.role)?.id || ''}
                                        onChange={(e) => handleRoleChange(userObj.id, e.target.value)}
                                    >
                                        {roles.map((role) => (
                                            <option key={role.id} value={role.id}>
                                                {role.roleName}
                                            </option>
                                        ))}
                                    </select>
                                </td>
                                <td>
                                    <button className="btn btn-danger" onClick={() => handleDelete(userObj.id)}>
                                        Delete
                                    </button>
                                </td>
                            </tr>
                        ))}
                    </tbody>
                </table>
            )}

        </div>
    );
}
