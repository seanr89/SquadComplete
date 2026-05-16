-- Create teams table
CREATE TABLE IF NOT EXISTS teams (
    id INTEGER PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    logo TEXT,
    last_update TIMESTAMP WITH TIME ZONE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- Create players table
CREATE TABLE IF NOT EXISTS players (
    id SERIAL PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    photo TEXT,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- Create player_fixture_statistics table to link players to fixtures and teams with performance data
CREATE TABLE IF NOT EXISTS player_fixture_statistics (
    fixture_id INTEGER REFERENCES fixtures(id),
    team_id INTEGER REFERENCES teams(id),
    player_id INTEGER REFERENCES players(id),
    minutes INTEGER,
    number INTEGER,
    position VARCHAR(50),
    rating NUMERIC(3,1),
    is_captain BOOLEAN DEFAULT FALSE,
    is_substitute BOOLEAN DEFAULT FALSE,
    PRIMARY KEY (fixture_id, player_id),
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- Indices for faster lookups
CREATE INDEX IF NOT EXISTS idx_player_stats_fixture_id ON player_fixture_statistics(fixture_id);
CREATE INDEX IF NOT EXISTS idx_player_stats_team_id ON player_fixture_statistics(team_id);
CREATE INDEX IF NOT EXISTS idx_player_stats_player_id ON player_fixture_statistics(player_id);

-- Ensure the update_updated_at_column function exists
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ language 'plpgsql';

-- Triggers to automatically update updated_at
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'update_teams_updated_at') THEN
        CREATE TRIGGER update_teams_updated_at
        BEFORE UPDATE ON teams
        FOR EACH ROW
        EXECUTE FUNCTION update_updated_at_column();
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'update_players_updated_at') THEN
        CREATE TRIGGER update_players_updated_at
        BEFORE UPDATE ON players
        FOR EACH ROW
        EXECUTE FUNCTION update_updated_at_column();
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'update_player_stats_updated_at') THEN
        CREATE TRIGGER update_player_stats_updated_at
        BEFORE UPDATE ON player_fixture_statistics
        FOR EACH ROW
        EXECUTE FUNCTION update_updated_at_column();
    END IF;
END $$;
