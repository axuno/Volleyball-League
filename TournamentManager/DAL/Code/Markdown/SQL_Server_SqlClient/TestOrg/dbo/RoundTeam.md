RoundTeam
============

## Fields

Field name | Ordinal | Native type | Length | Precision | Scale | Is Nullable | Is Identity
--|--
RoundId | 1 | bigint | 0 | 19 | 0 |  | 
RoundName | 2 | nvarchar | 255 | 0 | 0 |  | 
RoundDescription | 3 | nvarchar | 255 | 0 | 0 |  | 
RoundTypeName | 4 | nvarchar | 255 | 0 | 0 |  | 
RoundTypeDescription | 5 | nvarchar | 255 | 0 | 0 |  | 
TeamId | 6 | bigint | 0 | 19 | 0 |  | 
TeamName | 7 | nvarchar | 255 | 0 | 0 |  | 
TeamNameForRound | 8 | nvarchar | 255 | 0 | 0 | Yes | 
TeamClubName | 9 | nvarchar | 255 | 0 | 0 | Yes | 
TeamMatchDayOfWeek | 10 | int | 0 | 10 | 0 | Yes | 
TeamMatchTime | 11 | time | 0 | 0 | 0 | Yes | 
TeamModifiedOn | 12 | datetime | 0 | 0 | 0 |  | 
TournamentId | 13 | bigint | 0 | 19 | 0 |  | 

## Model elements mapped on this view

Model Element | Element type
--|--
[RoundTeam](../../../EntityModel/_DefaultGroup/TypedViews/RoundTeam.htm) | Typed View
