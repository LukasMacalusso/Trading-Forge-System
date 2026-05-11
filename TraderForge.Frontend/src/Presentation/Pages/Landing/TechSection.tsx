import { motion } from 'framer-motion';
import { Counter, METRICS } from './LandingShared.tsx';

const TECH_PILLARS = [
  {
    label: 'Precios en Tiempo Real',
    description: 'Los datos de mercado se actualizan vía hub SignalR WebSocket. Latencia sub-segundo en tu navegador.',
  },
  {
    label: 'Motor de Bots Asíncrono',
    description: 'Las estrategias automatizadas ejecutan en el servidor las 24/7. Los flujos pueden pausarse y reanudarse sin bloqueos.',
  },
  {
    label: 'Snapshots Inmutables',
    description: 'Cada simulación completada queda sellada como registro de solo lectura. Tu historial nunca cambia.',
  },
  {
    label: 'Autenticación JWT',
    description: 'Acceso basado en roles para Traders y Administradores. Las credenciales nunca salen de tu red sin cifrar.',
  },
];

export function TechSection() {
  return (
    <section className="py-28 relative overflow-hidden">
      {/* Background */}
      <div className="absolute inset-0 pointer-events-none">
        <div
          className="absolute inset-0 opacity-[0.02]"
          style={{
            backgroundImage:
              'linear-gradient(rgba(245,158,11,1) 1px, transparent 1px), linear-gradient(90deg, rgba(245,158,11,1) 1px, transparent 1px)',
            backgroundSize: '48px 48px',
          }}
        />
      </div>

      <div className="max-w-7xl mx-auto px-6">
        {/* Header */}
        <motion.div
          initial={{ opacity: 0, y: 16 }}
          whileInView={{ opacity: 1, y: 0 }}
          viewport={{ once: true, margin: '-80px' }}
          transition={{ duration: 0.7 }}
          className="text-center mb-20"
        >
          <span className="inline-block text-xs font-medium text-amber-400 uppercase tracking-[0.15em] mb-4">
            En Números
          </span>
          <h2 className="text-4xl xl:text-5xl font-black tracking-[-0.02em] text-neutral-50 mb-5">
            Construido para
            <br />
            <span className="text-neutral-400">Rendir de Verdad.</span>
          </h2>
        </motion.div>

        {/* Metrics */}
        <div className="grid grid-cols-2 xl:grid-cols-4 gap-5 mb-20">
          {METRICS.map(({ value, prefix, suffix, label, sub, decimals }, idx) => (
            <motion.div
              key={label}
              initial={{ opacity: 0, y: 20 }}
              whileInView={{ opacity: 1, y: 0 }}
              viewport={{ once: true, margin: '-60px' }}
              transition={{ duration: 0.6, delay: idx * 0.1 }}
              className="bg-neutral-900/60 border border-neutral-800/60 rounded-2xl p-6 text-center"
            >
              <p className="text-3xl xl:text-4xl font-black font-mono text-amber-400 mb-2">
                <Counter
                  target={value}
                  prefix={prefix}
                  suffix={suffix}
                  decimals={decimals}
                />
              </p>
              <p className="text-sm font-semibold text-neutral-200 mb-1">{label}</p>
              <p className="text-xs text-neutral-600">{sub}</p>
            </motion.div>
          ))}
        </div>

        {/* Tech pillars */}
        <div className="grid grid-cols-1 md:grid-cols-2 gap-5">
          {TECH_PILLARS.map(({ label, description }, idx) => (
            <motion.div
              key={label}
              initial={{ opacity: 0, x: idx % 2 === 0 ? -16 : 16 }}
              whileInView={{ opacity: 1, x: 0 }}
              viewport={{ once: true, margin: '-60px' }}
              transition={{ duration: 0.65, delay: idx * 0.08 }}
              className="flex items-start gap-4 bg-neutral-900/40 border border-neutral-800/50 rounded-xl p-5"
            >
              <div className="w-2 h-2 rounded-full bg-amber-500 mt-1.5 shrink-0" />
              <div>
                <h4 className="text-sm font-bold text-neutral-100 mb-1">{label}</h4>
                <p className="text-sm text-neutral-500 leading-relaxed">{description}</p>
              </div>
            </motion.div>
          ))}
        </div>
      </div>
    </section>
  );
}
