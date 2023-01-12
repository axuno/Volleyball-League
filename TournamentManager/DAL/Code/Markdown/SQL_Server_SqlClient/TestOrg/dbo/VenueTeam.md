VenueTeam
============

## Fields

Field name | Ordinal | Native type | Length | Precision | Scale | Is Nullable | Is Identity
--|--
VenueId | 1 | bigint | 0 | 19 | 0 |  | 
VenueName | 2 | nvarchar | 255 | 0 | 0 |  | 
VenueExtension | 3 | nvarchar | 255 | 0 | 0 |  | 
Street | 4 | nvarchar | 255 | 0 | 0 |  | 
PostalCode | 5 | nvarchar | 10 | 0 | 0 |  | 
City | 6 | nvarchar | 255 | 0 | 0 |  | 
Direction | 7 | nvarchar | 2147483647 | 0 | 0 |  | 
Longitude | 8 | float | 0 | 38 | 0 | Yes | 
Latitude | 9 | float | 0 | 38 | 0 | Yes | 
PrecisePosition | 10 | bit | 0 | 0 | 0 |  | 
TeamId | 11 | bigint | 0 | 19 | 0 |  | 
TeamName | 12 | nvarchar | 255 | 0 | 0 |  | 
TeamNameForRound | 13 | nvarchar | 255 | 0 | 0 | Yes | 
TeamClubName | 14 | nvarchar | 255 | 0 | 0 | Yes | 
TournamentId | 15 | bigint | 0 | 19 | 0 |  | 
RoundId | 16 | bigint | 0 | 19 | 0 |  | 

## Model elements mapped on this view

Model Element | Element type
--|--
[VenueTeam](../../../EntityModel/_DefaultGroup/TypedViews/VenueTeam.htm) | Typed View
