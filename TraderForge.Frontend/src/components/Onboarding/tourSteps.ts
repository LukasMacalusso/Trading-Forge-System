import type { TourStep } from '@models/Onboarding';

/**
 * Beginner onboarding flow (TFS-37). Steps are config-driven so the order or
 * copy can change without touching the renderer. The `bots` step is centered
 * for now because the Strategy Builder route is not part of this branch yet.
 */
export const TOUR_STEPS: TourStep[] = [
  {
    id: 'welcome',
    title: 'Bienvenido a Trading Forge',
    body: 'En menos de un minuto te mostramos cómo operar y cómo automatizar con bots. Puedes saltar el tutorial cuando quieras.',
    placement: 'center',
    route: '/dashboard',
  },
  {
    id: 'sidebar',
    target: '[data-tour="sidebar"]',
    title: 'Tu navegación',
    body: 'Desde aquí accedes al Dashboard, tu Portfolio, el mercado completo y tus estrategias automatizadas.',
    placement: 'right',
    route: '/dashboard',
  },
  {
    id: 'chart',
    target: '[data-tour="chart"]',
    title: 'Gráfico en vivo',
    body: 'Sigue el precio con velas japonesas y cambia el intervalo de tiempo para analizar el mercado a tu ritmo.',
    placement: 'left',
    route: '/dashboard',
  },
  {
    id: 'balance',
    target: '[data-tour="balance"]',
    title: 'Tu balance virtual',
    body: 'Operas con dinero virtual, sin riesgo real. Aquí ves tu balance, el valor de tu portfolio y tu P&L en tiempo real.',
    placement: 'left',
    route: '/dashboard',
  },
  {
    id: 'execution',
    target: '[data-tour="execution"]',
    title: 'Trading manual',
    body: 'Compra y vende el activo seleccionado al instante: define la cantidad y confirma tu orden.',
    placement: 'left',
    route: '/dashboard',
  },
  {
    id: 'bots',
    title: 'Bots y estrategias',
    body: '¿Prefieres automatizar? En Estrategias armas flujos visuales: un bot analiza el mercado, otro te avisa y otro ejecuta la orden por ti.',
    placement: 'center',
    route: '/dashboard',
  },
  {
    id: 'done',
    title: '¡Todo listo!',
    body: 'Ya conoces lo esencial. Puedes repetir este tutorial cuando quieras desde el botón de ayuda en la barra lateral.',
    placement: 'center',
  },
];
