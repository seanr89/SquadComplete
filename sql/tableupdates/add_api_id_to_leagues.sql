-- Leagues table updates
ALTER TABLE leagues
ADD COLUMN IF NOT EXISTS api_id INT NULL;

UPDATE leagues SET api_id = id;

-- Create the sequence for the id column
CREATE SEQUENCE IF NOT EXISTS leagues_id_seq;

-- Set the default value of the id column to pull from the sequence
ALTER TABLE leagues ALTER COLUMN id SET DEFAULT nextval('leagues_id_seq');

-- Link the sequence to the table column so it gets dropped automatically if table/column is dropped
ALTER SEQUENCE leagues_id_seq OWNED BY leagues.id;

-- Advance the sequence to just past the max existing id
SELECT setval('leagues_id_seq', COALESCE(MAX(id), 0) + 1, false) FROM leagues;
