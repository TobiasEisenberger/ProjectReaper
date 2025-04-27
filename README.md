# Project Reaper

Multiplayer online game created during a two-semester university project.
Two teams compete against each other: one team tries to pass through an obstacle course, while the other uses traps to hinder them.

***

## Requirements
- Unity version 2022.3.20f1
- (Optional) Mysql 8.0 Server - execute SQL statements in "Project_Reaper_0.2_Server/database setup.sql" and edit class "DbConnection" if needed

***

## Manual

### Client Project
Use Scene "Authenticaten" as starting scene and update the connection settings in the "NetworkManager" GameObject if needed before running the application.

### Server Project
Use Scene "Level2Complete" as starting scene. Change the settings in the "NetworkManager" GameObject if needed.
Inside the inspection panel of the "Network Manager" (Script) component you can find server specific configurations and under the "Game Logic" (script) component
you can find match specific configurations.

If a database has been set up (listed under the requirements section) you can tick the "HasServerDBAPISupport" flag to enable authentication and highscore tracking,
otherwise players will login anonymously without storing their highscores.

***

## Controls
- WASD: Movement
- Spacebar: (Double)Jump
- Shift: Sprint
- E: Activate trap
- Left Mouse Button: Use power-up (only for active effects available)

***

## Contributors 
Markus Kostrizki <br/>
Dominik Seibold <br/>
Tobias Eisenberger <br/>
Szymon Franczyk <br/>
