import type { TourStep } from '@models/Onboarding';

/**
 * Beginner onboarding flow (TFS-37). Steps are config-driven so the order or
 * copy can change without touching the renderer. It now walks through the live
 * chart and its analysis tools, manual trading and the Strategies manager.
 */
export const TOUR_STEPS: TourStep[] = [
  {
    id: 'welcome',
    title: 'Bienvenido a Trading Forge',
    body: 'En menos de un minuto te mostramos cómo analizar el mercado, operar y automatizar con estrategias. Puedes saltar el tutorial cuando quieras.',
    placement: 'center',
    route: '/dashboard',
  },
  {
    id: 'empty-dashboard',
    target: '[data-tour="empty-dashboard"]',
    title: 'Tu panel está vacío',
    body: 'Todavía no sigues ningún activo. Aquí aparecerá el gráfico en cuanto añadas una moneda o acción para operar.',
    placement: 'top',
    route: '/dashboard',
  },
  {
    id: 'add-asset',
    target: '[data-tour="add-asset"]',
    title: 'Añade un activo',
    body: 'Pulsa "+ Añadir" (arriba a la derecha) para elegir entre todas las monedas y acciones disponibles, y empieza a ver su gráfico.',
    // Centered so the tooltip never covers the asset dropdown, which opens just
    // below the button in the top-right corner.
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
    body: 'Sigue el precio con velas japonesas, cambia el intervalo y activa indicadores como medias móviles (MA) y RSI para analizar el mercado.',
    placement: 'left',
    route: '/dashboard',
  },
  {
    id: 'drawing-tools',
    target: '[data-tour="drawing-tools"]',
    title: 'Herramientas de dibujo',
    body: 'Marca tus análisis sobre el gráfico: líneas de tendencia, Fibonacci, figuras o la regla. Puedes mover, deshacer y ocultar tus dibujos.',
    placement: 'right',
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
    id: 'strategies-nav',
    target: '[data-tour="nav-strategy"]',
    title: 'Estrategias automatizadas',
    body: 'Una estrategia es el entorno donde conectas bots: uno analiza el mercado, otro avisa y otro ejecuta la orden por ti. Entra a "Estrategias" para gestionarlas.',
    placement: 'right',
    route: '/dashboard',
  },
  {
    id: 'strategies-manager',
    target: '[data-tour="new-strategy"]',
    title: 'Crea tu primera estrategia',
    body: 'Aquí creas, abres, activas y duplicas tus estrategias. Pulsa "Nueva estrategia" para abrir el lienzo y conectar tus bots.',
    placement: 'bottom',
    route: '/strategy',
  },
  {
    id: 'done',
    title: '¡Todo listo!',
    body: 'Ya conoces lo esencial. Puedes repetir este tutorial cuando quieras desde el botón "Tutorial" en la barra lateral.',
    placement: 'center',
  },
];
