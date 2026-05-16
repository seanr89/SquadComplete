-- Teams table updates
ALTER TABLE teams
ADD COLUMN IF NOT EXISTS api_id INT NULL;

UPDATE teams SET api_id = id;

-- Create the sequence for the id column
CREATE SEQUENCE IF NOT EXISTS teams_id_seq;

-- Set the default value of the id column to pull from the sequence
ALTER TABLE teams ALTER COLUMN id SET DEFAULT nextval('teams_id_seq');

-- Link the sequence to the table column so it gets dropped automatically if table/column is dropped
ALTER SEQUENCE teams_id_seq OWNED BY teams.id;

-- Advance the sequence to just past the max existing id
SELECT setval('teams_id_seq', COALESCE(MAX(id), 0) + 1, false) FROM teams;


-- Fixtures table updates
ALTER TABLE fixtures
ADD COLUMN IF NOT EXISTS api_id INT NULL;

UPDATE fixtures SET api_id = id;

-- Create the sequence for the id column
CREATE SEQUENCE IF NOT EXISTS fixtures_id_seq;

-- Set the default value of the id column to pull from the sequence
ALTER TABLE fixtures ALTER COLUMN id SET DEFAULT nextval('fixtures_id_seq');

-- Link the sequence to the table column so it gets dropped automatically if table/column is dropped
ALTER SEQUENCE fixtures_id_seq OWNED BY fixtures.id;

-- Advance the sequence to just past the max existing id
SELECT setval('fixtures_id_seq', COALESCE(MAX(id), 0) + 1, false) FROM fixtures;
