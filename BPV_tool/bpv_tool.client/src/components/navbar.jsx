import { Link, useNavigate } from 'react-router-dom';
import { useContext } from 'react';
import { AuthContext } from '../context/AuthContext.jsx';
import logo from '../assets/images/AventusLogo.png';

export default function Navbar() {
    const { user, logout } = useContext(AuthContext);
    const navigate = useNavigate();

    const handleLogout = () => {
        logout();
        navigate('/login');
    };

    return (
        <nav className="navbar navbar-expand-lg navbar-light bg-light px-5 py-3 shadow rounded">
            <Link className="navbar-brand" to="/">
                <img src={logo} alt="AVENTUS logo" style={{ height: "40px" }} />
            </Link>
            <button className="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target="#navbarNav">
                <span className="navbar-toggler-icon"></span>
            </button>
            <div className="collapse navbar-collapse" id="navbarNav">
                <ul className="navbar-nav ms-auto">
                    {user && (
                        <>
                            <li className="nav-item">
                                <Link className="nav-link" to="/dashboard">Dashboard</Link>
                            </li>
                            {user.Role === 'admin' && (
                                <li className="nav-item">
                                    <Link className="nav-link" to="/admin">Admin Panel</Link>
                                </li>
                            )}
                            <li className="nav-item">
                                <Link className="nav-link" to="/profile">Profile</Link>
                            </li>
                            <li className="nav-item">
                                <button className="nav-link btn btn-link text-warning" onClick={handleLogout}>Logout</button>
                            </li>
                        </>
                    )}
                    {!user && (
                        <li className="nav-item">
                            <Link className="nav-link" to="/views/login">Login</Link>
                        </li>
                    )}
                </ul>
            </div>
        </nav>
    );
}
