PlannedMatch
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
PlannedStart | 15 | datetime | 0 | 0 | 0 | Yes | 
PlannedEnd | 16 | datetime | 0 | 0 | 0 | Yes | 
OrigPlannedStart | 17 | datetime | 0 | 0 | 0 | Yes | 
OrigPlannedEnd | 18 | datetime | 0 | 0 | 0 | Yes | 
VenueId | 19 | bigint | 0 | 19 | 0 | Yes | 
VenueName | 20 | nvarchar | 255 | 0 | 0 | Yes | 
VenueExtension | 21 | nvarchar | 255 | 0 | 0 | Yes | 
OrigVenueId | 22 | bigint | 0 | 19 | 0 | Yes | 
OrigVenueName | 23 | nvarchar | 255 | 0 | 0 | Yes | 
OrigVenueExtension | 24 | nvarchar | 255 | 0 | 0 | Yes | 
ChangeSerial | 25 | bigint | 0 | 19 | 0 |  | 
Remarks | 26 | nvarchar | 2147483647 | 0 | 0 | Yes | 
ModifiedOn | 27 | datetime | 0 | 0 | 0 |  | 

## Model elements mapped on this view

Model Element | Element type
--|--
[PlannedMatch](../../../EntityModel/_DefaultGroup/TypedViews/PlannedMatch.htm) | Typed View
