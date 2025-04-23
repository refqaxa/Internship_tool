import { useState, useContext, useEffect } from 'react';
import { AuthContext } from '../context/AuthContext.jsx';
import { toast } from 'react-toastify';
import { Accordion, Card, Button, Form } from 'react-bootstrap';
import { Navigate } from 'react-router-dom';

function StartProcessModal({ show, onClose, onCreated }) {
    const { token } = useContext(AuthContext);
    const [company, setCompany] = useState('');
    const [supervisors, setSupervisors] = useState([]);
    const [supervisorId, setSupervisorId] = useState('');

    useEffect(() => {
        if (!show) return;
        fetch('/api/AppUsers/Supervisors', {
            headers: { Authorization: `Bearer ${token}` }
        })
            .then(r => r.json())
            .then(setSupervisors)
            .catch(() => toast.error('Failed to load supervisors'));
    }, [show, token]);

    const handleSubmit = async e => {
        e.preventDefault();
        const res = await fetch('/api/StudentDashboard/processes', {
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
function ProcessAccordion({ proc, token, onFileUpload }) {
    const [steps, setSteps] = useState([]);

    useEffect(() => {
        fetch(`/api/StudentDashboard/processes/${proc.id}/steps`, {
            headers: { Authorization: `Bearer ${token}` }
        })
            .then(r => r.json())
            .then(data => {
                setSteps(data);
                onFileUpload(proc.id, data.some(step => step.filePath));
            })
            .catch(() => toast.error('Kon stappen niet laden'));
    }, [proc.id, token]);

    const uploadFile = async (stepId, file) => {
        const fd = new FormData();
        fd.append('File', file);
        await fetch(`/api/StudentDashboard/processes/${proc.id}/steps/${stepId}/upload`, {
            method: 'POST',
            headers: { Authorization: `Bearer ${token}` },
            body: fd
        });
        toast.success('Bestand geüpload');
        // Re-fetch steps after upload
        const res = await fetch(`/api/StudentDashboard/processes/${proc.id}/steps`, {
            headers: { Authorization: `Bearer ${token}` }
        });
        const updated = await res.json();
        setSteps(updated);
        onFileUpload(proc.id, updated.some(step => step.filePath));
    };

    return (
        <Accordion>
            {steps.map((s, idx) => (
                <Accordion.Item eventKey={idx.toString()} key={s.id}>
                    <Accordion.Header>{s.stepName}</Accordion.Header>
                    <Accordion.Body>
                        <p>Status: {s.approvalStatus || 'Nog niet ingeleverd'}</p>
                        {s.filePath ? (
                            <>
                                <a href={`/${s.filePath}`} target="_blank" rel="noreferrer">
                                    Bekijk bestand
                                </a>
                                <br />
                                <small className="text-muted">
                                    Geüpload op {new Date(s.uploadedAt).toLocaleDateString()}
                                </small>
                            </>
                        ) : (
                            <Form.Group>
                                <Form.Control
                                    type="file"
                                    onChange={e => uploadFile(s.id, e.target.files[0])} />
                            </Form.Group>
                        )}
                        {s.feedback && (
                            <div className="mt-2">
                                <strong>Feedback:</strong> {s.feedback}
                            </div>
                        )}
                    </Accordion.Body>
                </Accordion.Item>
            ))}
        </Accordion>
    );
}

export default function Studentdashboard() {
    const { user, token } = useContext(AuthContext);
    const [processes, setProcesses] = useState([]);
    const [showModal, setShowModal] = useState(false);
    const [loading, setLoading] = useState(true);
    const [uploadStatus, setUploadStatus] = useState({});
    const [studentFiles, setStudentFiles] = useState([]);


    if (!user || user.role !== 'Student') {
        return <Navigate to="/views/login" />;
    }

    const hasAnyFiles = Object.values(uploadStatus).some(v => v);

    const fetchProcesses = async () => {
        setLoading(true);
        const res = await fetch('/api/StudentDashboard/processes', {
            headers: { Authorization: `Bearer ${token}` }
        });
        if (!res.ok) {
            toast.error('Kon BPV‑processen niet laden');
            setLoading(false);
            return;
        }

        const list = await res.json();
        setProcesses(list);

        // Voor elk process checken of er stappen zijn met een bestand 
        const fileStatusMap = {};
        for (const p of list) {
            try {
                const resSteps = await fetch(`/api/StudentDashboard/processes/${p.id}/steps`, {
                    headers: { Authorization: `Bearer ${token}` }
                });
                if (resSteps.ok) {
                    const steps = await resSteps.json();
                    fileStatusMap[p.id] = steps.some(s => s.filePath);
                }
            } catch (e) {
                console.error(`Kon stappen niet ophalen voor proces ${p.id}`);
            }
        }

        setUploadStatus(fileStatusMap);
        const allFiles = [];

        for (const p of list) {
            try {
                const resSteps = await fetch(`/api/StudentDashboard/processes/${p.id}/steps`, {
                    headers: { Authorization: `Bearer ${token}` }
                });

                if (resSteps.ok) {
                    const steps = await resSteps.json();
                    steps.forEach(s => {
                        if (s.filePath) {
                            allFiles.push({
                                name: s.filePath.split('/').pop(),
                                url: `/${s.filePath}`,
                                stepName: s.stepName
                            });
                        }
                    });
                }
            } catch (e) {
                console.error(`Fout bij ophalen bestanden voor proces ${p.id}`);
            }
        }

        setStudentFiles(allFiles);
        setLoading(false);
    };

    const handleDownloadZip = async () => {
        const res = await fetch('/api/StudentDashboard/download-zip', {
            headers: { Authorization: `Bearer ${token}` }
        });
        if (!res.ok) return toast.error('Download mislukt');

        const blob = await res.blob();
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = 'mijn_bpv_bestanden.zip';
        document.body.appendChild(link);
        link.click();
        link.remove();
    };


    useEffect(() => {
        fetchProcesses();
    }, [user, token]);

    return (
        <div className="container my-5">
            <div className="d-flex justify-content-between align-items-center mb-4">
                <h2>Mijn BPV‑processen</h2>
            </div>

            <StartProcessModal
                show={showModal}
                onClose={() => setShowModal(false)}
                onCreated={fetchProcesses}
            />

            {loading ? (
                <p>Loading processes...</p>
            ) : processes.length === 0 ? (
                <div className="alert alert-info">Je hebt nog geen BPV‑processen.</div>
            ) : (
                <>
                    <div className="row">
                        {/* Left cards */}
                        <div className="col-md-4">
                            <Card className="mb-3">
                                <Card.Body>
                                    <Card.Title>Jouw bestanden</Card.Title>
                                        {studentFiles.length > 0 ? (
                                            <>
                                                <ul className="list-unstyled mb-3">
                                                    {studentFiles.map((f, i) => (
                                                        <li key={i}>
                                                            <a href={f.url} target="_blank" rel="noreferrer">
                                                                {f.name}
                                                            </a>
                                                            <br />
                                                            <small className="text-muted">Stap: {f.stepName}</small>
                                                        </li>
                                                    ))}
                                                </ul>
                                                <Button
                                                    onClick={handleDownloadZip}
                                                    variant="primary"
                                                >
                                                    Download als ZIP
                                                </Button>
                                            </>
                                        ) : (
                                            <p className="text-muted">Nog geen bestanden geüpload.</p>
                                        )}
                                </Card.Body>
                            </Card>
                        </div>

                        {/* Main content */}
                        <div className="col-md-8 list-group">
                            {processes.map(proc => (
                                <div key={proc.id} className="mb-4 list-group-item">
                                    <div>
                                        <h5>{proc.companyName} — {proc.status}</h5>
                                        Begeleider: {proc.supervisorName}<br />
                                        <small className="text-muted">
                                            Gestart op: {new Date(proc.createdAt).toLocaleDateString()}
                                        </small>
                                    </div>
                                    <ProcessAccordion proc={proc} token={token} />
                                </div>
                            ))}
                        </div>
                    </div>
                </>
            )}

            <div className="d-flex justify-content-between align-items-start mt-4">
                <button className="btn btn-success" onClick={() => setShowModal(true)}>
                    Nieuw proces starten
                </button>
            </div>
        </div>
    );
}
