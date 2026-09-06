import React, { useState, useEffect } from 'react';
import { submitFeedback, fetchStatistics } from '../api';

interface AboutDialogProps {
  isOpen: boolean;
  onClose: () => void;
}

const AboutDialog: React.FC<AboutDialogProps> = ({ isOpen, onClose }) => {
  const [activeTab, setActiveTab] = useState<'about' | 'contact' | 'stats'>('about');
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [submitStatus, setSubmitStatus] = useState<'idle' | 'success' | 'error'>('idle');
  const [stats, setStats] = useState<any | null>(null);
  const [loadingStats, setLoadingStats] = useState(false);

  useEffect(() => {
    if (activeTab === 'stats' && !stats && !loadingStats) {
      setLoadingStats(true);
      fetchStatistics().then(data => {
        setStats(data);
        setLoadingStats(false);
      });
    }
  }, [activeTab, stats, loadingStats]);

  useEffect(() => {
    if (!isOpen) return;

    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === 'Escape') {
        e.preventDefault();
        onClose();
      }
    };

    window.addEventListener('keydown', handleKeyDown);
    return () => window.removeEventListener('keydown', handleKeyDown);
  }, [isOpen, onClose]);

  if (!isOpen) return null;

  const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();

    const formData = new FormData(e.currentTarget);
    const name = formData.get('name') as string;
    const email = formData.get('email') as string;
    const message = formData.get('message') as string;

    setIsSubmitting(true);
    setSubmitStatus('idle');

    const success = await submitFeedback(name, email, message);

    setIsSubmitting(false);

    if (success) {
      setSubmitStatus('success');
      setTimeout(() => {
        onClose();
        setSubmitStatus('idle');
      }, 2000);
    } else {
      setSubmitStatus('error');
    }
  };

  return (
    <div className="fixed inset-0 z-[100] flex items-center justify-center p-4">
      {/* Backdrop */}
      <div
        className="absolute inset-0 bg-slate-900/80 backdrop-blur-sm"
        onClick={onClose}
        aria-hidden="true"
      />

      {/* Dialog */}
      <div
        role="dialog"
        aria-modal="true"
        aria-labelledby="about-dialog-title"
        className="relative w-full max-w-lg bg-slate-800 rounded-2xl border border-slate-700 shadow-2xl flex flex-col overflow-hidden max-h-[85vh]"
      >

        {/* Header */}
        <div className="flex items-center justify-between p-4 md:p-6 border-b border-slate-700">
          <h2 id="about-dialog-title" className="text-xl font-bold text-white flex items-center gap-2">
            <i className="fas fa-info-circle text-yellow-400" aria-hidden="true"></i>
            About Ultimate 11
          </h2>
          <button
            type="button"
            onClick={onClose}
            aria-label="Close about dialog"
            className="text-slate-400 hover:text-white transition-colors w-8 h-8 flex items-center justify-center rounded-lg hover:bg-slate-700 focus-visible:ring-2 focus-visible:ring-yellow-400 focus-visible:outline-none"
          >
            <i className="fas fa-times" aria-hidden="true"></i>
          </button>
        </div>

        {/* Tabs */}
        <div role="tablist" aria-label="About dialog tabs" className="flex border-b border-slate-700 bg-slate-800/50">
          <button
            id="tab-about"
            role="tab"
            type="button"
            aria-selected={activeTab === 'about'}
            aria-controls="tabpanel-about"
            tabIndex={activeTab === 'about' ? 0 : -1}
            onClick={() => setActiveTab('about')}
            className={`flex-1 py-3 text-sm font-bold transition-colors focus-visible:ring-2 focus-visible:ring-yellow-400 focus-visible:outline-none ${activeTab === 'about' ? 'text-yellow-400 border-b-2 border-yellow-400' : 'text-slate-400 hover:text-slate-300'}`}
          >
            About Us
          </button>

          <button
            id="tab-stats"
            role="tab"
            type="button"
            aria-selected={activeTab === 'stats'}
            aria-controls="tabpanel-stats"
            tabIndex={activeTab === 'stats' ? 0 : -1}
            onClick={() => setActiveTab('stats')}
            className={`flex-1 py-3 text-sm font-bold transition-colors focus-visible:ring-2 focus-visible:ring-yellow-400 focus-visible:outline-none ${activeTab === 'stats' ? 'text-yellow-400 border-b-2 border-yellow-400' : 'text-slate-400 hover:text-slate-300'}`}
          >
            Stats
          </button>

          <button
            id="tab-contact"
            role="tab"
            type="button"
            aria-selected={activeTab === 'contact'}
            aria-controls="tabpanel-contact"
            tabIndex={activeTab === 'contact' ? 0 : -1}
            onClick={() => setActiveTab('contact')}
            className={`flex-1 py-3 text-sm font-bold transition-colors focus-visible:ring-2 focus-visible:ring-yellow-400 focus-visible:outline-none ${activeTab === 'contact' ? 'text-yellow-400 border-b-2 border-yellow-400' : 'text-slate-400 hover:text-slate-300'}`}
          >
            Feedback
          </button>
        </div>

        {/* Content Area */}
        <div className="p-4 md:p-6 overflow-y-auto custom-scrollbar">

          {activeTab === 'about' && (
            <div id="tabpanel-about" role="tabpanel" aria-labelledby="tab-about" tabIndex={0} className="space-y-4 text-slate-300 leading-relaxed focus:outline-none">
              <p>
                <strong>Ultimate 11: Squad Draft</strong> is a daily challenge game where you build your dream football team from a curated list of top-tier players.
              </p>
              <p>
                Every day, a new squad challenge is presented. Your goal is to strategically select one player from each daily squad and place them into your formation.
              </p>
              <div className="bg-slate-700/50 rounded-lg p-4 mt-4 border border-slate-600">
                <h3 className="font-bold text-white mb-2 text-sm uppercase tracking-wider">How to play</h3>
                <ul className="list-disc pl-5 space-y-1 text-sm">
                  <li>Review the current daily squad options.</li>
                  <li>Pick one player per step to add to your team.</li>
                  <li>Assign them to their natural positions for better synergy.</li>
                  <li>Build a complete 11-man squad with the highest average rating!</li>
                </ul>
              </div>
            </div>
          )}

          {activeTab === 'stats' && (
            <div id="tabpanel-stats" role="tabpanel" aria-labelledby="tab-stats" tabIndex={0} className="space-y-4 focus:outline-none">
              <p className="text-sm text-slate-300 mb-4">
                Current platform statistics from our database.
              </p>
              {loadingStats ? (
                <div className="flex justify-center p-8">
                  <i className="fas fa-spinner fa-spin text-2xl text-yellow-400" aria-label="Loading statistics"></i>
                </div>
              ) : stats ? (
                <div className="grid grid-cols-2 gap-4">
                  <div className="bg-slate-700/50 rounded-lg p-4 border border-slate-600 text-center">
                    <div className="text-2xl font-black text-white">{stats.leagues || stats.leaguesCount || stats.leagueCount || 0}</div>
                    <div className="text-xs text-slate-400 font-bold uppercase tracking-wider">Leagues</div>
                  </div>
                  <div className="bg-slate-700/50 rounded-lg p-4 border border-slate-600 text-center">
                    <div className="text-2xl font-black text-white">{stats.teams || stats.teamsCount || stats.teamCount || 0}</div>
                    <div className="text-xs text-slate-400 font-bold uppercase tracking-wider">Teams</div>
                  </div>
                  <div className="bg-slate-700/50 rounded-lg p-4 border border-slate-600 text-center">
                    <div className="text-2xl font-black text-white">{stats.players || stats.playersCount || stats.playerCount || 0}</div>
                    <div className="text-xs text-slate-400 font-bold uppercase tracking-wider">Players</div>
                  </div>
                  <div className="bg-slate-700/50 rounded-lg p-4 border border-slate-600 text-center">
                    <div className="text-2xl font-black text-white">{stats.fixtures || stats.fixturesCount || stats.fixtureCount || 0}</div>
                    <div className="text-xs text-slate-400 font-bold uppercase tracking-wider">Fixtures</div>
                  </div>
                  <div className="bg-slate-700/50 rounded-lg p-4 border border-slate-600 text-center col-span-2">
                    <div className="text-2xl font-black text-yellow-400">{stats.games || stats.gamesCount || stats.gameRecordsCount || stats.gameRecordCount || 0}</div>
                    <div className="text-xs text-slate-400 font-bold uppercase tracking-wider">Games Played</div>
                  </div>
                </div>
              ) : (
                <div className="text-center text-slate-400 p-4">
                  Failed to load statistics.
                </div>
              )}
            </div>
          )}

          {activeTab === 'contact' && (
            <div id="tabpanel-contact" role="tabpanel" aria-labelledby="tab-contact" tabIndex={0} className="focus:outline-none">
              <form onSubmit={handleSubmit} className="space-y-4">
                <p className="text-sm text-slate-300 mb-4">
                  Have an idea for a feature or found a bug? Let us know! Your feedback will help shape the future of Ultimate 11.
                </p>

                {submitStatus === 'success' && (
                  <div role="status" className="bg-green-500/10 border border-green-500/20 text-green-400 rounded-lg p-3 text-sm font-bold text-center">
                    Feedback sent successfully!
                  </div>
                )}

                {submitStatus === 'error' && (
                  <div role="alert" className="bg-red-500/10 border border-red-500/20 text-red-400 rounded-lg p-3 text-sm font-bold text-center">
                    Failed to send feedback. Please try again.
                  </div>
                )}

                <div>
                  <label htmlFor="name" className="block text-xs font-bold text-slate-400 uppercase tracking-wider mb-1">Name</label>
                  <input
                    type="text"
                    id="name"
                    name="name"
                    required
                    aria-required="true"
                    className="w-full bg-slate-900 border border-slate-700 rounded-lg px-4 py-2 text-white focus:outline-none focus:border-yellow-400 focus:ring-2 focus:ring-yellow-400 transition-colors"
                    placeholder="Your Name"
                  />
                </div>

                <div>
                  <label htmlFor="email" className="block text-xs font-bold text-slate-400 uppercase tracking-wider mb-1">Email</label>
                  <input
                    type="email"
                    id="email"
                    name="email"
                    required
                    aria-required="true"
                    className="w-full bg-slate-900 border border-slate-700 rounded-lg px-4 py-2 text-white focus:outline-none focus:border-yellow-400 focus:ring-2 focus:ring-yellow-400 transition-colors"
                    placeholder="your@email.com"
                  />
                </div>

                <div>
                  <label htmlFor="message" className="block text-xs font-bold text-slate-400 uppercase tracking-wider mb-1">Message</label>
                  <textarea
                    id="message"
                    name="message"
                    required
                    aria-required="true"
                    rows={4}
                    className="w-full bg-slate-900 border border-slate-700 rounded-lg px-4 py-2 text-white focus:outline-none focus:border-yellow-400 focus:ring-2 focus:ring-yellow-400 transition-colors resize-none"
                    placeholder="What's on your mind?"
                  ></textarea>
                </div>

                <button
                  type="submit"
                  disabled={isSubmitting || submitStatus === 'success'}
                  className="w-full bg-yellow-400 hover:bg-yellow-500 disabled:opacity-50 disabled:cursor-not-allowed text-slate-900 font-bold py-3 px-4 rounded-xl transition-colors shadow-lg shadow-yellow-400/20 focus-visible:ring-2 focus-visible:ring-yellow-400 focus-visible:outline-none"
                >
                  <i className={`fas ${isSubmitting ? 'fa-spinner fa-spin' : 'fa-paper-plane'} mr-2`} aria-hidden="true"></i>
                  {isSubmitting ? 'Sending...' : 'Send Feedback'}
                </button>
              </form>
            </div>
          )}

        </div>
      </div>
    </div>
  );
};

export default AboutDialog;
