ALTER TABLE players
ADD COLUMN api_id INT NULL;

UPDATE players SET api_id = id;


-- Create the sequence for the id column
CREATE SEQUENCE IF NOT EXISTS players_id_seq;

-- Set the default value of the id column to pull from the sequence
ALTER TABLE players ALTER COLUMN id SET DEFAULT nextval('players_id_seq');

-- Link the sequence to the table column so it gets dropped automatically if table/column is dropped
ALTER SEQUENCE players_id_seq OWNED BY players.id;

-- Advance the sequence to just past the max existing id
SELECT setval('players_id_seq', COALESCE(MAX(id), 0) + 1, false) FROM players;
