import { Link } from 'react-router-dom';
import { motion } from 'framer-motion';
import { ArrowRight } from 'lucide-react';

export function FinalCTASection() {
  return (
    <section className="py-28 relative overflow-hidden">
      {/* Background glow */}
      <div className="absolute inset-0 pointer-events-none">
        <div className="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 w-[600px] h-[300px] bg-amber-500/8 rounded-full blur-3xl" />
        <div className="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 w-[300px] h-[150px] bg-amber-400/6 rounded-full blur-2xl" />
      </div>

      <div className="relative max-w-4xl mx-auto px-6 text-center">
        <motion.div
          initial={{ opacity: 0, y: 24 }}
          whileInView={{ opacity: 1, y: 0 }}
          viewport={{ once: true, margin: '-80px' }}
          transition={{ duration: 0.8, ease: 'easeOut' }}
          className="flex flex-col items-center gap-8"
        >
          {/* Badge */}
          <span className="inline-flex items-center gap-2 px-3.5 py-1.5 rounded-full bg-amber-500/10 border border-amber-500/20 text-amber-400 text-xs font-medium">
            <span className="w-1.5 h-1.5 rounded-full bg-amber-400 animate-pulse" />
            Sin tarjeta · Cancela cuando quieras
          </span>

          {/* Headline */}
          <div>
            <h2 className="text-5xl xl:text-6xl font-black tracking-[-0.03em] text-neutral-50 leading-tight mb-3">
              Empieza a Operar
              <br />
              <span className="bg-gradient-to-r from-amber-400 via-amber-300 to-amber-500 bg-clip-text text-transparent">
                Sin Riesgo.
              </span>
            </h2>
            <p className="text-lg text-neutral-400 leading-relaxed max-w-xl mx-auto">
              Únete a Trading Forge y obtén $100,000 en capital virtual. Construye tu ventaja en un
              entorno de mercado real — antes de comprometer un solo peso.
            </p>
          </div>

          {/* CTA */}
          <div className="flex flex-col sm:flex-row items-center gap-4">
            <Link
              to="/register"
              className="group inline-flex items-center gap-2 px-8 py-4 bg-amber-500 hover:bg-amber-400 text-neutral-950 font-semibold rounded-xl transition-all duration-200 text-base shadow-2xl shadow-amber-500/30 hover:shadow-amber-500/50"
            >
              Comenzar Prueba Gratis
              <ArrowRight size={16} className="group-hover:translate-x-0.5 transition-transform duration-200" />
            </Link>
            <Link
              to="/login"
              className="text-sm text-neutral-500 hover:text-neutral-300 transition-colors"
            >
              ¿Ya tienes cuenta? Inicia sesión →
            </Link>
          </div>

          {/* Divider */}
          <div className="w-full max-w-xs h-px bg-gradient-to-r from-transparent via-neutral-800 to-transparent" />

          {/* Social proof */}
          <p className="text-xs text-neutral-600">
            Practica el oficio. Domina la disciplina. Opera con convicción.
          </p>
        </motion.div>
      </div>
    </section>
  );
}
