
import { Squad, FormationSpot } from './types';

export const SQUADS: Squad[] = [
  {
    id: 's1',
    teamName: 'Real Madrid',
    season: '2016/17',
    players: [
      { id: 'rm1', name: 'C. Ronaldo', position: 'FWD', rating: 94, nationality: 'Portugal', club: 'Real Madrid', image: 'https://picsum.photos/seed/cr7/200' },
      { id: 'rm2', name: 'L. Modric', position: 'MID', rating: 89, nationality: 'Croatia', club: 'Real Madrid', image: 'https://picsum.photos/seed/modric/200' },
      { id: 'rm3', name: 'Sergio Ramos', position: 'DEF', rating: 90, nationality: 'Spain', club: 'Real Madrid', image: 'https://picsum.photos/seed/ramos/200' },
      { id: 'rm4', name: 'K. Navas', position: 'GK', rating: 85, nationality: 'Costa Rica', club: 'Real Madrid', image: 'https://picsum.photos/seed/navas/200' },
    ]
  },
  {
    id: 's2',
    teamName: 'FC Barcelona',
    season: '2010/11',
    players: [
      { id: 'bar1', name: 'Lionel Messi', position: 'FWD', rating: 94, nationality: 'Argentina', club: 'Barcelona', image: 'https://picsum.photos/seed/messi/200' },
      { id: 'bar2', name: 'Xavi', position: 'MID', rating: 91, nationality: 'Spain', club: 'Barcelona', image: 'https://picsum.photos/seed/xavi/200' },
      { id: 'bar3', name: 'Carles Puyol', position: 'DEF', rating: 88, nationality: 'Spain', club: 'Barcelona', image: 'https://picsum.photos/seed/puyol/200' },
      { id: 'bar4', name: 'V. Valdes', position: 'GK', rating: 85, nationality: 'Spain', club: 'Barcelona', image: 'https://picsum.photos/seed/valdes/200' },
    ]
  },
  {
    id: 's3',
    teamName: 'Manchester City',
    season: '2022/23',
    players: [
      { id: 'mc1', name: 'E. Haaland', position: 'FWD', rating: 91, nationality: 'Norway', club: 'Man City', image: 'https://picsum.photos/seed/haaland/200' },
      { id: 'mc2', name: 'K. De Bruyne', position: 'MID', rating: 91, nationality: 'Belgium', club: 'Man City', image: 'https://picsum.photos/seed/kdb/200' },
      { id: 'mc3', name: 'Ruben Dias', position: 'DEF', rating: 88, nationality: 'Portugal', club: 'Man City', image: 'https://picsum.photos/seed/dias/200' },
      { id: 'mc4', name: 'Ederson', position: 'GK', rating: 89, nationality: 'Brazil', club: 'Man City', image: 'https://picsum.photos/seed/ederson/200' },
    ]
  },
  {
    id: 's4',
    teamName: 'Bayern Munich',
    season: '2019/20',
    players: [
      { id: 'bm1', name: 'R. Lewandowski', position: 'FWD', rating: 91, nationality: 'Poland', club: 'Bayern', image: 'https://picsum.photos/seed/lewa/200' },
      { id: 'bm2', name: 'T. Muller', position: 'MID', rating: 86, nationality: 'Germany', club: 'Bayern', image: 'https://picsum.photos/seed/muller/200' },
      { id: 'bm3', name: 'A. Davies', position: 'DEF', rating: 82, nationality: 'Canada', club: 'Bayern', image: 'https://picsum.photos/seed/davies/200' },
      { id: 'bm4', name: 'M. Neuer', position: 'GK', rating: 89, nationality: 'Germany', club: 'Bayern', image: 'https://picsum.photos/seed/neuer/200' },
    ]
  },
  {
    id: 's5',
    teamName: 'Liverpool',
    season: '2018/19',
    players: [
      { id: 'liv1', name: 'M. Salah', position: 'FWD', rating: 90, nationality: 'Egypt', club: 'Liverpool', image: 'https://picsum.photos/seed/salah/200' },
      { id: 'liv2', name: 'Fabinho', position: 'MID', rating: 85, nationality: 'Brazil', club: 'Liverpool', image: 'https://picsum.photos/seed/fab/200' },
      { id: 'liv3', name: 'V. Van Dijk', position: 'DEF', rating: 90, nationality: 'Netherlands', club: 'Liverpool', image: 'https://picsum.photos/seed/vvd/200' },
      { id: 'liv4', name: 'Alisson', position: 'GK', rating: 89, nationality: 'Brazil', club: 'Liverpool', image: 'https://picsum.photos/seed/alisson/200' },
    ]
  },
  {
    id: 's6',
    teamName: 'Arsenal',
    season: '2003/04',
    players: [
      { id: 'ars1', name: 'T. Henry', position: 'FWD', rating: 93, nationality: 'France', club: 'Arsenal', image: 'https://picsum.photos/seed/henry/200' },
      { id: 'ars2', name: 'P. Vieira', position: 'MID', rating: 90, nationality: 'France', club: 'Arsenal', image: 'https://picsum.photos/seed/vieira/200' },
      { id: 'ars3', name: 'Sol Campbell', position: 'DEF', rating: 88, nationality: 'England', club: 'Arsenal', image: 'https://picsum.photos/seed/campbell/200' },
      { id: 'ars4', name: 'J. Lehmann', position: 'GK', rating: 84, nationality: 'Germany', club: 'Arsenal', image: 'https://picsum.photos/seed/lehmann/200' },
    ]
  },
  {
    id: 's7',
    teamName: 'AC Milan',
    season: '2006/07',
    players: [
      { id: 'acm1', name: 'Kaka', position: 'MID', rating: 91, nationality: 'Brazil', club: 'AC Milan', image: 'https://picsum.photos/seed/kaka/200' },
      { id: 'acm2', name: 'F. Inzaghi', position: 'FWD', rating: 85, nationality: 'Italy', club: 'AC Milan', image: 'https://picsum.photos/seed/inzaghi/200' },
      { id: 'acm3', name: 'P. Maldini', position: 'DEF', rating: 92, nationality: 'Italy', club: 'AC Milan', image: 'https://picsum.photos/seed/maldini/200' },
      { id: 'acm4', name: 'Dida', position: 'GK', rating: 84, nationality: 'Brazil', club: 'AC Milan', image: 'https://picsum.photos/seed/dida/200' },
    ]
  },
  {
    id: 's8',
    teamName: 'Inter Milan',
    season: '2009/10',
    players: [
      { id: 'int1', name: 'S. Eto\'o', position: 'FWD', rating: 89, nationality: 'Cameroon', club: 'Inter', image: 'https://picsum.photos/seed/etoo/200' },
      { id: 'int2', name: 'W. Sneijder', position: 'MID', rating: 90, nationality: 'Netherlands', club: 'Inter', image: 'https://picsum.photos/seed/sneijder/200' },
      { id: 'int3', name: 'Lúcio', position: 'DEF', rating: 87, nationality: 'Brazil', club: 'Inter', image: 'https://picsum.photos/seed/lucio/200' },
      { id: 'int4', name: 'J. Cesar', position: 'GK', rating: 88, nationality: 'Brazil', club: 'Inter', image: 'https://picsum.photos/seed/cesar/200' },
    ]
  },
  {
    id: 's9',
    teamName: 'Chelsea',
    season: '2011/12',
    players: [
      { id: 'che1', name: 'D. Drogba', position: 'FWD', rating: 90, nationality: 'Ivory Coast', club: 'Chelsea', image: 'https://picsum.photos/seed/drogba/200' },
      { id: 'che2', name: 'F. Lampard', position: 'MID', rating: 89, nationality: 'England', club: 'Chelsea', image: 'https://picsum.photos/seed/lampard/200' },
      { id: 'che3', name: 'J. Terry', position: 'DEF', rating: 88, nationality: 'England', club: 'Chelsea', image: 'https://picsum.photos/seed/terry/200' },
      { id: 'che4', name: 'P. Cech', position: 'GK', rating: 89, nationality: 'Czech Rep', club: 'Chelsea', image: 'https://picsum.photos/seed/cech/200' },
    ]
  },
  {
    id: 's10',
    teamName: 'PSG',
    season: '2021/22',
    players: [
      { id: 'psg1', name: 'K. Mbappe', position: 'FWD', rating: 91, nationality: 'France', club: 'PSG', image: 'https://picsum.photos/seed/mbappe/200' },
      { id: 'psg2', name: 'Neymar Jr', position: 'FWD', rating: 91, nationality: 'Brazil', club: 'PSG', image: 'https://picsum.photos/seed/neymar/200' },
      { id: 'psg3', name: 'Marquinhos', position: 'DEF', rating: 87, nationality: 'Brazil', club: 'PSG', image: 'https://picsum.photos/seed/marq/200' },
      { id: 'psg4', name: 'G. Donnarumma', position: 'GK', rating: 88, nationality: 'Italy', club: 'PSG', image: 'https://picsum.photos/seed/donna/200' },
    ]
  },
  {
    id: 's11',
    teamName: 'Juventus',
    season: '2014/15',
    players: [
      { id: 'juv1', name: 'C. Tevez', position: 'FWD', rating: 87, nationality: 'Argentina', club: 'Juventus', image: 'https://picsum.photos/seed/tevez/200' },
      { id: 'juv2', name: 'A. Pirlo', position: 'MID', rating: 88, nationality: 'Italy', club: 'Juventus', image: 'https://picsum.photos/seed/pirlo/200' },
      { id: 'juv3', name: 'G. Chiellini', position: 'DEF', rating: 88, nationality: 'Italy', club: 'Juventus', image: 'https://picsum.photos/seed/chiellini/200' },
      { id: 'juv4', name: 'G. Buffon', position: 'GK', rating: 90, nationality: 'Italy', club: 'Juventus', image: 'https://picsum.photos/seed/buffon/200' },
    ]
  }
];

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
