import { motion } from 'framer-motion';
import { ShieldCheck, Activity, Bot, BarChart2, BookOpen, Layers } from 'lucide-react';

const BENEFITS = [
  {
    icon: ShieldCheck,
    title: 'Riesgo Financiero Cero',
    description:
      'Cada operación usa capital virtual. Experimenta con apalancamiento, volatilidad y estrategias agresivas sin arriesgar ni un peso real.',
  },
  {
    icon: Activity,
    title: 'Datos de Mercado en Vivo',
    description:
      'Los precios y gráficos reflejan condiciones reales del mercado. Tu portfolio simulado responde igual que uno real ante noticias, resultados y eventos macro.',
  },
  {
    icon: Bot,
    title: 'Estrategias Algorítmicas',
    description:
      'Encadena Bots de Análisis, Bots de Notificación y Bots de Acción para construir flujos de trading completamente automatizados. Sin código — solo lógica.',
  },
  {
    icon: BarChart2,
    title: 'Analítica Profesional',
    description:
      'Monitorea tu P&L en tiempo real, revisa cada operación de tu historial y compara entre ciclos de simulación con registros inmutables.',
  },
  {
    icon: BookOpen,
    title: 'Aprendizaje Estructurado',
    description:
      'Reinicia cuando quieras. Cada simulación es un nuevo experimento. Construye intuición a través de la práctica, no de la teoría.',
  },
  {
    icon: Layers,
    title: 'Universo Multi-Activo',
    description:
      'Opera acciones, criptomonedas y otros instrumentos. Diversifica estrategias entre clases de activos — exactamente como los traders profesionales.',
  },
];

const containerVariants = {
  hidden: {},
  visible: { transition: { staggerChildren: 0.1 } },
};

const itemVariants = {
  hidden: { opacity: 0, y: 20 },
  visible: { opacity: 1, y: 0 },
};

export function BenefitsSection() {
  return (
    <section id="features" className="py-28 relative overflow-hidden">
      {/* Background orb */}
      <div className="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 w-[700px] h-[400px] bg-emerald-500/4 rounded-full blur-3xl pointer-events-none" />

      <div className="max-w-7xl mx-auto px-6">
        {/* Header */}
        <motion.div
          initial={{ opacity: 0, y: 16 }}
          whileInView={{ opacity: 1, y: 0 }}
          viewport={{ once: true, margin: '-80px' }}
          transition={{ duration: 0.7 }}
          className="text-center mb-20"
        >
          <span className="inline-block text-xs font-medium text-emerald-400 uppercase tracking-[0.15em] mb-4">
            Por qué Trading Forge
          </span>
          <h2 className="text-4xl xl:text-5xl font-black tracking-[-0.02em] text-neutral-50 mb-5">
            Todo lo que Necesitas
            <br />
            <span className="text-neutral-400">Para Operar Bien.</span>
          </h2>
          <p className="text-neutral-500 text-lg max-w-xl mx-auto leading-relaxed">
            Construido alrededor de las mecánicas reales del trading profesional — no un simulador simplificado.
          </p>
        </motion.div>

        {/* Grid */}
        <motion.div
          variants={containerVariants}
          initial="hidden"
          whileInView="visible"
          viewport={{ once: true, margin: '-60px' }}
          className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-5"
        >
          {BENEFITS.map(({ icon: Icon, title, description }) => (
            <motion.div
              key={title}
              variants={itemVariants}
              transition={{ duration: 0.6, ease: 'easeOut' }}
              className="group bg-neutral-900/50 border border-neutral-800/70 rounded-2xl p-6 hover:border-emerald-500/15 hover:bg-neutral-900/70 transition-all duration-300"
            >
              <div className="w-10 h-10 bg-emerald-500/10 border border-emerald-500/15 rounded-xl flex items-center justify-center mb-5 group-hover:bg-emerald-500/15 transition-colors">
                <Icon size={18} className="text-emerald-400" strokeWidth={1.75} />
              </div>
              <h3 className="text-base font-bold text-neutral-100 mb-2">{title}</h3>
              <p className="text-sm text-neutral-500 leading-relaxed">{description}</p>
            </motion.div>
          ))}
        </motion.div>
      </div>
    </section>
  );
}
