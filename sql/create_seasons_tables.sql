-- Create seasons table
CREATE TABLE IF NOT EXISTS seasons (
    year INTEGER PRIMARY KEY,
    start_date DATE,
    end_date DATE,
    is_current BOOLEAN DEFAULT false,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- Create team_seasons table to track if data has been requested for a team in a specific season
CREATE TABLE IF NOT EXISTS team_seasons (
    id SERIAL PRIMARY KEY,
    team_id INTEGER NOT NULL,
    season_year INTEGER REFERENCES seasons(year),
    data_requested BOOLEAN DEFAULT false,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    UNIQUE(team_id, season_year)
);

-- Indices for faster lookups
CREATE INDEX IF NOT EXISTS idx_team_seasons_team_id ON team_seasons(team_id);
CREATE INDEX IF NOT EXISTS idx_team_seasons_season_year ON team_seasons(season_year);

-- Triggers to automatically update updated_at
-- (Assuming the function update_updated_at_column already exists)
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'update_seasons_updated_at') THEN
        CREATE TRIGGER update_seasons_updated_at
        BEFORE UPDATE ON seasons
        FOR EACH ROW
        EXECUTE FUNCTION update_updated_at_column();
    END IF;

    IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'update_team_seasons_updated_at') THEN
        CREATE TRIGGER update_team_seasons_updated_at
        BEFORE UPDATE ON team_seasons
        FOR EACH ROW
        EXECUTE FUNCTION update_updated_at_column();
    END IF;
END $$;
