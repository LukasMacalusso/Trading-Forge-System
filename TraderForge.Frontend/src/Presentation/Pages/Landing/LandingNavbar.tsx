import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { motion, AnimatePresence } from 'framer-motion';
import { TrendingUp, Menu, X } from 'lucide-react';

const NAV_LINKS = [
  { label: 'Funcionalidades', href: '#features' },
  { label: 'Cómo Funciona', href: '#how-it-works' },
  { label: 'Plataforma', href: '#platform' },
  { label: 'Precios', href: '#pricing' },
];

export function LandingNavbar() {
  const [scrolled, setScrolled] = useState(false);
  const [mobileOpen, setMobileOpen] = useState(false);

  useEffect(() => {
    const onScroll = () => setScrolled(window.scrollY > 24);
    window.addEventListener('scroll', onScroll, { passive: true });
    return () => window.removeEventListener('scroll', onScroll);
  }, []);

  return (
    <motion.header
      initial={{ opacity: 0, y: -16 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ duration: 0.6, ease: 'easeOut' }}
      className={`fixed top-0 left-0 right-0 z-50 transition-all duration-500 ${
        scrolled
          ? 'bg-neutral-950/75 backdrop-blur-2xl border-b border-white/5 shadow-2xl shadow-black/30'
          : 'bg-transparent'
      }`}
    >
      <div className="max-w-7xl mx-auto px-6">
        <div className="h-16 flex items-center justify-between">
          {/* Logo */}
          <Link to="/" className="flex items-center gap-2.5 group">
            <div className="w-7 h-7 bg-emerald-500 rounded-lg flex items-center justify-center shadow-lg shadow-emerald-500/25 group-hover:shadow-emerald-500/40 transition-shadow">
              <TrendingUp size={14} className="text-neutral-950" strokeWidth={2.5} />
            </div>
            <span className="text-[15px] font-bold text-neutral-100 tracking-tight">
              Trading <span className="text-emerald-400">Forge</span>
            </span>
          </Link>

          {/* Desktop nav */}
          <nav className="hidden md:flex items-center gap-8">
            {NAV_LINKS.map(({ label, href }) => (
              <a
                key={label}
                href={href}
                className="text-sm text-neutral-400 hover:text-neutral-100 transition-colors duration-200"
              >
                {label}
              </a>
            ))}
          </nav>

          {/* Desktop CTAs */}
          <div className="hidden md:flex items-center gap-3">
            <Link
              to="/login"
              className="text-sm text-neutral-400 hover:text-neutral-100 transition-colors px-3 py-1.5"
            >
              Iniciar sesión
            </Link>
            <Link
              to="/register"
              className="inline-flex items-center px-4 py-2 bg-emerald-500 hover:bg-emerald-400 text-neutral-950 text-sm font-semibold rounded-lg transition-all duration-200 shadow-lg shadow-emerald-500/20 hover:shadow-emerald-500/35"
            >
              Comenzar gratis
            </Link>
          </div>

          {/* Mobile toggle */}
          <button
            className="md:hidden p-2 text-neutral-400 hover:text-neutral-100 transition-colors"
            onClick={() => setMobileOpen((o) => !o)}
            aria-label="Abrir menú"
          >
            {mobileOpen ? <X size={20} /> : <Menu size={20} />}
          </button>
        </div>
      </div>

      {/* Mobile menu */}
      <AnimatePresence>
        {mobileOpen && (
          <motion.div
            initial={{ opacity: 0, height: 0 }}
            animate={{ opacity: 1, height: 'auto' }}
            exit={{ opacity: 0, height: 0 }}
            transition={{ duration: 0.25 }}
            className="md:hidden bg-neutral-950/95 backdrop-blur-2xl border-t border-white/5 overflow-hidden"
          >
            <div className="px-6 py-4 flex flex-col gap-4">
              {NAV_LINKS.map(({ label, href }) => (
                <a
                  key={label}
                  href={href}
                  className="text-sm text-neutral-400 hover:text-neutral-100 transition-colors py-1"
                  onClick={() => setMobileOpen(false)}
                >
                  {label}
                </a>
              ))}
              <div className="pt-2 border-t border-neutral-800 flex flex-col gap-2">
                <Link to="/login" className="text-sm text-neutral-400 py-1" onClick={() => setMobileOpen(false)}>
                  Iniciar sesión
                </Link>
                <Link
                  to="/register"
                  className="text-center px-4 py-2.5 bg-emerald-500 text-neutral-950 text-sm font-semibold rounded-lg"
                  onClick={() => setMobileOpen(false)}
                >
                  Comenzar gratis
                </Link>
              </div>
            </div>
          </motion.div>
        )}
      </AnimatePresence>
    </motion.header>
  );
}
