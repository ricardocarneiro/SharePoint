import { useState } from 'react'
import { Link } from 'react-router-dom'
import { supabase } from '../lib/supabase'
import { Button } from '../components/ui/button'
import { AuthCard } from './LoginPage'

export default function SignupPage() {
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [message, setMessage] = useState('')
  const [error, setError] = useState('')

  const onSubmit = async (e) => {
    e.preventDefault(); setError(''); setMessage('')
    const { error } = await supabase.auth.signUp({ email, password })
    if (error) setError(error.message)
    else setMessage('Conta criada! Verifique seu email para confirmar.')
  }

  return <AuthCard title="Signup" onSubmit={onSubmit} error={error}>
    {message && <p className="text-sm text-green-700">{message}</p>}
    <input className="w-full rounded border p-2" placeholder="Email" value={email} onChange={(e)=>setEmail(e.target.value)} />
    <input className="w-full rounded border p-2" type="password" placeholder="Password" value={password} onChange={(e)=>setPassword(e.target.value)} />
    <Button type="submit" className="w-full">Criar conta</Button>
    <p className="text-sm">Já tem conta? <Link className="underline" to="/login">Entrar</Link></p>
  </AuthCard>
}
