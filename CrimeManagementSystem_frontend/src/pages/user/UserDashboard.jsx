import { useState, useEffect } from 'react'
import { useNavigate } from 'react-router-dom'
import { useAuth } from '../../context/AuthContext'
import API from '../../api/axios'

export default function UserDashboard() {
  const navigate = useNavigate()
  const { name, logout } = useAuth()
  const [incidents, setIncidents] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  useEffect(() => {
    fetchIncidents()
  }, [])

  const fetchIncidents = async () => {
    try {
      const response = await API.get('/Incident/my-incidents')
      setIncidents(response.data)
    } catch (err) {
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
        <span className="navbar-brand fw-bold text-white">🚔 CMS — User Dashboard</span>
        <div className="d-flex align-items-center gap-3">
          <span className="text-white">👤 {name}</span>
          <button className="btn btn-outline-light btn-sm"
            onClick={() => navigate('/user/profile')}>Profile</button>
          <button className="btn btn-danger btn-sm" onClick={handleLogout}>Logout</button>
        </div>
      </nav>

      <div className="container py-4">

        {/* Welcome + Report button */}
        <div className="d-flex justify-content-between align-items-center mb-4">
          <div>
            <h4 className="fw-bold mb-0">Welcome, {name}!</h4>
            <p className="text-muted mb-0">Here are your reported incidents</p>
          </div>
          <button className="btn fw-bold text-white px-4"
            style={{ backgroundColor: '#1a1a2e' }}
            onClick={() => navigate('/user/create-incident')}>
            + Report New Incident
          </button>
        </div>

        {/* Stats */}
        <div className="row g-3 mb-4">
          {['Initiated', 'Active', 'Closed', 'Verified'].map(status => (
            <div className="col-6 col-md-3" key={status}>
              <div className="card border-0 shadow-sm text-center p-3">
                <h3 className="fw-bold mb-0">
                  {incidents.filter(i => i.status === status).length}
                </h3>
                <small className="text-muted">{status}</small>
              </div>
            </div>
          ))}
        </div>

        {/* Incidents List */}
        {loading && <div className="text-center py-5">Loading incidents...</div>}
        {error && <div className="alert alert-danger">{error}</div>}

        {!loading && incidents.length === 0 && (
          <div className="card border-0 shadow-sm p-5 text-center">
            <h5 className="text-muted">No incidents reported yet</h5>
            <button className="btn mt-3 fw-bold text-white"
              style={{ backgroundColor: '#1a1a2e' }}
              onClick={() => navigate('/user/create-incident')}>
              Report Your First Incident
            </button>
          </div>
        )}

        {!loading && incidents.length > 0 && (
          <div className="card border-0 shadow-sm">
            <div className="card-body p-0">
              <table className="table table-hover mb-0">
                <thead style={{ backgroundColor: '#1a1a2e', color: 'white' }}>
                  <tr>
                    <th className="p-3">Incident Code</th>
                    <th className="p-3">Type</th>
                    <th className="p-3">Location</th>
                    <th className="p-3">Status</th>
                    <th className="p-3">Date</th>
                    <th className="p-3">Action</th>
                  </tr>
                </thead>
                <tbody>
                  {incidents.map(incident => (
                    <tr key={incident.incidentId}>
                      <td className="p-3 fw-semibold">{incident.incidentCode}</td>
                      <td className="p-3">{incident.type}</td>
                      <td className="p-3">{incident.location}</td>
                      <td className="p-3">{getStatusBadge(incident.status)}</td>
                      <td className="p-3">
                        {new Date(incident.incidentDate).toLocaleDateString()}
                      </td>
                      <td className="p-3">
                        <button className="btn btn-sm btn-outline-dark"
                          onClick={() => navigate(`/user/incident/${incident.incidentId}`)}>
                          View
                        </button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        )}
      </div>
    </div>
  )
}