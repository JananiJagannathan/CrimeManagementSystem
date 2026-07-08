import { useState } from 'react'
import { useNavigate, Link } from 'react-router-dom'
import API from '../api/axios'

export default function ForgotPassword() {
  const navigate = useNavigate()
  const [email, setEmail] = useState('')
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')

  const handleSubmit = async (e) => {
    e.preventDefault()
    setError('')
    if (!email.trim()) { setError('Please enter your email.'); return }
    if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) {
      setError('Please enter a valid email address.'); return
    }
    setLoading(true)
    try {
      await API.post('/Auth/forgot-password', { email })
      // Pass email to next page via URL
      navigate(`/verify-otp?email=${encodeURIComponent(email)}`)
    } catch (err) {
      setError(err.response?.data?.message || 'No account found with this email address. Please register first.')
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
          <h4 className="fw-bold">Forgot Password?</h4>
          <p className="text-muted">
            Enter your registered email. We'll send you an OTP to reset your password.
          </p>
        </div>

        {error && <div className="alert alert-danger py-2">{error}</div>}

        <form onSubmit={handleSubmit}>
          <div className="mb-3">
            <label className="form-label fw-semibold">
              Email Address <span className="text-danger">*</span>
            </label>
            <input type="email" className="form-control"
              placeholder="Enter your registered email"
              value={email}
              onChange={(e) => { setEmail(e.target.value); setError('') }}
              required />
          </div>
          <button type="submit"
            className="btn w-100 fw-bold text-white"
            style={{ backgroundColor: '#1a1a2e' }}
            disabled={loading}>
            {loading ? 'Sending OTP...' : 'Send OTP'}
          </button>
        </form>

        <div className="text-center mt-3">
          <p className="text-muted mb-0" style={{ fontSize: '13px' }}>
            Remember your password?{' '}
            <Link to="/login" className="fw-semibold" style={{ color: '#1a1a2e' }}>
              Back to Login
            </Link>
          </p>
        </div>
      </div>
    </div>
  )
}