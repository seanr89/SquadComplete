
import React from 'react';
import { Player } from '../types';

interface PlayerCardProps {
  player: Player;
  onClick?: (player: Player) => void;
  isSelected?: boolean;
  compact?: boolean;
  disabled?: boolean;
  draggable?: boolean;
  onDragStart?: (e: React.DragEvent<HTMLDivElement>, player: Player) => void;
}

const PlayerCard: React.FC<PlayerCardProps> = ({ player, onClick, isSelected, compact, disabled, draggable, onDragStart }) => {
  const [imgError, setImgError] = React.useState(false);

  const getPositionColor = (pos: string) => {
    switch (pos) {
      case 'GK': return 'bg-yellow-500';
      case 'DEF': return 'bg-blue-500';
      case 'MID': return 'bg-green-500';
      case 'FWD': return 'bg-red-500';
      default: return 'bg-gray-500';
    }
  };

  const getInitials = (name: string) => {
    const parts = name.trim().split(/\s+/);
    if (parts.length === 1) return parts[0].substring(0, 2).toUpperCase();
    return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase();
  };

  const handleKeyDown = (e: React.KeyboardEvent<HTMLDivElement>) => {
    if (!disabled && (e.key === 'Enter' || e.key === ' ')) {
      e.preventDefault();
      onClick?.(player);
    }
  };

  if (compact) {
    return (
      <div
        role="button"
        tabIndex={disabled ? -1 : 0}
        aria-label={`${player.name}, ${player.position}`}
        aria-pressed={isSelected}
        aria-disabled={disabled}
        onClick={() => !disabled && onClick?.(player)}
        onKeyDown={handleKeyDown}
        draggable={draggable && !disabled}
        onDragStart={(e) => !disabled && onDragStart?.(e, player)}
        className={`relative flex flex-col items-center group cursor-pointer transition-transform hover:scale-110 focus-visible:ring-2 focus-visible:ring-yellow-400 focus-visible:outline-none rounded-full p-0.5 ${disabled ? 'opacity-50 grayscale' : ''}`}
      >
        <div className={`w-12 h-12 md:w-16 md:h-16 rounded-full border-2 overflow-hidden flex items-center justify-center bg-slate-800 ${isSelected ? 'border-yellow-400 shadow-lg shadow-yellow-400/50 ring-2 ring-yellow-400/50' : 'border-slate-400'}`}>
          {!imgError && player.image ? (
            <img
              src={player.image}
              alt=""
              onError={() => setImgError(true)}
              className="w-full h-full object-cover"
            />
          ) : (
            <span className="text-xs md:text-sm font-black text-slate-300" aria-hidden="true">
              {getInitials(player.name)}
            </span>
          )}
        </div>
        <div className={`mt-1 px-2 py-0.5 rounded text-[10px] md:text-xs font-bold text-white shadow-sm ${getPositionColor(player.position)}`}>
          {player.name.split(' ').pop()}
        </div>
      </div>
    );
  }

  const ariaLabel = `${player.name}, Position: ${player.position}, Rating: ${player.rating} Overall, Club: ${player.club}${disabled ? ', already drafted' : isSelected ? ', currently selected' : ', press Enter to select'}`;

  return (
    <div
      role="button"
      tabIndex={disabled ? -1 : 0}
      aria-label={ariaLabel}
      aria-pressed={isSelected}
      aria-disabled={disabled}
      onClick={() => !disabled && onClick?.(player)}
      onKeyDown={handleKeyDown}
      draggable={draggable && !disabled}
      onDragStart={(e) => !disabled && onDragStart?.(e, player)}
      className={`relative w-full p-4 rounded-xl border transition-all cursor-pointer overflow-hidden focus-visible:ring-2 focus-visible:ring-yellow-400 focus-visible:outline-none
        ${isSelected
          ? 'border-yellow-400 bg-slate-800/80 shadow-lg shadow-yellow-400/20 ring-1 ring-yellow-400/40'
          : 'border-slate-700 bg-slate-800/50 hover:border-slate-600 hover:bg-slate-800'
        } ${disabled ? 'opacity-40 grayscale pointer-events-none' : ''}`}
    >
      <div className="flex items-center gap-4">
        {/* Profile Image with white backing or initials fallback */}
        <div className="w-14 h-14 md:w-16 md:h-16 rounded-xl bg-white p-0.5 flex-shrink-0 overflow-hidden flex items-center justify-center">
          {!imgError && player.image ? (
            <img
              src={player.image}
              alt=""
              onError={() => setImgError(true)}
              className="w-full h-full rounded-lg object-cover"
            />
          ) : (
            <div className="w-full h-full rounded-lg bg-slate-800 flex items-center justify-center text-slate-200 font-bold text-sm" aria-hidden="true">
              {getInitials(player.name)}
            </div>
          )}
        </div>

        {/* Text Container */}
        <div className="flex-1 min-w-0 flex flex-col justify-center">
          <div className="flex justify-between items-center gap-2">
            <h3 className="font-bold text-base md:text-lg text-white truncate leading-tight">{player.name}</h3>
            <span className="font-black text-[10px] md:text-xs px-1.5 py-0.5 rounded tracking-wide bg-yellow-500/20 text-yellow-400 border border-yellow-500/30 whitespace-nowrap">
              {player.rating} OVR
            </span>
          </div>
          <div className="flex items-center gap-2 mt-1 md:mt-1.5">
            <span className="font-bold text-[10px] md:text-xs px-1.5 py-0.5 rounded tracking-wide bg-slate-700 text-slate-200 uppercase leading-none">
              {player.position}
            </span>
            <span className="text-slate-300 text-xs md:text-sm truncate">
              {player.club}
            </span>
          </div>
        </div>

        {/* Selected state indicator */}
        {isSelected && (
          <div className="flex items-center justify-center w-6 h-6 md:w-8 md:h-8 rounded-full bg-yellow-400 text-slate-900 border-2 border-slate-800" aria-hidden="true">
            <i className="fas fa-check text-xs md:text-sm"></i>
          </div>
        )}
      </div>
    </div>
  );
};

export default PlayerCard;
