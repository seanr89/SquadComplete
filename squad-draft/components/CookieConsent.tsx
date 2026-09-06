import React, { useState, useEffect } from 'react';

export type ConsentCategory = 'necessary' | 'analytics' | 'marketing';

export interface Consent {
  necessary: boolean;
  analytics: boolean;
  marketing: boolean;
}

const STORAGE_KEY = 'squad-cookie-consent';

export const checkConsent = (category: ConsentCategory): boolean => {
  if (category === 'necessary') return true;
  try {
    const saved = localStorage.getItem(STORAGE_KEY);
    if (saved) {
      const consent: Consent = JSON.parse(saved);
      return !!consent[category];
    }
  } catch (e) {
    console.error('Failed to parse cookie consent', e);
  }
  return false;
};

const CookieConsent: React.FC = () => {
  const [isVisible, setIsVisible] = useState(false);
  const [showModal, setShowModal] = useState(false);
  const [consent, setConsent] = useState<Consent>({
    necessary: true,
    analytics: false,
    marketing: false,
  });

  useEffect(() => {
    const saved = localStorage.getItem(STORAGE_KEY);
    if (!saved) {
      setIsVisible(true);
    } else {
      try {
        setConsent(JSON.parse(saved));
      } catch (e) {
        setIsVisible(true);
      }
    }
  }, []);

  useEffect(() => {
    if (!showModal) return;

    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === 'Escape') {
        e.preventDefault();
        setShowModal(false);
      }
    };

    window.addEventListener('keydown', handleKeyDown);
    return () => window.removeEventListener('keydown', handleKeyDown);
  }, [showModal]);

  const saveConsent = (newConsent: Consent) => {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(newConsent));
    setConsent(newConsent);
    setIsVisible(false);
    setShowModal(false);
  };

  const handleAcceptAll = () => {
    saveConsent({ necessary: true, analytics: true, marketing: true });
  };

  const handleRejectAll = () => {
    saveConsent({ necessary: true, analytics: false, marketing: false });
  };

  const handleSaveSettings = () => {
    saveConsent(consent);
  };

  if (!isVisible && !showModal) return null;

  return (
    <>
      {isVisible && !showModal && (
        <div
          role="region"
          aria-label="Cookie consent banner"
          className="fixed bottom-0 left-0 right-0 z-[100] bg-slate-800 border-t border-slate-700 p-4 md:p-6 shadow-2xl animate-in slide-in-from-bottom duration-300"
        >
          <div className="max-w-5xl mx-auto flex flex-col md:flex-row items-center justify-between gap-4">
            <div className="flex-1">
              <h3 className="text-white font-bold mb-1 flex items-center gap-2">
                <i className="fas fa-cookie-bite text-yellow-400" aria-hidden="true"></i>
                Cookie Settings
              </h3>
              <p className="text-slate-300 text-sm">
                We use cookies to enhance your experience and analyze our traffic.
                Please choose your preferences below.
              </p>
            </div>
            <div className="flex flex-wrap items-center gap-2 w-full md:w-auto justify-center">
              <button
                type="button"
                onClick={() => setShowModal(true)}
                className="px-4 py-2 text-sm font-bold text-slate-300 hover:text-white transition-colors focus-visible:ring-2 focus-visible:ring-yellow-400 focus-visible:outline-none rounded-lg"
              >
                Settings
              </button>
              <button
                type="button"
                onClick={handleRejectAll}
                className="px-4 py-2 text-sm font-bold bg-slate-700 text-white rounded-lg hover:bg-slate-600 transition-colors focus-visible:ring-2 focus-visible:ring-yellow-400 focus-visible:outline-none"
              >
                Reject All
              </button>
              <button
                type="button"
                onClick={handleAcceptAll}
                className="px-4 py-2 text-sm font-bold bg-yellow-400 text-slate-900 rounded-lg hover:bg-yellow-500 transition-colors shadow-lg shadow-yellow-400/20 focus-visible:ring-2 focus-visible:ring-yellow-400 focus-visible:outline-none"
              >
                Accept All
              </button>
            </div>
          </div>
        </div>
      )}

      {showModal && (
        <div className="fixed inset-0 z-[110] flex items-center justify-center p-4">
          <div
            className="absolute inset-0 bg-slate-900/80 backdrop-blur-sm"
            onClick={() => setShowModal(false)}
            aria-hidden="true"
          />
          <div
            role="dialog"
            aria-modal="true"
            aria-labelledby="cookie-modal-title"
            className="relative w-full max-w-md bg-slate-800 rounded-2xl border border-slate-700 shadow-2xl flex flex-col overflow-hidden animate-in zoom-in-95 duration-200"
          >
            <div className="flex items-center justify-between p-4 md:p-6 border-b border-slate-700">
              <h2 id="cookie-modal-title" className="text-xl font-bold text-white flex items-center gap-2">
                <i className="fas fa-cog text-yellow-400" aria-hidden="true"></i>
                Cookie Preferences
              </h2>
              <button
                type="button"
                onClick={() => setShowModal(false)}
                aria-label="Close cookie preferences dialog"
                className="text-slate-400 hover:text-white transition-colors w-8 h-8 flex items-center justify-center rounded-lg hover:bg-slate-700 focus-visible:ring-2 focus-visible:ring-yellow-400 focus-visible:outline-none"
              >
                <i className="fas fa-times" aria-hidden="true"></i>
              </button>
            </div>

            <div className="p-4 md:p-6 space-y-6">
              {/* Necessary */}
              <div className="flex items-start justify-between gap-4">
                <div className="flex-1">
                  <h4 className="text-white font-bold text-sm uppercase tracking-wider mb-1">Strictly Necessary</h4>
                  <p className="text-slate-300 text-xs">Essential for the website to function properly. Cannot be disabled.</p>
                </div>
                <div
                  role="switch"
                  aria-checked={true}
                  aria-disabled={true}
                  aria-label="Strictly necessary cookies (always on)"
                  className="relative inline-flex h-6 w-11 flex-shrink-0 cursor-not-allowed rounded-full border-2 border-transparent bg-yellow-400 transition-colors duration-200"
                >
                  <span className="translate-x-5 pointer-events-none inline-block h-5 w-5 transform rounded-full bg-slate-900 shadow ring-0 transition duration-200" />
                </div>
              </div>

              {/* Analytics */}
              <div className="flex items-start justify-between gap-4">
                <div className="flex-1">
                  <h4 className="text-white font-bold text-sm uppercase tracking-wider mb-1">Analytics</h4>
                  <p className="text-slate-300 text-xs">Help us understand how visitors interact with the website.</p>
                </div>
                <button
                  type="button"
                  role="switch"
                  aria-checked={consent.analytics}
                  aria-label="Toggle Analytics cookies"
                  onClick={() => setConsent(prev => ({ ...prev, analytics: !prev.analytics }))}
                  className={`relative inline-flex h-6 w-11 flex-shrink-0 cursor-pointer rounded-full border-2 border-transparent transition-colors duration-200 focus:outline-none focus:ring-2 focus:ring-yellow-400 focus:ring-offset-2 focus:ring-offset-slate-800 ${consent.analytics ? 'bg-yellow-400' : 'bg-slate-600'}`}
                >
                  <span className={`${consent.analytics ? 'translate-x-5' : 'translate-x-0'} pointer-events-none inline-block h-5 w-5 transform rounded-full bg-slate-900 shadow ring-0 transition duration-200`} />
                </button>
              </div>

              {/* Marketing */}
              <div className="flex items-start justify-between gap-4">
                <div className="flex-1">
                  <h4 className="text-white font-bold text-sm uppercase tracking-wider mb-1">Marketing</h4>
                  <p className="text-slate-300 text-xs">Used to deliver more relevant advertisements and content.</p>
                </div>
                <button
                  type="button"
                  role="switch"
                  aria-checked={consent.marketing}
                  aria-label="Toggle Marketing cookies"
                  onClick={() => setConsent(prev => ({ ...prev, marketing: !prev.marketing }))}
                  className={`relative inline-flex h-6 w-11 flex-shrink-0 cursor-pointer rounded-full border-2 border-transparent transition-colors duration-200 focus:outline-none focus:ring-2 focus:ring-yellow-400 focus:ring-offset-2 focus:ring-offset-slate-800 ${consent.marketing ? 'bg-yellow-400' : 'bg-slate-600'}`}
                >
                  <span className={`${consent.marketing ? 'translate-x-5' : 'translate-x-0'} pointer-events-none inline-block h-5 w-5 transform rounded-full bg-slate-900 shadow ring-0 transition duration-200`} />
                </button>
              </div>
            </div>

            <div className="p-4 md:p-6 bg-slate-800/50 border-t border-slate-700 flex gap-3">
              <button
                type="button"
                onClick={handleRejectAll}
                className="flex-1 py-3 text-sm font-bold bg-slate-700 text-white rounded-xl hover:bg-slate-600 transition-colors focus-visible:ring-2 focus-visible:ring-yellow-400 focus-visible:outline-none"
              >
                Reject All
              </button>
              <button
                type="button"
                onClick={handleSaveSettings}
                className="flex-1 py-3 text-sm font-bold bg-yellow-400 text-slate-900 rounded-xl hover:bg-yellow-500 transition-colors shadow-lg shadow-yellow-400/20 focus-visible:ring-2 focus-visible:ring-yellow-400 focus-visible:outline-none"
              >
                Save Preferences
              </button>
            </div>
          </div>
        </div>
      )}
    </>
  );
};

export default CookieConsent;
