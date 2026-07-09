import { useNavigate } from 'react-router-dom'

export default function Landing() {
  const navigate = useNavigate()

  return (
    <div style={{ backgroundColor: '#1a1a2e', width: '100%' }}>

      {/* Navbar */}
      <nav className="navbar px-4 py-3 sticky-top" style={{ backgroundColor: '#16213e' }}>
        <div className="navbar-brand d-flex align-items-center fw-bold text-white fs-5">
  <img
    src="/logo1.png"
    alt="CMS Logo"
    width="80"
    height="50"
    className="me-2"
    style={{ objectFit: "contain" }}
  />
  Crime Management System
 
</div>
        <div className="d-flex gap-2 align-items-center">
  <a
    href="#roles"
    className="text-white text-decoration-none me-2"
    style={{ fontSize: '14px' }}
  >
    Roles
  </a>

  <a
    href="#how-it-works"
    className="text-white text-decoration-none me-2"
    style={{ fontSize: '14px' }}
  >
    How It Works
  </a>

  <a
    href="#features"
    className="text-white text-decoration-none me-2"
    style={{ fontSize: '14px' }}
  >
    Features
  </a>

  <button
    className="btn btn-outline-light btn-sm"
    onClick={() => navigate('/login')}
  >
    Login
  </button>

  <button
    className="btn btn-sm fw-bold text-white"
    style={{ backgroundColor: '#e94560' }}
    onClick={() => navigate('/register')}
  >
    Register
  </button>
</div>
      </nav>

      {/* Hero Section — tighter */}
      <div className="text-center text-white py-3 px-3"
        style={{
          minHeight: '50vh',
          display: 'flex', flexDirection: 'column',
          alignItems: 'center', justifyContent: 'center',
          background: 'linear-gradient(135deg, #1a1a2e 0%, #16213e 50%, #1a1a2e 100%)'
        }}>
        
        <h1 className="display-5 fw-bold mb-3">Crime Management System</h1>
        <p className="mb-4" style={{ maxWidth: '600px', color: '#a8b2d8', fontSize: '16px' }}>
          Report crimes online, track investigation progress in real time, and enable
          officers and station heads to manage cases securely through one centralized platform.
        </p>
        <div className="d-flex gap-3 flex-wrap justify-content-center">
          <button className="btn btn-lg fw-bold px-4 py-2 text-white"
            style={{ backgroundColor: '#e94560', borderRadius: '8px' }}
            onClick={() => navigate('/register')}>
            🚨 Report an Incident
          </button>
          <button className="btn btn-lg fw-bold px-4 py-2 btn-outline-light"
            style={{ borderRadius: '8px' }}
            onClick={() => navigate('/login')}>
            Login to Dashboard →
          </button>
        </div>
      </div>

      

      {/* User Roles */}
      <div id="roles" className="py-5 px-3" style={{ backgroundColor: '#1a1a2e' }}>
        <h2 className="text-white fw-bold text-center mb-1">Who Uses This Platform?</h2>
        <p className="text-center mb-5" style={{ color: '#a8b2d8', fontSize: '14px' }}>
          Three roles, each with specific access and responsibilities
        </p>
        <div className="row g-4 justify-content-center">
          {[
            {
              icon: '👤', role: 'Civilian / User',
              color: '#0d6efd', bg: 'rgba(13,110,253,0.1)',
              features: ['Register an account', 'Submit incident reports', 'Track investigation status', 'Download incident PDF', 'View assigned officer']
            },
            {
              icon: '👮', role: 'Police Officer',
              color: '#ffc107', bg: 'rgba(255,193,7,0.1)',
              features: ['Login to dashboard', 'View assigned cases', 'Investigate incidents', 'Close resolved cases', 'View full case details']
            },
            {
              icon: '🏛️', role: 'Station Head',
              color: '#198754', bg: 'rgba(25,135,84,0.1)',
              features: ['Manage all officers', 'Assign cases to officers', 'View all incidents', 'Verify closed cases', 'View all registered users']
            },
          ].map(role => (
            <div className="col-md-4" key={role.role}>
              <div className="card border-0 p-4 h-100 text-center"
                style={{
                  backgroundColor: '#16213e',
                  borderRadius: '12px',
                  border: `1px solid ${role.color}33`,
                  transition: 'transform 0.2s'
                }}
                onMouseEnter={e => e.currentTarget.style.transform = 'translateY(-4px)'}
                onMouseLeave={e => e.currentTarget.style.transform = 'translateY(0)'}>
                <div className="rounded-circle d-flex align-items-center justify-content-center mx-auto mb-3"
                  style={{ width: '64px', height: '64px', backgroundColor: role.bg, fontSize: '28px' }}>
                  {role.icon}
                </div>
                <h5 className="fw-bold mb-3" style={{ color: role.color }}>{role.role}</h5>
                {role.features.map(f => (
                  <p key={f} className="mb-1" style={{ color: '#a8b2d8', fontSize: '13px' }}>
                    ✓ {f}
                  </p>
                ))}
              </div>
            </div>
          ))}
        </div>
      </div>

      {/* How It Works */}
      <div id="how-it-works" className="py-5 px-3"
        style={{ backgroundColor: '#16213e' }}>
        <h2 className="text-white fw-bold text-center mb-1">How It Works</h2>
        <p className="text-center mb-5" style={{ color: '#a8b2d8', fontSize: '14px' }}>
          Four steps from report to resolution
        </p>
        <div className="row g-4 justify-content-center">
          {[
            { step: '1', icon: '📋', title: 'Citizen Submits Incident Report', desc: 'Civilian registers and submits an incident report online with all relevant details' },
            { step: '2', icon: '🧑‍💼', title: 'Station Head Assigns Officer', desc: 'Station Head reviews the incident and assigns it to an available officer' },
            { step: '3', icon: '🔍', title: 'Officer Investigates & Closes', desc: 'Assigned officer investigates the case and marks it closed after completion' },
            { step: '4', icon: '✅', title: 'Station Head Verifies', desc: 'Station Head reviews the closed case and gives final verification and approval' },
          ].map((item) => (
            <div className="col-md-3 col-sm-6" key={item.step}>
              <div className="card border-0 p-4 h-100 text-center"
                style={{
                  backgroundColor: '#1a1a2e',
                  borderRadius: '12px',
                  transition: 'transform 0.2s'
                }}
                onMouseEnter={e => e.currentTarget.style.transform = 'translateY(-4px)'}
                onMouseLeave={e => e.currentTarget.style.transform = 'translateY(0)'}>
                <div className="rounded-circle d-flex align-items-center justify-content-center fw-bold text-white mx-auto mb-3"
                  style={{ width: '44px', height: '44px', backgroundColor: '#e94560', fontSize: '18px' }}>
                  {item.step}
                </div>
                <div style={{ fontSize: '32px' }} className="mb-2">{item.icon}</div>
                <h6 className="fw-bold text-white mb-2" style={{ fontSize: '13px' }}>{item.title}</h6>
                <p style={{ color: '#a8b2d8', fontSize: '12px', marginBottom: 0 }}>{item.desc}</p>
              </div>
            </div>
          ))}
        </div>
      </div>

      {/* Features */}
      <div id="features" className="py-5 px-3" style={{ backgroundColor: '#1a1a2e' }}>
        <h2 className="text-white fw-bold text-center mb-1">Key Features</h2>
        <p className="text-center mb-5" style={{ color: '#a8b2d8', fontSize: '14px' }}>
          Everything you need in one platform
        </p>
        <div className="row g-4 justify-content-center">
          {[
            { icon: '🔐', title: 'Secure Authentication', desc: 'JWT-based login with BCrypt password hashing for all three roles' },
            { icon: '📧', title: 'Email Notifications', desc: 'Automatic emails on registration, login, incident creation and closure' },
            { icon: '📄', title: 'PDF Report Generation', desc: 'Download formatted incident reports as PDF using QuestPDF' },
            { icon: '👥', title: 'Role-Based Access', desc: 'Three separate dashboards — User, Officer, and Station Head' },
            { icon: '🔍', title: 'Real-Time Tracking', desc: 'Track incident status from Initiated to Verified in real time' },
            { icon: '🧪', title: 'Fully Tested', desc: '46 NUnit unit tests covering all services with Moq and in-memory DB' },
          ].map(card => (
            <div className="col-md-4 col-sm-6" key={card.title}>
              <div className="card border-0 p-4 h-100"
                style={{
                  backgroundColor: '#16213e',
                  borderRadius: '12px',
                  borderLeft: '3px solid #e94560',
                  transition: 'transform 0.2s',
                  cursor: 'default'
                }}
                onMouseEnter={e => e.currentTarget.style.transform = 'translateY(-4px)'}
                onMouseLeave={e => e.currentTarget.style.transform = 'translateY(0)'}>
                <div className="badge mb-3 px-3 py-2 text-white d-inline-block"
                  style={{ backgroundColor: '#e9456022', fontSize: '22px', borderRadius: '8px' }}>
                  {card.icon}
                </div>
                <h6 className="fw-bold text-white mb-2">{card.title}</h6>
                <p style={{ color: '#a8b2d8', fontSize: '13px', marginBottom: 0 }}>{card.desc}</p>
              </div>
            </div>
          ))}
        </div>
      </div>


      {/* CTA Banner */}
      <div className="py-5 text-center px-3"
        style={{ background: 'linear-gradient(135deg, #e94560 0%, #0f3460 100%)' }}>
        <h2 className="text-white fw-bold mb-2">Ready to Report an Incident?</h2>
        <p className="mb-4" style={{ color: 'rgba(255,255,255,0.8)' }}>
          Join our platform and help keep your community safe
        </p>
        <button className="btn btn-light fw-bold px-5 py-2"
          style={{ borderRadius: '8px', color: '#1a1a2e' }}
          onClick={() => navigate('/register')}>
          Get Started — Register Now
        </button>
      </div>

  {/* Footer */}
<div
  className="py-4 text-center"
  style={{
    backgroundColor: "#0d0d1a",
    borderTop: "1px solid #16213e",
  }}
>
  {/* Logo + Title */}
  <div className="d-flex justify-content-center align-items-center mb-2">
    <img
      src="/logo1.png"
      alt="Crime Management System Logo"
      width="90"
      height="50"
      className="me-2"
      style={{ objectFit: "contain" }}
    />
    <span className="fw-bold text-white fs-5">Crime Management System</span>
  </div>

  {/* Technology */}
  <p
    style={{ color: "#a8b2d8", fontSize: "13px" }}
    className="mb-2"
  >
    Built with ASP.NET Core + React.js
  </p>

  {/* Quick Links */}
  <div className="d-flex justify-content-center gap-4 mb-3">
    <span
      style={{ color: "#a8b2d8", fontSize: "13px", cursor: "pointer" }}
      onClick={() => navigate("/")}
    >
      Home
    </span>

    <span
      style={{ color: "#a8b2d8", fontSize: "13px", cursor: "pointer" }}
      onClick={() => navigate("/login")}
    >
      Login
    </span>

    <span
      style={{ color: "#a8b2d8", fontSize: "13px", cursor: "pointer" }}
      onClick={() => navigate("/register")}
    >
      Register
    </span>
  </div>

  {/* Copyright */}
  <p style={{ color: "#666", fontSize: "12px", marginBottom: 0 }}>
    © 2026 Crime Management System. All Rights Reserved.
  </p>
</div>

    </div>
  )
}