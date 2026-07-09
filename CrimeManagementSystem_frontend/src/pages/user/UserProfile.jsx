import { useState, useEffect } from 'react'
import { useNavigate } from 'react-router-dom'
import { useAuth } from '../../context/AuthContext'
import API from '../../api/axios'

export default function UserProfile() {
  const navigate = useNavigate()
  const { logout, login } = useAuth()

  const [profile, setProfile] = useState(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')
  const [profilePic, setProfilePic] = useState(null)
  const [uploadingPic, setUploadingPic] = useState(false)
  const [showCurrentPassword, setShowCurrentPassword] = useState(false)
  const [showNewPassword, setShowNewPassword] = useState(false)
  const [editData, setEditData] = useState({ name: '', phone: '' })
  const [editErrors, setEditErrors] = useState({})
  const [passwordData, setPasswordData] = useState({
    currentPassword: '', newPassword: ''
  })
  const [passwordErrors, setPasswordErrors] = useState({})
  const [activeTab, setActiveTab] = useState('profile')

  useEffect(() => { fetchProfile() }, [])

  const fetchProfile = async () => {
    try {
      const response = await API.get('/Auth/profile')
      setProfile(response.data)
      setEditData({ name: response.data.name, phone: response.data.phone })
      if (response.data.profilePicture) {
        setProfilePic(`https://localhost:7025${response.data.profilePicture}`)
      }
    } catch {
      setError('Failed to load profile.')
    } finally {
      setLoading(false)
    }
  }

  const handleProfilePicUpload = async (e) => {
    const file = e.target.files[0]
    if (!file) return
    setUploadingPic(true)
    setError(''); setSuccess('')
    try {
      const fd = new FormData()
      fd.append('file', file)
      const response = await API.post('/Auth/upload-profile-picture', fd, {
        headers: { 'Content-Type': 'multipart/form-data' }
      })
      setProfilePic(`https://localhost:7025${response.data.profilePicturePath}`)
      setSuccess('Profile picture updated successfully!')
    } catch {
      setError('Photo upload failed. Please try again.')
    } finally {
      setUploadingPic(false)
    }
  }

  const validateEdit = () => {
    const errors = {}
    if (!editData.name.trim())
      errors.name = 'Full name is required'
    else if (editData.name.trim().length < 2)
      errors.name = 'Full name must be at least 2 characters'
    if (!editData.phone.trim())
      errors.phone = 'Phone number is required'
    else if (!/^\d{10}$/.test(editData.phone))
      errors.phone = 'Phone number must be exactly 10 digits'
    return errors
  }

  const validatePassword = () => {
    const errors = {}
    if (!passwordData.currentPassword)
      errors.currentPassword = 'Current password is required'
    if (!passwordData.newPassword)
      errors.newPassword = 'New password is required'
    else if (passwordData.newPassword.length < 8)
      errors.newPassword = 'Password must be at least 8 characters'
    else if (!/(?=.*[A-Z])/.test(passwordData.newPassword))
      errors.newPassword = 'Password must contain at least 1 uppercase letter'
    else if (!/(?=.*[a-z])/.test(passwordData.newPassword))
      errors.newPassword = 'Password must contain at least 1 lowercase letter'
    else if (!/(?=.*\d)/.test(passwordData.newPassword))
      errors.newPassword = 'Password must contain at least 1 number'
    else if (!/(?=.*[@$!%*?&])/.test(passwordData.newPassword))
      errors.newPassword = 'Password must contain at least 1 special character (@$!%*?&)'
    return errors
  }

  const handleUpdateProfile = async (e) => {
    e.preventDefault()
    setError(''); setSuccess('')
    const errors = validateEdit()
    if (Object.keys(errors).length > 0) { setEditErrors(errors); return }
    try {
      await API.put('/Auth/profile', editData)
      setSuccess('Profile updated successfully!')
      localStorage.setItem('name', editData.name)
      login({
        token: localStorage.getItem('token'),
        role: localStorage.getItem('role'),
        userId: localStorage.getItem('userId'),
        name: editData.name
      })
      fetchProfile()
    } catch (err) {
      setError(err.response?.data?.message || 'Update failed.')
    }
  }

  const handleChangePassword = async (e) => {
    e.preventDefault()
    setError(''); setSuccess('')
    const errors = validatePassword()
    if (Object.keys(errors).length > 0) { setPasswordErrors(errors); return }
    try {
      await API.put('/Auth/change-password', passwordData)
      setSuccess('Password changed successfully!')
      setPasswordData({ currentPassword: '', newPassword: '' })
      setPasswordErrors({})
    } catch (err) {
      const msg = err.response?.data?.message || ''
      if (msg.toLowerCase().includes('incorrect') ||
          msg.toLowerCase().includes('wrong') ||
          msg.toLowerCase().includes('invalid') ||
          msg.toLowerCase().includes('current')) {
        setPasswordErrors({
          currentPassword: 'Invalid password. Current password does not match.'
        })
      } else {
        setError(msg || 'Password change failed.')
      }
    }
  }

  const handleLogout = async () => {
    try { await API.post('/Auth/logout') } catch {}
    logout()
    navigate('/')
  }

  if (loading) return (
    <div className="min-vh-100 d-flex align-items-center justify-content-center">
      <div className="spinner-border" style={{ color: '#1a1a2e' }}></div>
    </div>
  )

  return (
    <div style={{ backgroundColor: '#f0f2f5', minHeight: '100vh' }}>
      <nav className="navbar px-4 py-3" style={{ backgroundColor: '#1a1a2e' }}>
        <span className="navbar-brand fw-bold text-white">🚔 CMS — My Profile</span>
        <div className="d-flex gap-2">
          <button className="btn btn-outline-light btn-sm"
            onClick={() => navigate('/user/dashboard')}>← Dashboard</button>
          <button className="btn btn-danger btn-sm" onClick={handleLogout}>Logout</button>
        </div>
      </nav>

      <div className="container py-4">
        <div className="card border-0 shadow-sm p-4"
          style={{ maxWidth: '600px', margin: '0 auto' }}>

          {/* Profile Header */}
          <div className="text-center mb-4">
            <div className="position-relative d-inline-block mb-3">
              {profilePic ? (
                <img src={profilePic} alt="Profile" className="rounded-circle"
                  style={{ width: '80px', height: '80px', objectFit: 'cover', border: '3px solid #1a1a2e' }}
                  onError={() => setProfilePic(null)} />
              ) : (
                <div className="rounded-circle d-inline-flex align-items-center justify-content-center fw-bold text-white"
                  style={{ width: '80px', height: '80px', backgroundColor: '#1a1a2e', fontSize: '28px' }}>
                  {profile?.name?.charAt(0).toUpperCase()}
                </div>
              )}
              <label htmlFor="photoUpload"
                className="position-absolute bottom-0 end-0 btn btn-sm btn-dark rounded-circle"
                style={{ width: '28px', height: '28px', padding: '0', cursor: 'pointer', fontSize: '12px', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
                {uploadingPic ? '⏳' : '📷'}
              </label>
              <input id="photoUpload" type="file" accept="image/*"
                style={{ display: 'none' }} onChange={handleProfilePicUpload} />
            </div>
            <h4 className="fw-bold mb-0">{profile?.name}</h4>
            <p className="text-muted">{profile?.email}</p>
            <span className="badge" style={{ backgroundColor: '#1a1a2e' }}>{profile?.role}</span>
          </div>

          {/* Tabs */}
          <ul className="nav nav-tabs mb-4">
            <li className="nav-item">
              <button className={`nav-link ${activeTab === 'profile' ? 'active fw-bold' : ''}`}
                onClick={() => { setActiveTab('profile'); setError(''); setSuccess(''); setEditErrors({}) }}>
                Edit Profile
              </button>
            </li>
            <li className="nav-item">
              <button className={`nav-link ${activeTab === 'password' ? 'active fw-bold' : ''}`}
                onClick={() => { setActiveTab('password'); setError(''); setSuccess(''); setPasswordErrors({}) }}>
                Change Password
              </button>
            </li>
          </ul>

          {error && <div className="alert alert-danger py-2">{error}</div>}
          {success && <div className="alert alert-success py-2">{success}</div>}

          {/* Edit Profile Tab */}
          {activeTab === 'profile' && (
            <form onSubmit={handleUpdateProfile} noValidate>
              <div className="mb-3">
                <label className="form-label fw-semibold">
                  Full Name <span className="text-danger">*</span>
                </label>
                <input type="text"
                  className={`form-control ${editErrors.name ? 'is-invalid' : ''}`}
                  placeholder="Enter your full name"
                  value={editData.name}
                  onChange={(e) => {
                    setEditData({ ...editData, name: e.target.value })
                    setEditErrors({ ...editErrors, name: '' })
                  }} />
                {editErrors.name && (
                  <div className="invalid-feedback">{editErrors.name}</div>
                )}
              </div>

              <div className="mb-3">
                <label className="form-label fw-semibold">
                  Phone Number <span className="text-danger">*</span>
                </label>
                <input type="text"
                  className={`form-control ${editErrors.phone ? 'is-invalid' : ''}`}
                  placeholder="10 digit phone number"
                  maxLength={10}
                  value={editData.phone}
                  onChange={(e) => {
                    setEditData({ ...editData, phone: e.target.value })
                    setEditErrors({ ...editErrors, phone: '' })
                  }} />
                {editErrors.phone && (
                  <div className="invalid-feedback">{editErrors.phone}</div>
                )}
              </div>

              <div className="mb-3">
                <label className="form-label fw-semibold">Email</label>
                <input type="email" className="form-control"
                  value={profile?.email} disabled />
                <small className="text-muted">Email cannot be changed</small>
              </div>

              <button type="submit" className="btn fw-bold text-white w-100"
                style={{ backgroundColor: '#1a1a2e' }}>
                Update Profile
              </button>
            </form>
          )}

          {/* Change Password Tab */}
          {activeTab === 'password' && (
            <form onSubmit={handleChangePassword} noValidate>
              <div className="mb-3">
                <label className="form-label fw-semibold">
                  Current Password <span className="text-danger">*</span>
                </label>
                <div className="input-group">
                  <input
                    type={showCurrentPassword ? 'text' : 'password'}
                    className={`form-control ${passwordErrors.currentPassword ? 'is-invalid' : ''}`}
                    placeholder="Enter current password"
                    value={passwordData.currentPassword}
                    onChange={(e) => {
                      setPasswordData({ ...passwordData, currentPassword: e.target.value })
                      setPasswordErrors({ ...passwordErrors, currentPassword: '' })
                    }} />
                  <button type="button" className="btn btn-outline-secondary"
                    onClick={() => setShowCurrentPassword(!showCurrentPassword)}>
                    {showCurrentPassword ? '🙈' : '👁️'}
                  </button>
                  {passwordErrors.currentPassword && (
                    <div className="invalid-feedback d-block">
                      {passwordErrors.currentPassword}
                    </div>
                  )}
                </div>
              </div>

              <div className="mb-3">
                <label className="form-label fw-semibold">
                  New Password <span className="text-danger">*</span>
                </label>
                <div className="input-group">
                  <input
                    type={showNewPassword ? 'text' : 'password'}
                    className={`form-control ${passwordErrors.newPassword ? 'is-invalid' : ''}`}
                    placeholder="Min 8 chars, uppercase, number, special char"
                    value={passwordData.newPassword}
                    onChange={(e) => {
                      setPasswordData({ ...passwordData, newPassword: e.target.value })
                      setPasswordErrors({ ...passwordErrors, newPassword: '' })
                    }} />
                  <button type="button" className="btn btn-outline-secondary"
                    onClick={() => setShowNewPassword(!showNewPassword)}>
                    {showNewPassword ? '🙈' : '👁️'}
                  </button>
                  {passwordErrors.newPassword && (
                    <div className="invalid-feedback d-block">
                      {passwordErrors.newPassword}
                    </div>
                  )}
                </div>
                <small className="text-muted">
                  Must contain uppercase, lowercase, number and special character (@$!%*?&)
                </small>
              </div>

              <button type="submit" className="btn fw-bold text-white w-100"
                style={{ backgroundColor: '#1a1a2e' }}>
                Change Password
              </button>
            </form>
          )}

        </div>
      </div>
    </div>
  )
}