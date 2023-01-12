ExcludeMatchDate
============

## Fields

Field name | Ordinal | Native type | Length | Precision | Scale | Is Nullable | Is PK | Is FK | Is Identity | Is Computed  | Default value | Default sequence
--|--
Id | 1 | bigint | 0 | 19 | 0 |  | Yes |  | Yes |  |  | 
TournamentId | 2 | bigint | 0 | 19 | 0 |  |  | Yes |  |  |  | 
RoundId | 3 | bigint | 0 | 19 | 0 | Yes |  | Yes |  |  |  | 
TeamId | 4 | bigint | 0 | 19 | 0 | Yes |  | Yes |  |  |  | 
DateFrom | 5 | datetime | 0 | 0 | 0 |  |  |  |  |  |  | 
DateTo | 6 | datetime | 0 | 0 | 0 |  |  |  |  |  |  | 
Reason | 7 | nvarchar | 60 | 0 | 0 | Yes |  |  |  |  |  | 
CreatedOn | 8 | datetime | 0 | 0 | 0 |  |  |  |  |  | getdate() | 
ModifiedOn | 9 | datetime | 0 | 0 | 0 |  |  |  |  |  | getdate() | 

## Foreign key constraints

#### ExcludeMatchDate_TournamentId_fk

Aspect | Value
--|--
Primary key table | [dbo.Tournament](../dbo/Tournament.htm)
Delete rule | Cascade
Update rule | Cascade 

Foreign key field | Primary key field
--|--
TournamentId | dbo.Tournament.Id

#### FK_839b5ab411ba7bce29b0d4782c7

Aspect | Value
--|--
Primary key table | [dbo.Round](../dbo/Round.htm)
Delete rule | SetNULL
Update rule | Cascade 

Foreign key field | Primary key field
--|--
RoundId | dbo.Round.Id

#### FK_e059c2f4a6fbdd3147184f47ed1

Aspect | Value
--|--
Primary key table | [dbo.Team](../dbo/Team.htm)
Delete rule | SetNULL
Update rule | Cascade 

Foreign key field | Primary key field
--|--
TeamId | dbo.Team.Id

## Model elements mapped on this table

Model Element | Element type
--|--
[ExcludeMatchDate](../../../EntityModel/_DefaultGroup/Entities/ExcludeMatchDate.htm) | Entity
