import { Link } from 'react-router-dom';
import logo from '../assets/images/AventusLogo.png';

export default function Navbar() {
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
                    <li className="nav-item">
                        <Link className="nav-link" to="/dashboard">Dashboard</Link>
                    </li>
                    <li className="nav-item">
                        <Link className="nav-link" to="/admin">Admin Panel</Link>
                    </li>
                    <li className="nav-item">
                        <Link className="nav-link" to="/profile">Profile</Link>
                    </li>
                    <li className="nav-item">
                        <Link className="nav-link text-warning" to="/logout">Logout</Link>
                    </li>
                </ul>
            </div>
        </nav>
    );
}
