import { useState } from 'react'
import { useNavigate, useSearchParams, Link } from 'react-router-dom'
import API from '../api/axios'

export default function VerifyOtp() {
  const navigate = useNavigate()
  const [searchParams] = useSearchParams()
  const email = searchParams.get('email') || ''

  const [otp, setOtp] = useState('')
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')

  const handleSubmit = async (e) => {
    e.preventDefault()
    setError('')
    if (!otp.trim()) { setError('Please enter the OTP.'); return }
    setLoading(true)
    try {
      const response = await API.post('/Auth/verify-otp', { email, otp })
      const resetToken = response.data.resetToken
      // Pass email and resetToken to next page
      navigate(`/reset-password?email=${encodeURIComponent(email)}&token=${encodeURIComponent(resetToken)}`)
    } catch (err) {
      setError(err.response?.data?.message || 'Invalid or expired OTP. Please try again.')
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
          <h4 className="fw-bold">Verify OTP</h4>
          <p className="text-muted" style={{ fontSize: '13px' }}>
            OTP sent to <strong>{email}</strong>. Check your inbox and spam folder.
          </p>
        </div>

        {error && <div className="alert alert-danger py-2">{error}</div>}

        <form onSubmit={handleSubmit}>
          <div className="mb-3">
            <label className="form-label fw-semibold">
              Enter OTP <span className="text-danger">*</span>
            </label>
            <input type="text"
              className="form-control text-center fw-bold"
              placeholder="Enter OTP"
              value={otp}
              maxLength={6}
              onChange={(e) => { setOtp(e.target.value); setError('') }}
              style={{ fontSize: '24px', letterSpacing: '10px' }}
              required />
            <small className="text-muted">Enter the 6-digit OTP from your email</small>
          </div>
          <button type="submit"
            className="btn w-100 fw-bold text-white mb-2"
            style={{ backgroundColor: '#1a1a2e' }}
            disabled={loading}>
            {loading ? 'Verifying...' : 'Verify OTP'}
          </button>
          <button type="button"
            className="btn w-100 btn-outline-secondary"
            onClick={() => navigate('/forgot-password')}>
            ← Resend OTP
          </button>
        </form>

        <div className="text-center mt-3">
          <Link to="/login" className="fw-semibold"
            style={{ color: '#1a1a2e', fontSize: '13px' }}>
            ← Back to Login
          </Link>
        </div>
      </div>
    </div>
  )
}