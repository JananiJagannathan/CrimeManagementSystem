import { useState, useEffect } from 'react'
import { useNavigate } from 'react-router-dom'
import { useAuth } from '../../context/AuthContext'
import API from '../../api/axios'

export default function OfficerDashboard() {
  const navigate = useNavigate()
  const { name, logout } = useAuth()
  const [incidents, setIncidents] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [closingId, setClosingId] = useState(null)
  const [successMsg, setSuccessMsg] = useState('')

  useEffect(() => {
    fetchIncidents()
  }, [])

  const fetchIncidents = async () => {
    try {
      const response = await API.get('/Officer/my-incidents')
      setIncidents(response.data)
    } catch {
      setError('Failed to load incidents.')
    } finally {
      setLoading(false)
    }
  }

  const handleLogout = async () => {
    try { await API.post('/Auth/logout') } catch {}
    logout()
    navigate('/')
  }

  const handleClose = async (incidentId) => {
    if (!window.confirm('Are you sure you want to close this incident?')) return
    setClosingId(incidentId)
    try {
      await API.put(`/Incident/close/${incidentId}`)
      setSuccessMsg('Incident closed successfully!')
      fetchIncidents()
      setTimeout(() => setSuccessMsg(''), 3000)
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to close incident.')
    } finally {
      setClosingId(null)
    }
  }

  const handleDownloadPDF = async (incident) => {
    try {
      const response = await API.get(`/Incident/download/${incident.incidentId}`, {
        responseType: 'blob'
      })
      const url = window.URL.createObjectURL(new Blob([response.data]))
      const link = document.createElement('a')
      link.href = url
      link.setAttribute('download', `Incident_${incident.incidentCode}.pdf`)
      document.body.appendChild(link)
      link.click()
      link.remove()
    } catch {
      setError('Failed to download PDF.')
    }
  }

  const getStatusBadge = (status) => {
    const colors = {
      Initiated: 'primary',
      Active: 'warning',
      Closed: 'danger',
      Verified: 'success'
    }
    return <span className={`badge bg-${colors[status] || 'secondary'}`}>{status}</span>
  }

  return (
    <div style={{ backgroundColor: '#f0f2f5', minHeight: '100vh' }}>

      {/* Navbar */}
      <nav className="navbar px-4 py-3" style={{ backgroundColor: '#1a1a2e' }}>
        <span className="navbar-brand fw-bold text-white">🚔 CMS — Officer Dashboard</span>
        <div className="d-flex align-items-center gap-3">
          <span className="text-white">👮 {name}</span>
          <button className="btn btn-outline-light btn-sm"
            onClick={() => navigate('/officer/profile')}>Profile</button>
          <button className="btn btn-danger btn-sm" onClick={handleLogout}>Logout</button>
        </div>
      </nav>

      <div className="container py-4">

{/* Header */}
<div className="mb-4">
  <h4 className="fw-bold mb-0">Hello, {name}!</h4>
  <p className="text-muted mb-0">My Assigned Incidents</p>
</div>

        {/* Stats */}
        <div className="row g-3 mb-4">
          {['Active', 'Closed', 'Verified'].map(status => (
            <div className="col-4" key={status}>
              <div className="card border-0 shadow-sm text-center p-3">
                <h3 className="fw-bold mb-0">
                  {incidents.filter(i => i.status === status).length}
                </h3>
                <small className="text-muted">{status}</small>
              </div>
            </div>
          ))}
        </div>

        {successMsg && <div className="alert alert-success">{successMsg}</div>}
        {error && <div className="alert alert-danger">{error}</div>}

        {loading && (
          <div className="text-center py-5">
            <div className="spinner-border" style={{ color: '#1a1a2e' }}></div>
          </div>
        )}

        {!loading && incidents.length === 0 && (
          <div className="card border-0 shadow-sm p-5 text-center">
            <h5 className="text-muted">No incidents assigned to you yet</h5>
            <p className="text-muted">The Station Head will assign cases to you shortly.</p>
          </div>
        )}

        {!loading && incidents.length > 0 && (
          <div className="row g-3">
            {incidents.map(incident => (
              <div className="col-12" key={incident.incidentId}>
                <div className="card border-0 shadow-sm p-4">
                  <div className="d-flex justify-content-between align-items-start">
                    <div className="flex-grow-1">
                      <div className="d-flex align-items-center gap-2 mb-2">
                        <h6 className="fw-bold mb-0">{incident.incidentCode}</h6>
                        {getStatusBadge(incident.status)}
                      </div>
                      <div className="row g-2">
                        <div className="col-md-3">
                          <small className="text-muted">Type</small>
                          <p className="mb-0 fw-semibold">{incident.type}</p>
                        </div>
                        <div className="col-md-3">
                          <small className="text-muted">Location</small>
                          <p className="mb-0 fw-semibold">{incident.location}</p>
                        </div>
                        <div className="col-md-3">
                          <small className="text-muted">Reported By</small>
                          <p className="mb-0 fw-semibold">{incident.reportedByName}</p>
                        </div>
                        <div className="col-md-3">
                          <small className="text-muted">Date</small>
                          <p className="mb-0 fw-semibold">
                            {new Date(incident.incidentDate).toLocaleDateString()}
                          </p>
                        </div>
                      </div>
                      <div className="mt-2">
                        <small className="text-muted">Description: </small>
                        <small>{incident.description}</small>
                      </div>
                    </div>

                    {/* Actions */}
                    <div className="d-flex flex-column gap-2 ms-3">
                      <button className="btn btn-sm btn-outline-dark"
                        onClick={() => handleDownloadPDF(incident)}>
                        📄 PDF
                      </button>
                      {incident.status === 'Active' && (
                        <button
                          className="btn btn-sm btn-danger fw-bold"
                          onClick={() => handleClose(incident.incidentId)}
                          disabled={closingId === incident.incidentId}>
                          {closingId === incident.incidentId ? 'Closing...' : 'Close Case'}
                        </button>
                      )}
                    </div>
                  </div>
                </div>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  )
}