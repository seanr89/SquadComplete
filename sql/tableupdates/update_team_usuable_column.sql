-- Add sql logic to try and add a column to the teams table called usable
-- Should be done via a migration script of postgres and allow null with a default of false
-- If the column already exists, do nothing

ALTER TABLE teams ADD COLUMN IF NOT EXISTS usable BOOLEAN DEFAULT FALSE;

-- now update all teams that have a fixture in the fixtures table to be usable

UPDATE teams
SET usable = TRUE
WHERE id IN (SELECT DISTINCT home_team_id FROM fixtures WHERE home_team_id IS NOT NULL)
   OR id IN (SELECT DISTINCT away_team_id FROM fixtures WHERE away_team_id IS NOT NULL);
