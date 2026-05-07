import { Navigate, Route, Routes } from 'react-router-dom'
import { useEffect, useState } from 'react'
import { supabase } from './lib/supabase'
import Layout from './components/Layout'
import LoginPage from './pages/LoginPage'
import SignupPage from './pages/SignupPage'
import DashboardPage from './pages/DashboardPage'
import ClientsPage from './pages/ClientsPage'
import ProjectsPage from './pages/ProjectsPage'

function ProtectedRoute({ session, children }) {
  if (!session) return <Navigate to="/login" replace />
  return <Layout>{children}</Layout>
}

export default function App() {
  const [session, setSession] = useState(null)
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    supabase.auth.getSession().then(({ data }) => {
      setSession(data.session)
      setLoading(false)
    })
    const { data: listener } = supabase.auth.onAuthStateChange((_event, newSession) => setSession(newSession))
    return () => listener.subscription.unsubscribe()
  }, [])

  if (loading) return <div className="p-8">Loading...</div>

  return (
    <Routes>
      <Route path="/login" element={session ? <Navigate to="/dashboard" /> : <LoginPage />} />
      <Route path="/signup" element={session ? <Navigate to="/dashboard" /> : <SignupPage />} />
      <Route path="/dashboard" element={<ProtectedRoute session={session}><DashboardPage /></ProtectedRoute>} />
      <Route path="/clients" element={<ProtectedRoute session={session}><ClientsPage /></ProtectedRoute>} />
      <Route path="/projects" element={<ProtectedRoute session={session}><ProjectsPage /></ProtectedRoute>} />
      <Route path="*" element={<Navigate to={session ? '/dashboard' : '/login'} replace />} />
    </Routes>
  )
}
