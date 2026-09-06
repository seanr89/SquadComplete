
import React from 'react';
import { FormationSpot, Player } from '../types';
import PlayerCard from './PlayerCard';

interface PitchProps {
  formation: FormationSpot[];
  onSpotClick?: (spot: FormationSpot) => void;
  onPlayerClick?: (player: Player) => void;
  activeSpotId: number | null;
  selectedPlayerId?: string | null;
  disabledPlayerIds?: string[];
  isDroppable?: boolean;
  onSpotDrop?: (spotId: number) => void;
  isDraggable?: boolean;
  onPlayerDragStart?: (player: Player) => void;
}

const Pitch: React.FC<PitchProps> = ({ formation, onSpotClick, onPlayerClick, activeSpotId, selectedPlayerId, disabledPlayerIds = [], isDroppable, onSpotDrop, isDraggable, onPlayerDragStart }) => {
  const [dragOverSpotId, setDragOverSpotId] = React.useState<number | null>(null);


  return (
    <div
      role="region"
      aria-label="Tactical pitch formation"
      className="pitch-bg w-full aspect-[2/3] md:aspect-auto md:h-[600px] rounded-3xl relative overflow-hidden shadow-2xl border-4 border-slate-800"
    >
      {/* Pitch Markings */}
      <div className="pitch-lines m-4 border-2" aria-hidden="true"></div>
      <div className="absolute top-0 left-1/2 -translate-x-1/2 w-1/3 h-20 border-b-2 border-x-2 border-white/10" aria-hidden="true"></div>
      <div className="absolute bottom-0 left-1/2 -translate-x-1/2 w-1/3 h-20 border-t-2 border-x-2 border-white/10" aria-hidden="true"></div>
      <div className="pitch-center" aria-hidden="true"></div>
      <div className="absolute top-1/2 left-0 w-full h-[1px] bg-white/10" aria-hidden="true"></div>

      {/* Players */}
      {formation.map((spot) => (
        <div
          key={spot.id}
          style={{ top: spot.top, left: spot.left }}
          className="absolute -translate-x-1/2 -translate-y-1/2 flex flex-col items-center"
        >
          {spot.player ? (
            <PlayerCard
              player={spot.player}
              compact
              onClick={onPlayerClick}
              isSelected={selectedPlayerId === spot.player.id}
              disabled={disabledPlayerIds.includes(spot.player.id)}
              draggable={isDraggable}
              onDragStart={() => onPlayerDragStart?.(spot.player)}
            />
          ) : (
            <button
              type="button"
              onClick={() => onSpotClick?.(spot)}
              aria-label={`Empty ${spot.position} spot. Press Enter or Space to place player.`}
              onDragOver={(e) => {
                if (isDroppable) {
                  e.preventDefault();
                }
              }}
              onDragEnter={() => {
                if (isDroppable) {
                  setDragOverSpotId(spot.id);
                }
              }}
              onDragLeave={() => {
                if (isDroppable) {
                  setDragOverSpotId(null);
                }
              }}
              onDrop={(e) => {
                if (isDroppable) {
                  e.preventDefault();
                  setDragOverSpotId(null);
                  onSpotDrop?.(spot.id);
                }
              }}
              className={`w-12 h-12 md:w-16 md:h-16 rounded-full border-2 border-dashed flex items-center justify-center transition-all focus-visible:ring-2 focus-visible:ring-yellow-400 focus-visible:outline-none
                ${(activeSpotId === spot.id || dragOverSpotId === spot.id)
                  ? 'border-yellow-400 bg-yellow-400/20 scale-110 shadow-lg shadow-yellow-400/30 ring-2 ring-yellow-400/50'
                  : 'border-white/30 hover:border-white/60 hover:bg-white/10'
                }`}
            >
              <span className="text-[10px] md:text-xs font-black text-white/70">{spot.position}</span>
            </button>
          )}
        </div>
      ))}
    </div>
  );
};

export default Pitch;
