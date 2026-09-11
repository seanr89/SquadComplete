import React from 'react';
import { Player, Position } from '../types';
import PlayerCard from './PlayerCard';

interface TeamRosterProps {
  players: Player[];
  onPlayerHover?: (player: Player | null) => void;
  highlightedPlayerId?: string | null;
  onPlayerClick?: (player: Player) => void;
}

interface TacticalLineConfig {
  position: Position;
  title: string;
  icon: string;
  badgeBg: string;
  textColor: string;
  borderColor: string;
}

const TACTICAL_LINES: TacticalLineConfig[] = [
  {
    position: 'GK',
    title: 'Goalkeeper',
    icon: 'fa-solid fa-hand',
    badgeBg: 'bg-yellow-500/15',
    textColor: 'text-yellow-400',
    borderColor: 'border-yellow-500/30',
  },
  {
    position: 'DEF',
    title: 'Defenders',
    icon: 'fa-solid fa-shield-halved',
    badgeBg: 'bg-blue-500/15',
    textColor: 'text-blue-400',
    borderColor: 'border-blue-500/30',
  },
  {
    position: 'MID',
    title: 'Midfielders',
    icon: 'fa-solid fa-compass',
    badgeBg: 'bg-emerald-500/15',
    textColor: 'text-emerald-400',
    borderColor: 'border-emerald-500/30',
  },
  {
    position: 'FWD',
    title: 'Forwards',
    icon: 'fa-solid fa-bolt',
    badgeBg: 'bg-rose-500/15',
    textColor: 'text-rose-400',
    borderColor: 'border-rose-500/30',
  },
];

export const TeamRoster: React.FC<TeamRosterProps> = ({
  players,
  onPlayerHover,
  highlightedPlayerId,
  onPlayerClick,
}) => {
  if (players.length === 0) {
    return (
      <div className="bg-slate-800/80 rounded-2xl p-8 border border-slate-700/80 text-center backdrop-blur-sm">
        <div className="w-16 h-16 rounded-full bg-slate-700/40 text-slate-500 mx-auto flex items-center justify-center mb-4 text-2xl">
          <i className="fa-solid fa-users-slash" aria-hidden="true"></i>
        </div>
        <h3 className="text-lg font-bold text-white mb-2">No Players Drafted Yet</h3>
        <p className="text-slate-400 text-sm max-w-sm mx-auto">
          Start your draft journey to build your dream starting XI from legendary squads.
        </p>
      </div>
    );
  }

  return (
    <div className="space-y-5">
      {TACTICAL_LINES.map((line) => {
        const linePlayers = players.filter((p) => p.position === line.position);

        return (
          <section
            key={line.position}
            aria-labelledby={`roster-heading-${line.position}`}
            className="bg-slate-800/70 rounded-2xl p-4 md:p-5 border border-slate-700/60 shadow-md backdrop-blur-sm"
          >
            {/* Tactical Line Header */}
            <div className="flex items-center justify-between mb-3.5 pb-2 border-b border-slate-700/50">
              <div className="flex items-center gap-2.5">
                <span
                  className={`w-7 h-7 rounded-lg flex items-center justify-center text-xs ${line.badgeBg} ${line.textColor} border ${line.borderColor}`}
                  aria-hidden="true"
                >
                  <i className={line.icon}></i>
                </span>
                <h3
                  id={`roster-heading-${line.position}`}
                  className="font-bold text-sm md:text-base text-white tracking-wide flex items-center gap-2"
                >
                  {line.title}
                </h3>
              </div>
              <span
                className={`text-xs font-bold px-2.5 py-0.5 rounded-full ${line.badgeBg} ${line.textColor} border ${line.borderColor}`}
              >
                {linePlayers.length} {linePlayers.length === 1 ? 'Player' : 'Players'}
              </span>
            </div>

            {/* Players Grid or Empty Line */}
            {linePlayers.length > 0 ? (
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-2.5">
                {linePlayers.map((player) => (
                  <div
                    key={player.id}
                    onMouseEnter={() => onPlayerHover?.(player)}
                    onMouseLeave={() => onPlayerHover?.(null)}
                    className="transition-transform"
                  >
                    <PlayerCard
                      player={player}
                      onClick={onPlayerClick}
                      isHighlighted={highlightedPlayerId === player.id}
                    />
                  </div>
                ))}
              </div>
            ) : (
              <div className="py-4 text-center border border-dashed border-slate-700/60 rounded-xl bg-slate-900/20">
                <p className="text-xs text-slate-500 italic">No {line.title.toLowerCase()} selected yet</p>
              </div>
            )}
          </section>
        );
      })}
    </div>
  );
};

export default TeamRoster;
