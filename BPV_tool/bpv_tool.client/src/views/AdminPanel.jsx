import { useContext } from 'react';
import { AuthContext } from '../context/AuthContext.jsx';
import { Navigate } from 'react-router-dom';

export default function AdminPanel() {
    const { user } = useContext(AuthContext);

    if (!user || user.Role !== 'admin') {
        return <Navigate to="/login" />;
    }

    return (
        <div className="container my-5">
            <h2>Admin Panel</h2>
            {/* users list etc */}
        </div>
    );
}
