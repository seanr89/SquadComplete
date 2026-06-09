import React, { createContext, useContext, useState, useEffect } from 'react';
import { checkConsent } from './CookieConsent';

interface AdContextType {
  isPremium: boolean;
  setIsPremium: (premium: boolean) => void;
  adBlockerDetected: boolean;
  marketingConsent: boolean;
  refreshConsent: () => void;
}

const AdContext = createContext<AdContextType | undefined>(undefined);

export const AdProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [isPremium, setIsPremium] = useState(() => localStorage.getItem('squad-premium') === 'true');
  const [adBlockerDetected, setAdBlockerDetected] = useState(false);
  const [marketingConsent, setMarketingConsent] = useState(() => checkConsent('marketing'));

  const refreshConsent = () => {
    setMarketingConsent(checkConsent('marketing'));
  };

  useEffect(() => {
    localStorage.setItem('squad-premium', String(isPremium));
  }, [isPremium]);

  useEffect(() => {
    const detectAdBlocker = async () => {
      // Method 1: Fetch check for ads script
      try {
        const url = 'https://pagead2.googlesyndication.com/pagead/js/adsbygoogle.js';
        const response = await fetch(new Request(url), { method: 'HEAD', mode: 'no-cors', cache: 'no-store' });
        // Standard ad blocker will block the connection completely or return a network error
        if (!response.ok && response.status === 0) {
          setAdBlockerDetected(true);
          return;
        }
      } catch (e) {
        setAdBlockerDetected(true);
        return;
      }

      // Method 2: Decoy DOM element check
      const decoy = document.createElement('div');
      decoy.className = 'ad-banner adsense-ad banner-ads pub_300x250';
      decoy.setAttribute('style', 'position: absolute; left: -9999px; top: -9999px; width: 1px; height: 1px;');
      document.body.appendChild(decoy);
      
      // Allow browser styling to render
      setTimeout(() => {
        if (decoy.offsetHeight === 0 || window.getComputedStyle(decoy).display === 'none') {
          setAdBlockerDetected(true);
        }
        if (document.body.contains(decoy)) {
          document.body.removeChild(decoy);
        }
      }, 100);
    };

    detectAdBlocker();
  }, []);

  useEffect(() => {
    const handleConsentChange = () => {
      setMarketingConsent(checkConsent('marketing'));
    };
    window.addEventListener('squad-cookie-consent-changed', handleConsentChange);
    return () => {
      window.removeEventListener('squad-cookie-consent-changed', handleConsentChange);
    };
  }, []);

  return (
    <AdContext.Provider value={{ isPremium, setIsPremium, adBlockerDetected, marketingConsent, refreshConsent }}>
      {children}
    </AdContext.Provider>
  );
};

export const useAd = () => {
  const context = useContext(AdContext);
  if (!context) throw new Error('useAd must be used within an AdProvider');
  return context;
};
