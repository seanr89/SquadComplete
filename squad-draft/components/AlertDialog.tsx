import React from 'react';

interface AlertDialogProps {
  isOpen: boolean;
  title: string;
  message: string;
  type: 'success' | 'error' | 'info';
  onClose: () => void;
}

const AlertDialog: React.FC<AlertDialogProps> = ({ isOpen, title, message, type, onClose }) => {
  if (!isOpen) return null;

  const iconClass = {
    success: 'fa-check-circle text-green-400',
    error: 'fa-exclamation-circle text-red-500',
    info: 'fa-info-circle text-blue-400',
  }[type];

  return (
    <div className="fixed inset-0 z-[110] flex items-center justify-center p-4">
      {/* Backdrop */}
      <div
        className="absolute inset-0 bg-slate-900/80 backdrop-blur-sm"
        onClick={onClose}
      />

      {/* Dialog */}
      <div className="relative w-full max-w-sm bg-slate-800 rounded-2xl border border-slate-700 shadow-2xl flex flex-col overflow-hidden animate-in zoom-in-95 duration-200">
        
        {/* Content Area */}
        <div className="p-6 text-center space-y-4">
           <div className="flex justify-center mb-2">
               <div className={`w-16 h-16 rounded-full flex flex-col items-center justify-center ${type === 'success' ? 'bg-green-500/10' : type === 'error' ? 'bg-red-500/10' : 'bg-blue-500/10'}`}>
                 <i className={`fas ${iconClass} text-4xl`}></i>
               </div>
           </div>
           <h2 className="text-2xl font-bold text-white tracking-tight">{title}</h2>
           <p className="text-slate-300">
             {message}
           </p>
        </div>

        {/* Footer */}
        <div className="p-4 bg-slate-800/80 border-t border-slate-700 flex justify-center">
            <button
                onClick={onClose}
                className="w-full bg-yellow-400 hover:bg-yellow-500 text-slate-900 font-bold py-3 px-4 rounded-xl transition-all shadow-lg hover:shadow-xl hover:scale-[1.02]"
            >
                Continue
            </button>
        </div>
      </div>
    </div>
  );
};

export default AlertDialog;
