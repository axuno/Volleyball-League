MatchCompleteRaw
============

## Fields

Field name | Ordinal | Native type | Length | Precision | Scale | Is Nullable | Is Identity
--|--
Id | 1 | bigint | 0 | 19 | 0 |  | 
TournamentId | 2 | bigint | 0 | 19 | 0 | Yes | 
RoundId | 3 | bigint | 0 | 19 | 0 |  | 
HomeTeamId | 4 | bigint | 0 | 19 | 0 |  | 
GuestTeamId | 5 | bigint | 0 | 19 | 0 |  | 
VenueId | 6 | bigint | 0 | 19 | 0 | Yes | 
MatchDate | 7 | datetime | 0 | 0 | 0 | Yes | 
HomeMatchPoints | 8 | int | 0 | 10 | 0 | Yes | 
GuestMatchPoints | 9 | int | 0 | 10 | 0 | Yes | 
HomeSetPoints | 10 | int | 0 | 10 | 0 | Yes | 
GuestSetPoints | 11 | int | 0 | 10 | 0 | Yes | 
HomeBallPoints | 12 | int | 0 | 10 | 0 | Yes | 
GuestBallPoints | 13 | int | 0 | 10 | 0 | Yes | 
IsOverruled | 14 | bit | 0 | 0 | 0 |  | 
ModifiedOn | 15 | datetime | 0 | 0 | 0 |  | 

## Model elements mapped on this view

Model Element | Element type
--|--
[MatchCompleteRaw](../../../EntityModel/_DefaultGroup/TypedViews/MatchCompleteRaw.htm) | Typed View
