import { useState } from 'react'
import { useNavigate, useSearchParams, Link } from 'react-router-dom'
import API from '../api/axios'

export default function ResetPassword() {
  const navigate = useNavigate()
  const [searchParams] = useSearchParams()
  const emailFromUrl = searchParams.get('email') || ''
  const tokenFromUrl = searchParams.get('token') || ''

  const [formData, setFormData] = useState({
    email: emailFromUrl,
    token: tokenFromUrl,
    newPassword: ''
  })
  const [showPassword, setShowPassword] = useState(false)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')
  const [errors, setErrors] = useState({})

  const validate = () => {
    const newErrors = {}
    if (!formData.email.trim()) newErrors.email = 'Email is required'
    if (!formData.token.trim()) newErrors.token = 'Reset token is required'
    if (!formData.newPassword) newErrors.newPassword = 'New password is required'
    else if (formData.newPassword.length < 8) newErrors.newPassword = 'Password must be at least 8 characters'
    else if (!/(?=.*[a-z])/.test(formData.newPassword)) newErrors.newPassword = 'Must contain at least 1 lowercase letter'
    else if (!/(?=.*[A-Z])/.test(formData.newPassword)) newErrors.newPassword = 'Must contain at least 1 uppercase letter'
    else if (!/(?=.*\d)/.test(formData.newPassword)) newErrors.newPassword = 'Must contain at least 1 number'
    else if (!/(?=.*[@$!%*?&])/.test(formData.newPassword)) newErrors.newPassword = 'Must contain at least 1 special character (@$!%*?&)'
    return newErrors
  }

  const handleSubmit = async (e) => {
    e.preventDefault()
    setError(''); setSuccess('')
    const validationErrors = validate()
    if (Object.keys(validationErrors).length > 0) { setErrors(validationErrors); return }
    setLoading(true)
    try {
      await API.post('/Auth/reset-password', {
        email: formData.email,
        token: formData.token,
        newPassword: formData.newPassword
      })
      setSuccess('Password reset successful! Redirecting to login...')
      setTimeout(() => navigate('/login'), 3000)
    } catch (err) {
      setError(err.response?.data?.message || 'Reset failed. Token may be expired. Please try again.')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="min-vh-100 d-flex align-items-center justify-content-center"
      style={{ backgroundColor: '#1a1a2e' }}>
      <div className="card shadow-lg p-4"
        style={{ width: '100%', maxWidth: '420px', borderRadius: '12px' }}>

        <div className="text-center mb-4">
          <h2 className="fw-bold" style={{ color: '#1a1a2e' }}>🚔 CMS</h2>
          <h4 className="fw-bold">Reset Password</h4>
          <p className="text-muted">Enter your new password below.</p>
        </div>

        {error && <div className="alert alert-danger py-2">{error}</div>}
        {success && (
          <div className="alert alert-success py-2">
            <p className="mb-1 fw-semibold">✅ Password Reset!</p>
            <p className="mb-0" style={{ fontSize: '13px' }}>{success}</p>
          </div>
        )}

        {!success && (
          <form onSubmit={handleSubmit} noValidate>
            <div className="mb-3">
              <label className="form-label fw-semibold">
                Email Address <span className="text-danger">*</span>
              </label>
              <input type="email"
                className={`form-control ${errors.email ? 'is-invalid' : ''}`}
                placeholder="Enter your email"
                value={formData.email}
                onChange={(e) => { setFormData({ ...formData, email: e.target.value }); setErrors({ ...errors, email: '' }) }} />
              {errors.email && <div className="invalid-feedback">{errors.email}</div>}
            </div>

            <div className="mb-3">
              <label className="form-label fw-semibold">
                Reset Token <span className="text-danger">*</span>
              </label>
              <input type="text"
                className={`form-control ${errors.token ? 'is-invalid' : ''}`}
                placeholder="Paste the token from your email"
                value={formData.token}
                onChange={(e) => { setFormData({ ...formData, token: e.target.value }); setErrors({ ...errors, token: '' }) }} />
              {errors.token && <div className="invalid-feedback">{errors.token}</div>}
              <small className="text-muted">Copy the reset token from the email we sent you</small>
            </div>

            <div className="mb-3">
              <label className="form-label fw-semibold">
                New Password <span className="text-danger">*</span>
              </label>
              <div className="input-group">
                <input
                  type={showPassword ? 'text' : 'password'}
                  className={`form-control ${errors.newPassword ? 'is-invalid' : ''}`}
                  placeholder="Min 8 chars, uppercase, number, special char"
                  value={formData.newPassword}
                  onChange={(e) => { setFormData({ ...formData, newPassword: e.target.value }); setErrors({ ...errors, newPassword: '' }) }} />
                <button type="button" className="btn btn-outline-secondary"
                  onClick={() => setShowPassword(!showPassword)}>
                  {showPassword ? '🙈' : '👁️'}
                </button>
                {errors.newPassword && (
                  <div className="invalid-feedback d-block">{errors.newPassword}</div>
                )}
              </div>
              <small className="text-muted">Must contain uppercase, lowercase, number and special character</small>
            </div>

            <button type="submit"
              className="btn w-100 fw-bold text-white mt-2"
              style={{ backgroundColor: '#1a1a2e' }}
              disabled={loading}>
              {loading ? 'Resetting...' : 'Reset Password'}
            </button>
          </form>
        )}

        <div className="text-center mt-3">
          <Link to="/login" className="fw-semibold" style={{ color: '#1a1a2e' }}>
            ← Back to Login
          </Link>
        </div>
      </div>
    </div>
  )
}