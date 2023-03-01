TeamVenueRound
============

## Fields

Field name | Ordinal | Native type | Length | Precision | Scale | Is Nullable | Is Identity
--|--
TeamId | 1 | bigint | 0 | 19 | 0 |  | 
TeamName | 2 | nvarchar | 255 | 0 | 0 |  | 
TeamNameForRound | 3 | nvarchar | 255 | 0 | 0 | Yes | 
TeamClubName | 4 | nvarchar | 255 | 0 | 0 | Yes | 
MatchDayOfWeek | 5 | int | 0 | 10 | 0 | Yes | 
MatchWeekday | 6 | nvarchar | 4000 | 0 | 0 | Yes | 
MatchTime | 7 | time | 0 | 0 | 0 | Yes | 
TeamModifiedOn | 8 | datetime | 0 | 0 | 0 |  | 
VenueId | 9 | bigint | 0 | 19 | 0 | Yes | 
VenueName | 10 | nvarchar | 255 | 0 | 0 | Yes | 
VenueExtension | 11 | nvarchar | 255 | 0 | 0 | Yes | 
VenueStreet | 12 | nvarchar | 255 | 0 | 0 | Yes | 
VenuePostalCode | 13 | nvarchar | 10 | 0 | 0 | Yes | 
VenueCity | 14 | nvarchar | 255 | 0 | 0 | Yes | 
VenueDirection | 15 | nvarchar | 2147483647 | 0 | 0 | Yes | 
VenueLongitude | 16 | float | 0 | 38 | 0 | Yes | 
VenueLatitude | 17 | float | 0 | 38 | 0 | Yes | 
VenuePrecisePosition | 18 | bit | 0 | 0 | 0 | Yes | 
VenueModifiedOn | 19 | datetime | 0 | 0 | 0 | Yes | 
RoundId | 20 | bigint | 0 | 19 | 0 |  | 
RoundName | 21 | nvarchar | 255 | 0 | 0 |  | 
RoundDescription | 22 | nvarchar | 255 | 0 | 0 |  | 
RoundTypeName | 23 | nvarchar | 255 | 0 | 0 |  | 
RoundTypeDescription | 24 | nvarchar | 255 | 0 | 0 |  | 
TournamentId | 25 | bigint | 0 | 19 | 0 |  | 

## Model elements mapped on this view

Model Element | Element type
--|--
[TeamVenueRound](../../../EntityModel/_DefaultGroup/TypedViews/TeamVenueRound.htm) | Typed View
