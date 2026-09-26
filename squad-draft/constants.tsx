
import { Squad, FormationSpot } from './types';

export const INITIAL_FORMATION: FormationSpot[] = [
  { id: 1, position: 'GK', top: '86%', left: '50%', player: null },
  { id: 2, position: 'DEF', top: '68%', left: '18%', player: null },
  { id: 3, position: 'DEF', top: '68%', left: '39%', player: null },
  { id: 4, position: 'DEF', top: '68%', left: '61%', player: null },
  { id: 5, position: 'DEF', top: '68%', left: '82%', player: null },
  { id: 6, position: 'MID', top: '41%', left: '22%', player: null },
  { id: 7, position: 'MID', top: '48%', left: '50%', player: null },
  { id: 8, position: 'MID', top: '41%', left: '78%', player: null },
  { id: 9, position: 'FWD', top: '18%', left: '22%', player: null },
  { id: 10, position: 'FWD', top: '11%', left: '50%', player: null },
  { id: 11, position: 'FWD', top: '18%', left: '78%', player: null },
];

export const generateFormationSpots = (defence: number, midfield: number, attack: number): FormationSpot[] => {
  const formation: FormationSpot[] = [];
  let idCounter = 1;

  const rowConfigs: { pos: import('./types').Position; defaultTop: string; count: number }[] = [
    { pos: 'GK', defaultTop: '86%', count: 1 },
    { pos: 'DEF', defaultTop: '68%', count: defence },
    { pos: 'MID', defaultTop: '43%', count: midfield },
    { pos: 'FWD', defaultTop: '16%', count: attack }
  ];

  rowConfigs.forEach(({ pos, defaultTop, count }) => {
    for (let i = 0; i < count; i++) {
      let left = '50%';
      let spotTop = defaultTop;

      if (pos === 'GK') {
        left = '50%';
        spotTop = '86%';
      } else if (pos === 'DEF') {
        spotTop = '68%';
        if (count === 3) {
          left = i === 0 ? '24%' : i === 1 ? '50%' : '76%';
        } else if (count === 4) {
          left = (18 + i * 21.3) + '%';
        } else if (count === 5) {
          left = (14 + (i * 72) / 4) + '%';
          if (i === 0 || i === 4) spotTop = '63%'; // Wingbacks slightly advanced
        } else {
          left = (15 + (i * 70) / Math.max(count - 1, 1)) + '%';
        }
      } else if (pos === 'MID') {
        // Midfield spacing: wider horizontal span and natural tactical staggering
        if (count === 1) {
          left = '50%';
          spotTop = '45%';
        } else if (count === 2) {
          left = i === 0 ? '32%' : '68%';
          spotTop = '44%';
        } else if (count === 3) {
          // Tactical trio: LCM (22%, 41%), CDM/Holding (50%, 48%), RCM (78%, 41%)
          if (i === 0) {
            left = '22%';
            spotTop = '41%';
          } else if (i === 1) {
            left = '50%';
            spotTop = '48%';
          } else {
            left = '78%';
            spotTop = '41%';
          }
        } else if (count === 4) {
          // 4-midfield: LM (16%, 39%), LCM (38%, 46%), RCM (62%, 46%), RM (84%, 39%)
          if (i === 0) {
            left = '16%';
            spotTop = '39%';
          } else if (i === 1) {
            left = '38%';
            spotTop = '46%';
          } else if (i === 2) {
            left = '62%';
            spotTop = '46%';
          } else {
            left = '84%';
            spotTop = '39%';
          }
        } else if (count === 5) {
          // 5-midfield: LM (15%, 38%), LCM (32%, 44%), CM (50%, 48%), RCM (68%, 44%), RM (85%, 38%)
          const midTops = ['38%', '44%', '48%', '44%', '38%'];
          const midLefts = ['15%', '32%', '50%', '68%', '85%'];
          left = midLefts[i];
          spotTop = midTops[i];
        } else {
          left = (15 + (i * 70) / (count - 1)) + '%';
          spotTop = i % 2 === 0 ? '40%' : '47%';
        }
      } else if (pos === 'FWD') {
        if (count === 1) {
          left = '50%';
          spotTop = '13%';
        } else if (count === 2) {
          left = i === 0 ? '34%' : '66%';
          spotTop = '15%';
        } else if (count === 3) {
          if (i === 0) {
            left = '22%';
            spotTop = '18%';
          } else if (i === 1) {
            left = '50%';
            spotTop = '11%';
          } else {
            left = '78%';
            spotTop = '18%';
          }
        } else {
          left = (18 + (i * 64) / (count - 1)) + '%';
          spotTop = i % 2 === 1 ? '11%' : '17%';
        }
      }

      formation.push({
        id: idCounter++,
        position: pos,
        top: spotTop,
        left,
        player: null
      });
    }
  });

  return formation;
};

/**
 * Carries placed players from a saved formation onto freshly generated spots.
 * Spot ids are a running counter across lines (GK, DEF, MID, FWD), so the same id can belong to a
 * different line once the formation shape changes. Players are therefore matched by line and slot
 * number within that line (e.g. "2nd DEF"), never by raw id.
 * Returns null if any placed player's slot doesn't exist in the new shape: the caller should then
 * keep the saved layout rather than silently moving the player to a different line.
 */
export const remapFormationPlayers = (savedSpots: FormationSpot[], newSpots: FormationSpot[]): FormationSpot[] | null => {
  const slotKey = (spots: FormationSpot[], spot: FormationSpot) =>
    `${spot.position}:${spots.filter(s => s.position === spot.position).indexOf(spot)}`;

  const playersBySlot = new Map(
    savedSpots.filter(s => s.player).map(s => [slotKey(savedSpots, s), s.player] as const)
  );

  const remapped = newSpots.map(spot => {
    const key = slotKey(newSpots, spot);
    const player = playersBySlot.get(key) ?? null;
    playersBySlot.delete(key);
    return { ...spot, player };
  });

  return playersBySlot.size === 0 ? remapped : null;
};
