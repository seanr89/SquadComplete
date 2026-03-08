import React, { useState } from 'react';

interface AboutDialogProps {
  isOpen: boolean;
  onClose: () => void;
}

const AboutDialog: React.FC<AboutDialogProps> = ({ isOpen, onClose }) => {
  const [activeTab, setActiveTab] = useState<'about' | 'changelog' | 'contact'>('about');

  if (!isOpen) return null;

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    alert("Thanks for your feedback! This will be sent on at a later date.");
    onClose();
  };

  return (
    <div className="fixed inset-0 z-[100] flex items-center justify-center p-4">
      {/* Backdrop */}
      <div
        className="absolute inset-0 bg-slate-900/80 backdrop-blur-sm"
        onClick={onClose}
      />

      {/* Dialog */}
      <div className="relative w-full max-w-lg bg-slate-800 rounded-2xl border border-slate-700 shadow-2xl flex flex-col overflow-hidden max-h-[85vh]">

        {/* Header */}
        <div className="flex items-center justify-between p-4 md:p-6 border-b border-slate-700">
          <h2 className="text-xl font-bold text-white flex items-center gap-2">
            <i className="fas fa-info-circle text-yellow-400"></i>
            About Ultimate 11
          </h2>
          <button
            onClick={onClose}
            className="text-slate-400 hover:text-white transition-colors w-8 h-8 flex items-center justify-center rounded-lg hover:bg-slate-700"
          >
            <i className="fas fa-times"></i>
          </button>
        </div>

        {/* Tabs */}
        <div className="flex border-b border-slate-700 bg-slate-800/50">
          <button
            onClick={() => setActiveTab('about')}
            className={`flex-1 py-3 text-sm font-bold transition-colors ${activeTab === 'about' ? 'text-yellow-400 border-b-2 border-yellow-400' : 'text-slate-400 hover:text-slate-300'}`}
          >
            About Us
          </button>
          <button
            onClick={() => setActiveTab('changelog')}
            className={`flex-1 py-3 text-sm font-bold transition-colors ${activeTab === 'changelog' ? 'text-yellow-400 border-b-2 border-yellow-400' : 'text-slate-400 hover:text-slate-300'}`}
          >
            Changelog
          </button>
          <button
            onClick={() => setActiveTab('contact')}
            className={`flex-1 py-3 text-sm font-bold transition-colors ${activeTab === 'contact' ? 'text-yellow-400 border-b-2 border-yellow-400' : 'text-slate-400 hover:text-slate-300'}`}
          >
            Feedback
          </button>
        </div>

        {/* Content Area */}
        <div className="p-4 md:p-6 overflow-y-auto custom-scrollbar">

          {activeTab === 'about' && (
            <div className="space-y-4 text-slate-300 leading-relaxed">
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

          {activeTab === 'changelog' && (
            <div className="space-y-6">
              <div className="relative pl-6 border-l-2 border-slate-700">
                <div className="absolute w-3 h-3 bg-yellow-400 rounded-full -left-[7px] top-1"></div>
                <h3 className="text-white font-bold">v1.0.0 - Launch</h3>
                <p className="text-slate-400 text-sm mb-2">Initial Release</p>
                <ul className="list-disc pl-4 text-sm text-slate-300 space-y-1">
                  <li>Daily squad drafting mechanics</li>
                  <li>Interactive pitch visualization</li>
                  <li>Local storage saving for drafts</li>
                  <li>Team rating calculations</li>
                </ul>
              </div>
              <div className="relative pl-6 border-l-2 border-slate-700">
                <div className="absolute w-3 h-3 bg-slate-600 rounded-full -left-[7px] top-1"></div>
                <h3 className="text-slate-300 font-bold">v0.9.0 - Beta</h3>
                <p className="text-slate-500 text-sm mb-2">Early Testing</p>
                <ul className="list-disc pl-4 text-sm text-slate-400 space-y-1">
                  <li>Core drafting loop established</li>
                  <li>Initial player database integration</li>
                </ul>
              </div>
            </div>
          )}

          {activeTab === 'contact' && (
            <form onSubmit={handleSubmit} className="space-y-4">
              <p className="text-sm text-slate-300 mb-4">
                Have an idea for a feature or found a bug? Let us know! Your feedback will help shape the future of Ultimate 11.
              </p>

              <div>
                <label htmlFor="name" className="block text-xs font-bold text-slate-400 uppercase tracking-wider mb-1">Name</label>
                <input
                  type="text"
                  id="name"
                  required
                  className="w-full bg-slate-900 border border-slate-700 rounded-lg px-4 py-2 text-white focus:outline-none focus:border-yellow-400 focus:ring-1 focus:ring-yellow-400 transition-colors"
                  placeholder="Your Name"
                />
              </div>

              <div>
                <label htmlFor="email" className="block text-xs font-bold text-slate-400 uppercase tracking-wider mb-1">Email</label>
                <input
                  type="email"
                  id="email"
                  required
                  className="w-full bg-slate-900 border border-slate-700 rounded-lg px-4 py-2 text-white focus:outline-none focus:border-yellow-400 focus:ring-1 focus:ring-yellow-400 transition-colors"
                  placeholder="your@email.com"
                />
              </div>

              <div>
                <label htmlFor="message" className="block text-xs font-bold text-slate-400 uppercase tracking-wider mb-1">Message</label>
                <textarea
                  id="message"
                  required
                  rows={4}
                  className="w-full bg-slate-900 border border-slate-700 rounded-lg px-4 py-2 text-white focus:outline-none focus:border-yellow-400 focus:ring-1 focus:ring-yellow-400 transition-colors resize-none"
                  placeholder="What's on your mind?"
                ></textarea>
              </div>

              <button
                type="submit"
                className="w-full bg-yellow-400 hover:bg-yellow-500 text-slate-900 font-bold py-3 px-4 rounded-xl transition-colors shadow-lg shadow-yellow-400/20"
              >
                <i className="fas fa-paper-plane mr-2"></i> Send Feedback
              </button>
            </form>
          )}

        </div>
      </div>
    </div>
  );
};

export default AboutDialog;
