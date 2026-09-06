
import React, { useState, useMemo, useRef } from 'react';

import { INITIAL_FORMATION, generateFormationSpots } from './constants';
import { DraftState, Player, Squad, FormationSpot, Position } from './types';
import { fetchDailySquads, submitUserSquad, recordRequest } from './api';
import Pitch from './components/Pitch';
import PlayerCard from './components/PlayerCard';
import AboutDialog from './components/AboutDialog';
import Leaderboard from './components/Leaderboard';
import AlertDialog from './components/AlertDialog';
import FixtureInfo from './components/FixtureInfo';
import CookieConsent from './components/CookieConsent';

const App: React.FC = () => {
  const [view, setView] = useState<'draft' | 'team' | 'leaderboard'>('draft');
  const [isAboutOpen, setIsAboutOpen] = useState(false);
  const [isInstructionsOpen, setIsInstructionsOpen] = useState(false);
  const [isResetConfirmOpen, setIsResetConfirmOpen] = useState(false);
  const [liveAnnouncement, setLiveAnnouncement] = useState('');
  const [alertConfig, setAlertConfig] = useState<{isOpen: boolean, title: string, message: string, type: 'success' | 'error' | 'info' | 'warning'}>({isOpen: false, title: '', message: '', type: 'info'});
  const [squads, setSquads] = useState<Squad[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const [draft, setDraft] = useState<DraftState>(() => {
    try {
      const today = new Date().toISOString().split('T')[0];
      const savedDraft = localStorage.getItem(`squad-draft-${today}`);
      if (savedDraft) {
        return JSON.parse(savedDraft);
      }
    } catch (e) {
      console.error("Failed to load draft from local storage", e);
    }
    return {
      currentStep: 0,
      selectedPlayers: [],
      formation: [...INITIAL_FORMATION],
      completed: false
    };
  });

  React.useEffect(() => {
    const hasRecorded = sessionStorage.getItem('squad-draft-recorded');
    if (!hasRecorded) {
      recordRequest().then((success) => {
        if (success) {
          sessionStorage.setItem('squad-draft-recorded', 'true');
        }
      });
    }
  }, []);

  React.useEffect(() => {
    const loadSquads = async () => {
      setLoading(true);
      try {
        const fetchedChallenge = await fetchDailySquads();
        if (fetchedChallenge && fetchedChallenge.squads.length > 0) {
          setSquads(fetchedChallenge.squads);
          
          // If the draft is new (or empty), initialize the formation from the API
          setDraft(prev => {
             const updates: Partial<DraftState> = {
                gameRecordId: fetchedChallenge.id,
                formationId: fetchedChallenge.formation?.id
             };
             if (prev.selectedPlayers.length === 0 && fetchedChallenge.formation) {
                 updates.formation = generateFormationSpots(
                     fetchedChallenge.formation.defence,
                     fetchedChallenge.formation.midfield,
                     fetchedChallenge.formation.attack
                 );
             }
             return { ...prev, ...updates };
          });
        } else {
          // Fallback to hardcoded squads if API fails or returns no data
          setError('No game records returned from the API. Please try again later.');
        }
      } catch (err) {
        console.error('Failed to load squads', err);
        setError('Failed to load today\'s challenge. Using local data.');

      } finally {
        setLoading(false);
      }
    };
    loadSquads();
  }, []);

  React.useEffect(() => {
    try {
      const today = new Date().toISOString().split('T')[0];
      localStorage.setItem(`squad-draft-${today}`, JSON.stringify(draft));
    } catch (e) {
      console.error("Failed to save draft to local storage", e);
    }
  }, [draft]);

  const [activeSpotId, setActiveSpotId] = useState<number | null>(null);
  const [tempPlayer, setTempPlayer] = useState<Player | null>(null);

  const currentSquad = squads[draft.currentStep];
  const isDraftComplete = draft.selectedPlayers.length === 11 || draft.currentStep >= squads.length;

  const currentSquadFormation = useMemo(() => {
    if (!currentSquad) return [];

    const playersByPos: Record<string, Player[]> = {
      GK: [], DEF: [], MID: [], FWD: []
    };

    currentSquad.players.forEach(p => {
      const pos = p.position || 'MID';
      if (playersByPos[pos as string]) {
        playersByPos[pos as string].push(p);
      } else {
        playersByPos['MID'].push(p);
      }
    });

    const formation: FormationSpot[] = [];
    let idCounter = 100;

    const rowConfigs: { pos: Position; top: string }[] = [
      { pos: 'GK', top: '85%' },
      { pos: 'DEF', top: '65%' },
      { pos: 'MID', top: '40%' },
      { pos: 'FWD', top: '15%' }
    ];

    rowConfigs.forEach(({ pos, top }) => {
      const players = playersByPos[pos];
      const count = players.length;

      players.forEach((player, i) => {
        let left = '50%';
        let spotTop = top;

        if (count === 1) {
          left = '50%';
        } else if (count === 2) {
          left = i === 0 ? '35%' : '65%';
        } else if (count === 3) {
          left = i === 0 ? '25%' : (i === 1 ? '50%' : '75%');
          if (pos === 'FWD' && i === 1) spotTop = '10%';
        } else if (count === 4) {
          left = (20 + i * 20) + '%';
        } else if (count >= 5) {
          left = (15 + (i * 70) / (count - 1)) + '%';
        }

        formation.push({
          id: idCounter++,
          position: pos,
          top: spotTop,
          left,
          player
        });
      });
    });

    return formation;
  }, [currentSquad]);

  const handlePlayerSelect = (player: Player) => {
    if (draft.selectedPlayers.find(p => p.id === player.id)) return;
    setTempPlayer(player);
    setActiveSpotId(null);
    setLiveAnnouncement(`Selected ${player.name}, position ${player.position}. Now select an open spot on the tactical pitch to confirm placement.`);
  };

  const confirmPlacement = (spotId: number) => {
    if (!tempPlayer) return;

    const spot = draft.formation.find(s => s.id === spotId);
    if (!spot || spot.player) return;

    const placedPlayer = tempPlayer;
    const nextCount = draft.selectedPlayers.length + 1;
    const isNowComplete = nextCount === 11;

    setDraft(prev => ({
      ...prev,
      selectedPlayers: [...prev.selectedPlayers, placedPlayer],
      formation: prev.formation.map(s => s.id === spotId ? { ...s, player: placedPlayer } : s),
      currentStep: prev.currentStep + 1,
      completed: isNowComplete
    }));

    if (isNowComplete) {
      setLiveAnnouncement(`Placed ${placedPlayer.name} into ${spot.position}. Draft complete! All 11 players selected.`);
    } else {
      setLiveAnnouncement(`Placed ${placedPlayer.name} into ${spot.position}. Advancing to pick number ${draft.currentStep + 2} of 11.`);
    }

    setTempPlayer(null);
    setActiveSpotId(null);
  };

  const cancelSelection = () => {
    if (tempPlayer) {
      setLiveAnnouncement(`Cancelled selection for ${tempPlayer.name}.`);
    }
    setTempPlayer(null);
    setActiveSpotId(null);
  };

  const executeResetDraft = () => {
    const today = new Date().toISOString().split('T')[0];
    localStorage.removeItem(`squad-draft-${today}`);
    setDraft(prev => ({
      currentStep: 0,
      selectedPlayers: [],
      formation: prev.formation.map(s => ({ ...s, player: null })),
      completed: false
    }));
    setView('draft');
    setIsResetConfirmOpen(false);
    setLiveAnnouncement('Draft has been reset. You can start over.');
  };

  const totalRating = useMemo(() => {
    if (draft.selectedPlayers.length === 0) return 0;
    return (draft.selectedPlayers.reduce((acc, p) => acc + p.rating, 0) / draft.selectedPlayers.length).toFixed(1);
  }, [draft.selectedPlayers]);

  const [userName, setUserName] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);

  const getShareText = () => {
    const formattedDate = new Date().toLocaleDateString(undefined, {
      weekday: 'short',
      year: 'numeric',
      month: 'short',
      day: 'numeric',
    });
    const playUrl = window.location.origin;

    return `🏆 I just completed today's Ultimate 11 Draft Challenge! ⚽\n\n` +
           `📅 Date: ${formattedDate}\n` +
           `⭐ My Team Avg Rating: ${totalRating} / 100\n\n` +
           `Can you build a better squad? Play today's challenge here:\n` +
           `🔗 ${playUrl}`;
  };

  const handleShareWhatsApp = () => {
    const shareText = encodeURIComponent(getShareText());
    const whatsappUrl = `https://api.whatsapp.com/send?text=${shareText}`;
    window.open(whatsappUrl, '_blank');
  };

  const getBrowserId = () => {
    let id = localStorage.getItem('squad-browser-id');
    if (!id) {
      id = crypto.randomUUID ? crypto.randomUUID() : Math.random().toString(36).substring(2, 15);
      localStorage.setItem('squad-browser-id', id);
    }
    return id;
  };

  const handleSubmitTeam = async () => {
    if (!draft.completed || draft.submitted) return;
    if (!userName.trim()) {
        setAlertConfig({
            isOpen: true,
            title: 'Name Required',
            message: 'Please enter your name to submit your team.',
            type: 'error'
        });
        return;
    }

    setIsSubmitting(true);
    const payload = {
        BrowserIdentifierId: getBrowserId(),
        UserName: userName.trim(),
        GameRecordId: draft.gameRecordId,
        FormationId: draft.formationId,
        Players: draft.formation.map(spot => ({
            PlayerId: parseInt(spot.player?.id || '0', 10),
            Position: spot.position,
            IsCaptain: false,
            IsViceCaptain: false
        }))
    };

    const success = await submitUserSquad(payload);
    if (success) {
        setDraft(prev => ({ ...prev, submitted: true }));
        setAlertConfig({
            isOpen: true,
            title: 'Success!',
            message: 'Team submitted successfully!',
            type: 'success'
        });
    } else {
        setAlertConfig({
            isOpen: true,
            title: 'Submission Failed',
            message: 'Failed to submit team. You may have already submitted one for today.',
            type: 'error'
        });
    }
    setIsSubmitting(false);
  };

  return (
    <div className="min-h-screen flex flex-col max-w-5xl mx-auto p-4 md:p-8">
      {/* Skip to Main Content Link for Keyboard Accessibility */}
      <a
        href="#main-content"
        className="sr-only focus:not-sr-only focus:fixed focus:top-4 focus:left-4 focus:z-[200] focus:px-4 focus:py-2 focus:bg-yellow-400 focus:text-slate-900 focus:font-bold focus:rounded-xl focus:shadow-2xl focus:outline-none"
      >
        Skip to main content
      </a>

      {/* Screen Reader Live Region for Dynamic Game Announcements */}
      <div aria-live="polite" aria-atomic="true" className="sr-only">
        {liveAnnouncement}
      </div>

      {/* Header */}
      <header className="flex flex-col md:flex-row justify-between items-start md:items-center mb-8 gap-4">
        <div>
          <h1 className="text-3xl md:text-4xl font-extrabold tracking-tight text-white flex items-center gap-3">
            <span className="text-yellow-400" aria-hidden="true"><i className="fas fa-trophy"></i></span>
            ULTIMATE 11
          </h1>
          <div className="flex items-center gap-2 relative group w-max">
            <p className="text-slate-300 font-medium">Daily Squad Draft Challenge</p>
            <button
              type="button"
              onClick={() => setIsInstructionsOpen(prev => !prev)}
              aria-expanded={isInstructionsOpen}
              aria-label="Toggle draft instructions"
              className="text-slate-400 hover:text-slate-200 focus-visible:ring-2 focus-visible:ring-yellow-400 focus-visible:outline-none rounded-full p-1 transition-colors"
            >
              <i className="fas fa-info-circle text-sm" aria-hidden="true"></i>
            </button>

            <div
              className={`absolute left-0 top-full mt-2 w-72 bg-slate-800 rounded-xl p-4 border border-slate-700 shadow-xl transition-all duration-200 z-50 ${
                isInstructionsOpen ? 'opacity-100 visible' : 'opacity-0 invisible group-hover:opacity-100 group-hover:visible pointer-events-none group-hover:pointer-events-auto'
              }`}
            >
              <h2 className="text-slate-400 text-xs font-bold uppercase mb-2">Instructions</h2>
              <ul className="text-xs text-slate-300 space-y-1.5 list-disc pl-4">
                <li>Choose ONE player from each daily squad</li>
                <li>Assign them to a specific spot on your formation</li>
                <li>Balanced positions lead to higher team synergy</li>
              </ul>
            </div>
          </div>
        </div>

        <nav aria-label="Main navigation" className="flex gap-2">
          <button
            type="button"
            onClick={() => setIsAboutOpen(true)}
            className="px-4 py-2 rounded-lg font-bold transition-all bg-slate-800 text-slate-300 hover:bg-slate-700 focus-visible:ring-2 focus-visible:ring-yellow-400 focus-visible:outline-none"
            title="About Us"
          >
            <i className="fas fa-question-circle md:mr-2" aria-hidden="true"></i> <span className="hidden md:inline">About</span>
          </button>
          <button
            type="button"
            onClick={() => setView('draft')}
            aria-current={view === 'draft' ? 'page' : undefined}
            className={`px-4 py-2 rounded-lg font-bold transition-all focus-visible:ring-2 focus-visible:ring-yellow-400 focus-visible:outline-none ${
              view === 'draft'
                ? 'bg-yellow-400 text-slate-900 shadow-lg shadow-yellow-400/20'
                : 'bg-slate-800 text-slate-300 hover:bg-slate-700'
            }`}
          >
            <i className="fas fa-list-ul mr-2" aria-hidden="true"></i> Draft
          </button>
          <button
            type="button"
            onClick={() => setView('team')}
            aria-current={view === 'team' ? 'page' : undefined}
            className={`px-4 py-2 rounded-lg font-bold transition-all focus-visible:ring-2 focus-visible:ring-yellow-400 focus-visible:outline-none ${
              view === 'team'
                ? 'bg-yellow-400 text-slate-900 shadow-lg shadow-yellow-400/20'
                : 'bg-slate-800 text-slate-300 hover:bg-slate-700'
            }`}
          >
            <i className="fas fa-tshirt mr-2" aria-hidden="true"></i> My Team
          </button>
          <button
            type="button"
            onClick={() => setView('leaderboard')}
            aria-current={view === 'leaderboard' ? 'page' : undefined}
            disabled={!draft.completed}
            className={`px-4 py-2 rounded-lg font-bold transition-all focus-visible:ring-2 focus-visible:ring-yellow-400 focus-visible:outline-none ${
              !draft.completed
                ? 'opacity-50 cursor-not-allowed bg-slate-800 text-slate-500'
                : view === 'leaderboard'
                ? 'bg-yellow-400 text-slate-900 shadow-lg shadow-yellow-400/20'
                : 'bg-slate-800 text-slate-300 hover:bg-slate-700'
            }`}
            title={!draft.completed ? 'Complete draft to view leaderboard' : ''}
          >
            <i className="fas fa-list-ol mr-2" aria-hidden="true"></i> Leaderboard
          </button>
        </nav>
      </header>

      {/* Main Content */}
      <main id="main-content" tabIndex={-1} className="flex-1 flex flex-col justify-center focus:outline-none">
        {loading && (
          <div className="flex flex-col items-center justify-center p-12 text-slate-400">
            <i className="fas fa-spinner fa-spin text-4xl mb-4 text-yellow-400"></i>
            <p className="font-bold">Loading Daily Challenge...</p>
          </div>
        )}

        {!loading && error && view !== 'leaderboard' && (
          <div className="flex flex-col items-center justify-center p-12 text-center max-w-lg mx-auto">
            <div className="bg-red-500/10 text-red-400 p-8 rounded-2xl border border-red-500/20 shadow-lg shadow-red-500/10 w-full">
              <i className="fas fa-exclamation-triangle text-5xl mb-6 text-red-500"></i>
              <h2 className="text-2xl font-bold text-white mb-4">Error Loading Draft</h2>
              <p className="text-slate-300 font-medium mb-8 leading-relaxed">
                {error}
              </p>
              <button
                onClick={() => window.location.reload()}
                className="bg-red-500 hover:bg-red-600 text-white px-8 py-3 rounded-xl font-bold transition-all shadow-lg hover:shadow-xl hover:scale-105"
              >
                <i className="fas fa-sync-alt mr-2"></i> Try Again
              </button>
            </div>
          </div>
        )}

        {!loading && view === 'leaderboard' && (
          <Leaderboard />
        )}

        {!loading && !error && currentSquad && view === 'draft' && !draft.completed && (
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-8 items-start">
            {/* Squad Selection Area */}
            <div className="space-y-6">
              <div className="bg-slate-800/80 rounded-2xl p-6 border border-slate-700">
                <div className="flex justify-between items-center mb-6">
                  <div>
                    <h2 className="text-xl font-bold text-white">{currentSquad.teamName}</h2>
                    {currentSquad.fixtureId && (
                      <FixtureInfo fixtureId={currentSquad.fixtureId} />
                    )}
                  </div>
                  <div className="text-right">
                    <span className="block text-xs font-bold text-slate-500 uppercase tracking-widest">Pick</span>
                    <span className="text-2xl font-black text-yellow-400">#{draft.currentStep + 1} <span className="text-slate-600 text-sm">/ 11</span></span>
                  </div>
                </div>

                <div className="mx-auto w-full">
                  <Pitch
                    formation={currentSquadFormation}
                    activeSpotId={null}
                    onPlayerClick={handlePlayerSelect}
                    selectedPlayerId={tempPlayer?.id}
                    disabledPlayerIds={draft.selectedPlayers.map(p => p.id)}
                    isDraggable={true}
                    onPlayerDragStart={handlePlayerSelect}
                  />
                </div>
              </div>

              {tempPlayer && (
                <div
                  role="status"
                  aria-live="polite"
                  className="bg-blue-600 rounded-xl p-4 flex items-center justify-between shadow-lg"
                >
                  <div className="flex items-center gap-3">
                    <i className="fas fa-info-circle text-white text-xl" aria-hidden="true"></i>
                    <div>
                      <p className="font-bold text-white">Place {tempPlayer.name}</p>
                      <p className="text-blue-100 text-xs">Tap an empty spot on the tactical pitch to confirm placement</p>
                    </div>
                  </div>
                  <button
                    type="button"
                    onClick={cancelSelection}
                    aria-label={`Cancel placement for ${tempPlayer.name}`}
                    className="p-2 hover:bg-blue-700 rounded-lg text-white focus-visible:ring-2 focus-visible:ring-white focus-visible:outline-none transition-colors"
                  >
                    <i className="fas fa-times" aria-hidden="true"></i>
                  </button>
                </div>
              )}
            </div>

            {/* Pitch Visualization Area */}
            <div className="sticky top-8">
              <Pitch
                formation={draft.formation}
                activeSpotId={activeSpotId}
                onSpotClick={(spot) => {
                  if (tempPlayer) {
                    confirmPlacement(spot.id);
                  } else {
                    setActiveSpotId(spot.id);
                  }
                }}
                isDroppable={true}
                onSpotDrop={(spotId) => {
                  if (tempPlayer) {
                    confirmPlacement(spotId);
                  }
                }}
              />
            </div>
          </div>
        )}

        {!loading && !error && view !== 'leaderboard' && (view === 'team' || draft.completed) && (
          <div className="animate-in fade-in slide-in-from-bottom-4 duration-500">
            <div className="flex flex-col md:flex-row gap-8">
              <div className="md:w-2/3">
                <Pitch formation={draft.formation} activeSpotId={null} />
              </div>

              <div className="md:w-1/3 space-y-6">
                <div className="bg-slate-800/80 rounded-2xl p-6 border border-slate-700 text-center">
                  <h3 className="text-slate-400 font-bold text-sm uppercase mb-4 tracking-widest">Team Performance</h3>
                  <div className="flex justify-around items-end h-24 mb-6">
                    <div className="flex flex-col items-center gap-2">
                      <div className="text-3xl font-black text-white">{totalRating}</div>
                      <div className="text-[10px] text-slate-500 font-bold uppercase">Avg Rating</div>
                    </div>
                    <div className="w-[2px] h-full bg-slate-700"></div>
                    <div className="flex flex-col items-center gap-2">
                      <div className="text-3xl font-black text-yellow-400">{draft.selectedPlayers.length}</div>
                      <div className="text-[10px] text-slate-500 font-bold uppercase">Players</div>
                    </div>
                  </div>
                  {draft.completed && (
                    <div className="bg-green-500/10 border border-green-500/20 text-green-400 rounded-lg p-4 mb-6">
                      <p className="font-bold">Draft Complete!</p>
                      <p className="text-xs opacity-80">You've built an incredible squad of legends.</p>
                    </div>
                  )}
                  {draft.completed && !draft.submitted && (
                    <div className="mb-6 space-y-3">
                      <label htmlFor="user-name-input" className="sr-only">Your Name</label>
                      <input
                        id="user-name-input"
                        type="text"
                        placeholder="Enter your name"
                        value={userName}
                        onChange={(e) => setUserName(e.target.value)}
                        className="w-full bg-slate-900 border border-slate-700 rounded-lg px-4 py-2 text-white placeholder-slate-400 focus:outline-none focus:border-yellow-400 focus:ring-2 focus:ring-yellow-400"
                        maxLength={50}
                        required
                        aria-required="true"
                      />
                      <button
                        type="button"
                        onClick={handleSubmitTeam}
                        disabled={isSubmitting || !userName.trim()}
                        className="w-full py-3 px-4 bg-yellow-400 text-slate-900 rounded-xl font-bold hover:bg-yellow-500 transition-all flex items-center justify-center gap-2 disabled:opacity-50 focus-visible:ring-2 focus-visible:ring-yellow-400 focus-visible:outline-none"
                      >
                        {isSubmitting ? (
                            <><i className="fas fa-spinner fa-spin" aria-hidden="true"></i> Submitting...</>
                        ) : (
                            <><i className="fas fa-upload" aria-hidden="true"></i> Submit Team</>
                        )}
                      </button>
                    </div>
                  )}
                  {draft.submitted && (
                     <div role="status" className="bg-blue-500/10 border border-blue-500/20 text-blue-400 rounded-lg p-4 mb-6">
                       <p className="font-bold">Team Submitted!</p>
                       <p className="text-xs opacity-80">Check the leaderboard to see how you rank.</p>
                     </div>
                  )}
                  <div className="border-t border-slate-700/50 my-6 pt-6 text-left">
                    <h4 className="text-slate-400 font-bold text-xs uppercase mb-3 tracking-widest">Share Challenge</h4>
                    <div className="mb-4">
                      <button
                        type="button"
                        onClick={handleShareWhatsApp}
                        className="w-full py-3 px-4 bg-emerald-600/10 border border-emerald-500/20 text-emerald-400 rounded-xl font-bold hover:bg-emerald-600 hover:text-white transition-all flex items-center justify-center gap-2 focus-visible:ring-2 focus-visible:ring-emerald-400 focus-visible:outline-none"
                      >
                        <i className="fa-brands fa-whatsapp text-lg" aria-hidden="true"></i> WhatsApp
                      </button>
                    </div>
                  </div>
                  <button
                    type="button"
                    onClick={() => setIsResetConfirmOpen(true)}
                    className="w-full py-3 px-4 bg-red-500/10 border border-red-500/20 text-red-500 rounded-xl font-bold hover:bg-red-500 hover:text-white transition-all flex items-center justify-center gap-2 focus-visible:ring-2 focus-visible:ring-red-400 focus-visible:outline-none"
                  >
                    <i className="fas fa-redo" aria-hidden="true"></i> Reset Draft
                  </button>
                </div>

                <div className="bg-slate-800/80 rounded-2xl p-6 border border-slate-700">
                  <h3 className="text-slate-400 font-bold text-sm uppercase mb-4 tracking-widest">Roster List</h3>
                  <div className="space-y-3 max-h-[400px] overflow-y-auto pr-2 custom-scrollbar">
                    {draft.selectedPlayers.length === 0 ? (
                      <p className="text-slate-600 text-sm text-center py-4">No players drafted yet</p>
                    ) : (
                      draft.selectedPlayers.map(p => (
                        <PlayerCard
                          key={p.id}
                          player={p}
                        />
                      ))
                    )}
                  </div>
                </div>
              </div>
            </div>
          </div>
        )}
      </main>

      {/* Footer Navigation (Mobile Only) */}
      <div className="md:hidden h-20"></div> {/* Spacer for fixed footer */}
      {/* About Dialog */}
      <AboutDialog isOpen={isAboutOpen} onClose={() => setIsAboutOpen(false)} />

      {/* Alert Dialog */}
      <AlertDialog 
        isOpen={alertConfig.isOpen} 
        title={alertConfig.title} 
        message={alertConfig.message} 
        type={alertConfig.type} 
        onClose={() => setAlertConfig(prev => ({ ...prev, isOpen: false }))} 
      />

      {/* Reset Confirmation Dialog */}
      <AlertDialog
        isOpen={isResetConfirmOpen}
        title="Reset Draft?"
        message="Are you sure you want to reset your draft? Your currently placed players will be cleared and you can start today's challenge over."
        type="warning"
        confirmText="Reset Draft"
        cancelText="Cancel"
        onClose={() => setIsResetConfirmOpen(false)}
        onConfirm={executeResetDraft}
      />

      <CookieConsent />

      <footer className="fixed bottom-0 left-0 right-0 bg-slate-900 border-t border-slate-800 p-4 md:hidden z-50">
        <nav aria-label="Mobile navigation" className="flex justify-around items-center max-w-lg mx-auto">
          <button
            type="button"
            onClick={() => setView('draft')}
            aria-current={view === 'draft' ? 'page' : undefined}
            className={`flex flex-col items-center gap-1 p-2 rounded-lg focus-visible:ring-2 focus-visible:ring-yellow-400 focus-visible:outline-none ${view === 'draft' ? 'text-yellow-400' : 'text-slate-400'}`}
          >
            <i className="fas fa-list-ul" aria-hidden="true"></i>
            <span className="text-[10px] font-bold uppercase">Draft</span>
          </button>
          <div className="w-12 h-12 rounded-full bg-slate-800 -mt-10 border-4 border-slate-900 flex items-center justify-center text-yellow-400 shadow-xl" aria-hidden="true">
            <i className="fas fa-plus"></i>
          </div>
          <button
            type="button"
            onClick={() => setView('team')}
            aria-current={view === 'team' ? 'page' : undefined}
            className={`flex flex-col items-center gap-1 p-2 rounded-lg focus-visible:ring-2 focus-visible:ring-yellow-400 focus-visible:outline-none ${view === 'team' ? 'text-yellow-400' : 'text-slate-400'}`}
          >
            <i className="fas fa-tshirt" aria-hidden="true"></i>
            <span className="text-[10px] font-bold uppercase">Team</span>
          </button>
          <button
            type="button"
            onClick={() => setView('leaderboard')}
            aria-current={view === 'leaderboard' ? 'page' : undefined}
            disabled={!draft.completed}
            className={`flex flex-col items-center gap-1 p-2 rounded-lg focus-visible:ring-2 focus-visible:ring-yellow-400 focus-visible:outline-none ${
              !draft.completed
                ? 'opacity-50 cursor-not-allowed text-slate-600'
                : view === 'leaderboard'
                ? 'text-yellow-400'
                : 'text-slate-400'
            }`}
            title={!draft.completed ? 'Complete draft to view leaderboard' : ''}
          >
            <i className="fas fa-list-ol" aria-hidden="true"></i>
            <span className="text-[10px] font-bold uppercase">Ranks</span>
          </button>
        </nav>
      </footer>
    </div>
  );
};

export default App;
