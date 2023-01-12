Team
============

## Fields

Field name | Ordinal | Native type | Length | Precision | Scale | Is Nullable | Is PK | Is FK | Is Identity | Is Computed  | Default value | Default sequence
--|--
Id | 1 | bigint | 0 | 19 | 0 |  | Yes |  | Yes |  |  | 
VenueId | 2 | bigint | 0 | 19 | 0 | Yes |  | Yes |  |  |  | 
Name | 3 | nvarchar | 255 | 0 | 0 |  |  |  |  |  | '' | 
MatchDayOfWeek | 4 | int | 0 | 10 | 0 | Yes |  |  |  |  |  | 
MatchTime | 5 | time | 0 | 0 | 0 | Yes |  |  |  |  |  | 
ClubName | 6 | nvarchar | 255 | 0 | 0 | Yes |  |  |  |  | '' | 
CreatedOn | 7 | datetime | 0 | 0 | 0 |  |  |  |  |  | getdate() | 
ModifiedOn | 8 | datetime | 0 | 0 | 0 |  |  |  |  |  | getdate() | 

## Foreign key constraints

#### FK_e13a4a24741b2e5a717f9c696a8

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
[Team](../../../EntityModel/_DefaultGroup/Entities/Team.htm) | Entity
