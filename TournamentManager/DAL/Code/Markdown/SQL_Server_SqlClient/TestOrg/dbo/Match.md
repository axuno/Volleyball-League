Match
============

## Fields

Field name | Ordinal | Native type | Length | Precision | Scale | Is Nullable | Is PK | Is FK | Is Identity | Is Computed  | Default value | Default sequence
--|--
Id | 1 | bigint | 0 | 19 | 0 |  | Yes |  | Yes |  |  | 
HomeTeamId | 2 | bigint | 0 | 19 | 0 |  |  | Yes |  |  |  | 
GuestTeamId | 3 | bigint | 0 | 19 | 0 |  |  | Yes |  |  |  | 
RefereeId | 4 | bigint | 0 | 19 | 0 | Yes |  | Yes |  |  |  | 
RoundId | 5 | bigint | 0 | 19 | 0 |  |  | Yes |  |  |  | 
VenueId | 6 | bigint | 0 | 19 | 0 | Yes |  | Yes |  |  |  | 
OrigVenueId | 7 | bigint | 0 | 19 | 0 | Yes |  | Yes |  |  |  | 
LegSequenceNo | 8 | int | 0 | 10 | 0 | Yes |  |  |  |  |  | 
HomePoints | 9 | int | 0 | 10 | 0 | Yes |  |  |  |  |  | 
GuestPoints | 10 | int | 0 | 10 | 0 | Yes |  |  |  |  |  | 
IsComplete | 11 | bit | 0 | 0 | 0 |  |  |  |  |  | (1) | 
IsOverruled | 12 | bit | 0 | 0 | 0 |  |  |  |  |  | (0) | 
PlannedStart | 13 | datetime | 0 | 0 | 0 | Yes |  |  |  |  |  | 
PlannedEnd | 14 | datetime | 0 | 0 | 0 | Yes |  |  |  |  |  | 
OrigPlannedStart | 15 | datetime | 0 | 0 | 0 | Yes |  |  |  |  |  | 
OrigPlannedEnd | 16 | datetime | 0 | 0 | 0 | Yes |  |  |  |  |  | 
RealStart | 17 | datetime | 0 | 0 | 0 | Yes |  |  |  |  |  | 
RealEnd | 18 | datetime | 0 | 0 | 0 | Yes |  |  |  |  |  | 
Remarks | 19 | nvarchar | 2147483647 | 0 | 0 | Yes |  |  |  |  |  | 
ChangeSerial | 20 | bigint | 0 | 19 | 0 |  |  |  |  |  | (0) | 
CreatedOn | 21 | datetime | 0 | 0 | 0 |  |  |  |  |  | getdate() | 
ModifiedOn | 22 | datetime | 0 | 0 | 0 |  |  |  |  |  | getdate() | 

## Foreign key constraints

#### FK_5c03c0c403195643ad89303cc4b

Aspect | Value
--|--
Primary key table | [dbo.Round](../dbo/Round.htm)
Delete rule | Cascade
Update rule | Cascade 

Foreign key field | Primary key field
--|--
RoundId | dbo.Round.Id

#### FK_cb028e742dbbcefcdc18bc33d8c

Aspect | Value
--|--
Primary key table | [dbo.Venue](../dbo/Venue.htm)
Delete rule | NoAction
Update rule | NoAction 

Foreign key field | Primary key field
--|--
VenueId | dbo.Venue.Id

#### FK_ce641dd44a79b6a0536029e5095

Aspect | Value
--|--
Primary key table | [dbo.Team](../dbo/Team.htm)
Delete rule | NoAction
Update rule | NoAction 

Foreign key field | Primary key field
--|--
GuestTeamId | dbo.Team.Id

#### FK_ef1387a4716a86685ca94abfe03

Aspect | Value
--|--
Primary key table | [dbo.Team](../dbo/Team.htm)
Delete rule | NoAction
Update rule | NoAction 

Foreign key field | Primary key field
--|--
HomeTeamId | dbo.Team.Id

#### FK_f7bed0a4ecbbddad75d0016be12

Aspect | Value
--|--
Primary key table | [dbo.Venue](../dbo/Venue.htm)
Delete rule | NoAction
Update rule | NoAction 

Foreign key field | Primary key field
--|--
OrigVenueId | dbo.Venue.Id

#### FK_fc8dfc04f2d86db9b7c1f85ab55

Aspect | Value
--|--
Primary key table | [dbo.Team](../dbo/Team.htm)
Delete rule | NoAction
Update rule | NoAction 

Foreign key field | Primary key field
--|--
RefereeId | dbo.Team.Id

## Model elements mapped on this table

Model Element | Element type
--|--
[Match](../../../EntityModel/_DefaultGroup/Entities/Match.htm) | Entity
