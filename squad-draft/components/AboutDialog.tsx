import React, { useState } from 'react';
import { submitFeedback } from '../api';

interface AboutDialogProps {
  isOpen: boolean;
  onClose: () => void;
}

const AboutDialog: React.FC<AboutDialogProps> = ({ isOpen, onClose }) => {
  const [activeTab, setActiveTab] = useState<'about' | 'contact'>('about');
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [submitStatus, setSubmitStatus] = useState<'idle' | 'success' | 'error'>('idle');

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



          {activeTab === 'contact' && (
            <form onSubmit={handleSubmit} className="space-y-4">
              <p className="text-sm text-slate-300 mb-4">
                Have an idea for a feature or found a bug? Let us know! Your feedback will help shape the future of Ultimate 11.
              </p>

              {submitStatus === 'success' && (
                <div className="bg-green-500/10 border border-green-500/20 text-green-400 rounded-lg p-3 text-sm font-bold text-center">
                  Feedback sent successfully!
                </div>
              )}

              {submitStatus === 'error' && (
                <div className="bg-red-500/10 border border-red-500/20 text-red-400 rounded-lg p-3 text-sm font-bold text-center">
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
                  className="w-full bg-slate-900 border border-slate-700 rounded-lg px-4 py-2 text-white focus:outline-none focus:border-yellow-400 focus:ring-1 focus:ring-yellow-400 transition-colors"
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
                  className="w-full bg-slate-900 border border-slate-700 rounded-lg px-4 py-2 text-white focus:outline-none focus:border-yellow-400 focus:ring-1 focus:ring-yellow-400 transition-colors"
                  placeholder="your@email.com"
                />
              </div>

              <div>
                <label htmlFor="message" className="block text-xs font-bold text-slate-400 uppercase tracking-wider mb-1">Message</label>
                <textarea
                  id="message"
                  name="message"
                  required
                  rows={4}
                  className="w-full bg-slate-900 border border-slate-700 rounded-lg px-4 py-2 text-white focus:outline-none focus:border-yellow-400 focus:ring-1 focus:ring-yellow-400 transition-colors resize-none"
                  placeholder="What's on your mind?"
                ></textarea>
              </div>

              <button
                type="submit"
                disabled={isSubmitting || submitStatus === 'success'}
                className="w-full bg-yellow-400 hover:bg-yellow-500 disabled:opacity-50 disabled:cursor-not-allowed text-slate-900 font-bold py-3 px-4 rounded-xl transition-colors shadow-lg shadow-yellow-400/20"
              >
                <i className={`fas ${isSubmitting ? 'fa-spinner fa-spin' : 'fa-paper-plane'} mr-2`}></i>
                {isSubmitting ? 'Sending...' : 'Send Feedback'}
              </button>
            </form>
          )}

        </div>
      </div>
    </div>
  );
};

export default AboutDialog;
