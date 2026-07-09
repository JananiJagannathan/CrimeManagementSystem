import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import API from '../../api/axios'

export default function AddOfficer() {
  const navigate = useNavigate()
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')
  const [showPassword, setShowPassword] = useState(false)

  const [formData, setFormData] = useState({
    name: '', email: '', password: '',
    badgeNumber: '', phone: '', address: ''
  })

  const [formErrors, setFormErrors] = useState({})

  const validate = () => {
    const errors = {}

    if (!formData.name.trim())
      errors.name = 'Full name is required'
    else if (formData.name.trim().length < 2)
      errors.name = 'Full name must be at least 2 characters'

    if (!formData.email.trim())
      errors.email = 'Email is required'
    else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(formData.email))
      errors.email = 'Enter a valid email address (e.g. officer@cms.com)'

    if (!formData.password)
      errors.password = 'Password is required'
    else if (formData.password.length < 8)
      errors.password = 'Password must be at least 8 characters'
    else if (!/(?=.*[A-Z])/.test(formData.password))
      errors.password = 'Password must contain at least 1 uppercase letter'
    else if (!/(?=.*[a-z])/.test(formData.password))
      errors.password = 'Password must contain at least 1 lowercase letter'
    else if (!/(?=.*\d)/.test(formData.password))
      errors.password = 'Password must contain at least 1 number'
    else if (!/(?=.*[@$!%*?&])/.test(formData.password))
      errors.password = 'Password must contain at least 1 special character (@$!%*?&)'

    if (!formData.badgeNumber.trim())
      errors.badgeNumber = 'Badge number is required'
    else if (!/^[A-Z]{2}-\d{3}$/.test(formData.badgeNumber))
      errors.badgeNumber = 'Badge number format must be like TN-001 (2 uppercase letters, dash, 3 digits)'

    if (!formData.phone.trim())
      errors.phone = 'Phone number is required'
    else if (!/^\d{10}$/.test(formData.phone))
      errors.phone = 'Phone number must be exactly 10 digits'

    if (!formData.address.trim())
      errors.address = 'Address is required'
    else if (formData.address.trim().length < 5)
      errors.address = 'Address must be at least 5 characters'

    return errors
  }

  const handleChange = (e) => {
    setFormData({ ...formData, [e.target.name]: e.target.value })
    setFormErrors({ ...formErrors, [e.target.name]: '' })
    setError('')
  }

  const handleSubmit = async (e) => {
    e.preventDefault()
    setError(''); setSuccess('')

    const errors = validate()
    if (Object.keys(errors).length > 0) {
      setFormErrors(errors)
      return
    }

    setLoading(true)
    try {
      await API.post('/Officer/add', formData)
      setSuccess('Officer added successfully! Redirecting...')
      setTimeout(() => navigate('/stationhead/officers'), 2000)
    } catch (err) {
      const msg = err.response?.data?.message || ''
      if (msg.toLowerCase().includes('email') &&
         (msg.toLowerCase().includes('exist') ||
          msg.toLowerCase().includes('already') ||
          msg.toLowerCase().includes('duplicate'))) {
        setFormErrors({
          ...formErrors,
          email: 'This email is already registered. Please use a different email.'
        })
      } else if (msg.toLowerCase().includes('badge')) {
        setFormErrors({
          ...formErrors,
          badgeNumber: 'This badge number is already in use. Please use a different one.'
        })
      } else {
        setError(msg || 'Failed to add officer. Please try again.')
      }
    } finally {
      setLoading(false)
    }
  }

  const inputClass = (field) =>
    `form-control ${formErrors[field] ? 'is-invalid' : ''}`

  return (
    <div style={{ backgroundColor: '#f0f2f5', minHeight: '100vh' }}>

      <nav className="navbar px-4 py-3" style={{ backgroundColor: '#1a1a2e' }}>
        <span className="navbar-brand fw-bold text-white">🚔 CMS — Add Officer</span>
        <button className="btn btn-outline-light btn-sm"
          onClick={() => navigate('/stationhead/officers')}>
          ← Back to Officers
        </button>
      </nav>

      <div className="container py-4">
        <div className="card border-0 shadow-sm p-4"
          style={{ maxWidth: '550px', margin: '0 auto' }}>

          <h4 className="fw-bold mb-1">Add New Officer</h4>
          <p className="text-muted mb-4">Register a new officer to the system</p>

          {error && <div className="alert alert-danger">{error}</div>}
          {success && <div className="alert alert-success">{success}</div>}

          <form onSubmit={handleSubmit} noValidate>
            <div className="row g-3">

              {/* Full Name */}
              <div className="col-12">
                <label className="form-label fw-semibold">
                  Full Name <span className="text-danger">*</span>
                </label>
                <input type="text" name="name"
                  className={inputClass('name')}
                  placeholder="Officer full name"
                  value={formData.name}
                  onChange={handleChange} />
                {formErrors.name && (
                  <div className="invalid-feedback">{formErrors.name}</div>
                )}
              </div>

              {/* Email */}
              <div className="col-12">
                <label className="form-label fw-semibold">
                  Email <span className="text-danger">*</span>
                </label>
                <input type="email" name="email"
                  className={inputClass('email')}
                  placeholder="Officer email (e.g. officer@cms.com)"
                  value={formData.email}
                  onChange={handleChange} />
                {formErrors.email && (
                  <div className="invalid-feedback">{formErrors.email}</div>
                )}
              </div>

              {/* Password */}
              <div className="col-12">
                <label className="form-label fw-semibold">
                  Password <span className="text-danger">*</span>
                </label>
                <div className="input-group">
                  <input
                    type={showPassword ? 'text' : 'password'}
                    name="password"
                    className={inputClass('password')}
                    placeholder="Min 8 chars, uppercase, number, special char"
                    value={formData.password}
                    onChange={handleChange} />
                  <button type="button"
                    className="btn btn-outline-secondary"
                    onClick={() => setShowPassword(!showPassword)}>
                    {showPassword ? '🙈' : '👁️'}
                  </button>
                  {formErrors.password && (
                    <div className="invalid-feedback d-block">{formErrors.password}</div>
                  )}
                </div>
                <small className="text-muted">
                  Must contain uppercase, lowercase, number and special character (@$!%*?&)
                </small>
              </div>

              {/* Badge Number */}
              <div className="col-md-6">
                <label className="form-label fw-semibold">
                  Badge Number <span className="text-danger">*</span>
                </label>
                <input type="text" name="badgeNumber"
                  className={inputClass('badgeNumber')}
                  placeholder="e.g. TN-001"
                  maxLength={6}
                  value={formData.badgeNumber}
                  onChange={(e) => {
                    e.target.value = e.target.value.toUpperCase()
                    handleChange(e)
                  }} />
                {formErrors.badgeNumber && (
                  <div className="invalid-feedback">{formErrors.badgeNumber}</div>
                )}
                <small className="text-muted">Format: XX-000 (e.g. TN-001)</small>
              </div>

              {/* Phone */}
              <div className="col-md-6">
                <label className="form-label fw-semibold">
                  Phone <span className="text-danger">*</span>
                </label>
                <input type="text" name="phone"
                  className={inputClass('phone')}
                  placeholder="10 digit phone number"
                  maxLength={10}
                  value={formData.phone}
                  onChange={handleChange} />
                {formErrors.phone && (
                  <div className="invalid-feedback">{formErrors.phone}</div>
                )}
              </div>

              {/* Address */}
              <div className="col-12">
                <label className="form-label fw-semibold">
                  Address <span className="text-danger">*</span>
                </label>
                <textarea name="address"
                  className={inputClass('address')}
                  rows="2"
                  placeholder="Officer address/station"
                  value={formData.address}
                  onChange={handleChange} />
                {formErrors.address && (
                  <div className="invalid-feedback">{formErrors.address}</div>
                )}
              </div>

            </div>

            <small className="text-muted d-block mt-2 mb-3">
              <span className="text-danger">*</span> All fields are required
            </small>

            <div className="d-flex gap-2">
              <button type="submit"
                className="btn fw-bold text-white px-4"
                style={{ backgroundColor: '#1a1a2e' }}
                disabled={loading}>
                {loading ? 'Adding...' : 'Add Officer'}
              </button>
              <button type="button"
                className="btn btn-outline-secondary px-4"
                onClick={() => navigate('/stationhead/officers')}>
                Cancel
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  )
}