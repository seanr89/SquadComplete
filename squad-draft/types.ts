
export type Position = 'GK' | 'DEF' | 'MID' | 'FWD';

export interface Player {
  id: string;
  name: string;
  position: Position;
  rating: number;
  nationality: string;
  club: string;
  image: string;
}

export interface Squad {
  id: string;
  teamName: string;
  season: string;
  players: Player[];
}

export interface FormationSpot {
  id: number;
  position: Position;
  top: string;
  left: string;
  player: Player | null;
}

export interface DraftState {
  currentStep: number;
  selectedPlayers: Player[];
  formation: FormationSpot[];
  completed: boolean;
}
