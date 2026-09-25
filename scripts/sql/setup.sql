ATTACH DATABASE 'data/bison_comment_cli.db' AS comment_db;
ATTACH DATABASE 'data/bison_observe_cli.db' AS observe_db;
ATTACH DATABASE 'data/bison_proposal_cli.db' AS proposal_db;

CREATE TABLE IF NOT EXISTS observe_db.bison_observe_cli_db (
	obsID INTEGER PRIMARY KEY,
	Author TEXT NOT NULL,
	Observation TEXT NOT NULL,
	Location TEXT NOT NULL,
	Timestamp INTEGER NOT NULL
);

CREATE TABLE IF NOT EXISTS comment_db.bison_comment_cli_db (
	obsID INTEGER NOT NULL,
	Comment TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS proposal_db.bison_proposal_cli_db (
	obsID INTEGER NOT NULL,
	taxonID TEXT NOT NULL
);

DETACH DATABASE comment_db;
DETACH DATABASE observe_db;
DETACH DATABASE proposal_db;
