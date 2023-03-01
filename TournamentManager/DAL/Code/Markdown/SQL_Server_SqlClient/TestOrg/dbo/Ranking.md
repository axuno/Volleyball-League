Ranking
============

## Fields

Field name | Ordinal | Native type | Length | Precision | Scale | Is Nullable | Is PK | Is FK | Is Identity | Is Computed  | Default value | Default sequence
--|--
Id | 1 | bigint | 0 | 19 | 0 |  | Yes |  | Yes |  |  | 
TournamentId | 2 | bigint | 0 | 19 | 0 |  |  | Yes |  |  |  | 
RoundId | 3 | bigint | 0 | 19 | 0 |  |  | Yes |  |  |  | 
TeamId | 4 | bigint | 0 | 19 | 0 |  |  | Yes |  |  |  | 
ValuationDate | 5 | datetime | 0 | 0 | 0 |  |  |  |  |  |  | 
Rank | 6 | int | 0 | 10 | 0 |  |  |  |  |  |  | 
MatchPointsWon | 7 | int | 0 | 10 | 0 | Yes |  |  |  |  |  | 
MatchPointsLost | 8 | int | 0 | 10 | 0 | Yes |  |  |  |  |  | 
SetPointsWon | 9 | int | 0 | 10 | 0 | Yes |  |  |  |  |  | 
SetPointsLost | 10 | int | 0 | 10 | 0 | Yes |  |  |  |  |  | 
BallPointsWon | 11 | int | 0 | 10 | 0 | Yes |  |  |  |  |  | 
BallPointsLost | 12 | int | 0 | 10 | 0 | Yes |  |  |  |  |  | 
MatchesPlayed | 13 | int | 0 | 10 | 0 |  |  |  |  |  | (0) | 
MatchesToPlay | 14 | int | 0 | 10 | 0 |  |  |  |  |  | (0) | 
CreatedOn | 15 | datetime | 0 | 0 | 0 |  |  |  |  |  | getdate() | 
ModifiedOn | 16 | datetime | 0 | 0 | 0 |  |  |  |  |  | getdate() | 

## Foreign key constraints

#### FK_2074dbd428a8d48582fdeaa4b8c

Aspect | Value
--|--
Primary key table | [dbo.Team](../dbo/Team.htm)
Delete rule | Cascade
Update rule | Cascade 

Foreign key field | Primary key field
--|--
TeamId | dbo.Team.Id

#### FK_ba19e0b4cd4a24b99b094d481f7

Aspect | Value
--|--
Primary key table | [dbo.Round](../dbo/Round.htm)
Delete rule | Cascade
Update rule | Cascade 

Foreign key field | Primary key field
--|--
RoundId | dbo.Round.Id

#### FK_c8e677848abac5f1b8bf13ea775

Aspect | Value
--|--
Primary key table | [dbo.Tournament](../dbo/Tournament.htm)
Delete rule | Cascade
Update rule | Cascade 

Foreign key field | Primary key field
--|--
TournamentId | dbo.Tournament.Id

## Model elements mapped on this table

Model Element | Element type
--|--
[Ranking](../../../EntityModel/_DefaultGroup/Entities/Ranking.htm) | Entity
