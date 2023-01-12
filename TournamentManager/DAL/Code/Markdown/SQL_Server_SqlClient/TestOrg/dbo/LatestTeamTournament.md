LatestTeamTournament
============

## Fields

Field name | Ordinal | Native type | Length | Precision | Scale | Is Nullable | Is Identity
--|--
TournamentId | 1 | bigint | 0 | 19 | 0 |  | 
TournamentName | 2 | nvarchar | 255 | 0 | 0 |  | 
TournamentDescription | 3 | nvarchar | 255 | 0 | 0 | Yes | 
RoundId | 4 | bigint | 0 | 19 | 0 |  | 
RoundName | 5 | nvarchar | 255 | 0 | 0 |  | 
RoundDescription | 6 | nvarchar | 255 | 0 | 0 |  | 
RoundTypeName | 7 | nvarchar | 255 | 0 | 0 |  | 
RoundTypeDescription | 8 | nvarchar | 255 | 0 | 0 |  | 
TeamId | 9 | bigint | 0 | 19 | 0 |  | 
TeamName | 10 | nvarchar | 255 | 0 | 0 |  | 
TeamNameForRound | 11 | nvarchar | 255 | 0 | 0 | Yes | 
TeamClubName | 12 | nvarchar | 255 | 0 | 0 | Yes | 
TeamMatchDayOfWeek | 13 | int | 0 | 10 | 0 | Yes | 
TeamMatchTime | 14 | time | 0 | 0 | 0 | Yes | 

## Model elements mapped on this view

Model Element | Element type
--|--
[LatestTeamTournament](../../../EntityModel/_DefaultGroup/TypedViews/LatestTeamTournament.htm) | Typed View
