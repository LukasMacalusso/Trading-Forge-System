import { useState, useEffect } from 'react';
import { motion } from 'framer-motion';
import { LayoutDashboard, Briefcase, Bot, Clock, CreditCard, TrendingUp } from 'lucide-react';

// Simulated assets for the ticker
const ASSETS = [
  { symbol: 'AAPL', name: 'Apple Inc.', price: 213.45, change: 1.82 },
  { symbol: 'BTC', name: 'Bitcoin', price: 68425.3, change: 2.34 },
  { symbol: 'TSLA', name: 'Tesla Inc.', price: 178.92, change: -0.74 },
  { symbol: 'ETH', name: 'Ethereum', price: 3512.8, change: 1.17 },
  { symbol: 'NVDA', name: 'NVIDIA', price: 875.4, change: 3.21 },
];

// Order book rows
const ASKS = [
  { price: 68450, qty: 0.834 },
  { price: 68470, qty: 1.203 },
  { price: 68490, qty: 0.562 },
  { price: 68510, qty: 2.114 },
  { price: 68535, qty: 0.773 },
];
const BIDS = [
  { price: 68415, qty: 1.452 },
  { price: 68395, qty: 0.987 },
  { price: 68370, qty: 1.834 },
  { price: 68345, qty: 0.621 },
  { price: 68320, qty: 2.302 },
];


export function PlatformPreviewSection() {
  const [selectedAsset, setSelectedAsset] = useState(ASSETS[1]);
  const [livePrice, setLivePrice] = useState(ASSETS[1].price);
  const [orderFlash, setOrderFlash] = useState<string | null>(null);

  useEffect(() => {
    const id = setInterval(() => {
      setLivePrice((p) => {
        const delta = (Math.random() - 0.47) * 95;
        return Math.max(selectedAsset.price * 0.97, Math.min(selectedAsset.price * 1.03, p + delta));
      });
    }, 1_800);
    return () => clearInterval(id);
  }, [selectedAsset]);

  useEffect(() => {
    setLivePrice(selectedAsset.price);
  }, [selectedAsset]);

  // Simulate occasional order fills
  useEffect(() => {
    const msgs = ['COMPRA 0.05 BTC ejecutada a $68,425', 'VENTA 2 AAPL ejecutada a $213.45', 'BOT activado: entrada NVDA'];
    let i = 0;
    const id = setInterval(() => {
      setOrderFlash(msgs[i % msgs.length]);
      i++;
      setTimeout(() => setOrderFlash(null), 2_800);
    }, 5_000);
    return () => clearInterval(id);
  }, []);

  const isUp = livePrice >= selectedAsset.price;

  return (
    <section id="platform" className="pb-24 overflow-hidden">
      <div className="max-w-7xl mx-auto px-6">
        {/* Platform mockup */}
        <motion.div
          initial={{ opacity: 0, y: 32 }}
          whileInView={{ opacity: 1, y: 0 }}
          viewport={{ once: true, margin: '-60px' }}
          transition={{ duration: 0.8, ease: 'easeOut' }}
          className="relative"
        >
          {/* Glow */}
          <div className="absolute -inset-2 bg-amber-500/4 rounded-3xl blur-2xl" />

          <div className="relative bg-neutral-950 border border-neutral-800 rounded-2xl overflow-hidden shadow-[0_40px_100px_rgba(0,0,0,0.7)]">
            {/* Window bar */}
            <div className="flex items-center gap-2 px-4 py-3 bg-neutral-900/60 border-b border-neutral-800">
              <div className="w-3 h-3 rounded-full bg-[#FF5F57]" />
              <div className="w-3 h-3 rounded-full bg-[#FFBD2E]" />
              <div className="w-3 h-3 rounded-full bg-[#28C840]" />
              <span className="text-xs text-neutral-600 font-mono ml-3">Trading Forge — Dashboard</span>
              <div className="ml-auto flex items-center gap-2 text-[10px] text-neutral-600 font-mono">
                <span className="w-1.5 h-1.5 rounded-full bg-amber-400 animate-pulse" />
                MERCADOS ABIERTOS
              </div>
            </div>

            <div className="flex h-[500px]">
              {/* Sidebar */}
              <aside className="w-14 flex flex-col items-center py-4 gap-3 bg-neutral-900 border-r border-neutral-800 shrink-0">
                <div className="w-7 h-7 bg-amber-500/20 rounded-lg flex items-center justify-center mb-2">
                  <TrendingUp size={12} className="text-amber-400" />
                </div>
                {[LayoutDashboard, Briefcase, Bot, Clock, CreditCard].map((Icon, i) => (
                  <div
                    key={i}
                    className={`w-9 h-9 rounded-lg flex items-center justify-center transition-colors ${
                      i === 0 ? 'bg-amber-500/10' : 'hover:bg-neutral-800'
                    }`}
                  >
                    <Icon size={14} className={i === 0 ? 'text-amber-400' : 'text-neutral-600'} />
                  </div>
                ))}
              </aside>

              {/* Main area */}
              <div className="flex flex-col flex-1 min-w-0">
                {/* Ticker bar */}
                <div className="flex border-b border-neutral-800 overflow-x-auto bg-neutral-950 shrink-0">
                  {ASSETS.map((asset) => {
                    const selected = asset.symbol === selectedAsset.symbol;
                    return (
                      <button
                        key={asset.symbol}
                        onClick={() => setSelectedAsset(asset)}
                        className={`flex flex-col items-start px-4 py-2.5 border-r border-neutral-800 shrink-0 transition-colors ${
                          selected ? 'bg-neutral-900 border-b-2 border-b-amber-500' : 'hover:bg-neutral-900/50'
                        }`}
                      >
                        <div className="flex items-center gap-2">
                          <span className="text-[11px] font-bold text-neutral-200">{asset.symbol}</span>
                          <span
                            className={`text-[10px] ${asset.change >= 0 ? 'text-amber-400' : 'text-red-400'}`}
                          >
                            {asset.change >= 0 ? '+' : ''}
                            {asset.change.toFixed(2)}%
                          </span>
                        </div>
                        <span className="text-xs font-mono text-neutral-400">
                          ${asset.price.toLocaleString('en-US', { maximumFractionDigits: 2 })}
                        </span>
                      </button>
                    );
                  })}
                </div>

                {/* Content row */}
                <div className="flex flex-1 min-h-0">
                  {/* Center: Chart + info */}
                  <div className="flex-1 flex flex-col gap-3 p-4 min-w-0">
                    {/* Asset header */}
                    <div className="flex items-center gap-3">
                      <span className="text-base font-bold text-neutral-100">{selectedAsset.symbol}</span>
                      <span className="text-sm text-neutral-500">{selectedAsset.name}</span>
                      <motion.span
                        key={Math.round(livePrice / 5)}
                        initial={{ opacity: 0.5 }}
                        animate={{ opacity: 1 }}
                        className={`text-lg font-mono font-bold ${isUp ? 'text-emerald-400' : 'text-red-400'}`}
                      >
                        ${livePrice.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}
                      </motion.span>
                      <span
                        className={`text-[10px] px-1.5 py-0.5 rounded font-mono ${
                          selectedAsset.change >= 0
                            ? 'bg-amber-500/10 text-amber-400'
                            : 'bg-red-500/10 text-red-400'
                        }`}
                      >
                        {selectedAsset.change >= 0 ? '+' : ''}
                        {selectedAsset.change.toFixed(2)}%
                      </span>
                    </div>

                    {/* Simulated chart area */}
                    <div className="flex-1 bg-[#080809] rounded-xl border border-neutral-800/50 relative overflow-hidden">
                      <svg viewBox="0 0 600 200" className="w-full h-full" preserveAspectRatio="none">
                        <defs>
                          <linearGradient id="previewAreaGrad" x1="0" y1="0" x2="0" y2="1">
                            <stop offset="0%" stopColor="#f59e0b" stopOpacity="0.18" />
                            <stop offset="100%" stopColor="#f59e0b" stopOpacity="0" />
                          </linearGradient>
                        </defs>
                        {/* Grid */}
                        {[50, 100, 150].map((y) => (
                          <line key={y} x1="0" y1={y} x2="600" y2={y} stroke="#1a1a1f" strokeWidth="1" />
                        ))}
                        {/* Area + line */}
                        <path
                          d="M0,160 L60,145 L120,130 L180,140 L240,110 L300,95 L360,80 L420,85 L480,60 L540,45 L600,30 L600,200 L0,200 Z"
                          fill="url(#previewAreaGrad)"
                        />
                        <path
                          d="M0,160 L60,145 L120,130 L180,140 L240,110 L300,95 L360,80 L420,85 L480,60 L540,45 L600,30"
                          stroke="#f59e0b"
                          strokeWidth="1.5"
                          fill="none"
                        />
                        {/* Volume bars */}
                        {[0, 60, 120, 180, 240, 300, 360, 420, 480, 540].map((x, i) => (
                          <rect
                            key={x}
                            x={x + 2}
                            y={200 - (15 + Math.sin(i) * 8)}
                            width="48"
                            height={15 + Math.sin(i) * 8}
                            fill={i % 3 === 0 ? '#ef444420' : '#f59e0b18'}
                          />
                        ))}
                      </svg>

                      {/* Fake candle overlay for realism */}
                      <div className="absolute top-3 left-3 text-[10px] font-mono text-neutral-600">
                        1D · OHLC
                      </div>
                    </div>

                    {/* Portfolio bar */}
                    <div className="flex items-center gap-5 px-3 py-2 bg-neutral-900 rounded-lg border border-neutral-800 text-xs">
                      <div>
                        <span className="text-neutral-500">Balance </span>
                        <span className="font-mono font-semibold text-neutral-100">$87,240.50</span>
                      </div>
                      <div>
                        <span className="text-neutral-500">Portfolio </span>
                        <span className="font-mono font-semibold text-neutral-100">$127,450.00</span>
                      </div>
                      <div>
                        <span className="text-neutral-500">Total P&L </span>
                        <span className="font-mono font-semibold text-emerald-400">+$27,450 (+27.4%)</span>
                      </div>
                    </div>
                  </div>

                  {/* Right panel: Order book + execution */}
                  <div className="w-60 shrink-0 flex flex-col border-l border-neutral-800">
                    {/* Order book */}
                    <div className="flex-1 overflow-hidden">
                      <div className="flex justify-between items-center px-3 py-2 border-b border-neutral-800">
                        <span className="text-[10px] font-semibold text-neutral-500 uppercase tracking-wide">Order Book</span>
                      </div>
                      <div className="px-3 py-1 text-[9px] text-neutral-700 flex justify-between">
                        <span>Precio</span><span>Cant.</span>
                      </div>
                      {/* Asks */}
                      {ASKS.map((ask) => (
                        <div key={ask.price} className="relative flex justify-between px-3 py-0.5">
                          <div
                            className="absolute right-0 top-0 bottom-0 bg-red-500/10"
                            style={{ width: `${(ask.qty / 2.5) * 100}%` }}
                          />
                          <span className="relative text-[10px] font-mono text-red-400">${ask.price.toLocaleString()}</span>
                          <span className="relative text-[10px] font-mono text-neutral-500">{ask.qty.toFixed(3)}</span>
                        </div>
                      ))}
                      {/* Spread */}
                      <div className="flex justify-between px-3 py-1 bg-neutral-800/40 border-y border-neutral-800">
                        <span className="text-[11px] font-mono font-bold text-neutral-100">
                          ${livePrice.toLocaleString('en-US', { minimumFractionDigits: 0, maximumFractionDigits: 0 })}
                        </span>
                        <span className="text-[9px] text-neutral-600">Spread $15</span>
                      </div>
                      {/* Bids */}
                      {BIDS.map((bid) => (
                        <div key={bid.price} className="relative flex justify-between px-3 py-0.5">
                          <div
                            className="absolute left-0 top-0 bottom-0 bg-emerald-500/10"
                            style={{ width: `${(bid.qty / 2.5) * 100}%` }}
                          />
                          <span className="relative text-[10px] font-mono text-emerald-400">${bid.price.toLocaleString()}</span>
                          <span className="relative text-[10px] font-mono text-neutral-500">{bid.qty.toFixed(3)}</span>
                        </div>
                      ))}
                    </div>

                    {/* Execution panel */}
                    <div className="border-t border-neutral-800 p-3 flex flex-col gap-2">
                      <div className="flex rounded-lg overflow-hidden border border-neutral-800">
                        <div className="flex-1 py-1.5 text-center text-[11px] font-semibold bg-amber-500 text-neutral-950">
                          Buy
                        </div>
                        <div className="flex-1 py-1.5 text-center text-[11px] text-neutral-500">Sell</div>
                      </div>
                      <div className="bg-neutral-800/50 rounded px-2 py-1.5 flex justify-between items-center">
                        <span className="text-[10px] text-neutral-500">Cant.</span>
                        <span className="text-[10px] font-mono text-neutral-300">0.05</span>
                      </div>
                      <div className="bg-neutral-800/50 rounded px-2 py-1.5 flex justify-between items-center">
                        <span className="text-[10px] text-neutral-500">Total est.</span>
                        <span className="text-[10px] font-mono text-neutral-300">$3,421.26</span>
                      </div>
                      <div className="bg-amber-500/90 rounded py-1.5 text-center text-[11px] font-semibold text-neutral-950">
                        Ejecutar Compra
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>

            {/* Status bar */}
            <div className="flex items-center gap-4 px-4 py-2 bg-neutral-900/40 border-t border-neutral-800 text-[10px] font-mono text-neutral-600">
              <span className="text-amber-500">● Conectado</span>
              <span>Latencia 12ms</span>
              <span>NYSE · NASDAQ · CRYPTO</span>
              <div className="ml-auto">
                {orderFlash ? (
                  <motion.span
                    key={orderFlash}
                    initial={{ opacity: 0, x: 8 }}
                    animate={{ opacity: 1, x: 0 }}
                    className="text-amber-400"
                  >
                    ✓ {orderFlash}
                  </motion.span>
                ) : (
                  <span>Listo</span>
                )}
              </div>
            </div>
          </div>
        </motion.div>

      </div>
    </section>
  );
}
