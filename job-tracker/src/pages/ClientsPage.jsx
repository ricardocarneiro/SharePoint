import { useEffect, useState } from 'react'
import { supabase } from '../lib/supabase'
import { Button } from '../components/ui/button'

export default function ClientsPage() {
  const [clients, setClients] = useState([])
  const [name, setName] = useState('')

  const load = async () => {
    const { data } = await supabase.from('clients').select('*').order('created_at', { ascending: false })
    setClients(data ?? [])
  }
  useEffect(() => { load() }, [])

  const add = async (e) => {
    e.preventDefault()
    if (!name) return
    await supabase.from('clients').insert({ name })
    setName(''); load()
  }

  return <section className="space-y-4"><h2 className="text-2xl font-semibold">Clients</h2>
    <form onSubmit={add} className="flex gap-2"><input className="flex-1 rounded border p-2" placeholder="Nome do cliente" value={name} onChange={(e)=>setName(e.target.value)} /><Button type="submit">Adicionar</Button></form>
    <ul className="space-y-2">{clients.map(c => <li key={c.id} className="rounded bg-white p-3 shadow-sm">{c.name}</li>)}</ul>
  </section>
}
