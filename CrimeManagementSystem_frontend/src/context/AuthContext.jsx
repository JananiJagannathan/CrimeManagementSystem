import { createContext, useContext, useState } from 'react'

const AuthContext = createContext()

export function AuthProvider({ children }) {
  const [token, setToken] = useState(localStorage.getItem('token') || null)
  const [role, setRole] = useState(localStorage.getItem('role') || null)
  const [userId, setUserId] = useState(localStorage.getItem('userId') || null)
  const [name, setName] = useState(localStorage.getItem('name') || null)

  const login = (data) => {
    localStorage.setItem('token', data.token)
    localStorage.setItem('role', data.role)
    localStorage.setItem('userId', data.userId || '')
    localStorage.setItem('name', data.name)
    setToken(data.token)
    setRole(data.role)
    setUserId(data.userId)
    setName(data.name)
  }

  const logout = () => {
    localStorage.removeItem('token')
    localStorage.removeItem('role')
    localStorage.removeItem('userId')
    localStorage.removeItem('name')
    setToken(null)
    setRole(null)
    setUserId(null)
    setName(null)
  }

  return (
    <AuthContext.Provider value={{ token, role, userId, name, login, logout }}>
      {children}
    </AuthContext.Provider>
  )
}

export function useAuth() {
  return useContext(AuthContext)
}