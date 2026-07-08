import { useState, useEffect } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import API from '../../api/axios'

export default function IncidentDetails() {
  const navigate = useNavigate()
  const { id } = useParams()
  const [incident, setIncident] = useState(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [downloading, setDownloading] = useState(false)

  useEffect(() => {
    fetchIncident()
  }, [id])

  const fetchIncident = async () => {
    try {
      const response = await API.get(`/Incident/${id}`)
      setIncident(response.data)
    } catch (err) {
      setError('Failed to load incident details.')
    } finally {
      setLoading(false)
    }
  }

  const handleDownloadPDF = async () => {
    setDownloading(true)
    try {
      const response = await API.get(`/Incident/download/${id}`, {
        responseType: 'blob'
      })
      const url = window.URL.createObjectURL(new Blob([response.data]))
      const link = document.createElement('a')
      link.href = url
      link.setAttribute('download', `Incident_${incident.incidentCode}.pdf`)
      document.body.appendChild(link)
      link.click()
      link.remove()
    } catch (err) {
      setError('Failed to download PDF.')
    } finally {
      setDownloading(false)
    }
  }

  const getStatusBadge = (status) => {
    const colors = {
      Initiated: 'primary',
      Active: 'warning',
      Closed: 'danger',
      Verified: 'success'
    }
    return (
      <span className={`badge bg-${colors[status] || 'secondary'} fs-6 px-3 py-2`}>
        {status}
      </span>
    )
  }

  if (loading) return (
    <div className="min-vh-100 d-flex align-items-center justify-content-center">
      <div className="text-center">
        <div className="spinner-border" style={{ color: '#1a1a2e' }}></div>
        <p className="mt-2">Loading incident details...</p>
      </div>
    </div>
  )

  if (error) return (
    <div className="min-vh-100 d-flex align-items-center justify-content-center">
      <div className="alert alert-danger">{error}</div>
    </div>
  )

  return (
    <div style={{ backgroundColor: '#f0f2f5', minHeight: '100vh' }}>

      {/* Navbar */}
      <nav className="navbar px-4 py-3" style={{ backgroundColor: '#1a1a2e' }}>
        <span className="navbar-brand fw-bold text-white">🚔 CMS — Incident Details</span>
        <button className="btn btn-outline-light btn-sm"
          onClick={() => navigate('/user/dashboard')}>
          ← Back to Dashboard
        </button>
      </nav>

      <div className="container py-4">
        <div className="card border-0 shadow-sm p-4"
          style={{ maxWidth: '700px', margin: '0 auto' }}>

          {/* Header */}
          <div className="d-flex justify-content-between align-items-start mb-4">
            <div>
              <h4 className="fw-bold mb-1">{incident.incidentCode}</h4>
              <p className="text-muted mb-0">
                Reported on {new Date(incident.reportedAt).toLocaleDateString()}
              </p>
            </div>
            {getStatusBadge(incident.status)}
          </div>

          {/* Details */}
          <div className="row g-3">
            <div className="col-md-6">
              <div className="p-3 rounded" style={{ backgroundColor: '#f8f9fa' }}>
                <small className="text-muted fw-semibold">TYPE</small>
                <p className="mb-0 fw-bold">{incident.type}</p>
              </div>
            </div>
            <div className="col-md-6">
              <div className="p-3 rounded" style={{ backgroundColor: '#f8f9fa' }}>
                <small className="text-muted fw-semibold">LOCATION</small>
                <p className="mb-0 fw-bold">{incident.location}</p>
              </div>
            </div>
            <div className="col-md-6">
              <div className="p-3 rounded" style={{ backgroundColor: '#f8f9fa' }}>
                <small className="text-muted fw-semibold">INCIDENT DATE</small>
                <p className="mb-0 fw-bold">
                  {new Date(incident.incidentDate).toLocaleDateString()}
                </p>
              </div>
            </div>
            <div className="col-md-6">
              <div className="p-3 rounded" style={{ backgroundColor: '#f8f9fa' }}>
                <small className="text-muted fw-semibold">ASSIGNED OFFICER</small>
                <p className="mb-0 fw-bold">
                  {incident.assignedOfficerName || 'Not yet assigned'}
                </p>
              </div>
            </div>
            <div className="col-12">
              <div className="p-3 rounded" style={{ backgroundColor: '#f8f9fa' }}>
                <small className="text-muted fw-semibold">DESCRIPTION</small>
                <p className="mb-0">{incident.description}</p>
              </div>
            </div>

            {incident.propertyDescription && (
              <div className="col-12">
                <div className="p-3 rounded" style={{ backgroundColor: '#f8f9fa' }}>
                  <small className="text-muted fw-semibold">PROPERTY DESCRIPTION</small>
                  <p className="mb-0">{incident.propertyDescription}</p>
                </div>
              </div>
            )}
            {incident.estimatedValue && (
              <div className="col-md-6">
                <div className="p-3 rounded" style={{ backgroundColor: '#f8f9fa' }}>
                  <small className="text-muted fw-semibold">ESTIMATED VALUE</small>
                  <p className="mb-0 fw-bold">₹{incident.estimatedValue}</p>
                </div>
              </div>
            )}
            {incident.suspectDescription && (
              <div className="col-12">
                <div className="p-3 rounded" style={{ backgroundColor: '#f8f9fa' }}>
                  <small className="text-muted fw-semibold">SUSPECT DESCRIPTION</small>
                  <p className="mb-0">{incident.suspectDescription}</p>
                </div>
              </div>
            )}
          </div>

          {/* Status Timeline — Centered */}
          <div className="mt-4 text-center">
            <h6 className="fw-bold mb-4">Investigation Progress</h6>
            <div className="d-flex align-items-center justify-content-center">
              {['Initiated', 'Active', 'Closed', 'Verified'].map((step, idx) => {
                const steps = ['Initiated', 'Active', 'Closed', 'Verified']
                const currentIdx = steps.indexOf(incident.status)
                const isDone = idx <= currentIdx
                return (
                  <div key={step} className="d-flex align-items-center">
                    <div className="d-flex flex-column align-items-center">
                      <div
                        className="rounded-circle d-flex align-items-center justify-content-center fw-bold"
                        style={{
                          width: '44px', height: '44px',
                          backgroundColor: isDone ? '#1a1a2e' : '#dee2e6',
                          color: isDone ? 'white' : '#6c757d',
                          fontSize: '16px'
                        }}>
                        {isDone ? '✓' : idx + 1}
                      </div>
                      <small className="mt-2 fw-semibold"
                        style={{
                          fontSize: '12px',
                          color: isDone ? '#1a1a2e' : '#6c757d'
                        }}>
                        {step}
                      </small>
                    </div>
                    {idx < 3 && (
                      <div style={{
                        height: '3px',
                        width: '60px',
                        backgroundColor: idx < currentIdx ? '#1a1a2e' : '#dee2e6',
                        marginBottom: '22px'
                      }} />
                    )}
                  </div>
                )
              })}
            </div>
          </div>

          {/* Download PDF */}
          <div className="mt-4 text-center">
            <button
              className="btn fw-bold text-white px-4"
              style={{ backgroundColor: '#1a1a2e' }}
              onClick={handleDownloadPDF}
              disabled={downloading}>
              {downloading ? 'Downloading...' : '📄 Download Incident Report (PDF)'}
            </button>
          </div>

        </div>
      </div>
    </div>
  )
}