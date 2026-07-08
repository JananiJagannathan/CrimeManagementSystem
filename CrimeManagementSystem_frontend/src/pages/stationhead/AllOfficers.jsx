import { useState, useEffect } from 'react'
import { useNavigate } from 'react-router-dom'
import API from '../../api/axios'

export default function AllOfficers() {
  const navigate = useNavigate()
  const [officers, setOfficers] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')

  useEffect(() => { fetchOfficers() }, [])

  const fetchOfficers = async () => {
    try {
      const response = await API.get('/Officer/all')
      setOfficers(response.data)
    } catch {
      setError('Failed to load officers.')
    } finally {
      setLoading(false)
    }
  }

  const handleRemove = async (officerId) => {
    if (!window.confirm('Remove this officer?')) return
    try {
      await API.delete(`/Officer/remove/${officerId}`)
      setSuccess('Officer removed successfully!')
      fetchOfficers()
      setTimeout(() => setSuccess(''), 3000)
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to remove officer.')
    }
  }

  const handleReactivate = async (officerId) => {
    try {
      await API.put(`/Officer/reactivate/${officerId}`)
      setSuccess('Officer reactivated successfully!')
      fetchOfficers()
      setTimeout(() => setSuccess(''), 3000)
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to reactivate.')
    }
  }

  return (
    <div style={{ backgroundColor: '#f0f2f5', minHeight: '100vh' }}>

      <nav className="navbar px-4 py-3" style={{ backgroundColor: '#1a1a2e' }}>
        <span className="navbar-brand fw-bold text-white">🚔 CMS — All Officers</span>
        <div className="d-flex gap-2">
          <button className="btn btn-outline-light btn-sm"
            onClick={() => navigate('/stationhead/add-officer')}>
            + Add Officer
          </button>
          <button className="btn btn-outline-light btn-sm"
            onClick={() => navigate('/stationhead/dashboard')}>← Dashboard</button>
        </div>
      </nav>

      <div className="container py-4">

        {success && <div className="alert alert-success">{success}</div>}
        {error && <div className="alert alert-danger">{error}</div>}

        {loading ? (
          <div className="text-center py-5">
            <div className="spinner-border" style={{ color: '#1a1a2e' }}></div>
          </div>
        ) : (
          <div className="card border-0 shadow-sm">
            <div className="card-body p-0">
              <table className="table table-hover mb-0">
                <thead style={{ backgroundColor: '#1a1a2e', color: 'white' }}>
                  <tr>
                    <th className="p-3">Name</th>
                    <th className="p-3">Email</th>
                    <th className="p-3">Badge</th>
                    <th className="p-3">Phone</th>
                    <th className="p-3">Active Cases</th>
                    <th className="p-3">Status</th>
                    <th className="p-3">Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {officers.map(officer => (
                    <tr key={officer.officerId}>
                      <td className="p-3 fw-semibold">{officer.name}</td>
                      <td className="p-3">{officer.email}</td>
                      <td className="p-3">{officer.badgeNumber}</td>
                      <td className="p-3">{officer.phone}</td>
                      <td className="p-3">
                        <span className={`badge ${officer.activeIncidentsCount > 0
                          ? 'bg-warning text-dark' : 'bg-secondary'}`}>
                          {officer.activeIncidentsCount} active
                        </span>
                      </td>
                      <td className="p-3">
                        <span className={`badge ${officer.isActive
                          ? 'bg-success' : 'bg-danger'}`}>
                          {officer.isActive ? 'Active' : 'Inactive'}
                        </span>
                      </td>
                      <td className="p-3">
                        {officer.isActive ? (
                          <button className="btn btn-sm btn-danger"
                            onClick={() => handleRemove(officer.officerId)}>
                            Remove
                          </button>
                        ) : (
                          <button className="btn btn-sm btn-success"
                            onClick={() => handleReactivate(officer.officerId)}>
                            Reactivate
                          </button>
                        )}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        )}
      </div>
    </div>
  )
}