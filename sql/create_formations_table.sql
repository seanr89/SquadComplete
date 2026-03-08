CREATE TABLE IF NOT EXISTS formations (
    id SERIAL PRIMARY KEY,
    name VARCHAR(20) UNIQUE NOT NULL,
    defence INTEGER NOT NULL,
    midfield INTEGER NOT NULL,
    attack INTEGER NOT NULL
);

-- Insert standard formations
INSERT INTO formations (name, defence, midfield, attack) VALUES
    ('4-4-2', 4, 4, 2),
    ('4-3-3', 4, 3, 3),
    ('3-5-2', 3, 5, 2),
    ('3-4-3', 3, 4, 3),
    ('5-3-2', 5, 3, 2),
    ('5-4-1', 5, 4, 1),
    ('4-5-1', 4, 5, 1),
    ('4-2-3-1', 4, 5, 1),
    ('4-1-4-1', 4, 5, 1),
    ('4-3-2-1', 4, 5, 1)
ON CONFLICT (name) DO NOTHING;
