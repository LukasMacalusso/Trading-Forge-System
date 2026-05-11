import { Link } from 'react-router-dom';
import { TrendingUp, Github, Twitter, Linkedin } from 'lucide-react';

const FOOTER_LINKS = {
  Producto: [
    { label: 'Funcionalidades', href: '#features' },
    { label: 'Cómo Funciona', href: '#how-it-works' },
    { label: 'Plataforma', href: '#platform' },
    { label: 'Precios', href: '#pricing' },
  ],
  Cuenta: [
    { label: 'Iniciar sesión', href: '/login' },
    { label: 'Crear cuenta', href: '/register' },
    { label: 'Panel principal', href: '/dashboard' },
  ],
  Legal: [
    { label: 'Política de Privacidad', href: '#' },
    { label: 'Términos de Uso', href: '#' },
    { label: 'Política de Cookies', href: '#' },
  ],
};

const SOCIAL = [
  { icon: Twitter, href: '#', label: 'Twitter' },
  { icon: Github, href: '#', label: 'GitHub' },
  { icon: Linkedin, href: '#', label: 'LinkedIn' },
];

export function LandingFooter() {
  return (
    <footer className="border-t border-neutral-800/60 bg-neutral-950">
      <div className="max-w-7xl mx-auto px-6 py-16">
        <div className="grid grid-cols-2 xl:grid-cols-5 gap-10 mb-14">
          {/* Brand */}
          <div className="col-span-2">
            <Link to="/" className="flex items-center gap-2.5 mb-4">
              <div className="w-7 h-7 bg-amber-500 rounded-lg flex items-center justify-center">
                <TrendingUp size={14} className="text-neutral-950" strokeWidth={2.5} />
              </div>
              <span className="text-[15px] font-bold text-neutral-100 tracking-tight">
                Trading <span className="text-amber-400">Forge</span>
              </span>
            </Link>
            <p className="text-sm text-neutral-500 leading-relaxed max-w-xs mb-6">
              La plataforma de simulación de trading de nivel profesional. Mercados reales, riesgo cero,
              aprendizaje ilimitado.
            </p>
            <div className="flex items-center gap-3">
              {SOCIAL.map(({ icon: Icon, href, label }) => (
                <a
                  key={label}
                  href={href}
                  aria-label={label}
                  className="w-8 h-8 rounded-lg bg-neutral-900 border border-neutral-800 flex items-center justify-center text-neutral-500 hover:text-neutral-100 hover:border-neutral-700 transition-all"
                >
                  <Icon size={14} />
                </a>
              ))}
            </div>
          </div>

          {/* Link columns */}
          {Object.entries(FOOTER_LINKS).map(([section, links]) => (
            <div key={section}>
              <h4 className="text-xs font-semibold text-neutral-400 uppercase tracking-[0.1em] mb-4">{section}</h4>
              <ul className="flex flex-col gap-2.5">
                {links.map(({ label, href }) => (
                  <li key={label}>
                    {href.startsWith('/') ? (
                      <Link
                        to={href}
                        className="text-sm text-neutral-500 hover:text-neutral-200 transition-colors"
                      >
                        {label}
                      </Link>
                    ) : (
                      <a
                        href={href}
                        className="text-sm text-neutral-500 hover:text-neutral-200 transition-colors"
                      >
                        {label}
                      </a>
                    )}
                  </li>
                ))}
              </ul>
            </div>
          ))}
        </div>

        {/* Bottom bar */}
        <div className="pt-6 border-t border-neutral-800/50 flex flex-col sm:flex-row items-center justify-between gap-4">
          <p className="text-xs text-neutral-600">
            © {new Date().getFullYear()} Trading Forge. Todos los derechos reservados.
          </p>
          <p className="text-xs text-neutral-700">
            Solo simulación de trading. No se involucran instrumentos financieros reales.
          </p>
        </div>
      </div>
    </footer>
  );
}
