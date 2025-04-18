//  handle login and out + session with localStorage that keeps the user logged in across page refreshes.
// context/AuthContext.jsx
import { createContext, useState, useEffect } from 'react';
import jwtDecode from "jwt-decode";

export const AuthContext = createContext();

export default function AuthProvider({ children }) {
    const [user, setUser] = useState(null);
    const [token, setToken] = useState(null);

    // set timeout for login session
    useEffect(() => {
        if (token) {
            const decoded = jwtDecode(token);
            const expireIn = (decoded.exp * 1000) - Date.now();

            const timer = setTimeout(() => {
                logout();
                alert("Session expired. Please login again.");
            }, expireIn);

            return () => clearTimeout(timer);
        }
    }, [token]);


    useEffect(() => {
        const storedUser = localStorage.getItem('user');
        const storedToken = localStorage.getItem('token');
        if (storedUser && storedToken) {
            setUser(JSON.parse(storedUser));
            setToken(storedToken);
        }
    }, []);

    const login = (userData, token) => {
        localStorage.setItem('user', JSON.stringify(userData));
        localStorage.setItem('token', token);
        setUser(userData);
        setToken(token);
    };

    const logout = () => {
        localStorage.removeItem('user');
        localStorage.removeItem('token');
        setUser(null);
        setToken(null);
    };

    return (
        <AuthContext.Provider value={{ user, token, login, logout }}>
            {children}
        </AuthContext.Provider>
    );
}

