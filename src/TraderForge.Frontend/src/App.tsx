import { AppRouter } from '@router/AppRouter';
import { SignalRService } from '@api/SignalRService';
import { useEffect } from 'react';

export default function App() {
  useEffect(() => {
    SignalRService.startConnection();
    return () => {
      SignalRService.stopConnection();
    };
  }, []);

  return <AppRouter />;
}
