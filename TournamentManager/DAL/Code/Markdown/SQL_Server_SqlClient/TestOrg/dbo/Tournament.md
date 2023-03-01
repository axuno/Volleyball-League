Tournament
============

## Fields

Field name | Ordinal | Native type | Length | Precision | Scale | Is Nullable | Is PK | Is FK | Is Identity | Is Computed  | Default value | Default sequence
--|--
Id | 1 | bigint | 0 | 19 | 0 |  | Yes |  | Yes |  |  | 
Name | 2 | nvarchar | 255 | 0 | 0 |  |  |  |  |  | '' | 
Description | 3 | nvarchar | 255 | 0 | 0 | Yes |  |  |  |  | '' | 
TypeId | 4 | bigint | 0 | 19 | 0 | Yes |  | Yes |  |  |  | 
IsComplete | 5 | bit | 0 | 0 | 0 |  |  |  |  |  | (0) | 
IsPlanningMode | 6 | bit | 0 | 0 | 0 |  |  |  |  |  | (0) | 
NextTournamentId | 7 | bigint | 0 | 19 | 0 | Yes |  | Yes |  |  |  | 
CreatedOn | 8 | datetime | 0 | 0 | 0 |  |  |  |  |  | getdate() | 
ModifiedOn | 9 | datetime | 0 | 0 | 0 |  |  |  |  |  | getdate() | 

## Foreign key constraints

#### FK_7fdd8614f9d8df5c5df69e60586

Aspect | Value
--|--
Primary key table | [dbo.Tournament](../dbo/Tournament.htm)
Delete rule | NoAction
Update rule | NoAction 

Foreign key field | Primary key field
--|--
NextTournamentId | dbo.Tournament.Id

#### FK_b1c85dc49a0ad94cf65182015cb

Aspect | Value
--|--
Primary key table | [dbo.TournamentType](../dbo/TournamentType.htm)
Delete rule | SetNULL
Update rule | Cascade 

Foreign key field | Primary key field
--|--
TypeId | dbo.TournamentType.Id

## Model elements mapped on this table

Model Element | Element type
--|--
[Tournament](../../../EntityModel/_DefaultGroup/Entities/Tournament.htm) | Entity
