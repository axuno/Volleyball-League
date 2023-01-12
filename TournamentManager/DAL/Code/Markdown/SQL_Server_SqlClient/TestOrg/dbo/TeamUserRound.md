TeamUserRound
============

## Fields

Field name | Ordinal | Native type | Length | Precision | Scale | Is Nullable | Is Identity
--|--
TeamId | 1 | bigint | 0 | 19 | 0 |  | 
TeamName | 2 | nvarchar | 255 | 0 | 0 |  | 
TeamNameForRound | 3 | nvarchar | 255 | 0 | 0 | Yes | 
MatchDayOfWeek | 4 | int | 0 | 10 | 0 | Yes | 
MatchWeekday | 5 | nvarchar | 4000 | 0 | 0 | Yes | 
MatchTime | 6 | time | 0 | 0 | 0 | Yes | 
ClubName | 7 | nvarchar | 255 | 0 | 0 | Yes | 
TeamModifiedOn | 8 | datetime | 0 | 0 | 0 |  | 
UserId | 9 | bigint | 0 | 19 | 0 |  | 
Gender | 10 | nvarchar | 1 | 0 | 0 |  | 
Title | 11 | nvarchar | 255 | 0 | 0 |  | 
FirstName | 12 | nvarchar | 255 | 0 | 0 |  | 
MiddleName | 13 | nvarchar | 255 | 0 | 0 |  | 
LastName | 14 | nvarchar | 255 | 0 | 0 |  | 
Nickname | 15 | nvarchar | 255 | 0 | 0 |  | 
PhoneNumber | 16 | nvarchar | 40 | 0 | 0 |  | 
PhoneNumber2 | 17 | nvarchar | 40 | 0 | 0 |  | 
Email | 18 | nvarchar | 255 | 0 | 0 |  | 
Email2 | 19 | nvarchar | 100 | 0 | 0 |  | 
UserModifiedOn | 20 | datetime | 0 | 0 | 0 |  | 
IsManager | 21 | bit | 0 | 0 | 0 |  | 
IsPlayer | 22 | bit | 0 | 0 | 0 |  | 
RoundId | 23 | bigint | 0 | 19 | 0 |  | 
RoundName | 24 | nvarchar | 255 | 0 | 0 |  | 
RoundDescription | 25 | nvarchar | 255 | 0 | 0 |  | 
TournamentId | 26 | bigint | 0 | 19 | 0 | Yes | 

## Model elements mapped on this view

Model Element | Element type
--|--
[TeamUserRound](../../../EntityModel/_DefaultGroup/TypedViews/TeamUserRound.htm) | Typed View
