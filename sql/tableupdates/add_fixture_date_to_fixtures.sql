-- Add fixture_date column as a nullable datetime to the fixtures table
ALTER TABLE fixtures 
ADD COLUMN IF NOT EXISTS fixture_date TIMESTAMP WITH TIME ZONE;
