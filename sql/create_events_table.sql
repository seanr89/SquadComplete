-- Create events table to log and store general system events
CREATE TABLE IF NOT EXISTS events (
    id SERIAL PRIMARY KEY,
    title VARCHAR(255) NOT NULL,
    message TEXT NOT NULL,
    level VARCHAR(50) NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- Index for filtering by level
CREATE INDEX IF NOT EXISTS idx_events_level ON events(level);

-- Index for chronological sorting/filtering
CREATE INDEX IF NOT EXISTS idx_events_created_at ON events(created_at);
