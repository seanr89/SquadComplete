-- Create fixtures table to link fixtures, leagues and teams
CREATE TABLE IF NOT EXISTS fixtures (
    id INTEGER PRIMARY KEY,
    league_id INTEGER REFERENCES leagues(id),
    home_team_id INTEGER,
    home_team_name VARCHAR(255),
    away_team_id INTEGER,
    away_team_name VARCHAR(255),
    home_goal_count INTEGER,
    away_goal_count INTEGER,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- Indices for faster lookups
CREATE INDEX IF NOT EXISTS idx_fixtures_league_id ON fixtures(league_id);
CREATE INDEX IF NOT EXISTS idx_fixtures_home_team_id ON fixtures(home_team_id);
CREATE INDEX IF NOT EXISTS idx_fixtures_away_team_id ON fixtures(away_team_id);

-- Trigger to automatically update updated_at
-- (Assuming the function update_updated_at_column already exists from create_leagues_table.sql)
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'update_fixtures_updated_at') THEN
        CREATE TRIGGER update_fixtures_updated_at
        BEFORE UPDATE ON fixtures
        FOR EACH ROW
        EXECUTE FUNCTION update_updated_at_column();
    END IF;
END $$;
