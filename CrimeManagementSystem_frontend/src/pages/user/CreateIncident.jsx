import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import API from '../../api/axios'

export default function CreateIncident() {
  const navigate = useNavigate()
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')
  const [formErrors, setFormErrors] = useState({})

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
    setFormErrors({ ...formErrors, [e.target.name]: '' })
    setError('')
  }

  const validate = () => {
    const errors = {}

    if (!formData.description.trim())
      errors.description = 'Description is required'

    if (!formData.location.trim())
      errors.location = 'Location is required'

    if (!formData.incidentDate) {
  errors.incidentDate = 'Incident date is required'
} else {
  const selectedDate = new Date(formData.incidentDate)
  const now = new Date()

  if (selectedDate > now) {
    errors.incidentDate = 'Incident date cannot be in the future'
  }
}

    // Type-specific validations
    if (parseInt(formData.type) === 0 || parseInt(formData.type) === 1) {
      if (!formData.propertyDescription.trim())
        errors.propertyDescription = 'Property description is required'
    }

    if (parseInt(formData.type) === 1) {
      if (!formData.estimatedValue)
        errors.estimatedValue = 'Estimated value is required'
      else if (parseFloat(formData.estimatedValue) > 1000)
        errors.estimatedValue = 'Estimated value cannot exceed ₹1000'
      else if (parseFloat(formData.estimatedValue) <= 0)
        errors.estimatedValue = 'Estimated value must be greater than 0'
    }

    if (parseInt(formData.type) === 2) {
      if (!formData.suspectDescription.trim())
        errors.suspectDescription = 'Suspect description is required'
    }

    if (parseInt(formData.type) === 3) {
      if (!formData.graffitiImagePath)
        errors.graffitiImagePath = 'Graffiti image is required'
    }

    return errors
  }

  const handleSubmit = async (e) => {
    e.preventDefault()
    setLoading(false)
    setError('')

    const errors = validate()
    if (Object.keys(errors).length > 0) {
      setFormErrors(errors)
      return
    }

    setLoading(true)
    try {
      const payload = {
        type: parseInt(formData.type),
        description: formData.description,
        location: formData.location,
        incidentDate: formData.incidentDate,
        propertyDescription: formData.propertyDescription || null,
        estimatedValue: formData.estimatedValue
          ? parseFloat(formData.estimatedValue) : null,
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

  const inputClass = (field) =>
    `form-control ${formErrors[field] ? 'is-invalid' : ''}`

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
        <div className="card border-0 shadow-sm p-4"
          style={{ maxWidth: '650px', margin: '0 auto' }}>

          <h4 className="fw-bold mb-1">Report New Incident</h4>
          <p className="text-muted mb-4">Fill in the details below to report an incident</p>

          {error && <div className="alert alert-danger">{error}</div>}
          {success && <div className="alert alert-success">{success}</div>}

          <form onSubmit={handleSubmit} noValidate>

            {/* Incident Type */}
            <div className="mb-4">
              <label className="form-label fw-semibold">
                Incident Type <span className="text-danger">*</span>
              </label>
              <div className="row g-2">
                {incidentTypes.map(type => (
                  <div className="col-6" key={type.value}>
                    <div
                      className="card p-3"
                      style={{
                        cursor: 'pointer',
                        borderRadius: '8px',
                        border: parseInt(formData.type) === type.value
                          ? '2px solid #1a1a2e' : '1px solid #dee2e6',
                        backgroundColor: parseInt(formData.type) === type.value
                          ? '#e8e8ff' : 'white'
                      }}
                      onClick={() => {
                        setFormData({
                          ...formData, type: type.value,
                          propertyDescription: '',
                          estimatedValue: '',
                          suspectDescription: '',
                          graffitiImagePath: ''
                        })
                        setFormErrors({})
                      }}>
                      <div className="fw-semibold small">{type.label}</div>
                      <div className="text-muted" style={{ fontSize: '11px' }}>
                        {type.desc}
                      </div>
                    </div>
                  </div>
                ))}
              </div>
            </div>

            {/* Description */}
            <div className="mb-3">
              <label className="form-label fw-semibold">
                Description <span className="text-danger">*</span>
              </label>
              <textarea name="description"
                className={inputClass('description')}
                rows="3"
                placeholder="Describe what happened in detail"
                value={formData.description}
                onChange={handleChange} />
              {formErrors.description && (
                <div className="invalid-feedback">{formErrors.description}</div>
              )}
            </div>

            {/* Location */}
            <div className="mb-3">
              <label className="form-label fw-semibold">
                Location <span className="text-danger">*</span>
              </label>
              <input type="text" name="location"
                className={inputClass('location')}
                placeholder="Where did this happen?"
                value={formData.location}
                onChange={handleChange} />
              {formErrors.location && (
                <div className="invalid-feedback">{formErrors.location}</div>
              )}
            </div>

            {/* Date */}
            <div className="mb-3">
              <label className="form-label fw-semibold">
                Incident Date <span className="text-danger">*</span>
              </label>
              <input
  type="datetime-local"
  name="incidentDate"
  className={inputClass('incidentDate')}
  value={formData.incidentDate}
  onChange={handleChange}
  max={new Date().toISOString().slice(0, 16)}
/>
              {formErrors.incidentDate && (
                <div className="invalid-feedback">{formErrors.incidentDate}</div>
              )}
              
            </div>

            {/* Lost Property or Petit Larceny — Property Description */}
            {(parseInt(formData.type) === 0 || parseInt(formData.type) === 1) && (
              <div className="mb-3">
                <label className="form-label fw-semibold">
                  Property Description <span className="text-danger">*</span>
                </label>
                <input type="text" name="propertyDescription"
                  className={inputClass('propertyDescription')}
                  placeholder="Describe the property"
                  value={formData.propertyDescription}
                  onChange={handleChange} />
                {formErrors.propertyDescription && (
                  <div className="invalid-feedback">{formErrors.propertyDescription}</div>
                )}
              </div>
            )}

            {/* Petit Larceny — Estimated Value */}
            {parseInt(formData.type) === 1 && (
              <div className="mb-3">
                <label className="form-label fw-semibold">
                  Estimated Value (₹) <span className="text-danger">*</span>
                </label>
                <input type="number" name="estimatedValue"
                  className={inputClass('estimatedValue')}
                  placeholder="Value must be ₹1000 or less"
                  value={formData.estimatedValue}
                  onChange={handleChange}
                  min="1" max="1000" />
                {formErrors.estimatedValue && (
                  <div className="invalid-feedback">{formErrors.estimatedValue}</div>
                )}
                <small className="text-muted">Maximum value: ₹1000</small>
              </div>
            )}

            {/* Criminal Mischief — Suspect Description */}
            {parseInt(formData.type) === 2 && (
              <div className="mb-3">
                <label className="form-label fw-semibold">
                  Suspect Description <span className="text-danger">*</span>
                </label>
                <textarea name="suspectDescription"
                  className={inputClass('suspectDescription')}
                  rows="2"
                  placeholder="Describe the suspect if known"
                  value={formData.suspectDescription}
                  onChange={handleChange} />
                {formErrors.suspectDescription && (
                  <div className="invalid-feedback">{formErrors.suspectDescription}</div>
                )}
              </div>
            )}

            {/* Graffiti — Image Upload */}
            {parseInt(formData.type) === 3 && (
              <div className="mb-3">
                <label className="form-label fw-semibold">
                  Graffiti Image <span className="text-danger">*</span>
                </label>
                <input type="file"
                  className={`form-control ${formErrors.graffitiImagePath ? 'is-invalid' : ''}`}
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
                      setFormData(prev => ({
                        ...prev, graffitiImagePath: res.data.imagePath
                      }))
                      setFormErrors(prev => ({ ...prev, graffitiImagePath: '' }))
                      setSuccess('Image uploaded successfully!')
                    } catch {
                      setFormErrors(prev => ({
                        ...prev,
                        graffitiImagePath: 'Image upload failed. Please try again.'
                      }))
                    }
                  }} />
                {formErrors.graffitiImagePath && (
                  <div className="invalid-feedback">{formErrors.graffitiImagePath}</div>
                )}
                {formData.graffitiImagePath && (
                  <small className="text-success d-block mt-1">
                    ✅ Image uploaded successfully
                  </small>
                )}
              </div>
            )}

            <small className="text-muted d-block mb-3">
              <span className="text-danger">*</span> All fields are required
            </small>

            <div className="d-flex gap-2">
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