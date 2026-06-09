import React, { useState, useEffect } from 'react';
import { useAd } from './AdContext';

interface AdInterstitialProps {
  isOpen: boolean;
  onClose: () => void;
  countdownSeconds?: number;
}

export const AdInterstitial: React.FC<AdInterstitialProps> = ({ isOpen, onClose, countdownSeconds = 5 }) => {
  const { isPremium } = useAd();
  const [secondsLeft, setSecondsLeft] = useState(countdownSeconds);

  useEffect(() => {
    if (!isOpen || isPremium) return;
    
    setSecondsLeft(countdownSeconds);
    const interval = setInterval(() => {
      setSecondsLeft((prev) => {
        if (prev <= 1) {
          clearInterval(interval);
          return 0;
        }
        return prev - 1;
      });
    }, 1000);

    return () => clearInterval(interval);
  }, [isOpen, isPremium, countdownSeconds]);

  if (!isOpen || isPremium) return null;

  return (
    <div className="fixed inset-0 z-[120] flex items-center justify-center p-4">
      {/* Blurred Backdrop */}
      <div 
        className="absolute inset-0 bg-slate-950/80 backdrop-blur-md animate-fade-in" 
        onClick={() => {
          if (secondsLeft === 0) onClose();
        }}
      />
      
      {/* Modal Container */}
      <div className="relative w-full max-w-md bg-gradient-to-b from-slate-900 to-slate-950 border border-slate-700/50 rounded-2xl shadow-2xl p-6 md:p-8 text-center flex flex-col justify-between items-center overflow-hidden animate-in zoom-in-95 duration-200">
        
        {/* Decorative elements */}
        <div className="absolute -top-10 -right-10 w-32 h-32 bg-yellow-400/5 rounded-full blur-2xl pointer-events-none" />
        <div className="absolute -bottom-10 -left-10 w-32 h-32 bg-blue-500/5 rounded-full blur-2xl pointer-events-none" />

        {/* Sponsor Tag */}
        <div className="mb-4">
          <span className="px-2.5 py-1 rounded-full bg-slate-800 border border-slate-700 text-[10px] font-bold text-slate-400 uppercase tracking-widest">
            Sponsored Ad
          </span>
        </div>

        {/* Title */}
        <div className="mb-6">
          <h2 className="text-2xl font-black text-white flex items-center justify-center gap-2 tracking-tight">
            <span className="text-yellow-400"><i className="fas fa-crown"></i></span>
            SQUAD BUILDER PRO
          </h2>
          <p className="text-slate-400 text-sm mt-3 leading-relaxed">
            Take your fantasy draft game to the next level. Unlock historical draft logs, unlimited formations, and head-to-head match analytics.
          </p>
        </div>

        {/* Features list */}
        <div className="bg-slate-800/40 rounded-xl p-4 border border-slate-800/60 w-full mb-6 text-left space-y-3">
          <div className="flex items-center gap-3 text-xs text-slate-300">
            <div className="w-5 h-5 rounded-full bg-emerald-500/10 border border-emerald-500/20 flex items-center justify-center text-emerald-400 shrink-0">
              <i className="fas fa-check text-[9px]"></i>
            </div>
            <span>Complete ad-free drafting experience</span>
          </div>
          <div className="flex items-center gap-3 text-xs text-slate-300">
            <div className="w-5 h-5 rounded-full bg-emerald-500/10 border border-emerald-500/20 flex items-center justify-center text-emerald-400 shrink-0">
              <i className="fas fa-check text-[9px]"></i>
            </div>
            <span>Deep ratings, synergies, and traits overlay</span>
          </div>
          <div className="flex items-center gap-3 text-xs text-slate-300">
            <div className="w-5 h-5 rounded-full bg-emerald-500/10 border border-emerald-500/20 flex items-center justify-center text-emerald-400 shrink-0">
              <i className="fas fa-check text-[9px]"></i>
            </div>
            <span>Weekly high-roller ranks & token matches</span>
          </div>
        </div>

        {/* Action Buttons */}
        <div className="w-full space-y-3">
          <button 
            onClick={() => {
              alert('Mock subscription workflow initiated!');
              onClose();
            }}
            className="w-full py-3 bg-gradient-to-r from-yellow-400 to-amber-500 text-slate-950 rounded-xl font-bold hover:scale-[1.02] active:scale-[0.98] hover:shadow-lg hover:shadow-yellow-400/10 transition-all"
          >
            Start 7-Day Free Trial
          </button>
          
          <button
            disabled={secondsLeft > 0}
            onClick={onClose}
            className={`w-full py-2.5 rounded-xl text-xs font-bold transition-all border ${
              secondsLeft > 0 
                ? 'border-slate-800/80 bg-slate-900/40 text-slate-600 cursor-not-allowed'
                : 'border-slate-700 text-slate-400 hover:text-white hover:bg-slate-800/50 hover:border-slate-600'
            }`}
          >
            {secondsLeft > 0 ? (
              <span className="flex items-center justify-center gap-2">
                <i className="fas fa-spinner fa-spin text-[10px]"></i>
                Skip ad in {secondsLeft}s...
              </span>
            ) : (
              'Close Ad'
            )}
          </button>
        </div>
      </div>
    </div>
  );
};
export default AdInterstitial;
