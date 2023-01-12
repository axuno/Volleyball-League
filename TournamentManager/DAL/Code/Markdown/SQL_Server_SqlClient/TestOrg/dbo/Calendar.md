Calendar
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
HomeTeamId | 9 | bigint | 0 | 19 | 0 |  | 
HomeTeamNameForRound | 10 | nvarchar | 255 | 0 | 0 | Yes | 
GuestTeamId | 11 | bigint | 0 | 19 | 0 |  | 
GuestTeamNameForRound | 12 | nvarchar | 255 | 0 | 0 | Yes | 
PlannedStart | 13 | datetime | 0 | 0 | 0 | Yes | 
PlannedEnd | 14 | datetime | 0 | 0 | 0 | Yes | 
VenueId | 15 | bigint | 0 | 19 | 0 | Yes | 
VenueName | 16 | nvarchar | 255 | 0 | 0 | Yes | 
VenueExtension | 17 | nvarchar | 255 | 0 | 0 | Yes | 
VenueStreet | 18 | nvarchar | 255 | 0 | 0 | Yes | 
VenuePostalCode | 19 | nvarchar | 10 | 0 | 0 | Yes | 
VenueCity | 20 | nvarchar | 255 | 0 | 0 | Yes | 
VenueLongitude | 21 | float | 0 | 38 | 0 | Yes | 
VenueLatitude | 22 | float | 0 | 38 | 0 | Yes | 
VenueDirection | 23 | nvarchar | 2147483647 | 0 | 0 | Yes | 
ChangeSerial | 24 | bigint | 0 | 19 | 0 |  | 
ModifiedOn | 25 | datetime | 0 | 0 | 0 |  | 

## Model elements mapped on this view

Model Element | Element type
--|--
[Calendar](../../../EntityModel/_DefaultGroup/TypedViews/Calendar.htm) | Typed View
