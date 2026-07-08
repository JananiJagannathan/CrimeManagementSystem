import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import API from '../../api/axios'

export default function CreateIncident() {
  const navigate = useNavigate()
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')

  const [formData, setFormData] = useState({
    type: 0,
    description: '',
    location: '',
    incidentDate: '',
    propertyDescription: '',
    estimatedValue: '',
    suspectDescription: '',
    graffitiImagePath: ''
  })

  const handleChange = (e) => {
    setFormData({ ...formData, [e.target.name]: e.target.value })
    setError('')
  }

  const handleSubmit = async (e) => {
    e.preventDefault()
    setLoading(true)
    setError('')

    try {
      const payload = {
        type: parseInt(formData.type),
        description: formData.description,
        location: formData.location,
        incidentDate: formData.incidentDate,
        propertyDescription: formData.propertyDescription || null,
        estimatedValue: formData.estimatedValue ? parseFloat(formData.estimatedValue) : null,
        suspectDescription: formData.suspectDescription || null,
        graffitiImagePath: formData.graffitiImagePath || null
      }

      await API.post('/Incident/create', payload)
      setSuccess('Incident reported successfully! Check your email for confirmation.')
      setTimeout(() => navigate('/user/dashboard'), 2000)
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to create incident.')
    } finally {
      setLoading(false)
    }
  }

  const incidentTypes = [
    { value: 0, label: 'Lost Property', desc: 'Property you cannot locate' },
    { value: 1, label: 'Petit Larceny', desc: 'Property taken without permission (≤ ₹1000)' },
    { value: 2, label: 'Criminal Mischief', desc: 'Intentional damage to property' },
    { value: 3, label: 'Graffiti', desc: 'Intentional drawing/etching on property' },
  ]

  return (
    <div style={{ backgroundColor: '#f0f2f5', minHeight: '100vh' }}>

      {/* Navbar */}
      <nav className="navbar px-4 py-3" style={{ backgroundColor: '#1a1a2e' }}>
        <span className="navbar-brand fw-bold text-white">🚔 CMS — Report Incident</span>
        <button className="btn btn-outline-light btn-sm"
          onClick={() => navigate('/user/dashboard')}>
          ← Back to Dashboard
        </button>
      </nav>

      <div className="container py-4">
        <div className="card border-0 shadow-sm p-4" style={{ maxWidth: '650px', margin: '0 auto' }}>

          <h4 className="fw-bold mb-1">Report New Incident</h4>
          <p className="text-muted mb-4">Fill in the details below to report an incident</p>

          {error && <div className="alert alert-danger">{error}</div>}
          {success && <div className="alert alert-success">{success}</div>}

          <form onSubmit={handleSubmit}>

            {/* Incident Type */}
            <div className="mb-4">
              <label className="form-label fw-semibold">Incident Type</label>
              <div className="row g-2">
                {incidentTypes.map(type => (
                  <div className="col-6" key={type.value}>
                    <div
                      className={`card p-3 cursor-pointer ${parseInt(formData.type) === type.value
                        ? 'border-2 border-dark' : 'border'}`}
                      style={{ cursor: 'pointer', borderRadius: '8px',
                        backgroundColor: parseInt(formData.type) === type.value ? '#e8e8ff' : 'white' }}
                      onClick={() => setFormData({ ...formData, type: type.value })}
                    >
                      <div className="fw-semibold small">{type.label}</div>
                      <div className="text-muted" style={{ fontSize: '11px' }}>{type.desc}</div>
                    </div>
                  </div>
                ))}
              </div>
            </div>

            {/* Description */}
            <div className="mb-3">
              <label className="form-label fw-semibold">Description</label>
              <textarea name="description" className="form-control" rows="3"
                placeholder="Describe what happened in detail"
                value={formData.description} onChange={handleChange} required />
            </div>

            {/* Location */}
            <div className="mb-3">
              <label className="form-label fw-semibold">Location</label>
              <input type="text" name="location" className="form-control"
                placeholder="Where did this happen?"
                value={formData.location} onChange={handleChange} required />
            </div>

            {/* Date */}
            <div className="mb-3">
              <label className="form-label fw-semibold">Incident Date</label>
              <input type="datetime-local" name="incidentDate" className="form-control"
                value={formData.incidentDate} onChange={handleChange} required />
            </div>

            {/* Type-specific fields */}
            {(parseInt(formData.type) === 0 || parseInt(formData.type) === 1) && (
              <div className="mb-3">
                <label className="form-label fw-semibold">Property Description</label>
                <input type="text" name="propertyDescription" className="form-control"
                  placeholder="Describe the property"
                  value={formData.propertyDescription} onChange={handleChange} />
              </div>
            )}

            {parseInt(formData.type) === 1 && (
              <div className="mb-3">
                <label className="form-label fw-semibold">Estimated Value (₹)</label>
                <input type="number" name="estimatedValue" className="form-control"
                  placeholder="Value must be ₹1000 or less"
                  value={formData.estimatedValue} onChange={handleChange} />
              </div>
            )}

            {parseInt(formData.type) === 2 && (
              <div className="mb-3">
                <label className="form-label fw-semibold">Suspect Description</label>
                <textarea name="suspectDescription" className="form-control" rows="2"
                  placeholder="Describe the suspect if known"
                  value={formData.suspectDescription} onChange={handleChange} />
              </div>
            )}

            {parseInt(formData.type) === 3 && (
  <div className="mb-3">
    <label className="form-label fw-semibold">Graffiti Image</label>
    <input
      type="file"
      className="form-control"
      accept="image/*"
      onChange={async (e) => {
        const file = e.target.files[0]
        if (!file) return
        const fd = new FormData()
        fd.append('file', file)
        try {
          const res = await API.post('/Incident/upload-graffiti', fd, {
            headers: { 'Content-Type': 'multipart/form-data' }
          })
          setFormData(prev => ({ ...prev, graffitiImagePath: res.data.imagePath }))
          setSuccess('Image uploaded successfully!')
        } catch {
          setError('Image upload failed. Please try again.')
        }
      }}
    />
    {formData.graffitiImagePath && (
      <small className="text-success mt-1 d-block">
        ✅ Image uploaded: {formData.graffitiImagePath}
      </small>
    )}
  </div>
)}

            <div className="d-flex gap-2 mt-4">
              <button type="submit"
                className="btn fw-bold text-white px-4"
                style={{ backgroundColor: '#1a1a2e' }}
                disabled={loading}>
                {loading ? 'Submitting...' : 'Submit Report'}
              </button>
              <button type="button"
                className="btn btn-outline-secondary px-4"
                onClick={() => navigate('/user/dashboard')}>
                Cancel
              </button>
            </div>

          </form>
        </div>
      </div>
    </div>
  )
}