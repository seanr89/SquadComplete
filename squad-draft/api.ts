import { Squad, DailyChallenge } from './types';

// @ts-ignore - Vite provides import.meta.env
const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5212';

export const fetchDailySquads = async (): Promise<DailyChallenge | null> => {
    try {
        console.log('API_BASE_URL', API_BASE_URL);
        const today = new Date().toISOString().split('T')[0];
        const response = await fetch(`${API_BASE_URL}/api/game-records/date/${today}`);

        if (!response.ok) {
            console.error('Failed to fetch daily squads:', response.statusText);
            return null;
        }

        const data = await response.json();

        // Map GameRecordDto to Squad[]
        if (data && data.teams) {
            const squads = data.teams.map((team: any, index: number) => ({
                id: `s${index + 1}`, // Generate a unique ID since GameRecordTeamDto doesn't have one
                teamName: team.teamName,
                season: 'Current', // Fallback as Season is not in the DTO
                players: team.players.map((p: any) => ({
                    id: p.playerId.toString(),
                    name: p.playerName,
                    position: p.statistics?.position || 'UNK',
                    rating: p.statistics?.rating || 0,
                    nationality: 'Unknown', // Fallback
                    club: team.teamName,
                    image: p.playerPhoto || `https://ui-avatars.com/api/?name=${encodeURIComponent(p.playerName)}&background=random`
                }))
            }));
            
            return {
                id: data.id,
                squads,
                formation: data.formation || null
            };
        }

        return null;
    } catch (error) {
        console.error('Error fetching daily squads:', error);
        return null;
    }
};

export const submitUserSquad = async (payload: any): Promise<boolean> => {
    try {
        const response = await fetch(`${API_BASE_URL}/api/user-squads`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(payload),
        });

        if (!response.ok) {
            console.error('Failed to submit user squad:', response.statusText);
            return false;
        }

        return true;
    } catch (error) {
        console.error('Error submitting user squad:', error);
        return false;
    }
};

export const submitFeedback = async (name: string, email: string, message: string): Promise<boolean> => {
    try {
        const response = await fetch(`${API_BASE_URL}/api/feedbacks`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({ name, email, message }),
        });

        if (!response.ok) {
            console.error('Failed to submit feedback:', response.statusText);
            return false;
        }

        return true;
    } catch (error) {
        console.error('Error submitting feedback:', error);
        return false;
    }
};
