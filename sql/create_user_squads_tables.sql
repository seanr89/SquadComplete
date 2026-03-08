-- Create user_squads table to store a user's squad for a specific game record
CREATE TABLE IF NOT EXISTS user_squads (
    id SERIAL PRIMARY KEY,
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    game_record_id INTEGER NOT NULL REFERENCES game_records(id) ON DELETE CASCADE,
    formation_id INTEGER REFERENCES formations(id),
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    UNIQUE (user_id, game_record_id)
);

-- Create user_squad_players table to store the player selections for a user's squad
CREATE TABLE IF NOT EXISTS user_squad_players (
    id SERIAL PRIMARY KEY,
    user_squad_id INTEGER NOT NULL REFERENCES user_squads(id) ON DELETE CASCADE,
    player_id INTEGER NOT NULL REFERENCES players(id) ON DELETE CASCADE,
    is_captain BOOLEAN DEFAULT FALSE,
    is_vice_captain BOOLEAN DEFAULT FALSE,
    position VARCHAR(50), 
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    UNIQUE (user_squad_id, player_id)
);

-- Indices for faster lookups
CREATE INDEX IF NOT EXISTS idx_user_squads_user_id ON user_squads(user_id);
CREATE INDEX IF NOT EXISTS idx_user_squads_game_record_id ON user_squads(game_record_id);
CREATE INDEX IF NOT EXISTS idx_user_squad_players_squad_id ON user_squad_players(user_squad_id);
CREATE INDEX IF NOT EXISTS idx_user_squad_players_player_id ON user_squad_players(player_id);

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
    IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'update_user_squads_updated_at') THEN
        CREATE TRIGGER update_user_squads_updated_at
        BEFORE UPDATE ON user_squads
        FOR EACH ROW
        EXECUTE FUNCTION update_updated_at_column();
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'update_user_squad_players_updated_at') THEN
        CREATE TRIGGER update_user_squad_players_updated_at
        BEFORE UPDATE ON user_squad_players
        FOR EACH ROW
        EXECUTE FUNCTION update_updated_at_column();
    END IF;
END $$;
