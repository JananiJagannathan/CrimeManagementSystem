import { useState, useEffect } from 'react'
import { useNavigate } from 'react-router-dom'
import { useAuth } from '../../context/AuthContext'
import API from '../../api/axios'

export default function OfficerProfile() {
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
  const [passwordData, setPasswordData] = useState({
    currentPassword: '', newPassword: ''
  })
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

  const handleUpdateProfile = async (e) => {
    e.preventDefault()
    setError(''); setSuccess('')
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
    try {
      await API.put('/Auth/change-password', passwordData)
      setSuccess('Password changed successfully!')
      setPasswordData({ currentPassword: '', newPassword: '' })
    } catch (err) {
      setError(err.response?.data?.message || 'Password change failed.')
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
        <span className="navbar-brand fw-bold text-white">🚔 CMS — Officer Profile</span>
        <div className="d-flex gap-2">
          <button className="btn btn-outline-light btn-sm"
            onClick={() => navigate('/officer/dashboard')}>← Dashboard</button>
          <button className="btn btn-danger btn-sm" onClick={handleLogout}>Logout</button>
        </div>
      </nav>

      <div className="container py-4">
        <div className="card border-0 shadow-sm p-4"
          style={{ maxWidth: '600px', margin: '0 auto' }}>

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
              <label htmlFor="officerPhotoUpload"
                className="position-absolute bottom-0 end-0 btn btn-sm btn-dark rounded-circle"
                style={{ width: '28px', height: '28px', padding: '0', cursor: 'pointer', fontSize: '12px', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
                {uploadingPic ? '⏳' : '📷'}
              </label>
              <input id="officerPhotoUpload" type="file" accept="image/*"
                style={{ display: 'none' }} onChange={handleProfilePicUpload} />
            </div>
            <h4 className="fw-bold mb-0">{profile?.name}</h4>
            <p className="text-muted">{profile?.email}</p>
            <span className="badge bg-warning text-dark">Officer</span>
          </div>

          <ul className="nav nav-tabs mb-4">
            <li className="nav-item">
              <button className={`nav-link ${activeTab === 'profile' ? 'active fw-bold' : ''}`}
                onClick={() => { setActiveTab('profile'); setError(''); setSuccess('') }}>
                Edit Profile
              </button>
            </li>
            <li className="nav-item">
              <button className={`nav-link ${activeTab === 'password' ? 'active fw-bold' : ''}`}
                onClick={() => { setActiveTab('password'); setError(''); setSuccess('') }}>
                Change Password
              </button>
            </li>
          </ul>

          {error && <div className="alert alert-danger py-2">{error}</div>}
          {success && <div className="alert alert-success py-2">{success}</div>}

          {activeTab === 'profile' && (
            <form onSubmit={handleUpdateProfile}>
              <div className="mb-3">
                <label className="form-label fw-semibold">Full Name</label>
                <input type="text" className="form-control"
                  value={editData.name}
                  onChange={(e) => setEditData({ ...editData, name: e.target.value })}
                  required />
              </div>
              <div className="mb-3">
                <label className="form-label fw-semibold">Phone Number</label>
                <input type="text" className="form-control"
                  value={editData.phone}
                  onChange={(e) => setEditData({ ...editData, phone: e.target.value })}
                  required />
              </div>
              <div className="mb-3">
                <label className="form-label fw-semibold">Email</label>
                <input type="email" className="form-control" value={profile?.email} disabled />
                <small className="text-muted">Email cannot be changed</small>
              </div>
              <button type="submit" className="btn fw-bold text-white w-100"
                style={{ backgroundColor: '#1a1a2e' }}>Update Profile</button>
            </form>
          )}

          {activeTab === 'password' && (
            <form onSubmit={handleChangePassword}>
              <div className="mb-3">
                <label className="form-label fw-semibold">Current Password</label>
                <div className="input-group">
                  <input type={showCurrentPassword ? 'text' : 'password'}
                    className="form-control" placeholder="Enter current password"
                    value={passwordData.currentPassword}
                    onChange={(e) => setPasswordData({ ...passwordData, currentPassword: e.target.value })}
                    required />
                  <button type="button" className="btn btn-outline-secondary"
                    onClick={() => setShowCurrentPassword(!showCurrentPassword)}>
                    {showCurrentPassword ? '🙈' : '👁️'}
                  </button>
                </div>
              </div>
              <div className="mb-3">
                <label className="form-label fw-semibold">New Password</label>
                <div className="input-group">
                  <input type={showNewPassword ? 'text' : 'password'}
                    className="form-control" placeholder="Min 8 chars, uppercase, number, special char"
                    value={passwordData.newPassword}
                    onChange={(e) => setPasswordData({ ...passwordData, newPassword: e.target.value })}
                    required />
                  <button type="button" className="btn btn-outline-secondary"
                    onClick={() => setShowNewPassword(!showNewPassword)}>
                    {showNewPassword ? '🙈' : '👁️'}
                  </button>
                </div>
              </div>
              <button type="submit" className="btn fw-bold text-white w-100"
                style={{ backgroundColor: '#1a1a2e' }}>Change Password</button>
            </form>
          )}
        </div>
      </div>
    </div>
  )
}