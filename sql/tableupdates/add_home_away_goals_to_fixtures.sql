-- Add home and away goal count columns to fixtures table
ALTER TABLE fixtures 
ADD COLUMN IF NOT EXISTS home_goal_count INTEGER,
ADD COLUMN IF NOT EXISTS away_goal_count INTEGER;
