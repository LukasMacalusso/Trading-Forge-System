import { motion } from 'framer-motion';
import { UserPlus, TrendingUp, Bot } from 'lucide-react';

const STEPS = [
  {
    number: '01',
    icon: UserPlus,
    title: 'Crea tu Cuenta',
    description:
      'Regístrate en segundos. Sin tarjeta ni compromiso. Cada cuenta nueva recibe $100,000 en capital virtual y 7 días de prueba gratis de forma automática.',
  },
  {
    number: '02',
    icon: TrendingUp,
    title: 'Simula Operaciones Reales',
    description:
      'Compra y vende activos a precios de mercado reales. Experimenta comisiones de broker auténticas, horarios de mercado y mecánicas de ejecución — sin exposición financiera.',
  },
  {
    number: '03',
    icon: Bot,
    title: 'Automatiza tus Estrategias',
    description:
      'Diseña flujos algorítmicos encadenando bots de análisis, notificación y acción. Tus estrategias se ejecutan en el servidor de forma continua — operan por ti incluso cuando no estás conectado.',
  },
];

const containerVariants = {
  hidden: {},
  visible: { transition: { staggerChildren: 0.15 } },
};

const itemVariants = {
  hidden: { opacity: 0, y: 24 },
  visible: { opacity: 1, y: 0 },
};

export function HowItWorksSection() {
  return (
    <section id="how-it-works" className="relative pt-28 pb-8 overflow-hidden">
      {/* Subtle top separator */}
      <div className="absolute top-0 left-1/2 -translate-x-1/2 w-px h-16 bg-gradient-to-b from-neutral-800 to-transparent" />

      <div className="max-w-7xl mx-auto px-6">
        {/* Steps grid */}
        <motion.div
          variants={containerVariants}
          initial="hidden"
          whileInView="visible"
          viewport={{ once: true, margin: '-60px' }}
          className="grid grid-cols-1 md:grid-cols-3 xl:grid-cols-3 gap-6"
          transition={{ staggerChildren: 0.15 }}
        >
          {STEPS.map(({ number, icon: Icon, title, description }, idx) => (
            <motion.div key={number} variants={itemVariants} transition={{ duration: 0.65, ease: 'easeOut' }} className="relative group">
              {/* Connector line (desktop only, between cards) */}
              {idx < STEPS.length - 1 && (
                <div className="hidden xl:block absolute top-10 left-full w-6 h-px bg-gradient-to-r from-neutral-700 to-neutral-800 z-10" />
              )}

              <div className="h-full bg-neutral-900/60 border border-neutral-800/80 rounded-2xl p-6 flex flex-col gap-5 hover:border-emerald-500/20 hover:bg-neutral-900/80 transition-all duration-300 group-hover:shadow-lg group-hover:shadow-emerald-500/5">
                {/* Number + icon row */}
                <div className="flex items-start justify-between">
                  <div className="w-11 h-11 bg-emerald-500/10 border border-emerald-500/20 rounded-xl flex items-center justify-center group-hover:bg-emerald-500/15 transition-colors">
                    <Icon size={18} className="text-emerald-400" strokeWidth={1.75} />
                  </div>
                  <span className="text-3xl font-black text-neutral-800 group-hover:text-neutral-700 transition-colors font-mono">
                    {number}
                  </span>
                </div>

                {/* Text */}
                <div>
                  <h3 className="text-base font-bold text-neutral-100 mb-2">{title}</h3>
                  <p className="text-sm text-neutral-500 leading-relaxed">{description}</p>
                </div>
              </div>
            </motion.div>
          ))}
        </motion.div>
      </div>
    </section>
  );
}
