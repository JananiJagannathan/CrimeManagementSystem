import { useState, useEffect } from 'react'
import { useNavigate } from 'react-router-dom'
import API from '../../api/axios'

export default function AllIncidents() {
  const navigate = useNavigate()
  const [incidents, setIncidents] = useState([])
  const [officers, setOfficers] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')
  const [filter, setFilter] = useState('All')
  const [assignData, setAssignData] = useState({ incidentId: null, officerId: '' })
  const [verifyingId, setVerifyingId] = useState(null)

  useEffect(() => { fetchData() }, [])

  const fetchData = async () => {
    try {
      const [incRes, offRes] = await Promise.all([
        API.get('/Incident/all'),
        API.get('/Officer/all')
      ])
      setIncidents(incRes.data)
      setOfficers(offRes.data.filter(o => o.isActive))
    } catch {
      setError('Failed to load data.')
    } finally {
      setLoading(false)
    }
  }

  const handleAssign = async (incidentId) => {
    if (!assignData.officerId) return alert('Please select an officer!')
    try {
      await API.post('/Incident/assign', {
        incidentId,
        officerId: parseInt(assignData.officerId)
      })
      setSuccess('Officer assigned successfully!')
      setAssignData({ incidentId: null, officerId: '' })
      fetchData()
      setTimeout(() => setSuccess(''), 3000)
    } catch (err) {
      setError(err.response?.data?.message || 'Assignment failed.')
    }
  }

  const handleVerify = async (incidentId) => {
    if (!window.confirm('Verify this incident as resolved?')) return
    setVerifyingId(incidentId)
    try {
      await API.put(`/Incident/verify/${incidentId}`)
      setSuccess('Incident verified successfully!')
      fetchData()
      setTimeout(() => setSuccess(''), 3000)
    } catch (err) {
      setError(err.response?.data?.message || 'Verification failed.')
    } finally {
      setVerifyingId(null)
    }
  }

  const handleDownloadPDF = async (incident) => {
    try {
      const response = await API.get(`/Incident/download/${incident.incidentId}`,
        { responseType: 'blob' })
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
      Initiated: 'primary', Active: 'warning',
      Closed: 'danger', Verified: 'success'
    }
    return <span className={`badge bg-${colors[status] || 'secondary'}`}>{status}</span>
  }

  const filtered = filter === 'All'
    ? incidents
    : incidents.filter(i => i.status === filter)

  return (
    <div style={{ backgroundColor: '#f0f2f5', minHeight: '100vh' }}>

      <nav className="navbar px-4 py-3" style={{ backgroundColor: '#1a1a2e' }}>
        <span className="navbar-brand fw-bold text-white">🚔 CMS — All Incidents</span>
        <button className="btn btn-outline-light btn-sm"
          onClick={() => navigate('/stationhead/dashboard')}>← Dashboard</button>
      </nav>

      <div className="container py-4">

        {/* Filter */}
        <div className="d-flex gap-2 mb-4 flex-wrap">
          {['All', 'Initiated', 'Active', 'Closed', 'Verified'].map(f => (
            <button key={f}
              className={`btn btn-sm fw-semibold ${filter === f
                ? 'text-white' : 'btn-outline-secondary'}`}
              style={filter === f ? { backgroundColor: '#1a1a2e' } : {}}
              onClick={() => setFilter(f)}>
              {f} ({f === 'All' ? incidents.length
                : incidents.filter(i => i.status === f).length})
            </button>
          ))}
        </div>

        {success && <div className="alert alert-success">{success}</div>}
        {error && <div className="alert alert-danger">{error}</div>}

        {loading ? (
          <div className="text-center py-5">
            <div className="spinner-border" style={{ color: '#1a1a2e' }}></div>
          </div>
        ) : (
          <div className="row g-3">
            {filtered.map(incident => (
              <div className="col-12" key={incident.incidentId}>
                <div className="card border-0 shadow-sm p-4">
                  <div className="d-flex justify-content-between align-items-start flex-wrap gap-3">
                    <div className="flex-grow-1">
                      <div className="d-flex align-items-center gap-2 mb-2">
                        <h6 className="fw-bold mb-0">{incident.incidentCode}</h6>
                        {getStatusBadge(incident.status)}
                      </div>
                      <div className="row g-2">
                        <div className="col-md-3">
                          <small className="text-muted">Type</small>
                          <p className="mb-0 fw-semibold small">{incident.type}</p>
                        </div>
                        <div className="col-md-3">
                          <small className="text-muted">Location</small>
                          <p className="mb-0 fw-semibold small">{incident.location}</p>
                        </div>
                        <div className="col-md-3">
                          <small className="text-muted">Reported By</small>
                          <p className="mb-0 fw-semibold small">{incident.reportedByName}</p>
                        </div>
                        <div className="col-md-3">
                          <small className="text-muted">Assigned Officer</small>
                          <p className="mb-0 fw-semibold small">
                            {incident.assignedOfficerName || 'Not assigned'}
                          </p>
                        </div>
                      </div>

                      {/* Assign Officer (only for Initiated) */}
                      {incident.status === 'Initiated' && (
                        <div className="d-flex gap-2 mt-3 align-items-center">
                          <select className="form-select form-select-sm"
                            style={{ maxWidth: '220px' }}
                            value={assignData.incidentId === incident.incidentId
                              ? assignData.officerId : ''}
                            onChange={(e) => setAssignData({
                              incidentId: incident.incidentId,
                              officerId: e.target.value
                            })}>
                            <option value="">Select Officer</option>
                            {officers.map(o => (
                              <option key={o.officerId} value={o.officerId}>
                                {o.name} ({o.activeIncidentsCount} active)
                              </option>
                            ))}
                          </select>
                          <button className="btn btn-sm fw-bold text-white"
                            style={{ backgroundColor: '#1a1a2e' }}
                            onClick={() => handleAssign(incident.incidentId)}>
                            Assign
                          </button>
                        </div>
                      )}
                    </div>

                    {/* Actions */}
                    <div className="d-flex flex-column gap-2">
                      <button className="btn btn-sm btn-outline-dark"
                        onClick={() => handleDownloadPDF(incident)}>
                        📄 PDF
                      </button>
                      {incident.status === 'Closed' && (
                        <button className="btn btn-sm btn-success fw-bold"
                          onClick={() => handleVerify(incident.incidentId)}
                          disabled={verifyingId === incident.incidentId}>
                          {verifyingId === incident.incidentId ? '...' : '✅ Verify'}
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