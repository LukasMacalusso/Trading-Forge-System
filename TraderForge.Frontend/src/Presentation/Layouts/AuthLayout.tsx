import { Outlet } from 'react-router-dom';
import { motion } from 'framer-motion';

export function AuthLayout() {
  return (
    <div className="auth-layout">
      <div className="auth-layout__container">
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.3 }}
        >
          <div className="auth-layout__brand">
            <h1 className="auth-layout__title">
              Trader<span className="auth-layout__title-accent">Forge</span>
            </h1>
            <p className="auth-layout__subtitle">Trading Simulation Platform</p>
          </div>
          <Outlet />
        </motion.div>
      </div>
    </div>
  );
}
