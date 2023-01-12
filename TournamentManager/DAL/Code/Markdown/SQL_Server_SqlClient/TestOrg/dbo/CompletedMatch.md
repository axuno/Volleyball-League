CompletedMatch
============

## Fields

Field name | Ordinal | Native type | Length | Precision | Scale | Is Nullable | Is Identity
--|--
Id | 1 | bigint | 0 | 19 | 0 |  | 
TournamentId | 2 | bigint | 0 | 19 | 0 | Yes | 
TournamentName | 3 | nvarchar | 255 | 0 | 0 |  | 
RoundId | 4 | bigint | 0 | 19 | 0 |  | 
RoundName | 5 | nvarchar | 255 | 0 | 0 |  | 
RoundDescription | 6 | nvarchar | 255 | 0 | 0 |  | 
RoundTypeName | 7 | nvarchar | 255 | 0 | 0 |  | 
RoundTypeDescription | 8 | nvarchar | 255 | 0 | 0 |  | 
RoundLegSequenceNo | 9 | int | 0 | 10 | 0 |  | 
RoundLegDescription | 10 | nvarchar | 255 | 0 | 0 | Yes | 
HomeTeamId | 11 | bigint | 0 | 19 | 0 |  | 
HomeTeamNameForRound | 12 | nvarchar | 255 | 0 | 0 | Yes | 
GuestTeamId | 13 | bigint | 0 | 19 | 0 |  | 
GuestTeamNameForRound | 14 | nvarchar | 255 | 0 | 0 | Yes | 
VenueId | 15 | bigint | 0 | 19 | 0 | Yes | 
MatchDate | 16 | datetime | 0 | 0 | 0 | Yes | 
HomeMatchPoints | 17 | int | 0 | 10 | 0 | Yes | 
GuestMatchPoints | 18 | int | 0 | 10 | 0 | Yes | 
HomeSetPoints | 19 | int | 0 | 10 | 0 | Yes | 
GuestSetPoints | 20 | int | 0 | 10 | 0 | Yes | 
HomeBallPoints | 21 | int | 0 | 10 | 0 | Yes | 
GuestBallPoints | 22 | int | 0 | 10 | 0 | Yes | 
SetResults | 23 | nvarchar | 2147483647 | 0 | 0 | Yes | 
IsOverruled | 24 | bit | 0 | 0 | 0 |  | 
Remarks | 25 | nvarchar | 2147483647 | 0 | 0 | Yes | 
ModifiedOn | 26 | datetime | 0 | 0 | 0 |  | 

## Model elements mapped on this view

Model Element | Element type
--|--
[CompletedMatch](../../../EntityModel/_DefaultGroup/TypedViews/CompletedMatch.htm) | Typed View
