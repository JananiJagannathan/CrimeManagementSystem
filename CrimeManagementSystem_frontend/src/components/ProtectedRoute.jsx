import { Navigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'

function ProtectedRoute({ children, allowedRole }) {
  const { token, role } = useAuth()

  if (!token) {
    return <Navigate to="/" />
  }

  if (allowedRole && role !== allowedRole) {
    return <Navigate to="/" />
  }

  return children
}

export default ProtectedRoute