import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom'
import { AuthProvider } from './context/AuthContext'
import ProtectedRoute from './components/ProtectedRoute'

// Public pages
import Login from './pages/Login'
import Register from './pages/Register'
import Landing from './pages/Landing'
import ForgotPassword from './pages/ForgotPassword'
import VerifyOtp from './pages/VerifyOtp'
import ResetPassword from './pages/ResetPassword'

// User pages
import UserDashboard from './pages/user/UserDashboard'
import CreateIncident from './pages/user/CreateIncident'
import IncidentDetails from './pages/user/IncidentDetails'
import UserProfile from './pages/user/UserProfile'

// Officer pages
import OfficerDashboard from './pages/officer/OfficerDashboard'
import OfficerProfile from './pages/officer/OfficerProfile'

// Station Head pages
import StationHeadDashboard from './pages/stationhead/StationHeadDashboard'
import AllIncidents from './pages/stationhead/AllIncidents'
import AllOfficers from './pages/stationhead/AllOfficers'
import AddOfficer from './pages/stationhead/AddOfficer'
import AllUsers from './pages/stationhead/AllUsers'
import StationHeadProfile from './pages/stationhead/StationHeadProfile'

function App() {
  return (
    <AuthProvider>
      <BrowserRouter>
        <Routes>
          {/* Public */}
          <Route path="/" element={<Landing />} />
          <Route path="/login" element={<Login />} />
          <Route path="/register" element={<Register />} />
          <Route path="/forgot-password" element={<ForgotPassword />} />
          <Route path="/verify-otp" element={<VerifyOtp />} />
          <Route path="/reset-password" element={<ResetPassword />} />

          {/* User */}
          <Route path="/user/dashboard" element={
            <ProtectedRoute allowedRole="User">
              <UserDashboard />
            </ProtectedRoute>
          } />
          <Route path="/user/create-incident" element={
            <ProtectedRoute allowedRole="User">
              <CreateIncident />
            </ProtectedRoute>
          } />
          <Route path="/user/incident/:id" element={
            <ProtectedRoute allowedRole="User">
              <IncidentDetails />
            </ProtectedRoute>
          } />
          <Route path="/user/profile" element={
            <ProtectedRoute allowedRole="User">
              <UserProfile />
            </ProtectedRoute>
          } />

          {/* Officer */}
          <Route path="/officer/dashboard" element={
            <ProtectedRoute allowedRole="Officer">
              <OfficerDashboard />
            </ProtectedRoute>
          } />
          <Route path="/officer/profile" element={
            <ProtectedRoute allowedRole="Officer">
              <OfficerProfile />
            </ProtectedRoute>
          } />

          {/* Station Head */}
          <Route path="/stationhead/dashboard" element={
            <ProtectedRoute allowedRole="StationHead">
              <StationHeadDashboard />
            </ProtectedRoute>
          } />
          <Route path="/stationhead/incidents" element={
            <ProtectedRoute allowedRole="StationHead">
              <AllIncidents />
            </ProtectedRoute>
          } />
          <Route path="/stationhead/officers" element={
            <ProtectedRoute allowedRole="StationHead">
              <AllOfficers />
            </ProtectedRoute>
          } />
          <Route path="/stationhead/add-officer" element={
            <ProtectedRoute allowedRole="StationHead">
              <AddOfficer />
            </ProtectedRoute>
          } />
          <Route path="/stationhead/users" element={
            <ProtectedRoute allowedRole="StationHead">
              <AllUsers />
            </ProtectedRoute>
          } />
          <Route path="/stationhead/profile" element={
            <ProtectedRoute allowedRole="StationHead">
              <StationHeadProfile />
            </ProtectedRoute>
          } />

          {/* Fallback */}
          <Route path="*" element={<Navigate to="/" />} />
        </Routes>
      </BrowserRouter>
    </AuthProvider>
  )
}

export default App