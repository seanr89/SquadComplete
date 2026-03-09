ALTER TABLE game_records
ADD COLUMN formation_id INTEGER NULL REFERENCES formations(id);
