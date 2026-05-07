import { useState } from 'react'
import { Link } from 'react-router-dom'
import { supabase } from '../lib/supabase'
import { Button } from '../components/ui/button'

export default function LoginPage() {
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState('')

  const onSubmit = async (e) => {
    e.preventDefault(); setError('')
    const { error } = await supabase.auth.signInWithPassword({ email, password })
    if (error) setError(error.message)
  }

  return <AuthCard title="Login" onSubmit={onSubmit} error={error}>
    <input className="w-full rounded border p-2" placeholder="Email" value={email} onChange={(e)=>setEmail(e.target.value)} />
    <input className="w-full rounded border p-2" type="password" placeholder="Password" value={password} onChange={(e)=>setPassword(e.target.value)} />
    <Button type="submit" className="w-full">Entrar</Button>
    <p className="text-sm">Sem conta? <Link className="underline" to="/signup">Criar conta</Link></p>
  </AuthCard>
}

export function AuthCard({ title, onSubmit, error, children }) {
  return <div className="flex min-h-screen items-center justify-center"><form onSubmit={onSubmit} className="w-full max-w-sm space-y-3 rounded-lg bg-white p-6 shadow"><h2 className="text-2xl font-semibold">{title}</h2>{error && <p className="text-sm text-red-600">{error}</p>}{children}</form></div>
}
