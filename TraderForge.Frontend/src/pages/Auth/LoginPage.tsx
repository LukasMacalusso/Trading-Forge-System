import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '@hooks/useAuth';
import { useAuthStore } from '@store/authStore';
import { Button } from '@components/UI/Button';
import { Input } from '@components/UI/Input';

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
    if (success) {
      // Send admins to their dashboard, everyone else to the trading view.
      const role = useAuthStore.getState().role;
      navigate(role === 'SystemAdmin' ? '/admin' : '/dashboard');
    }
  }

  return (
    <div className="login-card">
      <div className="login-header">
        <h2 className="login-title">Sign in</h2>
        <p className="login-subtitle">Access your trading simulation</p>
      </div>

      <form onSubmit={handleSubmit} className="login-form">
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
          <p className="login-error-message">
            {error}
          </p>
        )}

        <Button type="submit" isLoading={isLoading} className="login-submit-button">
          Sign in
        </Button>
      </form>

      <p className="login-footer-text">
        No account?{' '}
        <Link to="/register" className="login-register-link">
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
