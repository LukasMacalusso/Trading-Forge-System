import { useEffect, useRef, useState } from 'react';
import { useInView } from 'framer-motion';

export interface CounterProps {
  target: number;
  prefix?: string;
  suffix?: string;
  decimals?: number;
  duration?: number;
}

export function Counter({ target, prefix = '', suffix = '', decimals = 0, duration = 1800 }: CounterProps) {
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
      {prefix}
      {decimals > 0 ? count.toFixed(decimals) : count.toLocaleString()}
      {suffix}
    </span>
  );
}

export const METRICS = [
  { value: 0, prefix: '$', suffix: '', label: 'Dinero real en riesgo', sub: 'Siempre y para siempre', decimals: 0 },
  { value: 100000, prefix: '$', suffix: '', label: 'Capital virtual inicial', sub: 'En cada cuenta nueva', decimals: 0 },
  { value: 7, prefix: '', suffix: ' días', label: 'Período de prueba gratis', sub: 'Sin tarjeta requerida', decimals: 0 },
  { value: 99.9, prefix: '', suffix: '%', label: 'Disponibilidad objetivo', sub: 'Excluyendo APIs externas', decimals: 1 },
] as const;
