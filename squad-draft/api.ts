import { Squad, DailyChallenge } from './types';

// @ts-ignore - Vite provides import.meta.env
const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5212';
// @ts-ignore
const FUNCTIONS_BASE_URL = import.meta.env.VITE_FUNCTIONS_BASE_URL || 'http://localhost:7172';

export const fetchDailySquads = async (date?: string): Promise<DailyChallenge | null> => {
    try {
        console.log('API_BASE_URL', API_BASE_URL);
        const targetDate = date || new Date().toISOString().split('T')[0];
        const response = await fetch(`${API_BASE_URL}/api/game-records/date/${targetDate}`);

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
                fixtureId: team.fixtureId,
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

export const fetchLeaderboard = async (gameRecordId: number): Promise<any[]> => {
    try {
        const response = await fetch(`${API_BASE_URL}/api/user-squads/${gameRecordId}/leaderboard`);
        if (!response.ok) {
            console.error('Failed to fetch leaderboard:', response.statusText);
            return [];
        }
        return await response.json();
    } catch (error) {
        console.error('Error fetching leaderboard:', error);
        return [];
    }
};

export const fetchFixture = async (fixtureId: number): Promise<any> => {
    try {
        const response = await fetch(`${API_BASE_URL}/api/fixtures/${fixtureId}`);
        if (!response.ok) {
            console.error('Failed to fetch fixture:', response.statusText);
            return null;
        }
        return await response.json();
    } catch (error) {
        console.error('Error fetching fixture:', error);
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

export const fetchStatistics = async (): Promise<any | null> => {
    try {
        const response = await fetch(`${API_BASE_URL}/api/statistics`);
        if (!response.ok) {
            console.error('Failed to fetch statistics:', response.statusText);
            return null;
        }
        return await response.json();
    } catch (error) {
        console.error('Error fetching statistics:', error);
        return null;
    }
};

export const recordRequest = async (): Promise<boolean> => {
    try {
        const response = await fetch(`${FUNCTIONS_BASE_URL}/api/record`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({
                dateTime: new Date().toISOString(),
                device: navigator.userAgent
            }),
        });

        if (!response.ok) {
            console.error('Failed to record request:', response.statusText);
            return false;
        }

        return true;
    } catch (error) {
        console.error('Error recording request:', error);
        return false;
    }
};

