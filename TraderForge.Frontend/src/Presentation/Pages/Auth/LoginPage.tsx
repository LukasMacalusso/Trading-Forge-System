import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../../../Application/Handlers/useAuth';
import { useAuthStore } from '../../../Application/Store/authStore';
import { Button } from '../../Components/UI/Button';
import { Input } from '../../Components/UI/Input';

export function LoginPage() {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const { login, isLoading, error } = useAuth();
  const { setToken } = useAuthStore();
  const navigate = useNavigate();

  function handleDevBypass() {
    setToken('dev-token');
    navigate('/dashboard');
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    const success = await login({ email, password });
    if (success) navigate('/dashboard');
  }

  return (
    <div className="bg-neutral-900 border border-neutral-800 rounded-xl p-6 flex flex-col gap-5">
      <div>
        <h2 className="text-lg font-semibold text-neutral-100">Sign in</h2>
        <p className="text-sm text-neutral-500 mt-0.5">Access your trading simulation</p>
      </div>

      <form onSubmit={handleSubmit} className="flex flex-col gap-4">
        <Input
          id="email"
          label="Email"
          type="email"
          placeholder="trader@example.com"
          value={email}
          onChange={(e) => setEmail(e.target.value)}
          required
          autoComplete="email"
        />
        <Input
          id="password"
          label="Password"
          type="password"
          placeholder="••••••••"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
          required
          autoComplete="current-password"
        />

        {error && (
          <p className="text-sm text-red-400 bg-red-500/10 border border-red-500/20 rounded-lg px-3 py-2">
            {error}
          </p>
        )}

        <Button type="submit" isLoading={isLoading} className="w-full">
          Sign in
        </Button>
      </form>

      <p className="text-center text-sm text-neutral-500">
        No account?{' '}
        <Link to="/register" className="text-emerald-400 hover:text-emerald-300 transition-colors">
          Start your 7-day free trial
        </Link>
      </p>

      {import.meta.env.DEV && (
        <div className="border-t border-neutral-800 pt-4">
          <button
            type="button"
            onClick={handleDevBypass}
            className="w-full py-2 rounded-lg text-xs font-medium text-neutral-600 border border-dashed border-neutral-800 hover:border-neutral-600 hover:text-neutral-400 transition-colors"
          >
            Continuar sin backend (solo desarrollo)
          </button>
        </div>
      )}
    </div>
  );
}
