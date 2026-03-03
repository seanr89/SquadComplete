
import React from 'react';
import { Player } from '../types';

interface PlayerCardProps {
  player: Player;
  onClick?: (player: Player) => void;
  isSelected?: boolean;
  compact?: boolean;
  disabled?: boolean;
}

const PlayerCard: React.FC<PlayerCardProps> = ({ player, onClick, isSelected, compact, disabled }) => {
  const getPositionColor = (pos: string) => {
    switch (pos) {
      case 'GK': return 'bg-yellow-500';
      case 'DEF': return 'bg-blue-500';
      case 'MID': return 'bg-green-500';
      case 'FWD': return 'bg-red-500';
      default: return 'bg-gray-500';
    }
  };

  if (compact) {
    return (
      <div
        onClick={() => !disabled && onClick?.(player)}
        className={`relative flex flex-col items-center group cursor-pointer transition-transform hover:scale-110 ${disabled ? 'opacity-50 grayscale' : ''}`}
      >
        <div className={`w-12 h-12 md:w-16 md:h-16 rounded-full border-2 overflow-hidden ${isSelected ? 'border-yellow-400 shadow-lg shadow-yellow-400/50' : 'border-slate-400'}`}>
          <img src={player.image} alt={player.name} className="w-full h-full object-cover" />
        </div>
        <div className={`mt-1 px-2 py-0.5 rounded text-[10px] md:text-xs font-bold text-white shadow-sm ${getPositionColor(player.position)}`}>
          {player.name.split(' ').pop()}
        </div>
      </div>
    );
  }

  return (
    <div
      onClick={() => !disabled && onClick?.(player)}
      className={`relative w-full p-4 rounded-xl border-2 transition-all cursor-pointer overflow-hidden
        ${isSelected
          ? 'border-yellow-400 bg-yellow-400/10 shadow-lg shadow-yellow-400/20'
          : 'border-slate-700 bg-slate-800/50 hover:border-slate-500 hover:bg-slate-800'
        } ${disabled ? 'opacity-40 grayscale pointer-events-none' : ''}`}
    >
      <div className="flex items-center gap-4">
        <div className="relative">
          <img src={player.image} alt={player.name} className="w-16 h-16 rounded-lg object-cover border border-slate-600" />
          <div className={`absolute -top-2 -left-2 w-8 h-8 rounded-full flex items-center justify-center text-xs font-bold text-white shadow-md ${getPositionColor(player.position)}`}>
            {player.rating}
          </div>
        </div>
        <div className="flex-1 min-w-0">
          <h3 className="font-bold text-lg text-white truncate">{player.name}</h3>
          <p className="text-slate-400 text-sm flex items-center gap-2">
            <span className="font-semibold text-xs px-1.5 py-0.5 rounded bg-slate-700 text-slate-300">{player.position}</span>
            <span className="truncate">{player.club} • {player.nationality}</span>
          </p>
        </div>
        {isSelected && (
          <div className="flex items-center justify-center w-8 h-8 rounded-full bg-yellow-400 text-slate-900">
            <i className="fas fa-check"></i>
          </div>
        )}
      </div>
    </div>
  );
};

export default PlayerCard;
