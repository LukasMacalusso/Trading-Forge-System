import { Ban, RotateCcw, Users } from 'lucide-react';
import type { AdminUser } from '@models/Admin';
import { Badge } from '@components/UI/Badge';
import { Button } from '@components/UI/Button';

interface UsersPanelProps {
  users: AdminUser[];
  busyId: string | null;
  onToggleStatus: (user: AdminUser) => void;
}

function formatDate(iso: string): string {
  return new Date(iso).toLocaleDateString('es-ES', { day: '2-digit', month: 'short', year: 'numeric' });
}

export function UsersPanel({ users, busyId, onToggleStatus }: UsersPanelProps) {
  if (users.length === 0) {
    return (
      <div className="flex flex-col items-center justify-center text-center gap-3 py-20 text-neutral-600">
        <Users size={36} className="text-neutral-700" />
        <p className="text-sm text-neutral-400">No hay usuarios para mostrar</p>
      </div>
    );
  }

  function handleToggle(user: AdminUser) {
    const suspending = user.status === 'Active';
    const message = suspending
      ? `¿Suspender la cuenta de ${user.email}? No podrá operar hasta que la reactives.`
      : `¿Reactivar la cuenta de ${user.email}?`;
    if (window.confirm(message)) onToggleStatus(user);
  }

  return (
    <div className="border border-neutral-800 rounded-xl overflow-hidden">
      <div className="overflow-x-auto scrollbar-thin">
        <table className="w-full text-sm min-w-[640px]">
          <thead>
            <tr className="text-left text-xs uppercase tracking-wider text-neutral-500 border-b border-neutral-800">
              <th className="font-medium px-4 py-3">Usuario</th>
              <th className="font-medium px-4 py-3">Plan</th>
              <th className="font-medium px-4 py-3">Estado</th>
              <th className="font-medium px-4 py-3">Registrado</th>
              <th className="font-medium px-4 py-3 text-right">Portafolio</th>
              <th className="font-medium px-4 py-3 text-right">Acción</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-neutral-800">
            {users.map((user) => {
              const isActive = user.status === 'Active';
              return (
                <tr key={user.id} className="hover:bg-neutral-900/60 transition-colors">
                  <td className="px-4 py-3">
                    <div className="flex flex-col">
                      <span className="text-neutral-100 font-medium">{user.email}</span>
                      <span className="text-xs text-neutral-500">{user.userName}</span>
                    </div>
                  </td>
                  <td className="px-4 py-3">
                    {user.plan ? (
                      <Badge variant="warning">{user.plan}</Badge>
                    ) : (
                      <span className="text-neutral-600">—</span>
                    )}
                  </td>
                  <td className="px-4 py-3">
                    <Badge variant={isActive ? 'up' : 'down'}>
                      {isActive ? 'Activo' : 'Suspendido'}
                    </Badge>
                  </td>
                  <td className="px-4 py-3 text-neutral-400">
                    {user.registeredAt ? formatDate(user.registeredAt) : '—'}
                  </td>
                  <td className="px-4 py-3 text-right font-mono text-neutral-300">
                    {user.portfolioValue != null
                      ? `$${user.portfolioValue.toLocaleString('en-US', { minimumFractionDigits: 2 })}`
                      : '—'}
                  </td>
                  <td className="px-4 py-3">
                    <div className="flex justify-end">
                      <Button
                        variant={isActive ? 'danger' : 'secondary'}
                        size="sm"
                        isLoading={busyId === user.id}
                        onClick={() => handleToggle(user)}
                      >
                        {isActive ? <Ban size={14} /> : <RotateCcw size={14} />}
                        {isActive ? 'Suspender' : 'Reactivar'}
                      </Button>
                    </div>
                  </td>
                </tr>
              );
            })}
          </tbody>
        </table>
      </div>
    </div>
  );
}
