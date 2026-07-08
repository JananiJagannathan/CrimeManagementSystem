import { useState } from 'react'
import { useNavigate, Link } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'
import API from '../api/axios'

export default function Login() {
  const navigate = useNavigate()
  const { login } = useAuth()

  const [formData, setFormData] = useState({ email: '', password: '' })
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(false)
  const [showPassword, setShowPassword] = useState(false)

  const handleChange = (e) => {
    setFormData({ ...formData, [e.target.name]: e.target.value })
    setError('')
  }

  const handleSubmit = async (e) => {
    e.preventDefault()
    setLoading(true)
    setError('')

    try {
      const response = await API.post('/Auth/login', formData)
      const data = response.data

      login({
        token: data.token,
        role: data.role,
        name: data.name,
        userId: data.userId
      })

      if (data.role === 'User') navigate('/user/dashboard')
      else if (data.role === 'Officer') navigate('/officer/dashboard')
      else if (data.role === 'StationHead') navigate('/stationhead/dashboard')

    } catch (err) {
      setError(err.response?.data?.message || 'Login failed. Please try again.')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="min-vh-100 d-flex align-items-center justify-content-center"
      style={{ backgroundColor: '#1a1a2e' }}>
      <div className="card shadow-lg p-4"
        style={{ width: '100%', maxWidth: '420px', borderRadius: '12px' }}>

        {/* Header */}
        <div className="text-center mb-4">
          <h2 className="fw-bold" style={{ color: '#1a1a2e' }}>🚔 CMS</h2>
          <h4 className="fw-bold">Welcome Back</h4>
          <p className="text-muted">Crime Management System</p>
        </div>

        {/* Error */}
        {error && (
          <div className="alert alert-danger py-2" role="alert">
            {error}
          </div>
        )}

        {/* Form */}
        <form onSubmit={handleSubmit}>

          {/* Email */}
          <div className="mb-3">
            <label className="form-label fw-semibold">Email Address</label>
            <input
              type="email"
              name="email"
              className="form-control"
              placeholder="Enter your email"
              value={formData.email}
              onChange={handleChange}
              required
            />
          </div>

          {/* Password with Forgot Password link */}
          <div className="mb-3">
            <div className="d-flex justify-content-between align-items-center">
            <label className="form-label fw-semibold">Password</label>
            <Link to="/forgot-password"
            style={{ fontSize: '13px', color: '#1a1a2e' }}
            className="fw-semibold text-decoration-none mb-2">
            Forgot Password?
            </Link>
          </div>
            <div className="input-group">
              <input
                type={showPassword ? 'text' : 'password'}
                name="password"
                className="form-control"
                placeholder="Enter your password"
                value={formData.password}
                onChange={handleChange}
                required
              />
              <button
                type="button"
                className="btn btn-outline-secondary"
                onClick={() => setShowPassword(!showPassword)}>
                {showPassword ? '🙈' : '👁️'}
              </button>
            </div>
          </div>

          <button
            type="submit"
            className="btn w-100 fw-bold text-white"
            style={{ backgroundColor: '#1a1a2e' }}
            disabled={loading}>
            {loading ? 'Logging in...' : 'Login'}
          </button>
        </form>

        {/* Register link */}
        <div className="text-center mt-3">
          <p className="text-muted mb-0">
            Don't have an account?{' '}
            <Link to="/register" className="fw-semibold" style={{ color: '#1a1a2e' }}>
              Register here
            </Link>
          </p>
        </div>

      </div>
    </div>
  )
}