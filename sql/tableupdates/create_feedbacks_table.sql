-- Create feedback table to store user suggestions and bug reports
CREATE TABLE IF NOT EXISTS feedbacks (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(255) NOT NULL,
    email VARCHAR(255) NOT NULL,
    message TEXT NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- Index for faster filtering by email if needed
CREATE INDEX IF NOT EXISTS idx_feedbacks_email ON feedbacks(email);

-- Index for chronological sorting
CREATE INDEX IF NOT EXISTS idx_feedbacks_created_at ON feedbacks(created_at);
