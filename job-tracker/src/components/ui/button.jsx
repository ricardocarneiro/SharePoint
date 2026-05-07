import { cn } from '../../lib/utils'

export function Button({ className, children, ...props }) {
  return (
    <button
      className={cn('inline-flex items-center justify-center rounded-md bg-slate-900 px-4 py-2 text-sm font-medium text-white hover:bg-slate-700 disabled:opacity-50', className)}
      {...props}
    >
      {children}
    </button>
  )
}
