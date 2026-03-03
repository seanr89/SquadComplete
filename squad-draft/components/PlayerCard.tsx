
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
      className={`relative w-full p-4 rounded-xl border transition-all cursor-pointer overflow-hidden
        ${isSelected
          ? 'border-yellow-400 bg-slate-800/80 shadow-lg shadow-yellow-400/20'
          : 'border-slate-700 bg-slate-800/50 hover:border-slate-600 hover:bg-slate-800'
        } ${disabled ? 'opacity-40 grayscale pointer-events-none' : ''}`}
    >
      <div className="flex items-center gap-4">
        {/* Profile Image with white backing, no rating */}
        <div className="w-14 h-14 md:w-16 md:h-16 rounded-xl bg-white p-0.5 flex-shrink-0 overflow-hidden">
          <img src={player.image} alt={player.name} className="w-full h-full rounded-lg object-cover" />
        </div>

        {/* Text Container */}
        <div className="flex-1 min-w-0 flex flex-col justify-center">
          <h3 className="font-bold text-base md:text-lg text-white truncate leading-tight">{player.name}</h3>
          <div className="flex items-center gap-2 mt-1 md:mt-1.5">
            <span className="font-bold text-[10px] md:text-xs px-1.5 py-0.5 rounded tracking-wide bg-slate-700 text-slate-300 uppercase leading-none">
              {player.position}
            </span>
            <span className="text-slate-400 text-xs md:text-sm truncate">
              {player.club}
            </span>
          </div>
        </div>

        {/* Selected state indicator */}
        {isSelected && (
          <div className="flex items-center justify-center w-6 h-6 md:w-8 md:h-8 rounded-full bg-yellow-400 text-slate-900 border-2 border-slate-800">
            <i className="fas fa-check text-xs md:text-sm"></i>
          </div>
        )}
      </div>
    </div>
  );
};

export default PlayerCard;
