import { Link, useLocation } from 'react-router-dom'
import { supabase } from '../lib/supabase'

const links = [
  { to: '/dashboard', label: 'Dashboard' },
  { to: '/clients', label: 'Clients' },
  { to: '/projects', label: 'Projects' }
]

export default function Layout({ children }) {
  const location = useLocation()
  return (
    <div className="min-h-screen">
      <header className="border-b bg-white">
        <div className="mx-auto flex max-w-5xl items-center justify-between px-4 py-3">
          <h1 className="text-xl font-semibold">Job Tracker</h1>
          <nav className="flex items-center gap-4">
            {links.map((l) => (
              <Link key={l.to} to={l.to} className={location.pathname === l.to ? 'font-semibold text-slate-900' : 'text-slate-500'}>{l.label}</Link>
            ))}
            <button className="text-sm text-red-600" onClick={() => supabase.auth.signOut()}>Logout</button>
          </nav>
        </div>
      </header>
      <main className="mx-auto max-w-5xl px-4 py-6">{children}</main>
    </div>
  )
}
