import { useState, useEffect } from 'react'
import { useNavigate } from 'react-router-dom'
import { useAuth } from '../../context/AuthContext'
import API from '../../api/axios'

export default function StationHeadDashboard() {
  const navigate = useNavigate()
  const { name, logout } = useAuth()
  const [stats, setStats] = useState({
    total: 0, initiated: 0, active: 0, closed: 0, verified: 0, officers: 0
  })
  const [loading, setLoading] = useState(true)

  useEffect(() => { fetchStats() }, [])

  const fetchStats = async () => {
    try {
      const [incidentsRes, officersRes] = await Promise.all([
        API.get('/Incident/all'),
        API.get('/Officer/all')
      ])
      const incidents = incidentsRes.data
      setStats({
        total: incidents.length,
        initiated: incidents.filter(i => i.status === 'Initiated').length,
        active: incidents.filter(i => i.status === 'Active').length,
        closed: incidents.filter(i => i.status === 'Closed').length,
        verified: incidents.filter(i => i.status === 'Verified').length,
        officers: officersRes.data.length
      })
    } catch {
    } finally {
      setLoading(false)
    }
  }

  const handleLogout = async () => {
    try { await API.post('/Auth/logout') } catch {}
    logout()
    navigate('/')
  }

  const cards = [
    { label: 'Total Incidents', value: stats.total, color: '#1a1a2e', icon: '📋' },
    { label: 'Initiated', value: stats.initiated, color: '#0d6efd', icon: '🆕' },
    { label: 'Active', value: stats.active, color: '#ffc107', icon: '🔍' },
    { label: 'Closed', value: stats.closed, color: '#dc3545', icon: '🔒' },
    { label: 'Verified', value: stats.verified, color: '#198754', icon: '✅' },
    { label: 'Total Officers', value: stats.officers, color: '#6f42c1', icon: '👮' },
  ]

  return (
    <div style={{ backgroundColor: '#f0f2f5', minHeight: '100vh' }}>

      {/* Navbar */}
      <nav className="navbar px-4 py-3" style={{ backgroundColor: '#1a1a2e' }}>
        <span className="navbar-brand fw-bold text-white">🚔 CMS — Station Head Dashboard</span>
        <div className="d-flex align-items-center gap-3">
          <span className="text-white">🧑‍💼 {name}</span>
          <button className="btn btn-outline-light btn-sm"
            onClick={() => navigate('/stationhead/profile')}>Profile</button>
          <button className="btn btn-danger btn-sm" onClick={handleLogout}>Logout</button>
        </div>
      </nav>

      <div className="container py-4">
        <div className="mb-4">
          <h4 className="fw-bold mb-0">Hello, {name}!</h4>
          <p className="text-muted">Overview of all incidents and officers</p>
        </div>

        {/* Stats Cards */}
        {loading ? (
          <div className="text-center py-5">
            <div className="spinner-border" style={{ color: '#1a1a2e' }}></div>
          </div>
        ) : (
          <div className="row g-3 mb-4">
            {cards.map(card => (
              <div className="col-6 col-md-4 col-lg-2" key={card.label}>
                <div className="card border-0 shadow-sm text-center p-3 h-100">
                  <div className="fs-2">{card.icon}</div>
                  <h3 className="fw-bold mb-0" style={{ color: card.color }}>
                    {card.value}
                  </h3>
                  <small className="text-muted">{card.label}</small>
                </div>
              </div>
            ))}
          </div>
        )}

        {/* Quick Actions */}
        <h5 className="fw-bold mb-3">Quick Actions</h5>
        <div className="row g-3">
          {[
            { label: '📋 View All Incidents', path: '/stationhead/incidents',
              desc: 'See all incidents and assign officers' },
            { label: '👮 Manage Officers', path: '/stationhead/officers',
              desc: 'View, add, remove or reactivate officers' },
            { label: '➕ Add New Officer', path: '/stationhead/add-officer',
              desc: 'Register a new officer to the system' },
            { label: '👥 View All Users', path: '/stationhead/users',
              desc: 'See all registered civilians' },
          ].map(action => (
            <div className="col-md-6" key={action.path}>
              <div className="card border-0 shadow-sm p-4 h-100"
                style={{ cursor: 'pointer' }}
                onClick={() => navigate(action.path)}>
                <h6 className="fw-bold mb-1">{action.label}</h6>
                <p className="text-muted mb-0 small">{action.desc}</p>
              </div>
            </div>
          ))}
        </div>
      </div>
    </div>
  )
}