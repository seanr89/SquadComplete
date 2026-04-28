-- Insert historical seasons from 2000 (2000/2001) to 2020 (2020/2021)
INSERT INTO seasons (year) VALUES
    (2008),
    (2009),
    (2010),
    (2011),
    (2012),
    (2013),
    (2014),
    (2015),
    (2016),
    (2017),
    (2018),
    (2019),
    (2020)
ON CONFLICT (year) DO NOTHING;

-- Insert the teams into the team_seasons table from the teams table for each season that are active
-- Insert historical seasons into the team_seasons table
-- 2008
INSERT INTO team_seasons (team_id, season_year) SELECT team_id, 2008 FROM teams ON CONFLICT (team_id, season_year) DO NOTHING;
-- 2009
INSERT INTO team_seasons (team_id, season_year) SELECT team_id, 2009 FROM teams ON CONFLICT (team_id, season_year) DO NOTHING;
-- 2010
INSERT INTO team_seasons (team_id, season_year) SELECT team_id, 2010 FROM teams ON CONFLICT (team_id, season_year) DO NOTHING;
-- 2011
INSERT INTO team_seasons (team_id, season_year) SELECT team_id, 2011 FROM teams ON CONFLICT (team_id, season_year) DO NOTHING;
-- 2012
INSERT INTO team_seasons (team_id, season_year) SELECT team_id, 2012 FROM teams ON CONFLICT (team_id, season_year) DO NOTHING;
INSERT INTO team_seasons (team_id, season_year) SELECT team_id, 2013 FROM teams ON CONFLICT (team_id, season_year) DO NOTHING;
INSERT INTO team_seasons (team_id, season_year) SELECT team_id, 2014 FROM teams ON CONFLICT (team_id, season_year) DO NOTHING;
INSERT INTO team_seasons (team_id, season_year) SELECT team_id, 2015 FROM teams ON CONFLICT (team_id, season_year) DO NOTHING;
INSERT INTO team_seasons (team_id, season_year) SELECT team_id, 2016 FROM teams ON CONFLICT (team_id, season_year) DO NOTHING;
INSERT INTO team_seasons (team_id, season_year) SELECT team_id, 2017 FROM teams ON CONFLICT (team_id, season_year) DO NOTHING;
INSERT INTO team_seasons (team_id, season_year) SELECT team_id, 2018 FROM teams ON CONFLICT (team_id, season_year) DO NOTHING;
INSERT INTO team_seasons (team_id, season_year) SELECT team_id, 2019 FROM teams ON CONFLICT (team_id, season_year) DO NOTHING;
INSERT INTO team_seasons (team_id, season_year) SELECT team_id, 2020 FROM teams ON CONFLICT (team_id, season_year) DO NOTHING;