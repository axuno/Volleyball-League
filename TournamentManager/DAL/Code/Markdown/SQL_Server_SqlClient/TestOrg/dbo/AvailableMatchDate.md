AvailableMatchDate
============

## Fields

Field name | Ordinal | Native type | Length | Precision | Scale | Is Nullable | Is PK | Is FK | Is Identity | Is Computed  | Default value | Default sequence
--|--
Id | 1 | bigint | 0 | 19 | 0 |  | Yes |  | Yes |  |  | 
TournamentId | 2 | bigint | 0 | 19 | 0 |  |  |  |  |  |  | 
HomeTeamId | 3 | bigint | 0 | 19 | 0 |  |  | Yes |  |  |  | 
VenueId | 4 | bigint | 0 | 19 | 0 |  |  | Yes |  |  |  | 
MatchStartTime | 5 | datetime | 0 | 0 | 0 |  |  |  |  |  |  | 
MatchEndTime | 6 | datetime | 0 | 0 | 0 |  |  |  |  |  |  | 
IsGenerated | 7 | bit | 0 | 0 | 0 |  |  |  |  |  |  | 
CreatedOn | 8 | datetime | 0 | 0 | 0 |  |  |  |  |  | getdate() | 
ModifiedOn | 9 | datetime | 0 | 0 | 0 |  |  |  |  |  | getdate() | 

## Foreign key constraints

#### FK_4d472ac4c12adb65ea0b4d3ff01

Aspect | Value
--|--
Primary key table | [dbo.Team](../dbo/Team.htm)
Delete rule | Cascade
Update rule | Cascade 

Foreign key field | Primary key field
--|--
HomeTeamId | dbo.Team.Id

#### FK_74cccb8440e8cf75d4dd871747b

Aspect | Value
--|--
Primary key table | [dbo.Venue](../dbo/Venue.htm)
Delete rule | NoAction
Update rule | NoAction 

Foreign key field | Primary key field
--|--
VenueId | dbo.Venue.Id

## Model elements mapped on this table

Model Element | Element type
--|--
[AvailableMatchDate](../../../EntityModel/_DefaultGroup/Entities/AvailableMatchDate.htm) | Entity
