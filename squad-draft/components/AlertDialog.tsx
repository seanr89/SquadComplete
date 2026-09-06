import React from 'react';

interface AlertDialogProps {
  isOpen: boolean;
  title: string;
  message: string;
  type: 'success' | 'error' | 'info' | 'warning';
  onClose: () => void;
  confirmText?: string;
  cancelText?: string;
  onConfirm?: () => void;
}

const AlertDialog: React.FC<AlertDialogProps> = ({
  isOpen,
  title,
  message,
  type,
  onClose,
  confirmText,
  cancelText,
  onConfirm,
}) => {
  const primaryButtonRef = React.useRef<HTMLButtonElement>(null);

  React.useEffect(() => {
    if (!isOpen) return;

    // Focus primary button when dialog opens
    const timer = setTimeout(() => {
      primaryButtonRef.current?.focus();
    }, 50);

    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === 'Escape') {
        e.preventDefault();
        onClose();
      }
    };

    window.addEventListener('keydown', handleKeyDown);
    return () => {
      clearTimeout(timer);
      window.removeEventListener('keydown', handleKeyDown);
    };
  }, [isOpen, onClose]);

  if (!isOpen) return null;

  const iconClass = {
    success: 'fa-check-circle text-green-400',
    error: 'fa-exclamation-circle text-red-500',
    info: 'fa-info-circle text-blue-400',
    warning: 'fa-exclamation-triangle text-yellow-400',
  }[type];

  return (
    <div className="fixed inset-0 z-[110] flex items-center justify-center p-4">
      {/* Backdrop */}
      <div
        className="absolute inset-0 bg-slate-900/80 backdrop-blur-sm"
        onClick={onClose}
        aria-hidden="true"
      />

      {/* Dialog */}
      <div
        role="alertdialog"
        aria-modal="true"
        aria-labelledby="alert-dialog-title"
        aria-describedby="alert-dialog-message"
        className="relative w-full max-w-sm bg-slate-800 rounded-2xl border border-slate-700 shadow-2xl flex flex-col overflow-hidden animate-in zoom-in-95 duration-200"
      >
        {/* Content Area */}
        <div className="p-6 text-center space-y-4">
          <div className="flex justify-center mb-2">
            <div
              className={`w-16 h-16 rounded-full flex flex-col items-center justify-center ${
                type === 'success'
                  ? 'bg-green-500/10'
                  : type === 'error'
                  ? 'bg-red-500/10'
                  : type === 'warning'
                  ? 'bg-yellow-500/10'
                  : 'bg-blue-500/10'
              }`}
              aria-hidden="true"
            >
              <i className={`fas ${iconClass} text-4xl`}></i>
            </div>
          </div>
          <h2 id="alert-dialog-title" className="text-2xl font-bold text-white tracking-tight">
            {title}
          </h2>
          <p id="alert-dialog-message" className="text-slate-300 text-sm leading-relaxed">
            {message}
          </p>
        </div>

        {/* Footer Actions */}
        <div className="p-4 bg-slate-800/80 border-t border-slate-700 flex gap-3 justify-center">
          {cancelText && (
            <button
              type="button"
              onClick={onClose}
              className="flex-1 bg-slate-700 hover:bg-slate-600 text-white font-bold py-3 px-4 rounded-xl transition-all focus-visible:ring-2 focus-visible:ring-yellow-400 focus-visible:outline-none"
            >
              {cancelText}
            </button>
          )}
          <button
            ref={primaryButtonRef}
            type="button"
            onClick={() => {
              if (onConfirm) {
                onConfirm();
              } else {
                onClose();
              }
            }}
            className={`flex-1 font-bold py-3 px-4 rounded-xl transition-all shadow-lg hover:shadow-xl focus-visible:ring-2 focus-visible:ring-yellow-400 focus-visible:outline-none ${
              type === 'error' || type === 'warning'
                ? 'bg-red-500 hover:bg-red-600 text-white'
                : 'bg-yellow-400 hover:bg-yellow-500 text-slate-900'
            }`}
          >
            {confirmText || 'Continue'}
          </button>
        </div>
      </div>
    </div>
  );
};

export default AlertDialog;
