import { useState, useContext, useEffect } from 'react';
import { AuthContext } from '../context/AuthContext.jsx';
import { toast } from 'react-toastify';
import { Accordion, Card, Button, Form } from 'react-bootstrap';

function StartProcessModal({ show, onClose, onCreated }) {
    const { token } = useContext(AuthContext);
    const [company, setCompany] = useState('');
    const [supervisors, setSupervisors] = useState([]);
    const [supervisorId, setSupervisorId] = useState('');

    useEffect(() => {
        if (!show) return;
        // fetch list of supervisors
        fetch('/api/AppUsers/Supervisors', {
            headers: { Authorization: `Bearer ${token}` }
        })
            .then(r => r.json())
            .then(setSupervisors)
            .catch(() => toast.error('Failed to load supervisors'));
        }, [show, token]);

    const handleSubmit = async e => {
        e.preventDefault();
        const res = await fetch('/api/BPVProcesses/Create', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                Authorization: `Bearer ${token}`
            },
            body: JSON.stringify({ companyName: company, supervisorId })
        });
        if (res.ok) {
            toast.success('BPV‑proces gestart!');
            onClose();
            onCreated();
        } else {
            toast.error('Kon proces niet starten');
        }
    };

    if (!show) return null;
    return (
        <div className="modal d-block" style={{ background: 'rgba(0,0,0,0.5)' }}>
            <div className="modal-dialog">
                <form className="modal-content" onSubmit={handleSubmit}>
                    <div className="modal-header">
                        <h5 className="modal-title">Start BPV‑proces</h5>
                        <button type="button" className="btn-close" onClick={onClose} />
                    </div>
                    <div className="modal-body">
                        <div className="mb-3">
                            <label>Company</label>
                            <input
                                className="form-control"
                                value={company}
                                onChange={e => setCompany(e.target.value)}
                                required
                            />
                        </div>
                        <div className="mb-3">
                            <label>Supervisor</label>
                            <select
                                className="form-select"
                                value={supervisorId}
                                onChange={e => setSupervisorId(e.target.value)}
                                required
                            >
                                <option value="">Selecteer begeleider</option>
                                {supervisors.map(s => (
                                    <option key={s.id} value={s.id}>
                                        {s.fullName}
                                    </option>
                                ))}
                            </select>
                        </div>
                    </div>
                    <div className="modal-footer">
                        <button className="btn btn-secondary" onClick={onClose}>Annuleren</button>
                        <button className="btn btn-primary" type="submit">Starten</button>
                    </div>
                </form>
            </div>
        </div>
    );
}

function ProcessAccordion({ proc, token }) {
    const [steps, setSteps] = useState([]);

    useEffect(() => {
        fetch(`/api/BPVProcessSteps/ByProcess/${proc.id}`, {
            headers: { Authorization: `Bearer ${token}` }
        })
            .then(r => r.json())
            .then(setSteps)
            .catch(() => toast.error('Kon stappen niet laden'));
    }, [proc.id, token]);

    const uploadFile = async (stepId, file) => {
        const fd = new FormData();
        fd.append('File', file);
        await fetch(`/api/BPVProcessSteps/UploadFile/${stepId}`, {
            method: 'POST',
            headers: { Authorization: `Bearer ${token}` },
            body: fd
        });
        toast.success('Bestand geüpload');
        // reload steps...
        setSteps(steps => /* re-fetch or update*/[]);
    };

    return (
        <Accordion>
            {steps.map(s => (
                <Card key={s.id}>
                    <Accordion.Header>{s.stepName}</Accordion.Header>
                    <Accordion.Body>
                        <p>Status: {s.approvalStatus || 'Niet beoordeeld'}</p>
                        {s.filePath
                            ? <a href={s.filePath} target="_blank">Download bestand</a>
                            : (
                                <Form.Group>
                                    <Form.Control
                                        type="file"
                                        onChange={e => uploadFile(s.id, e.target.files[0])} />
                                </Form.Group>
                            )
                        }
                        {s.approvalComment && (
                            <div className="mt-2">
                                <strong>Feedback:</strong> {s.approvalComment}
                            </div>
                        )}
                    </Accordion.Body>
                </Card>
            ))}
        </Accordion>
    );
}
export default function Studentdashboard() {
    const { user, token } = useContext(AuthContext);
    const [processes, setProcesses] = useState([]);
    const [showModal, setShowModal] = useState(false);

    if (!user || user.role !== 'Student') {
        return <Navigate to="/views/login" />;
    }

    const fetchProcesses = async () => {
        const res = await fetch('/api/BPVProcesses/My', {
            headers: { Authorization: `Bearer ${token}` }
        });
        if (res.ok) setProcesses(await res.json());
        else toast.error('Kon BPV‑processen niet laden');
    };

    useEffect(() => {
        fetchProcesses();
    }, [user, token]);

    return (
        <div className="container my-5">
            <div className="d-flex justify-content-between align-items-center mb-4">
                <h2>Mijn BPV‑processen</h2>
                <button className="btn btn-success" onClick={() => setShowModal(true)}>
                    Nieuw proces starten
                </button>
            </div>

            <StartProcessModal
                show={showModal}
                onClose={() => setShowModal(false)}
                onCreated={fetchProcesses}
            />

            {processes.length === 0 ? (
                <div className="alert alert-info">Je hebt nog geen BPV‑processen.</div>
            ) : (
                <>
                    <div className="row">
                        {/* Left cards */}
                        <div className="col-md-4">
                            <Card className="mb-3">
                                <Card.Body>
                                    <Card.Title>Jouw bestanden</Card.Title>
                                        {/*{processes.map(proc => (*/}
                                        {/*    <div key={proc.id} className="mb-4 list-group-item d-flex justify-content-between">*/}
                                        {/*    </div>*/}
                                        {/* )}*/}
                                        {/*<Button onClick={ZIP download }>Download ZIP</Button>*/}
                                </Card.Body>
                            </Card>
                        </div>

                        {/* Main content */}
                        <div className="col-md-8 list-group">
                            {processes.map(proc => (
                                <div key={proc.id} className="mb-4 list-group-item d-flex justify-content-between">
                                    <div>
                                        <h5>{proc.companyName} — {proc.status}</h5>
                                        Begeleider: {proc.supervisorName}<br />
                                    </div>
                                    <ProcessAccordion proc={proc} token={token} />
                                    <small className="text-muted">{new Date(proc.createdAt).toLocaleDateString()}</small>
                                </div>
                            ))}
                        </div>
                    </div>
                </>
            )}
        </div>
    );
}
