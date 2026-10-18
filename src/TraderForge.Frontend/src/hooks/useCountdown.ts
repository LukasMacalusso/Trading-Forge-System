import { useEffect, useRef, useState } from 'react';

interface Countdown {
  /** Milliseconds left until the deadline (never negative). */
  remainingMs: number;
  /** Human-readable `mm:ss` left. */
  label: string;
  /** True once the deadline has passed. */
  isExpired: boolean;
}

/**
 * Live `mm:ss` countdown to an ISO deadline, ticking once per second.
 * Calls `onExpire` exactly once when the deadline is first crossed.
 */
export function useCountdown(expiresAt: string, onExpire?: () => void): Countdown {
  const deadline = new Date(expiresAt).getTime();
  const [now, setNow] = useState(() => Date.now());
  const firedRef = useRef(false);

  // Keep the latest callback without re-subscribing the interval each tick.
  const onExpireRef = useRef(onExpire);
  onExpireRef.current = onExpire;

  useEffect(() => {
    firedRef.current = false;
    setNow(Date.now());
    const id = setInterval(() => setNow(Date.now()), 1000);
    return () => clearInterval(id);
  }, [expiresAt]);

  const remainingMs = Math.max(deadline - now, 0);
  const isExpired = remainingMs <= 0;

  useEffect(() => {
    if (isExpired && !firedRef.current) {
      firedRef.current = true;
      onExpireRef.current?.();
    }
  }, [isExpired]);

  const totalSeconds = Math.ceil(remainingMs / 1000);
  const minutes = Math.floor(totalSeconds / 60);
  const seconds = totalSeconds % 60;
  const label = `${String(minutes).padStart(2, '0')}:${String(seconds).padStart(2, '0')}`;

  return { remainingMs, label, isExpired };
}
