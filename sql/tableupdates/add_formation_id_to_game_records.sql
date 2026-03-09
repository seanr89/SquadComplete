ALTER TABLE game_records
ADD COLUMN formation_id INTEGER REFERENCES formations(id);
