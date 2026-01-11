
import React from 'react';
import { FormationSpot, Player } from '../types';
import PlayerCard from './PlayerCard';

interface PitchProps {
  formation: FormationSpot[];
  onSpotClick?: (spot: FormationSpot) => void;
  activeSpotId: number | null;
}

const Pitch: React.FC<PitchProps> = ({ formation, onSpotClick, activeSpotId }) => {
  return (
    <div className="pitch-bg w-full aspect-[2/3] md:aspect-auto md:h-[600px] rounded-3xl relative overflow-hidden shadow-2xl border-4 border-slate-800">
      {/* Pitch Markings */}
      <div className="pitch-lines m-4 border-2"></div>
      <div className="absolute top-0 left-1/2 -translate-x-1/2 w-1/3 h-20 border-b-2 border-x-2 border-white/10"></div>
      <div className="absolute bottom-0 left-1/2 -translate-x-1/2 w-1/3 h-20 border-t-2 border-x-2 border-white/10"></div>
      <div className="pitch-center"></div>
      <div className="absolute top-1/2 left-0 w-full h-[1px] bg-white/10"></div>

      {/* Players */}
      {formation.map((spot) => (
        <div 
          key={spot.id}
          style={{ top: spot.top, left: spot.left }}
          className="absolute -translate-x-1/2 -translate-y-1/2 flex flex-col items-center"
        >
          {spot.player ? (
            <PlayerCard player={spot.player} compact />
          ) : (
            <button
              onClick={() => onSpotClick?.(spot)}
              className={`w-12 h-12 md:w-16 md:h-16 rounded-full border-2 border-dashed flex items-center justify-center transition-all
                ${activeSpotId === spot.id 
                  ? 'border-yellow-400 bg-yellow-400/20 scale-110 shadow-lg shadow-yellow-400/30' 
                  : 'border-white/20 hover:border-white/40 hover:bg-white/5'
                }`}
            >
              <span className="text-[10px] md:text-xs font-bold text-white/50">{spot.position}</span>
            </button>
          )}
        </div>
      ))}
    </div>
  );
};

export default Pitch;
