
import React, { useState, useMemo } from 'react';
import { SQUADS, INITIAL_FORMATION } from './constants';
import { DraftState, Player, FormationSpot } from './types';
import Pitch from './components/Pitch';
import PlayerCard from './components/PlayerCard';

const App: React.FC = () => {
  const [view, setView] = useState<'draft' | 'team'>('draft');
  const [draft, setDraft] = useState<DraftState>({
    currentStep: 0,
    selectedPlayers: [],
    formation: [...INITIAL_FORMATION],
    completed: false
  });
  
  const [activeSpotId, setActiveSpotId] = useState<number | null>(null);
  const [tempPlayer, setTempPlayer] = useState<Player | null>(null);

  const currentSquad = SQUADS[draft.currentStep];
  const isDraftComplete = draft.selectedPlayers.length === 11;

  const handlePlayerSelect = (player: Player) => {
    if (draft.selectedPlayers.find(p => p.id === player.id)) return;
    setTempPlayer(player);
    // Automatically focus on first available spot for this position if none selected
    if (!activeSpotId) {
        const firstEmpty = draft.formation.find(s => s.player === null && s.position === player.position);
        if (firstEmpty) setActiveSpotId(firstEmpty.id);
    }
  };

  const confirmPlacement = (spotId: number) => {
    if (!tempPlayer) return;
    
    const spot = draft.formation.find(s => s.id === spotId);
    if (!spot || spot.player) return;

    setDraft(prev => ({
      ...prev,
      selectedPlayers: [...prev.selectedPlayers, tempPlayer],
      formation: prev.formation.map(s => s.id === spotId ? { ...s, player: tempPlayer } : s),
      currentStep: prev.currentStep + 1,
      completed: prev.selectedPlayers.length + 1 === 11
    }));
    
    setTempPlayer(null);
    setActiveSpotId(null);
  };

  const cancelSelection = () => {
    setTempPlayer(null);
    setActiveSpotId(null);
  };

  const resetDraft = () => {
    if (confirm("Reset your draft and start over?")) {
        setDraft({
            currentStep: 0,
            selectedPlayers: [],
            formation: [...INITIAL_FORMATION],
            completed: false
        });
        setView('draft');
    }
  };

  const totalRating = useMemo(() => {
      if (draft.selectedPlayers.length === 0) return 0;
      return Math.round(draft.selectedPlayers.reduce((acc, p) => acc + p.rating, 0) / draft.selectedPlayers.length);
  }, [draft.selectedPlayers]);

  return (
    <div className="min-h-screen flex flex-col max-w-5xl mx-auto p-4 md:p-8">
      {/* Header */}
      <header className="flex flex-col md:flex-row justify-between items-start md:items-center mb-8 gap-4">
        <div>
          <h1 className="text-3xl md:text-4xl font-extrabold tracking-tight text-white flex items-center gap-3">
            <span className="text-yellow-400"><i className="fas fa-trophy"></i></span>
            ULTIMATE 11
          </h1>
          <p className="text-slate-400 font-medium">Daily Squad Draft Challenge</p>
        </div>

        <div className="flex gap-2">
          <button 
            onClick={() => setView('draft')}
            className={`px-4 py-2 rounded-lg font-bold transition-all ${view === 'draft' ? 'bg-yellow-400 text-slate-900 shadow-lg shadow-yellow-400/20' : 'bg-slate-800 text-slate-300 hover:bg-slate-700'}`}
          >
            <i className="fas fa-list-ul mr-2"></i> Draft
          </button>
          <button 
            onClick={() => setView('team')}
            className={`px-4 py-2 rounded-lg font-bold transition-all ${view === 'team' ? 'bg-yellow-400 text-slate-900 shadow-lg shadow-yellow-400/20' : 'bg-slate-800 text-slate-300 hover:bg-slate-700'}`}
          >
            <i className="fas fa-tshirt mr-2"></i> My Team
          </button>
        </div>
      </header>

      {/* Main Content */}
      <main className="flex-1">
        {view === 'draft' && !draft.completed && (
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-8 items-start">
            {/* Squad Selection Area */}
            <div className="space-y-6">
              <div className="bg-slate-800/80 rounded-2xl p-6 border border-slate-700">
                <div className="flex justify-between items-center mb-6">
                  <div>
                    <h2 className="text-xl font-bold text-white">{currentSquad.teamName}</h2>
                    <span className="text-slate-400 text-sm">{currentSquad.season} Squad</span>
                  </div>
                  <div className="text-right">
                    <span className="block text-xs font-bold text-slate-500 uppercase tracking-widest">Pick</span>
                    <span className="text-2xl font-black text-yellow-400">#{draft.currentStep + 1} <span className="text-slate-600 text-sm">/ 11</span></span>
                  </div>
                </div>

                <div className="space-y-3">
                  {currentSquad.players.map(player => (
                    <PlayerCard 
                      key={player.id} 
                      player={player} 
                      isSelected={tempPlayer?.id === player.id}
                      onClick={handlePlayerSelect}
                    />
                  ))}
                </div>
              </div>

              {tempPlayer && (
                <div className="bg-blue-600 rounded-xl p-4 flex items-center justify-between animate-pulse">
                  <div className="flex items-center gap-3">
                    <i className="fas fa-info-circle text-white text-xl"></i>
                    <div>
                      <p className="font-bold text-white">Place {tempPlayer.name}</p>
                      <p className="text-blue-100 text-xs">Tap a spot on the pitch to confirm placement</p>
                    </div>
                  </div>
                  <button onClick={cancelSelection} className="p-2 hover:bg-blue-700 rounded-lg text-white">
                    <i className="fas fa-times"></i>
                  </button>
                </div>
              )}
              
              <div className="bg-slate-800/30 rounded-xl p-4 border border-dashed border-slate-700">
                <h4 className="text-slate-400 text-xs font-bold uppercase mb-2">Instructions</h4>
                <ul className="text-xs text-slate-500 space-y-1">
                    <li>• Choose ONE player from each daily squad</li>
                    <li>• Assign them to a specific spot on your formation</li>
                    <li>• Balanced positions lead to higher team synergy</li>
                </ul>
              </div>
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
              />
            </div>
          </div>
        )}

        {(view === 'team' || draft.completed) && (
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
                        <button 
                            onClick={resetDraft}
                            className="w-full py-3 px-4 bg-red-500/10 border border-red-500/20 text-red-500 rounded-xl font-bold hover:bg-red-500 hover:text-white transition-all flex items-center justify-center gap-2"
                        >
                            <i className="fas fa-redo"></i> Reset Draft
                        </button>
                    </div>

                    <div className="bg-slate-800/80 rounded-2xl p-6 border border-slate-700">
                        <h3 className="text-slate-400 font-bold text-sm uppercase mb-4 tracking-widest">Roster List</h3>
                        <div className="space-y-2 max-h-[300px] overflow-y-auto pr-2 custom-scrollbar">
                            {draft.selectedPlayers.length === 0 ? (
                                <p className="text-slate-600 text-sm text-center py-4">No players drafted yet</p>
                            ) : (
                                draft.selectedPlayers.map(p => (
                                    <div key={p.id} className="flex items-center gap-3 p-2 bg-slate-900/50 rounded-lg border border-slate-700/50">
                                        <img src={p.image} className="w-8 h-8 rounded-full" alt="" />
                                        <div className="flex-1 min-w-0">
                                            <p className="text-sm font-bold text-white truncate">{p.name}</p>
                                            <p className="text-[10px] text-slate-500">{p.position} • {p.club}</p>
                                        </div>
                                        <div className="text-yellow-400 font-black text-sm">{p.rating}</div>
                                    </div>
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
      <footer className="fixed bottom-0 left-0 right-0 bg-slate-900 border-t border-slate-800 p-4 md:hidden z-50">
        <div className="flex justify-around items-center max-w-lg mx-auto">
            <button 
                onClick={() => setView('draft')}
                className={`flex flex-col items-center gap-1 ${view === 'draft' ? 'text-yellow-400' : 'text-slate-500'}`}
            >
                <i className="fas fa-list-ul"></i>
                <span className="text-[10px] font-bold uppercase">Draft</span>
            </button>
            <div className="w-12 h-12 rounded-full bg-slate-800 -mt-10 border-4 border-slate-900 flex items-center justify-center text-yellow-400 shadow-xl">
                 <i className="fas fa-plus"></i>
            </div>
            <button 
                onClick={() => setView('team')}
                className={`flex flex-col items-center gap-1 ${view === 'team' ? 'text-yellow-400' : 'text-slate-500'}`}
            >
                <i className="fas fa-tshirt"></i>
                <span className="text-[10px] font-bold uppercase">Team</span>
            </button>
        </div>
      </footer>
    </div>
  );
};

export default App;
