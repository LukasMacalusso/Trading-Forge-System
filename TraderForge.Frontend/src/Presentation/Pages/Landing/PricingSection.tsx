import { useState, useEffect, useRef } from 'react';
import { Link } from 'react-router-dom';
import { motion, useInView } from 'framer-motion';
import { Check, Zap } from 'lucide-react';

function Counter({ target, prefix = '', suffix = '', decimals = 0, duration = 1800 }: {
  target: number; prefix?: string; suffix?: string; decimals?: number; duration?: number;
}) {
  const [count, setCount] = useState(0);
  const ref = useRef<HTMLSpanElement>(null);
  const isInView = useInView(ref, { once: true, margin: '-60px' });

  useEffect(() => {
    if (!isInView) return;
    let startTime: number | null = null;
    const step = (ts: number) => {
      if (!startTime) startTime = ts;
      const elapsed = ts - startTime;
      const progress = Math.min(elapsed / duration, 1);
      const eased = 1 - Math.pow(1 - progress, 3);
      setCount(parseFloat((eased * target).toFixed(decimals)));
      if (progress < 1) requestAnimationFrame(step);
    };
    requestAnimationFrame(step);
  }, [isInView, target, duration, decimals]);

  return (
    <span ref={ref}>
      {prefix}{decimals > 0 ? count.toFixed(decimals) : count.toLocaleString()}{suffix}
    </span>
  );
}

const METRICS = [
  { value: 0, prefix: '$', suffix: '', label: 'Dinero real en riesgo', sub: 'Siempre y para siempre', decimals: 0 },
  { value: 100000, prefix: '$', suffix: '', label: 'Capital virtual inicial', sub: 'En cada cuenta nueva', decimals: 0 },
  { value: 7, prefix: '', suffix: ' días', label: 'Período de prueba gratis', sub: 'Sin tarjeta requerida', decimals: 0 },
  { value: 99.9, prefix: '', suffix: '%', label: 'Disponibilidad objetivo', sub: 'Excluyendo APIs externas', decimals: 1 },
];

interface Plan {
  name: string;
  price: string;
  period: string;
  description: string;
  virtualBalance: string;
  strategies: string;
  assets: string;
  extras: string[];
  cta: string;
  highlighted: boolean;
  badge?: string;
}

const PLANS: Plan[] = [
  {
    name: 'Basic',
    price: '$9.99',
    period: '/mes',
    description: 'Para traders que están empezando a explorar el mercado simulado.',
    virtualBalance: '$50,000 virtuales',
    strategies: 'Hasta 2 estrategias simultáneas',
    assets: 'Hasta 5 activos en portfolio',
    extras: ['Historial de simulaciones', 'Datos de mercado en tiempo real', 'Soporte por email'],
    cta: 'Empezar con Basic',
    highlighted: false,
  },
  {
    name: 'Pro',
    price: '$24.99',
    period: '/mes',
    description: 'Para traders que quieren automatizar y escalar sus estrategias.',
    virtualBalance: '$100,000 virtuales',
    strategies: 'Hasta 10 estrategias simultáneas',
    assets: 'Hasta 20 activos en portfolio',
    extras: [
      'Todo lo de Basic',
      'Bots de Notificación y Acción',
      'Analítica avanzada de P&L',
      'Soporte prioritario',
    ],
    cta: 'Empezar con Pro',
    highlighted: true,
    badge: 'Más popular',
  },
  {
    name: 'Enterprise',
    price: '$59.99',
    period: '/mes',
    description: 'Sin límites. Para perfiles avanzados que operan como profesionales.',
    virtualBalance: 'Saldo virtual ilimitado y modificable',
    strategies: 'Estrategias ilimitadas',
    assets: 'Activos ilimitados',
    extras: [
      'Todo lo de Pro',
      'Modificación de saldo virtual',
      'Acceso anticipado a nuevas funciones',
      'Soporte dedicado',
    ],
    cta: 'Contactar para Enterprise',
    highlighted: false,
  },
];


export function PricingSection() {
  return (
    <section id="pricing" className="py-28 relative overflow-hidden">
      {/* Background */}
      <div className="absolute inset-0 pointer-events-none">
        <div className="absolute top-0 left-1/2 -translate-x-1/2 w-[800px] h-[1px] bg-gradient-to-r from-transparent via-neutral-800 to-transparent" />
      </div>

      <div className="max-w-7xl mx-auto px-6">
        {/* Header */}
        <motion.div
          initial={{ opacity: 0, y: 16 }}
          whileInView={{ opacity: 1, y: 0 }}
          viewport={{ once: true, margin: '-80px' }}
          transition={{ duration: 0.7 }}
          className="text-center mb-6"
        >
          <span className="inline-block text-xs font-medium text-emerald-400 uppercase tracking-[0.15em] mb-4">
            Planes y Precios
          </span>
          <h2 className="text-4xl xl:text-5xl font-black tracking-[-0.02em] text-neutral-50 mb-5">
            Elige tu Nivel.
            <br />
            <span className="text-neutral-400">Empieza Gratis.</span>
          </h2>
          <p className="text-neutral-500 text-lg max-w-xl mx-auto leading-relaxed">
            Todos los planes incluyen 7 días de prueba gratuita. Sin compromiso, sin tarjeta.
          </p>
        </motion.div>

        {/* Metrics strip */}
        <div className="grid grid-cols-2 xl:grid-cols-4 gap-5 mb-14">
          {METRICS.map(({ value, prefix, suffix, label, sub, decimals }, idx) => (
            <motion.div
              key={label}
              initial={{ opacity: 0, y: 20 }}
              whileInView={{ opacity: 1, y: 0 }}
              viewport={{ once: true, margin: '-60px' }}
              transition={{ duration: 0.6, delay: idx * 0.1 }}
              className="bg-neutral-900/60 border border-neutral-800/60 rounded-2xl p-6 text-center"
            >
              <p className="text-3xl xl:text-4xl font-black font-mono text-emerald-400 mb-2">
                <Counter target={value} prefix={prefix} suffix={suffix} decimals={decimals} />
              </p>
              <p className="text-sm font-semibold text-neutral-200 mb-1">{label}</p>
              <p className="text-xs text-neutral-600">{sub}</p>
            </motion.div>
          ))}
        </div>

        {/* Cards */}
        <div className="grid grid-cols-1 md:grid-cols-3 gap-5 items-start">
          {PLANS.map(({ name, price, period, description, virtualBalance, strategies, assets, extras, cta, highlighted, badge }, idx) => (
            <motion.div
              key={name}
              initial={{ opacity: 0, y: 24 }}
              whileInView={{ opacity: 1, y: 0 }}
              viewport={{ once: true, margin: '-60px' }}
              transition={{ duration: 0.6, ease: 'easeOut', delay: idx * 0.1 }}
              className={`relative rounded-2xl p-6 flex flex-col gap-6 ${
                highlighted
                  ? 'bg-neutral-900 border border-emerald-500/30 shadow-[0_0_60px_rgba(16,185,129,0.08)]'
                  : 'bg-neutral-900/50 border border-neutral-800/70'
              }`}
            >
              {/* Badge */}
              {badge && (
                <div className="absolute -top-3 left-1/2 -translate-x-1/2">
                  <span className="inline-flex items-center gap-1.5 px-3 py-1 rounded-full bg-emerald-500 text-neutral-950 text-[11px] font-bold uppercase tracking-wide shadow-lg shadow-emerald-500/30">
                    <Zap size={10} strokeWidth={2.5} />
                    {badge}
                  </span>
                </div>
              )}

              {/* Plan name + price */}
              <div>
                <p className={`text-xs font-semibold uppercase tracking-[0.12em] mb-3 ${highlighted ? 'text-emerald-400' : 'text-neutral-500'}`}>
                  {name}
                </p>
                <div className="flex items-baseline gap-1 mb-2">
                  <span className="text-4xl font-black text-neutral-100 tracking-tight">{price}</span>
                  <span className="text-sm text-neutral-500">{period}</span>
                </div>
                <p className="text-sm text-neutral-500 leading-relaxed">{description}</p>
              </div>

              {/* Divider */}
              <div className="h-px bg-neutral-800/60" />

              {/* Key limits */}
              <ul className="flex flex-col gap-3">
                {[virtualBalance, strategies, assets].map((feature) => (
                  <li key={feature} className="flex items-start gap-2.5">
                    <Check
                      size={14}
                      strokeWidth={2.5}
                      className={`mt-0.5 shrink-0 ${highlighted ? 'text-emerald-400' : 'text-emerald-500/60'}`}
                    />
                    <span className="text-sm text-neutral-300">{feature}</span>
                  </li>
                ))}
              </ul>

              {/* Divider */}
              <div className="h-px bg-neutral-800/60" />

              {/* Extras */}
              <ul className="flex flex-col gap-2.5">
                {extras.map((e) => (
                  <li key={e} className="flex items-start gap-2.5">
                    <Check size={13} strokeWidth={2} className="mt-0.5 shrink-0 text-neutral-600" />
                    <span className="text-xs text-neutral-500">{e}</span>
                  </li>
                ))}
              </ul>

              {/* CTA */}
              <Link
                to="/register"
                className={`mt-auto w-full text-center py-3 rounded-xl text-sm font-semibold transition-all duration-200 ${
                  highlighted
                    ? 'bg-emerald-500 hover:bg-emerald-400 text-neutral-950 shadow-lg shadow-emerald-500/25 hover:shadow-emerald-500/40'
                    : 'bg-neutral-800 hover:bg-neutral-700 text-neutral-200 border border-neutral-700 hover:border-neutral-600'
                }`}
              >
                {cta}
              </Link>
            </motion.div>
          ))}
        </div>

        {/* Fine print */}
        <p className="text-center text-xs text-neutral-700 mt-8">
          Precios en USD. No se realizan cargos durante los 7 días de prueba gratuita.
        </p>
      </div>
    </section>
  );
}
