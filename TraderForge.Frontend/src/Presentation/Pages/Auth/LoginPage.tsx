import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../../../Application/Handlers/useAuth';
import { Button } from '../../Components/UI/Button';
import { Input } from '../../Components/UI/Input';

export function LoginPage() {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const { login, isLoading, error } = useAuth();
  const navigate = useNavigate();

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    const success = await login({ email, password });
    if (success) navigate('/dashboard');
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
    </div>
  );
}
