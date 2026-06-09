import React, { useEffect, useState } from 'react';
import { useAd } from './AdContext';
import { AdErrorBoundary } from './AdErrorBoundary';

type AdSlotType = 'horizontal' | 'sidebar';

interface AdBannerProps {
  slotType: AdSlotType;
  adUnitId?: string;
}

interface MockAdCreative {
  title: string;
  subtitle: string;
  actionText: string;
  badge: string;
  icon: string;
  gradient: string;
  textColor: string;
  buttonBg: string;
  buttonText: string;
}

const HORIZONTAL_MOCKS: MockAdCreative[] = [
  {
    title: "APEX STRIKER ELITE",
    subtitle: "Precision engineering for the modern footballer. Get 20% off all boots.",
    actionText: "Shop Gear",
    badge: "Sponsor",
    icon: "fa-solid fa-shoe-prints",
    gradient: "from-blue-900 via-indigo-950 to-blue-900",
    textColor: "text-blue-200",
    buttonBg: "bg-indigo-500 hover:bg-indigo-600 text-white",
    buttonText: "text-white"
  },
  {
    title: "MATCHDAY MANAGER 2026",
    subtitle: "Build your club, train your squad, and dominate the global rankings.",
    actionText: "Play Free",
    badge: "Play Now",
    icon: "fa-solid fa-gamepad",
    gradient: "from-emerald-950 via-teal-900 to-slate-900",
    textColor: "text-emerald-300",
    buttonBg: "bg-emerald-400 hover:bg-emerald-500 text-slate-950",
    buttonText: "text-slate-950"
  }
];

const SIDEBAR_MOCKS: MockAdCreative[] = [
  {
    title: "APEX STRIKER ELITE",
    subtitle: "Engineered for maximum grip, extreme touch, and explosive speed on the pitch.",
    actionText: "Shop Boots",
    badge: "Featured Partner",
    icon: "fa-solid fa-shoe-prints",
    gradient: "from-slate-900 via-blue-950 to-slate-900",
    textColor: "text-blue-300",
    buttonBg: "bg-blue-500 hover:bg-blue-600 text-white",
    buttonText: "text-white"
  },
  {
    title: "FANZONE COLLECTIBLES",
    subtitle: "Buy, sell, and trade officially licensed digital player cards of international legends.",
    actionText: "Collect Now",
    badge: "NFT Partner",
    icon: "fa-solid fa-server",
    gradient: "from-slate-900 via-purple-950 to-slate-900",
    textColor: "text-purple-300",
    buttonBg: "bg-purple-500 hover:bg-purple-600 text-white",
    buttonText: "text-white"
  }
];

const AdBannerInner: React.FC<AdBannerProps> = ({ slotType, adUnitId }) => {
  const { isPremium, adBlockerDetected, marketingConsent } = useAd();
  const [adScriptFailed, setAdScriptFailed] = useState(false);
  const [selectedMock, setSelectedMock] = useState<MockAdCreative | null>(null);

  // Set up layout boundaries to prevent Cumulative Layout Shift (CLS)
  const containerClasses = slotType === 'horizontal'
    ? 'w-full max-w-[728px] h-[60px] md:h-[90px] mx-auto relative flex items-center justify-center overflow-hidden rounded-xl border border-slate-700 bg-slate-800/40 shadow-xl transition-all duration-300'
    : 'w-[300px] h-[250px] mx-auto relative flex items-center justify-center overflow-hidden rounded-xl border border-slate-700 bg-slate-800/40 shadow-xl transition-all duration-300';

  useEffect(() => {
    // Pick a random mock ad on mount
    const mocks = slotType === 'horizontal' ? HORIZONTAL_MOCKS : SIDEBAR_MOCKS;
    const randomIndex = Math.floor(Math.random() * mocks.length);
    setSelectedMock(mocks[randomIndex]);
  }, [slotType]);

  useEffect(() => {
    if (isPremium || adBlockerDetected || !marketingConsent) {
      return;
    }

    // Attempt to load adsbygoogle
    try {
      const windowWithAds = window as any;
      if (windowWithAds.adsbygoogle) {
        (windowWithAds.adsbygoogle = windowWithAds.adsbygoogle || []).push({});
      } else {
        // If script isn't loaded after 1.5 seconds, we trigger the script failure state
        const timeout = setTimeout(() => {
          if (!windowWithAds.adsbygoogle) {
            setAdScriptFailed(true);
          }
        }, 1500);
        return () => clearTimeout(timeout);
      }
    } catch (err) {
      console.error('Failed to load active network ad unit:', err);
      setAdScriptFailed(true);
    }
  }, [isPremium, adBlockerDetected, marketingConsent]);

  // Hide ads completely for premium users
  if (isPremium) return null;

  // Decide what banner content to show
  // If an ad blocker is detected or marketing consent is disabled, show the Premium upgrade fallback banner.
  // If no ad blocker is active but the ad script fails (or we are developing locally), show a beautiful sponsor mock.
  const showPremiumFallback = adBlockerDetected || !marketingConsent;
  const showSponsorMock = !showPremiumFallback && (adScriptFailed || !(window as any).adsbygoogle);

  if (showPremiumFallback) {
    // Premium upgrade promo (aesthetic dark gradient matching Ultimate 11 colors)
    return (
      <div className={containerClasses}>
        <div className="absolute inset-0 bg-gradient-to-r from-slate-900 via-blue-950 to-slate-900 flex items-center justify-between p-4 md:p-6 text-left w-full h-full select-none">
          <div className="flex items-center gap-3">
            <div className="w-10 h-10 rounded-lg bg-yellow-400/20 border border-yellow-400/30 flex items-center justify-center text-yellow-400 shrink-0">
              <i className="fas fa-crown text-lg"></i>
            </div>
            <div>
              <h4 className="text-white text-sm md:text-base font-bold tracking-tight">Go Ad-Free with Ultimate 11 Premium</h4>
              <p className="text-slate-400 text-xs mt-0.5 max-w-[200px] md:max-w-md truncate md:normal-case">
                Remove ads, view draft archives, and see advanced metrics on the leaderboards.
              </p>
            </div>
          </div>
          <button 
            onClick={() => alert('Mock Premium Checkout initialized!')}
            className="px-3 py-2 bg-gradient-to-r from-yellow-400 to-amber-500 text-slate-950 rounded-lg text-xs font-bold hover:scale-105 active:scale-95 transition-all shadow-lg shadow-yellow-400/10 shrink-0"
          >
            Go Premium
          </button>
        </div>
      </div>
    );
  }

  if (showSponsorMock && selectedMock) {
    // Beautiful mock sponsor ad (to make layout feel active and populated)
    return (
      <div className={containerClasses}>
        <div className={`absolute inset-0 bg-gradient-to-br ${selectedMock.gradient} flex p-4 text-left w-full h-full select-none ${slotType === 'horizontal' ? 'flex-row items-center justify-between gap-4' : 'flex-col justify-between'}`}>
          
          <div className={`flex gap-3 ${slotType === 'horizontal' ? 'items-center' : 'items-start'}`}>
            <div className="w-10 h-10 rounded-lg bg-white/10 flex items-center justify-center text-white shrink-0">
              <i className={`${selectedMock.icon} text-lg`}></i>
            </div>
            <div>
              <div className="flex items-center gap-2">
                <span className="px-1.5 py-0.5 rounded bg-white/20 text-[9px] font-bold text-white uppercase tracking-wider">
                  {selectedMock.badge}
                </span>
                <h4 className="text-white text-xs md:text-sm font-extrabold tracking-tight">
                  {selectedMock.title}
                </h4>
              </div>
              <p className={`text-xs mt-1 ${selectedMock.textColor} ${slotType === 'horizontal' ? 'max-w-md line-clamp-1' : 'line-clamp-3 leading-relaxed'}`}>
                {selectedMock.subtitle}
              </p>
            </div>
          </div>

          <button 
            onClick={() => window.open('https://example.com/mock-sponsor', '_blank')}
            className={`px-3 py-2 rounded-lg text-xs font-bold hover:scale-105 active:scale-95 transition-all shadow-md shrink-0 ${selectedMock.buttonBg} ${slotType === 'horizontal' ? '' : 'w-full'}`}
          >
            {selectedMock.actionText} <i className="fas fa-arrow-right ml-1.5 text-[10px]"></i>
          </button>
        </div>
      </div>
    );
  }

  // Fallback to the real Google AdSense container if script loaded successfully
  return (
    <div className={containerClasses}>
      <ins
        className="adsbygoogle"
        style={{ display: 'block', width: '100%', height: '100%' }}
        data-ad-client="ca-pub-mock-client-id"
        data-ad-slot={adUnitId || "mock-slot-id"}
        data-ad-format={slotType === 'horizontal' ? 'horizontal' : 'rectangle'}
        data-full-width-responsive="true"
      />
    </div>
  );
};

export const AdBanner: React.FC<AdBannerProps> = (props) => (
  <AdErrorBoundary fallback={<div className="h-0 overflow-hidden" />}>
    <AdBannerInner {...props} />
  </AdErrorBoundary>
);
