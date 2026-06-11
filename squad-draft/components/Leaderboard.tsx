import React, { useState, useEffect } from 'react';
import { Player } from '../types';
import { fetchDailySquads, fetchLeaderboard } from '../api';
import MaterialDatePicker from './MaterialDatePicker';

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

  // Date picker limit: not in the future, only go back 14 days
  const maxDate = new Date();
  const minDate = new Date();
  minDate.setDate(maxDate.getDate() - 14);

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
        setViewMonthYearFromDateString(selectedDate);
        setLoading(false);
      }
    };

    loadData();
  }, [selectedDate]);

  // Sync internal calendar view month/year to reflect selectedDate changes
  const [calendarMonth, setCalendarMonth] = useState(new Date().getMonth());
  const [calendarYear, setCalendarYear] = useState(new Date().getFullYear());

  const setViewMonthYearFromDateString = (dateStr: string) => {
    try {
      const parts = dateStr.split('-');
      if (parts.length === 3) {
        setCalendarMonth(parseInt(parts[1], 10) - 1);
        setCalendarYear(parseInt(parts[0], 10));
      }
    } catch (e) {
      console.error(e);
    }
  };

  const closeDialog = () => {
    setSelectedEntry(null);
  };

  return (
    <div className="animate-in fade-in slide-in-from-bottom-4 duration-500 w-full max-w-4xl mx-auto">
      <div className="bg-[#111827]/70 backdrop-blur-md rounded-2xl p-6 border border-slate-800 shadow-2xl">
        <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 mb-8">
          <h2 className="text-2xl font-extrabold text-white flex items-center gap-3">
            <span className="text-yellow-400"><i className="fas fa-list-ol"></i></span>
            Daily Leaderboard
          </h2>

          <div className="flex justify-end">
            <MaterialDatePicker
              value={selectedDate}
              onChange={setSelectedDate}
              minDate={minDate}
              maxDate={maxDate}
            />
          </div>
        </div>

        <div className="overflow-x-auto rounded-xl border border-slate-800/80 bg-slate-900/40">
          <table className="w-full text-left border-collapse">
            <thead>
              <tr className="border-b border-slate-800/80 bg-slate-900/50 text-slate-400 text-xs font-bold uppercase tracking-wider">
                <th className="p-4 py-5 pl-6 font-bold w-20">Rank</th>
                <th className="p-4 py-5 font-bold">Player</th>
                <th className="p-4 py-5 font-bold text-center w-36">Avg Rating</th>
                <th className="p-4 py-5 font-bold text-center w-36">Action</th>
              </tr>
            </thead>
            <tbody>
              {loading ? (
                <tr>
                  <td colSpan={4} className="p-12 text-center text-slate-400 font-bold">
                    <div className="flex items-center justify-center gap-3">
                      <i className="fas fa-spinner fa-spin text-yellow-400 text-lg"></i>
                      <span>Loading leaderboard...</span>
                    </div>
                  </td>
                </tr>
              ) : entries.length === 0 ? (
                <tr>
                  <td colSpan={4} className="p-16 text-center">
                    <div className="flex flex-col items-center justify-center text-slate-400 max-w-xs mx-auto">
                      <div className="w-16 h-16 rounded-full bg-slate-800 border border-slate-700 flex items-center justify-center mb-4 text-slate-500 shadow-inner">
                        <i className="fas fa-calendar-times text-2xl"></i>
                      </div>
                      <p className="text-lg font-bold text-white mb-1">No Entries Today</p>
                      <p className="text-sm text-slate-500 leading-relaxed">Be the first to build a squad and submit it to the leaderboard!</p>
                    </div>
                  </td>
                </tr>
              ) : (
                entries.map((entry, index) => (
                  <tr
                    key={entry.id}
                    className={`border-b border-slate-800/50 hover:bg-slate-800/30 transition-colors last:border-0 ${
                      index === 0 ? 'bg-yellow-500/5 hover:bg-yellow-500/10' :
                      index === 1 ? 'bg-slate-400/5 hover:bg-slate-400/10' :
                      index === 2 ? 'bg-amber-600/5 hover:bg-amber-600/10' : ''
                    }`}
                  >
                    <td className="p-4 pl-6">
                      <div className="flex items-center">
                        {index === 0 ? (
                          <div className="flex items-center justify-center w-8 h-8 rounded-full bg-gradient-to-r from-yellow-500 to-amber-400 text-slate-900 border border-yellow-300 font-extrabold shadow-lg shadow-yellow-500/20 text-sm">
                            🏆
                          </div>
                        ) : index === 1 ? (
                          <div className="flex items-center justify-center w-8 h-8 rounded-full bg-gradient-to-r from-slate-400 to-slate-300 text-slate-900 border border-slate-200 font-extrabold shadow-lg shadow-slate-300/20 text-sm">
                            🥈
                          </div>
                        ) : index === 2 ? (
                          <div className="flex items-center justify-center w-8 h-8 rounded-full bg-gradient-to-r from-amber-700 to-amber-600 text-white border border-amber-500 font-extrabold shadow-lg shadow-amber-700/20 text-sm">
                            🥉
                          </div>
                        ) : (
                          <div className="flex items-center justify-center w-8 h-8 rounded-full bg-slate-800/80 border border-slate-700 font-bold text-slate-400 text-xs">
                            {index + 1}
                          </div>
                        )}
                      </div>
                    </td>
                    <td className="p-4">
                      <div className="flex items-center gap-3">
                        <div className="w-9 h-9 rounded-full bg-slate-800/90 border border-slate-700/80 flex items-center justify-center text-slate-400 shadow-inner flex-shrink-0">
                          <i className="fas fa-user text-xs"></i>
                        </div>
                        <span className="font-bold text-white text-base truncate max-w-[180px] sm:max-w-none">{entry.playerName}</span>
                      </div>
                    </td>
                    <td className="p-4 text-center">
                      <span className="inline-block px-3 py-1 rounded-full bg-slate-800 border border-slate-700 font-black text-yellow-400 text-sm tracking-wider">
                        {entry.teamAverageRating.toFixed(1)}
                      </span>
                    </td>
                    <td className="p-4 text-center">
                      <button
                        onClick={() => setSelectedEntry(entry)}
                        className="mx-auto px-4 py-2 bg-yellow-400/10 hover:bg-yellow-400 text-yellow-400 hover:text-slate-950 border border-yellow-400/20 hover:border-yellow-400 rounded-xl font-bold transition-all shadow-md active:scale-[0.97] text-xs flex items-center justify-center gap-2 w-full sm:w-auto max-w-[120px]"
                      >
                        <i className="fas fa-eye"></i> View Squad
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
