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

  const handleChange = (e) => {
    setFormData({ ...formData, [e.target.name]: e.target.value })
    setError('')
  }

  const handleSubmit = async (e) => {
    e.preventDefault()
    setLoading(true)
    setError('')
    try {
      await API.post('/Officer/add', formData)
      setSuccess('Officer added successfully!')
      setTimeout(() => navigate('/stationhead/officers'), 2000)
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to add officer.')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div style={{ backgroundColor: '#f0f2f5', minHeight: '100vh' }}>

      <nav className="navbar px-4 py-3" style={{ backgroundColor: '#1a1a2e' }}>
        <span className="navbar-brand fw-bold text-white">🚔 CMS — Add Officer</span>
        <button className="btn btn-outline-light btn-sm"
          onClick={() => navigate('/stationhead/officers')}>← Back to Officers</button>
      </nav>

      <div className="container py-4">
        <div className="card border-0 shadow-sm p-4"
          style={{ maxWidth: '550px', margin: '0 auto' }}>

          <h4 className="fw-bold mb-1">Add New Officer</h4>
          <p className="text-muted mb-4">Register a new officer to the system</p>

          {error && <div className="alert alert-danger">{error}</div>}
          {success && <div className="alert alert-success">{success}</div>}

          <form onSubmit={handleSubmit}>
            <div className="row g-3">

              <div className="col-12">
                <label className="form-label fw-semibold">
                  Full Name <span className="text-danger">*</span>
                </label>
                <input type="text" name="name" className="form-control"
                  placeholder="Officer full name"
                  value={formData.name} onChange={handleChange} required />
              </div>

              <div className="col-12">
                <label className="form-label fw-semibold">
                  Email <span className="text-danger">*</span>
                </label>
                <input type="email" name="email" className="form-control"
                  placeholder="Officer email"
                  value={formData.email} onChange={handleChange} required />
              </div>

              <div className="col-12">
                <label className="form-label fw-semibold">
                  Password <span className="text-danger">*</span>
                </label>
                <div className="input-group">
                  <input
                    type={showPassword ? 'text' : 'password'}
                    name="password"
                    className="form-control"
                    placeholder="Min 8 chars, uppercase, number, special char"
                    value={formData.password}
                    onChange={handleChange}
                    required
                  />
                  <button type="button"
                    className="btn btn-outline-secondary"
                    onClick={() => setShowPassword(!showPassword)}>
                    {showPassword ? '🙈' : '👁️'}
                  </button>
                </div>
                <small className="text-muted">
                  Must contain uppercase, lowercase, number and special character (@$!%*?&)
                </small>
              </div>

              <div className="col-md-6">
                <label className="form-label fw-semibold">
                  Badge Number <span className="text-danger">*</span>
                </label>
                <input type="text" name="badgeNumber" className="form-control"
                  placeholder="e.g. TN-001"
                  value={formData.badgeNumber} onChange={handleChange} required />
              </div>

              <div className="col-md-6">
                <label className="form-label fw-semibold">
                  Phone <span className="text-danger">*</span>
                </label>
                <input type="text" name="phone" className="form-control"
                  placeholder="10 digit phone"
                  maxLength={10}
                  value={formData.phone} onChange={handleChange} required />
              </div>

              <div className="col-12">
                <label className="form-label fw-semibold">
                  Address <span className="text-danger">*</span>
                </label>
                <textarea name="address" className="form-control" rows="2"
                  placeholder="Officer address/station"
                  value={formData.address} onChange={handleChange} required />
              </div>

            </div>

            <small className="text-muted d-block mt-2">
              <span className="text-danger">*</span> All fields are required
            </small>

            <div className="d-flex gap-2 mt-4">
              <button type="submit"
                className="btn fw-bold text-white px-4"
                style={{ backgroundColor: '#1a1a2e' }}
                disabled={loading}>
                {loading ? 'Adding...' : 'Add Officer'}
              </button>
              <button type="button" className="btn btn-outline-secondary px-4"
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