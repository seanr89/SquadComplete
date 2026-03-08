-- Create game_records table to store a set of records for a game
CREATE TABLE IF NOT EXISTS game_records (
    id SERIAL PRIMARY KEY,
    game_date DATE NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- Create game_record_tags table to link fixtures and teams to a game record
-- This allows tagging multiple (e.g., up to 11) teams and their associated fixtures to a single game record
CREATE TABLE IF NOT EXISTS game_record_tags (
    id SERIAL PRIMARY KEY,
    game_record_id INTEGER NOT NULL REFERENCES game_records(id) ON DELETE CASCADE,
    fixture_id INTEGER NOT NULL REFERENCES fixtures(id),
    team_id INTEGER NOT NULL REFERENCES teams(id),
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    UNIQUE (game_record_id, fixture_id, team_id)
);

-- Helper function to tag a team to a game record
-- Usage: SELECT tag_team_to_game(1, 12345, 67);
CREATE OR REPLACE FUNCTION tag_team_to_game(
    p_game_record_id INTEGER,
    p_fixture_id INTEGER,
    p_team_id INTEGER
) RETURNS INTEGER AS $$
DECLARE
    v_tag_id INTEGER;
BEGIN
    INSERT INTO game_record_tags (game_record_id, fixture_id, team_id)
    VALUES (p_game_record_id, p_fixture_id, p_team_id)
    ON CONFLICT (game_record_id, fixture_id, team_id) DO UPDATE 
    SET updated_at = CURRENT_TIMESTAMP
    RETURNING id INTO v_tag_id;
    
    RETURN v_tag_id;
END;
$$ LANGUAGE plpgsql;

-- Indices for faster lookups
CREATE INDEX IF NOT EXISTS idx_game_record_tags_game_id ON game_record_tags(game_record_id);
CREATE INDEX IF NOT EXISTS idx_game_record_tags_fixture_id ON game_record_tags(fixture_id);
CREATE INDEX IF NOT EXISTS idx_game_record_tags_team_id ON game_record_tags(team_id);

-- Ensure the update_updated_at_column function exists (standard utility)
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
    IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'update_game_records_updated_at') THEN
        CREATE TRIGGER update_game_records_updated_at
        BEFORE UPDATE ON game_records
        FOR EACH ROW
        EXECUTE FUNCTION update_updated_at_column();
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'update_game_record_tags_updated_at') THEN
        CREATE TRIGGER update_game_record_tags_updated_at
        BEFORE UPDATE ON game_record_tags
        FOR EACH ROW
        EXECUTE FUNCTION update_updated_at_column();
    END IF;
END $$;
