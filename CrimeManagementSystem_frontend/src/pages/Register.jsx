import { useState } from 'react'
import { useNavigate, Link } from 'react-router-dom'
import API from '../api/axios'

export default function Register() {
  const navigate = useNavigate()

  const [formData, setFormData] = useState({
    name: '',
    email: '',
    password: '',
    aadhaarNumber: '',
    panNumber: '',
    dateOfBirth: '',
    address: '',
    phone: ''
  })

  const [errors, setErrors] = useState({})
  const [serverError, setServerError] = useState('')
  const [success, setSuccess] = useState('')
  const [loading, setLoading] = useState(false)
  const [showPassword, setShowPassword] = useState(false)

  const validate = () => {
    const newErrors = {}

    if (!formData.name.trim() || formData.name.trim().length < 2)
      newErrors.name = 'Full name must be at least 2 characters'

    if (!formData.email.trim())
      newErrors.email = 'Email is required'
    else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(formData.email))
      newErrors.email = 'Enter a valid email address (e.g. user@gmail.com)'

    if (!formData.password)
      newErrors.password = 'Password is required'
    else if (formData.password.length < 8)
      newErrors.password = 'Password must be at least 8 characters'
    else if (!/(?=.*[a-z])/.test(formData.password))
      newErrors.password = 'Password must contain at least 1 lowercase letter'
    else if (!/(?=.*[A-Z])/.test(formData.password))
      newErrors.password = 'Password must contain at least 1 uppercase letter'
    else if (!/(?=.*\d)/.test(formData.password))
      newErrors.password = 'Password must contain at least 1 number'
    else if (!/(?=.*[@$!%*?&])/.test(formData.password))
      newErrors.password = 'Password must contain at least 1 special character (@$!%*?&)'

    if (!formData.aadhaarNumber)
      newErrors.aadhaarNumber = 'Aadhaar number is required'
    else if (!/^\d{12}$/.test(formData.aadhaarNumber))
      newErrors.aadhaarNumber = 'Aadhaar number must be exactly 12 digits'

    if (!formData.panNumber)
      newErrors.panNumber = 'PAN number is required'
    else if (!/^[A-Z]{5}[0-9]{4}[A-Z]{1}$/.test(formData.panNumber))
      newErrors.panNumber = 'Invalid PAN format. Example: ABCDE1234F'

    if (!formData.dateOfBirth)
      newErrors.dateOfBirth = 'Date of birth is required'

    if (!formData.phone)
      newErrors.phone = 'Phone number is required'
    else if (!/^\d{10}$/.test(formData.phone))
      newErrors.phone = 'Phone number must be exactly 10 digits'

    if (!formData.address.trim() || formData.address.trim().length < 5)
      newErrors.address = 'Address must be at least 5 characters'

    return newErrors
  }

  const handleChange = (e) => {
    setFormData({ ...formData, [e.target.name]: e.target.value })
    if (errors[e.target.name]) {
      setErrors({ ...errors, [e.target.name]: '' })
    }
    setServerError('')
  }

  const handleSubmit = async (e) => {
    e.preventDefault()
    setServerError('')

    const validationErrors = validate()
    if (Object.keys(validationErrors).length > 0) {
      setErrors(validationErrors)
      return
    }

    setLoading(true)
    try {
      const payload = {
        ...formData,
        dateOfBirth: new Date(formData.dateOfBirth).toISOString()
      }
      await API.post('/Auth/register', payload)
      setSuccess('Registration successful! Redirecting to login...')
      setTimeout(() => navigate('/login'), 2000)
    } catch (err) {
      setServerError(
        err.response?.data?.message ||
        err.response?.data?.errors?.['$.dateOfBirth']?.[0] ||
        'Registration failed. Please try again.'
      )
    } finally {
      setLoading(false)
    }
  }

  const inputClass = (field) =>
    `form-control ${errors[field] ? 'is-invalid' : ''}`

  return (
    <div className="min-vh-100 d-flex align-items-center justify-content-center py-5"
      style={{ backgroundColor: '#1a1a2e' }}>
      <div className="card shadow-lg p-4"
        style={{ width: '100%', maxWidth: '550px', borderRadius: '12px' }}>

        {/* Header */}
        <div className="text-center mb-4">
          <h2 className="fw-bold" style={{ color: '#1a1a2e' }}>🚔 CMS</h2>
          <h4 className="fw-bold">Create Account</h4>
          <p className="text-muted">Register to report incidents</p>
        </div>

        {serverError && <div className="alert alert-danger py-2">{serverError}</div>}
        {success && <div className="alert alert-success py-2">{success}</div>}

        <form onSubmit={handleSubmit} noValidate>
          <div className="row g-3">

            {/* Full Name */}
            <div className="col-12">
              <label className="form-label fw-semibold">
                Full Name <span className="text-danger">*</span>
              </label>
              <input type="text" name="name"
                className={inputClass('name')}
                placeholder="Enter your full name"
                value={formData.name} onChange={handleChange} />
              {errors.name && <div className="invalid-feedback">{errors.name}</div>}
            </div>

            {/* Email */}
            <div className="col-12">
              <label className="form-label fw-semibold">
                Email Address <span className="text-danger">*</span>
              </label>
              <input type="email" name="email"
                className={inputClass('email')}
                placeholder="Enter your email (e.g. user@gmail.com)"
                value={formData.email} onChange={handleChange} />
              {errors.email && <div className="invalid-feedback">{errors.email}</div>}
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
                  onChange={handleChange}
                />
                <button type="button"
                  className="btn btn-outline-secondary"
                  onClick={() => setShowPassword(!showPassword)}>
                  {showPassword ? '🙈' : '👁️'}
                </button>
                {errors.password && (
                  <div className="invalid-feedback d-block">{errors.password}</div>
                )}
              </div>
              <small className="text-muted">
                Must contain uppercase, lowercase, number and special character (@$!%*?&)
              </small>
            </div>

            {/* Aadhaar */}
            <div className="col-md-6">
              <label className="form-label fw-semibold">
                Aadhaar Number <span className="text-danger">*</span>
              </label>
              <input type="text" name="aadhaarNumber"
                className={inputClass('aadhaarNumber')}
                placeholder="12 digit Aadhaar number"
                maxLength={12}
                value={formData.aadhaarNumber} onChange={handleChange} />
              {errors.aadhaarNumber && (
                <div className="invalid-feedback">{errors.aadhaarNumber}</div>
              )}
            </div>

            {/* PAN */}
            <div className="col-md-6">
              <label className="form-label fw-semibold">
                PAN Number <span className="text-danger">*</span>
              </label>
              <input type="text" name="panNumber"
                className={inputClass('panNumber')}
                placeholder="e.g. ABCDE1234F"
                maxLength={10}
                value={formData.panNumber}
                onChange={(e) => {
                  e.target.value = e.target.value.toUpperCase()
                  handleChange(e)
                }} />
              {errors.panNumber && (
                <div className="invalid-feedback">{errors.panNumber}</div>
              )}
            </div>

            {/* Date of Birth */}
            <div className="col-md-6">
              <label className="form-label fw-semibold">
                Date of Birth <span className="text-danger">*</span>
              </label>
              <input type="date" name="dateOfBirth"
                className={inputClass('dateOfBirth')}
                max={new Date().toISOString().split('T')[0]}
                value={formData.dateOfBirth} onChange={handleChange} />
              {errors.dateOfBirth && (
                <div className="invalid-feedback">{errors.dateOfBirth}</div>
              )}
            </div>

            {/* Phone */}
            <div className="col-md-6">
              <label className="form-label fw-semibold">
                Phone Number <span className="text-danger">*</span>
              </label>
              <input type="text" name="phone"
                className={inputClass('phone')}
                placeholder="10 digit phone number"
                maxLength={10}
                value={formData.phone} onChange={handleChange} />
              {errors.phone && (
                <div className="invalid-feedback">{errors.phone}</div>
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
                placeholder="Enter your full address"
                value={formData.address} onChange={handleChange} />
              {errors.address && (
                <div className="invalid-feedback">{errors.address}</div>
              )}
            </div>

          </div>

          <small className="text-muted d-block mt-2">
            <span className="text-danger">*</span> All fields are required
          </small>

          <button type="submit"
            className="btn w-100 fw-bold text-white mt-3"
            style={{ backgroundColor: '#1a1a2e' }}
            disabled={loading}>
            {loading ? 'Registering...' : 'Register'}
          </button>
        </form>

        <div className="text-center mt-3">
          <p className="text-muted mb-0">
            Already have an account?{' '}
            <Link to="/login" className="fw-semibold" style={{ color: '#1a1a2e' }}>
              Login here
            </Link>
          </p>
        </div>

      </div>
    </div>
  )
}