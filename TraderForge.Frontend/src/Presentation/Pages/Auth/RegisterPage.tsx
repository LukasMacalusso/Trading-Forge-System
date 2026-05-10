import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../../../Application/Handlers/useAuth';
import { Button } from '../../Components/UI/Button';
import { Input } from '../../Components/UI/Input';

export function RegisterPage() {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [confirm, setConfirm] = useState('');
  const [localError, setLocalError] = useState('');

  const { register, isLoading, error } = useAuth();
  const navigate = useNavigate();

  const passwordRules = [
    { label: 'At least 8 characters', ok: password.length >= 8 },
    { label: 'One uppercase letter (A-Z)', ok: /[A-Z]/.test(password) },
    { label: 'One number (0-9)', ok: /[0-9]/.test(password) },
    { label: 'One special character (!@#$…)', ok: /[^a-zA-Z0-9]/.test(password) },
  ];

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setLocalError('');

    if (password !== confirm) {
      setLocalError('Passwords do not match.');
      return;
    }
    if (passwordRules.some((r) => !r.ok)) {
      setLocalError('Password does not meet the requirements below.');
      return;
    }

    const success = await register({ email, password });
    if (success) navigate('/login', { state: { registered: true } });
  }

  const displayError = localError || error;

  return (
    <div className="register-card">
      <div className="register-header">
        <h2 className="register-title">Create account</h2>
        <p className="register-subtitle">Start your 7-day free trial — no credit card required</p>
      </div>

      <form onSubmit={handleSubmit} className="register-form">
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
          placeholder="Min. 8 characters, include a number"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
          required
          autoComplete="new-password"
        />
        <Input
          id="confirm"
          label="Confirm password"
          type="password"
          placeholder="••••••••"
          value={confirm}
          onChange={(e) => setConfirm(e.target.value)}
          required
          autoComplete="new-password"
        />

        {password.length > 0 && (
          <ul className="register-password-rules">
            {passwordRules.map((r) => (
              <li key={r.label} className={`register-rule-item ${r.ok ? 'register-rule-item--valid' : 'register-rule-item--invalid'}`}>
                <span>{r.ok ? '✓' : '○'}</span>
                {r.label}
              </li>
            ))}
          </ul>
        )}

        {displayError && (
          <p className="register-error-message">
            {displayError}
          </p>
        )}

        <Button type="submit" isLoading={isLoading} className="register-submit-button">
          Create account
        </Button>
      </form>

      <p className="register-footer-text">
        Already have an account?{' '}
        <Link to="/login" className="register-login-link">
          Sign in here
        </Link>
      </p>
    </div>
  );
}
