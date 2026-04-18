
import { Squad, FormationSpot } from './types';

export const INITIAL_FORMATION: FormationSpot[] = [
  { id: 1, position: 'GK', top: '85%', left: '50%', player: null },
  { id: 2, position: 'DEF', top: '65%', left: '20%', player: null },
  { id: 3, position: 'DEF', top: '65%', left: '40%', player: null },
  { id: 4, position: 'DEF', top: '65%', left: '60%', player: null },
  { id: 5, position: 'DEF', top: '65%', left: '80%', player: null },
  { id: 6, position: 'MID', top: '40%', left: '30%', player: null },
  { id: 7, position: 'MID', top: '40%', left: '50%', player: null },
  { id: 8, position: 'MID', top: '40%', left: '70%', player: null },
  { id: 9, position: 'FWD', top: '15%', left: '25%', player: null },
  { id: 10, position: 'FWD', top: '10%', left: '50%', player: null },
  { id: 11, position: 'FWD', top: '15%', left: '75%', player: null },
];

export const generateFormationSpots = (defence: number, midfield: number, attack: number): FormationSpot[] => {
  const formation: FormationSpot[] = [];
  let idCounter = 1;

  const rowConfigs: { pos: import('./types').Position; top: string; count: number }[] = [
    { pos: 'GK', top: '85%', count: 1 },
    { pos: 'DEF', top: '65%', count: defence },
    { pos: 'MID', top: '40%', count: midfield },
    { pos: 'FWD', top: '15%', count: attack }
  ];

  rowConfigs.forEach(({ pos, top, count }) => {
    for (let i = 0; i < count; i++) {
      let left = '50%';
      let spotTop = top;

      if (count === 1) {
        left = '50%';
      } else if (count === 2) {
        left = i === 0 ? '35%' : '65%';
      } else if (count === 3) {
        left = i === 0 ? '25%' : (i === 1 ? '50%' : '75%');
        if (pos === 'FWD' && i === 1) spotTop = '10%'; // central forward slightly up
      } else if (count === 4) {
        left = (20 + i * 20) + '%';
      } else if (count >= 5) {
        left = (15 + (i * 70) / (count - 1)) + '%';
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
