import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { motion } from 'framer-motion';
import { ArrowRight, Shield, Zap, BarChart2 } from 'lucide-react';

// Normalized prices (0–100) — uptrend with realistic noise
const PRICES = [44, 41, 50, 47, 56, 52, 60, 58, 68, 63, 72, 69, 77, 73, 81, 78, 85, 82, 90, 87, 94, 91, 98];

function buildLinePath(prices: number[], w: number, h: number, pad = 16): string {
  const W = w - pad * 2;
  const H = h - pad * 2;
  const min = Math.min(...prices);
  const max = Math.max(...prices);
  const range = max - min || 1;
  return prices
    .map((p, i) => {
      const x = (pad + (i / (prices.length - 1)) * W).toFixed(1);
      const y = (pad + H - ((p - min) / range) * H).toFixed(1);
      return `${i === 0 ? 'M' : 'L'}${x},${y}`;
    })
    .join(' ');
}

function buildAreaPath(prices: number[], w: number, h: number, pad = 16): string {
  const line = buildLinePath(prices, w, h, pad);
  const lastX = (w - pad).toFixed(1);
  const firstX = pad.toFixed(1);
  const bottom = (h - pad).toFixed(1);
  return `${line} L${lastX},${bottom} L${firstX},${bottom} Z`;
}

const W = 420;
const H = 160;

const linePath = buildLinePath(PRICES, W, H);
const areaPath = buildAreaPath(PRICES, W, H);

const lastPriceIndex = PRICES.length - 1;
const pad = 16;
const min = Math.min(...PRICES);
const max = Math.max(...PRICES);
const range = max - min;
const dotX = pad + ((lastPriceIndex / (PRICES.length - 1)) * (W - pad * 2));
const dotY = pad + (H - pad * 2) - ((PRICES[lastPriceIndex] - min) / range) * (H - pad * 2);

const TRUST_ITEMS = [
  { icon: Shield, label: 'Sin tarjeta de crédito' },
  { icon: BarChart2, label: 'Datos de mercado reales' },
  { icon: Zap, label: 'Listo en 60 segundos' },
];

export function HeroSection() {
  const [price, setPrice] = useState(68_425.3);
  const [change, setChange] = useState(2.34);
  const [isUp, setIsUp] = useState(true);

  useEffect(() => {
    const BASE = 68_425.3;
    const id = setInterval(() => {
      setPrice((prev) => {
        const delta = (Math.random() - 0.45) * 90; // NOSONAR:S2245 — UI animation only
        const next = Math.max(66_000, Math.min(71_000, prev + delta));
        const pct = ((next - BASE) / BASE) * 100;
        setChange(Math.abs(pct));
        setIsUp(pct >= 0);
        return next;
      });
    }, 2_200);
    return () => clearInterval(id);
  }, []);

  return (
    <section className="relative min-h-screen flex items-center pt-16 overflow-hidden">
      {/* Background */}
      <div className="absolute inset-0 pointer-events-none select-none">
        <div
          className="absolute inset-0 opacity-[0.025]"
          style={{
            backgroundImage:
              'linear-gradient(rgba(255,255,255,1) 1px, transparent 1px), linear-gradient(90deg, rgba(255,255,255,1) 1px, transparent 1px)',
            backgroundSize: '64px 64px',
          }}
        />
        <div className="absolute top-1/3 right-1/3 w-[520px] h-[520px] bg-amber-500/8 rounded-full blur-3xl" />
        <div className="absolute bottom-1/4 left-1/4 w-96 h-96 bg-indigo-500/6 rounded-full blur-3xl" />
        <div className="absolute top-1/2 left-1/2 w-64 h-64 bg-amber-400/4 rounded-full blur-3xl -translate-x-1/2 -translate-y-1/2" />
      </div>

      <div className="relative max-w-7xl mx-auto px-6 w-full grid grid-cols-1 lg:grid-cols-2 gap-12 xl:gap-20 items-center py-24">
        {/* ── Left: Copy ── */}
        <motion.div
          initial={{ opacity: 0, x: -24 }}
          animate={{ opacity: 1, x: 0 }}
          transition={{ duration: 0.85, ease: 'easeOut' }}
          className="flex flex-col gap-8"
        >
          {/* Badge */}
          <div>
            <span className="inline-flex items-center gap-2 px-3.5 py-1.5 rounded-full bg-amber-500/10 border border-amber-500/20 text-amber-400 text-xs font-medium tracking-wide">
              <span className="w-1.5 h-1.5 rounded-full bg-amber-400 animate-pulse" />
              En Beta — Prueba gratis de 7 días
            </span>
          </div>

          {/* Headline */}
          <div className="flex flex-col gap-1">
            <h1 className="text-5xl xl:text-6xl font-black tracking-[-0.03em] text-neutral-50 leading-[1.05]">
              Opera sin
            </h1>
            <h1 className="text-5xl xl:text-6xl font-black tracking-[-0.03em] leading-[1.05]">
              <span className="bg-gradient-to-r from-amber-400 via-amber-300 to-amber-500 bg-clip-text text-transparent">
                Riesgo.
              </span>{' '}
              <span className="text-neutral-50">Aprende</span>
            </h1>
            <h1 className="text-5xl xl:text-6xl font-black tracking-[-0.03em] text-neutral-50 leading-[1.05]">
              sin Límites.
            </h1>
          </div>

          {/* Sub */}
          <p className="text-[17px] text-neutral-400 leading-relaxed max-w-[480px]">
            Simula mercados financieros reales con precios en vivo. Construye estrategias
            automatizadas, analiza tu rendimiento y desarrolla disciplina profesional — con capital virtual.
          </p>

          {/* CTAs */}
          <div className="flex flex-col sm:flex-row items-start sm:items-center gap-3">
            <Link
              to="/register"
              className="group inline-flex items-center gap-2 px-6 py-3.5 bg-amber-500 hover:bg-amber-400 text-neutral-950 font-semibold rounded-xl transition-all duration-200 text-sm shadow-xl shadow-amber-500/25 hover:shadow-amber-500/40"
            >
              Comenzar prueba gratis
              <ArrowRight size={15} className="group-hover:translate-x-0.5 transition-transform duration-200" />
            </Link>
            <Link
              to="/login"
              className="inline-flex items-center gap-2 px-6 py-3.5 border border-neutral-700 hover:border-neutral-500 text-neutral-300 hover:text-neutral-100 rounded-xl transition-all duration-200 text-sm"
            >
              Iniciar sesión
            </Link>
          </div>

          {/* Trust */}
          <div className="flex flex-wrap items-center gap-5">
            {TRUST_ITEMS.map(({ icon: Icon, label }) => (
              <span key={label} className="flex items-center gap-1.5 text-xs text-neutral-500">
                <Icon size={12} className="text-amber-500/60" />
                {label}
              </span>
            ))}
          </div>
        </motion.div>

        {/* ── Right: Terminal ── */}
        <motion.div
          initial={{ opacity: 0, x: 24, y: 8 }}
          animate={{ opacity: 1, x: 0, y: 0 }}
          transition={{ duration: 0.95, delay: 0.15, ease: 'easeOut' }}
          className="relative flex items-center justify-center"
        >
          {/* Glow */}
          <div className="absolute inset-0 bg-amber-500/5 rounded-3xl blur-2xl scale-110" />

          {/* Terminal window */}
          <div className="relative w-full max-w-[500px] bg-[#0c0c0f] border border-neutral-800/70 rounded-2xl overflow-hidden shadow-[0_32px_80px_rgba(0,0,0,0.6)]">
            {/* Title bar */}
            <div className="flex items-center gap-2 px-4 py-3 border-b border-neutral-800/60 bg-neutral-900/40">
              <div className="w-2.5 h-2.5 rounded-full bg-[#FF5F57]" />
              <div className="w-2.5 h-2.5 rounded-full bg-[#FFBD2E]" />
              <div className="w-2.5 h-2.5 rounded-full bg-[#28C840]" />
              <span className="text-[11px] text-neutral-600 ml-2 font-mono">Trading Forge Terminal</span>
              <div className="ml-auto flex items-center gap-1.5">
                <span className="w-1.5 h-1.5 rounded-full bg-amber-400 animate-pulse" />
                <span className="text-[10px] text-amber-400 font-mono tracking-widest">LIVE</span>
              </div>
            </div>

            {/* Price header */}
            <div className="flex items-center gap-3 px-4 py-3 border-b border-neutral-800/40">
              <div className="flex items-baseline gap-1">
                <span className="text-sm font-bold text-neutral-200 font-mono">BTCUSDT</span>
                <span className="text-xs text-neutral-600">/USDT</span>
              </div>
              <motion.span
                key={Math.round(price / 10)}
                initial={{ opacity: 0.4 }}
                animate={{ opacity: 1 }}
                transition={{ duration: 0.4 }}
                className={`text-lg font-mono font-bold ${isUp ? 'text-emerald-400' : 'text-red-400'}`}
              >
                ${price.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}
              </motion.span>
              <span
                className={`text-[10px] font-mono px-1.5 py-0.5 rounded ${
                  isUp ? 'bg-emerald-500/15 text-emerald-400' : 'bg-red-500/15 text-red-400'
                }`}
              >
                {isUp ? '+' : '-'}{change.toFixed(2)}%
              </span>
              <span className="ml-auto text-[10px] text-neutral-600 font-mono">24h Vol 38.4B</span>
            </div>

            {/* Chart */}
            <div className="bg-[#080809] px-1 pt-2 pb-0">
              <svg viewBox={`0 0 ${W} ${H}`} className="w-full" style={{ height: 150 }}>
                <defs>
                  <linearGradient id="heroGrad" x1="0" y1="0" x2="0" y2="1">
                    <stop offset="0%" stopColor="#f59e0b" stopOpacity="0.22" />
                    <stop offset="100%" stopColor="#f59e0b" stopOpacity="0" />
                  </linearGradient>
                  <filter id="heroGlow">
                    <feGaussianBlur stdDeviation="2.5" result="blur" />
                    <feMerge>
                      <feMergeNode in="blur" />
                      <feMergeNode in="SourceGraphic" />
                    </feMerge>
                  </filter>
                  <clipPath id="chartReveal">
                    <motion.rect
                      x={0}
                      y={0}
                      height={H}
                      initial={{ width: 0 }}
                      animate={{ width: W }}
                      transition={{ duration: 1.1, ease: 'easeOut', delay: 0.3 }}
                    />
                  </clipPath>
                </defs>

                {/* Horizontal grid */}
                {[40, 80, 120].map((y) => (
                  <line key={y} x1="16" y1={y} x2={W - 16} y2={y} stroke="#18181f" strokeWidth="1" />
                ))}

                {/* Area */}
                <path d={areaPath} fill="url(#heroGrad)" clipPath="url(#chartReveal)" />

                {/* Line revealed by clipPath — no per-frame path calculation */}
                <path
                  d={linePath}
                  stroke="#f59e0b"
                  strokeWidth="2"
                  fill="none"
                  filter="url(#heroGlow)"
                  clipPath="url(#chartReveal)"
                />

                {/* Dot at current price */}
                <motion.g initial={{ opacity: 0 }} animate={{ opacity: 1 }} transition={{ delay: 1.5, duration: 0.3 }}>
                  <circle cx={dotX} cy={dotY} r="5" fill="#f59e0b" opacity="0.3" />
                  <circle cx={dotX} cy={dotY} r="3" fill="#f59e0b" />
                </motion.g>

                {/* Dashed horizontal line at current price */}
                <motion.line
                  x1="16" y1={dotY} x2={dotX - 6} y2={dotY}
                  stroke="#f59e0b" strokeWidth="1" strokeDasharray="3 3" opacity="0.4"
                  initial={{ pathLength: 0 }}
                  animate={{ pathLength: 1 }}
                  transition={{ delay: 1.5, duration: 0.4 }}
                />
              </svg>
            </div>

            {/* Stats bar */}
            <div className="grid grid-cols-3 divide-x divide-neutral-800/50 border-t border-neutral-800/50">
              {[
                { label: 'Valor del Portfolio', value: '$127,450', sub: '+9.7%', color: 'text-emerald-400' },
                { label: 'P&L de Hoy', value: '+$1,240', sub: '+0.98%', color: 'text-emerald-400' },
                { label: 'Efectividad', value: '73.2%', sub: '18 operaciones', color: 'text-neutral-200' },
              ].map(({ label, value, sub, color }) => (
                <div key={label} className="px-4 py-3">
                  <p className="text-[10px] text-neutral-500 uppercase tracking-wide mb-1">{label}</p>
                  <p className={`text-sm font-mono font-bold ${color}`}>{value}</p>
                  <p className="text-[10px] text-neutral-600">{sub}</p>
                </div>
              ))}
            </div>
          </div>

          {/* Floating card — Best trade */}
          <motion.div
            initial={{ opacity: 0, x: 12, y: -8 }}
            animate={{ opacity: 1, x: 0, y: 0 }}
            transition={{ delay: 1.4, duration: 0.5, ease: 'easeOut' }}
            className="absolute -right-4 top-10 hidden lg:block bg-neutral-900/90 backdrop-blur-xl border border-neutral-700/40 rounded-xl p-3 shadow-2xl"
          >
            <p className="text-[10px] text-neutral-500 uppercase tracking-wide mb-0.5">Mejor Operación</p>
            <p className="text-sm font-mono font-bold text-emerald-400">+$4,280</p>
            <p className="text-[10px] text-neutral-600">AAPL · Largo</p>
          </motion.div>

          {/* Floating card — Active bots */}
          <motion.div
            initial={{ opacity: 0, x: -12, y: 8 }}
            animate={{ opacity: 1, x: 0, y: 0 }}
            transition={{ delay: 1.7, duration: 0.5, ease: 'easeOut' }}
            className="absolute -left-4 bottom-20 hidden lg:block bg-neutral-900/90 backdrop-blur-xl border border-neutral-700/40 rounded-xl p-3 shadow-2xl"
          >
            <p className="text-[10px] text-neutral-500 uppercase tracking-wide mb-0.5">Bots Activos</p>
            <p className="text-sm font-mono font-bold text-neutral-100">3 activos</p>
            <p className="text-[10px] text-emerald-500">Todos rentables</p>
          </motion.div>
        </motion.div>
      </div>

      {/* Bottom fade */}
      <div className="absolute bottom-0 left-0 right-0 h-32 bg-gradient-to-t from-neutral-950 to-transparent pointer-events-none" />
    </section>
  );
}
