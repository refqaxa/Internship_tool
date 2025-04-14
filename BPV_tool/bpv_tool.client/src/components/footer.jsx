import logo from '../assets/images/AventusLogo.png';

export default function Footer() {
    return (
        <footer className="bg-light text-center text-muted py-3 mt-auto">
            <hr />
            <div className="footer-items">
                <img src={logo} alt="Logo" style={{ height: "40px" }} />
                <div className="vl"></div>
                <small className="px-3">© 2025 BPV Tool. All rights reserved.</small>
            </div>
        </footer>
    );
}

