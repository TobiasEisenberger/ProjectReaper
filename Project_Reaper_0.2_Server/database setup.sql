CREATE SCHEMA `mmo_project_reaper`;

CREATE TABLE player (
PlayerId int AUTO_INCREMENT PRIMARY KEY,
UNIQUE (PlayerId),
Name varchar(16) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
UNIQUE(Name),
Password varchar(255)
);

CREATE TABLE scores (
ScoreId int AUTO_INCREMENT PRIMARY KEY,
PlayerId int,
Points int,
ScoreTime Timestamp DEFAULT CURRENT_TIMESTAMP,
Team VARCHAR(16),
FOREIGN KEY(PlayerId) REFERENCES player(PlayerId)
);