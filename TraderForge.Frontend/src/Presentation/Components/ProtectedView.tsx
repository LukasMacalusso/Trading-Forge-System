import { Link } from 'react-router-dom';
import { Lock } from 'lucide-react';
import { useAuthStore } from '../../Application/Store/authStore';

interface ProtectedViewProps {
  children: React.ReactNode;
}

export function ProtectedView({ children }: ProtectedViewProps) {
  const { isAuthenticated } = useAuthStore();

  if (isAuthenticated) {
    return <>{children}</>;
  }

  return (
    <div className="relative w-full h-full min-h-[400px]">
      {/* Contenido borroso en el fondo */}
      <div className="absolute inset-0 blur-sm opacity-50 pointer-events-none select-none">
        {children}
      </div>
      
      {/* Mensaje superpuesto */}
      <div className="absolute inset-0 flex flex-col items-center justify-center p-6 z-10">
        <div className="bg-neutral-900 border border-neutral-800 rounded-xl p-8 flex flex-col items-center max-w-md text-center shadow-2xl">
          <div className="w-12 h-12 bg-neutral-800 rounded-full flex items-center justify-center mb-4">
            <Lock className="text-neutral-400" size={24} />
          </div>
          <h2 className="text-xl font-bold text-neutral-100 mb-2">
            Authentication Required
          </h2>
          <p className="text-neutral-400 mb-6 font-medium">
            ¡Para usar esta función debes iniciar sesión!
          </p>
          <div className="flex gap-4 w-full">
             <Link 
              to="/login" 
              className="flex-1 py-2 px-4 rounded-lg text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 transition-colors"
             >
               Log in
             </Link>
             <Link 
              to="/register" 
              className="flex-1 py-2 px-4 rounded-lg text-sm font-medium text-neutral-300 bg-neutral-800 hover:bg-neutral-700 border border-neutral-700 transition-colors"
             >
               Register
             </Link>
          </div>
        </div>
      </div>
    </div>
  );
}
