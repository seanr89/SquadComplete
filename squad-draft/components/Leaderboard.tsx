import React, { useState, useEffect } from 'react';
import { Player } from '../types';
import { fetchDailySquads, fetchLeaderboard } from '../api';

interface LeaderboardEntry {
  id: string;
  playerName: string;
  teamAverageRating: number;
  squad: Player[];
}

const Leaderboard: React.FC = () => {
  const [entries, setEntries] = useState<LeaderboardEntry[]>([]);
  const [loading, setLoading] = useState(true);
  const [selectedEntry, setSelectedEntry] = useState<LeaderboardEntry | null>(null);
  const [selectedDate, setSelectedDate] = useState<string>(new Date().toISOString().split('T')[0]);

  useEffect(() => {
    const loadData = async () => {
      setLoading(true);
      try {
        const challenge = await fetchDailySquads(selectedDate);
        if (challenge && challenge.id) {
          const data = await fetchLeaderboard(challenge.id);
          setEntries(data);
        } else {
          setEntries([]);
        }
      } catch (error) {
        console.error("Failed to load leaderboard", error);
        setEntries([]);
      } finally {
        setLoading(false);
      }
    };

    loadData();
  }, [selectedDate]);

  const closeDialog = () => {
    setSelectedEntry(null);
  };

  return (
    <div className="animate-in fade-in slide-in-from-bottom-4 duration-500 w-full max-w-4xl mx-auto">
      <div className="bg-slate-800/80 rounded-2xl p-6 border border-slate-700 shadow-xl">
        <div className="flex flex-col md:flex-row md:items-center justify-between gap-4 mb-6">
          <h2 className="text-2xl font-extrabold text-white flex items-center gap-3">
            <i className="fas fa-list-ol text-yellow-400"></i>
            Daily Leaderboard
          </h2>

          <div className="flex items-center gap-3 bg-slate-700/50 p-2 px-4 rounded-xl border border-slate-600">
            <i className="fas fa-calendar-alt text-slate-400 text-sm"></i>
            <input
              type="date"
              value={selectedDate}
              onChange={(e) => setSelectedDate(e.target.value)}
              className="bg-transparent text-white text-sm font-bold focus:outline-none [color-scheme:dark]"
            />
          </div>
        </div>

        <div className="overflow-x-auto">
          <table className="w-full text-left border-collapse">
            <thead>
              <tr className="border-b border-slate-700 text-slate-400 text-xs uppercase tracking-wider">
                <th className="p-4 font-bold">Rank</th>
                <th className="p-4 font-bold">Player</th>
                <th className="p-4 font-bold text-center">Avg Rating</th>
                <th className="p-4 font-bold text-center">Action</th>
              </tr>
            </thead>
            <tbody>
              {loading ? (
                <tr>
                  <td colSpan={4} className="p-8 text-center text-slate-400 font-bold">
                    <i className="fas fa-spinner fa-spin mr-2"></i> Loading leaderboard...
                  </td>
                </tr>
              ) : entries.length === 0 ? (
                <tr>
                  <td colSpan={4} className="p-8 text-center">
                    <div className="flex flex-col items-center justify-center text-slate-400">
                      <i className="fas fa-calendar-times text-4xl mb-3 text-slate-500"></i>
                      <p className="text-lg font-bold">No Entries Today</p>
                      <p className="text-sm">Be the first to submit your squad!</p>
                    </div>
                  </td>
                </tr>
              ) : (
                entries.map((entry, index) => (
                  <tr
                    key={entry.id}
                    className={`border-b border-slate-700/50 hover:bg-slate-700/30 transition-colors ${index === 0 ? 'bg-yellow-400/5' :
                        index === 1 ? 'bg-slate-300/5' :
                          index === 2 ? 'bg-amber-600/5' : ''
                      }`}
                  >
                    <td className="p-4">
                      <div className="flex items-center justify-center w-8 h-8 rounded-full bg-slate-800 border border-slate-600 font-bold text-white shadow-inner">
                        {index + 1}
                      </div>
                    </td>
                    <td className="p-4 font-bold text-white text-lg">{entry.playerName}</td>
                    <td className="p-4 text-center">
                      <span className="inline-block px-3 py-1 rounded-full bg-slate-800 border border-slate-600 font-black text-yellow-400">
                        {entry.teamAverageRating.toFixed(1)}
                      </span>
                    </td>
                    <td className="p-4 text-center">
                      <button
                        onClick={() => setSelectedEntry(entry)}
                        className="px-4 py-2 bg-slate-700 hover:bg-slate-600 text-white rounded-lg font-bold transition-all shadow-md hover:shadow-lg text-sm"
                      >
                        <i className="fas fa-eye mr-2"></i> View Squad
                      </button>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      </div>

      {/* Placeholder Dialog */}
      {selectedEntry && (
        <div className="fixed inset-0 z-[100] flex items-center justify-center p-4">
          <div className="absolute inset-0 bg-slate-900/80 backdrop-blur-sm" onClick={closeDialog} />

          <div className="relative w-full max-w-lg bg-slate-800 rounded-2xl border border-slate-700 shadow-2xl flex flex-col overflow-hidden max-h-[85vh]">
            <div className="flex items-center justify-between p-4 md:p-6 border-b border-slate-700">
              <h3 className="text-xl font-bold text-white flex items-center gap-2">
                <i className="fas fa-users text-yellow-400"></i>
                {selectedEntry.playerName}'s Squad
              </h3>
              <button
                onClick={closeDialog}
                className="text-slate-400 hover:text-white transition-colors w-8 h-8 flex items-center justify-center rounded-lg hover:bg-slate-700"
              >
                <i className="fas fa-times"></i>
              </button>
            </div>

            <div className="p-6 overflow-y-auto">
              <div className="flex justify-center mb-6">
                <div className="flex flex-col items-center gap-2">
                  <div className="text-4xl font-black text-yellow-400">{selectedEntry.teamAverageRating.toFixed(1)}</div>
                  <div className="text-xs text-slate-400 font-bold uppercase tracking-widest">Team Rating</div>
                </div>
              </div>

              <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
                {selectedEntry.squad.map((player) => (
                  <div key={player.id} className="bg-slate-700/40 rounded-xl p-3 border border-slate-600/50 flex items-center gap-3">
                    <div className="w-10 h-10 rounded-full bg-slate-800 border border-slate-600 flex items-center justify-center overflow-hidden flex-shrink-0">
                      {player.image ? (
                        <img src={player.image} alt={player.name} className="w-full h-full object-cover" />
                      ) : (
                        <span className="text-xs font-bold text-slate-500">{player.position}</span>
                      )}
                    </div>
                    <div className="flex-1 min-w-0">
                      <div className="text-sm font-bold text-white truncate">{player.name}</div>
                      <div className="text-[10px] text-slate-400 font-bold uppercase tracking-wider">{player.position}</div>
                    </div>
                    <div className="text-sm font-black text-yellow-500 bg-yellow-500/10 px-2 py-0.5 rounded">
                      {player.rating.toFixed(1)}
                    </div>
                  </div>
                ))}
              </div>
            </div>

            <div className="p-4 border-t border-slate-700 bg-slate-800/50 flex justify-end">
              <button
                onClick={closeDialog}
                className="px-6 py-2 bg-slate-700 hover:bg-slate-600 text-white rounded-xl font-bold transition-all"
              >
                Close
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default Leaderboard;
