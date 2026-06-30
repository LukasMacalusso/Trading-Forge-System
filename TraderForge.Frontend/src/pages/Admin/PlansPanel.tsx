import { useState } from 'react';
import { Plus, Pencil, Trash2, Layers, X } from 'lucide-react';
import type { AdminPlan, PlanFormData } from '@models/Admin';
import { Button } from '@components/UI/Button';
import { Input } from '@components/UI/Input';
import { ConfirmDialog } from '@components/UI/ConfirmDialog';

interface PlansPanelProps {
  plans: AdminPlan[];
  busyId: string | null;
  onCreate: (data: PlanFormData) => Promise<boolean>;
  onUpdate: (id: string, data: PlanFormData) => Promise<boolean>;
  onDelete: (id: string) => Promise<boolean>;
}

function limitLabel(value: number | null): string {
  return value == null ? '∞' : String(value);
}

export function PlansPanel({ plans, busyId, onCreate, onUpdate, onDelete }: PlansPanelProps) {
  const [editing, setEditing] = useState<AdminPlan | 'new' | null>(null);
  const [deleting, setDeleting] = useState<AdminPlan | null>(null);
  const [removing, setRemoving] = useState(false);

  async function handleDelete() {
    if (!deleting) return;
    setRemoving(true);
    const ok = await onDelete(deleting.id);
    setRemoving(false);
    if (ok) setDeleting(null);
  }

  return (
    <div className="flex flex-col gap-4">
      <div className="flex justify-end">
        <Button variant="primary" size="sm" onClick={() => setEditing('new')}>
          <Plus size={14} />
          Nuevo plan
        </Button>
      </div>

      {plans.length === 0 ? (
        <div className="flex flex-col items-center justify-center text-center gap-3 py-20 text-neutral-600">
          <Layers size={36} className="text-neutral-700" />
          <p className="text-sm text-neutral-400">No hay planes todavía</p>
        </div>
      ) : (
        <div className="border border-neutral-800 rounded-xl overflow-hidden">
          <div className="overflow-x-auto scrollbar-thin">
            <table className="w-full text-sm min-w-[720px]">
              <thead>
                <tr className="text-left text-xs uppercase tracking-wider text-neutral-500 border-b border-neutral-800">
                  <th className="font-medium px-4 py-3">Plan</th>
                  <th className="font-medium px-4 py-3 text-right">Precio/mes</th>
                  <th className="font-medium px-4 py-3 text-right">Capital inicial</th>
                  <th className="font-medium px-4 py-3 text-center">Estrategias</th>
                  <th className="font-medium px-4 py-3 text-center">Activos</th>
                  <th className="font-medium px-4 py-3 text-center">Saldo edit.</th>
                  <th className="font-medium px-4 py-3 text-right">Acciones</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-neutral-800">
                {plans.map((plan) => (
                  <tr key={plan.id} className="hover:bg-neutral-900/60 transition-colors">
                    <td className="px-4 py-3 font-medium text-neutral-100">{plan.name}</td>
                    <td className="px-4 py-3 text-right font-mono text-neutral-300">
                      ${plan.monthlyPrice.toLocaleString('en-US', { minimumFractionDigits: 2 })}
                    </td>
                    <td className="px-4 py-3 text-right font-mono text-neutral-300">
                      ${plan.initialVirtualBalance.toLocaleString()}
                    </td>
                    <td className="px-4 py-3 text-center font-mono text-neutral-400">
                      {limitLabel(plan.maxActiveStrategies)}
                    </td>
                    <td className="px-4 py-3 text-center font-mono text-neutral-400">
                      {limitLabel(plan.maxActiveAssets)}
                    </td>
                    <td className="px-4 py-3 text-center text-neutral-400">
                      {plan.canModifyVirtualBalance ? 'Sí' : 'No'}
                    </td>
                    <td className="px-4 py-3">
                      <div className="flex justify-end gap-1">
                        <button
                          onClick={() => setEditing(plan)}
                          aria-label={`Editar ${plan.name}`}
                          className="p-1.5 rounded-md text-neutral-400 hover:text-neutral-100 hover:bg-neutral-800 transition-colors"
                        >
                          <Pencil size={15} />
                        </button>
                        <button
                          onClick={() => setDeleting(plan)}
                          aria-label={`Eliminar ${plan.name}`}
                          className="p-1.5 rounded-md text-neutral-400 hover:text-red-400 hover:bg-red-500/10 transition-colors"
                        >
                          <Trash2 size={15} />
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {editing && (
        <PlanFormModal
          plan={editing === 'new' ? null : editing}
          isSaving={busyId === (editing === 'new' ? 'new' : editing.id)}
          onClose={() => setEditing(null)}
          onSubmit={async (data) => {
            const ok =
              editing === 'new' ? await onCreate(data) : await onUpdate(editing.id, data);
            if (ok) setEditing(null);
          }}
        />
      )}

      <ConfirmDialog
        isOpen={deleting != null}
        variant="danger"
        icon={<Trash2 size={20} />}
        title={deleting ? `¿Eliminar el plan "${deleting.name}"?` : ''}
        description="Esta acción no se puede deshacer."
        confirmLabel="Eliminar"
        isLoading={removing}
        onConfirm={handleDelete}
        onClose={() => setDeleting(null)}
      />
    </div>
  );
}

function PlanFormModal({
  plan,
  isSaving,
  onClose,
  onSubmit,
}: {
  plan: AdminPlan | null;
  isSaving: boolean;
  onClose: () => void;
  onSubmit: (data: PlanFormData) => void;
}) {
  const [name, setName] = useState(plan?.name ?? '');
  const [monthlyPrice, setMonthlyPrice] = useState(String(plan?.monthlyPrice ?? ''));
  const [initialBalance, setInitialBalance] = useState(String(plan?.initialVirtualBalance ?? ''));
  const [maxStrategies, setMaxStrategies] = useState(
    plan?.maxActiveStrategies != null ? String(plan.maxActiveStrategies) : '',
  );
  const [maxAssets, setMaxAssets] = useState(
    plan?.maxActiveAssets != null ? String(plan.maxActiveAssets) : '',
  );
  const [canModify, setCanModify] = useState(plan?.canModifyVirtualBalance ?? false);

  function parseLimit(value: string): number | null {
    const trimmed = value.trim();
    return trimmed === '' ? null : Math.max(0, Math.floor(Number(trimmed) || 0));
  }

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    onSubmit({
      name: name.trim(),
      monthlyPrice: Number(monthlyPrice) || 0,
      initialVirtualBalance: Number(initialBalance) || 0,
      maxActiveStrategies: parseLimit(maxStrategies),
      maxActiveAssets: parseLimit(maxAssets),
      canModifyVirtualBalance: canModify,
    });
  }

  return (
    <div className="fixed inset-0 z-[1000] flex items-center justify-center p-4">
      <div className="absolute inset-0 bg-black/70" onClick={onClose} />
      <form
        onSubmit={handleSubmit}
        className="relative w-full max-w-md bg-neutral-900 border border-neutral-700 rounded-2xl shadow-2xl p-5 flex flex-col gap-4"
      >
        <div className="flex items-center justify-between">
          <h3 className="text-base font-semibold text-neutral-100">
            {plan ? 'Editar plan' : 'Nuevo plan'}
          </h3>
          <button
            type="button"
            onClick={onClose}
            aria-label="Cerrar"
            className="p-1 -m-1 text-neutral-500 hover:text-neutral-300"
          >
            <X size={16} />
          </button>
        </div>

        <Input
          label="Nombre"
          value={name}
          onChange={(e) => setName(e.target.value)}
          placeholder="Pro"
          required
        />

        <div className="grid grid-cols-2 gap-3">
          <Input
            label="Precio mensual ($)"
            type="number"
            min="0"
            step="0.01"
            value={monthlyPrice}
            onChange={(e) => setMonthlyPrice(e.target.value)}
            placeholder="24.99"
            required
          />
          <Input
            label="Capital inicial ($)"
            type="number"
            min="0"
            step="1"
            value={initialBalance}
            onChange={(e) => setInitialBalance(e.target.value)}
            placeholder="100000"
            required
          />
        </div>

        <div className="grid grid-cols-2 gap-3">
          <Input
            label="Máx. estrategias"
            type="number"
            min="0"
            step="1"
            value={maxStrategies}
            onChange={(e) => setMaxStrategies(e.target.value)}
            placeholder="∞ (vacío)"
          />
          <Input
            label="Máx. activos"
            type="number"
            min="0"
            step="1"
            value={maxAssets}
            onChange={(e) => setMaxAssets(e.target.value)}
            placeholder="∞ (vacío)"
          />
        </div>
        <p className="-mt-2 text-[11px] text-neutral-600">Deja los límites vacíos para "ilimitado".</p>

        <label className="flex items-center gap-2.5 cursor-pointer">
          <input
            type="checkbox"
            checked={canModify}
            onChange={(e) => setCanModify(e.target.checked)}
            className="w-4 h-4 accent-amber-500"
          />
          <span className="text-sm text-neutral-300">Permite modificar el saldo virtual</span>
        </label>

        <div className="flex justify-end gap-2 mt-1">
          <Button type="button" variant="ghost" size="sm" onClick={onClose} disabled={isSaving}>
            Cancelar
          </Button>
          <Button type="submit" variant="primary" size="sm" isLoading={isSaving} disabled={!name.trim()}>
            {plan ? 'Guardar' : 'Crear plan'}
          </Button>
        </div>
      </form>
    </div>
  );
}
