
import React from 'react';
import { Player } from '../types';

interface PlayerCardProps {
  player: Player;
  onClick?: (player: Player) => void;
  isSelected?: boolean;
  isHighlighted?: boolean;
  compact?: boolean;
  disabled?: boolean;
  draggable?: boolean;
  onDragStart?: (e: React.DragEvent<HTMLDivElement>, player: Player) => void;
}

const PlayerCard: React.FC<PlayerCardProps> = ({
  player,
  onClick,
  isSelected,
  isHighlighted,
  compact,
  disabled,
  draggable,
  onDragStart,
}) => {
  const [imgError, setImgError] = React.useState(false);

  const getPositionClasses = (pos: string) => {
    switch (pos) {
      case 'GK':
        return {
          pill: 'bg-yellow-500/20 text-yellow-400 border-yellow-500/30',
          avatarBorder: 'border-yellow-500/70',
          badgeBg: 'bg-yellow-500',
        };
      case 'DEF':
        return {
          pill: 'bg-blue-500/20 text-blue-400 border-blue-500/30',
          avatarBorder: 'border-blue-500/70',
          badgeBg: 'bg-blue-500',
        };
      case 'MID':
        return {
          pill: 'bg-emerald-500/20 text-emerald-400 border-emerald-500/30',
          avatarBorder: 'border-emerald-500/70',
          badgeBg: 'bg-emerald-500',
        };
      case 'FWD':
        return {
          pill: 'bg-rose-500/20 text-rose-400 border-rose-500/30',
          avatarBorder: 'border-rose-500/70',
          badgeBg: 'bg-red-500',
        };
      default:
        return {
          pill: 'bg-slate-700 text-slate-300 border-slate-600',
          avatarBorder: 'border-slate-500',
          badgeBg: 'bg-gray-500',
        };
    }
  };

  const posStyles = getPositionClasses(player.position);

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
        className={`relative flex flex-col items-center group cursor-pointer transition-transform hover:scale-110 focus-visible:ring-2 focus-visible:ring-yellow-400 focus-visible:outline-none rounded-full p-0.5 ${
          disabled ? 'opacity-50 grayscale' : ''
        } ${isHighlighted ? 'scale-110' : ''}`}
      >
        <div
          className={`w-12 h-12 md:w-16 md:h-16 rounded-full border-2 overflow-hidden flex items-center justify-center bg-slate-900 ${
            isSelected || isHighlighted
              ? 'border-yellow-400 shadow-lg shadow-yellow-400/50 ring-2 ring-yellow-400/50'
              : `${posStyles.avatarBorder} shadow-md`
          }`}
        >
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
        <div
          className={`mt-1 px-2 py-0.5 rounded text-[10px] md:text-xs font-bold text-white shadow-sm ${posStyles.badgeBg}`}
        >
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
      className={`relative w-full p-3 md:p-3.5 rounded-xl border transition-all cursor-pointer overflow-hidden focus-visible:ring-2 focus-visible:ring-yellow-400 focus-visible:outline-none group
        ${isSelected || isHighlighted
          ? 'border-yellow-400 bg-slate-800/90 shadow-lg shadow-yellow-400/20 ring-1 ring-yellow-400/40'
          : 'border-slate-700/70 bg-slate-800/60 hover:border-slate-500/80 hover:bg-slate-800/90 shadow-sm'
        } ${disabled ? 'opacity-40 grayscale pointer-events-none' : ''}`}
    >
      <div className="flex items-center gap-3">
        {/* Modern Circular Avatar with position-accented border */}
        <div
          className={`w-11 h-11 md:w-12 md:h-12 rounded-full bg-slate-900 border-2 ${posStyles.avatarBorder} p-0.5 flex-shrink-0 overflow-hidden flex items-center justify-center shadow-inner`}
        >
          {!imgError && player.image ? (
            <img
              src={player.image}
              alt=""
              onError={() => setImgError(true)}
              className="w-full h-full rounded-full object-cover"
            />
          ) : (
            <div
              className="w-full h-full rounded-full bg-slate-800 flex items-center justify-center text-slate-200 font-black text-xs"
              aria-hidden="true"
            >
              {getInitials(player.name)}
            </div>
          )}
        </div>

        {/* Text Details */}
        <div className="flex-1 min-w-0 flex flex-col justify-center">
          <div className="flex items-center justify-between gap-2">
            <h4 className="font-bold text-sm md:text-base text-white group-hover:text-yellow-400 transition-colors truncate leading-tight">
              {player.name}
            </h4>
            <div className="flex items-center gap-1.5 flex-shrink-0">
              <span className="font-black text-[11px] md:text-xs px-2 py-0.5 rounded-lg bg-gradient-to-r from-amber-500/15 to-yellow-500/15 text-yellow-400 border border-amber-500/30 shadow-sm whitespace-nowrap">
                {player.rating} <span className="text-[9px] text-yellow-500/70">OVR</span>
              </span>
            </div>
          </div>
          <div className="flex items-center gap-2 mt-1">
            <span
              className={`font-black text-[9px] md:text-[10px] px-1.5 py-0.5 rounded border uppercase tracking-wider leading-none ${posStyles.pill}`}
            >
              {player.position}
            </span>
            <span className="text-slate-300 text-xs truncate">
              {player.club}
            </span>
          </div>
        </div>

        {/* Selected checkmark indicator */}
        {isSelected && (
          <div
            className="flex items-center justify-center w-5 h-5 rounded-full bg-yellow-400 text-slate-900 flex-shrink-0 shadow-sm"
            aria-hidden="true"
          >
            <i className="fas fa-check text-[10px]"></i>
          </div>
        )}
      </div>
    </div>
  );
};

export default PlayerCard;
