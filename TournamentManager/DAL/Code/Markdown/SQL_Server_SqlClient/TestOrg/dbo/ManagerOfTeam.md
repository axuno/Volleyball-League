ManagerOfTeam
============

## Fields

Field name | Ordinal | Native type | Length | Precision | Scale | Is Nullable | Is PK | Is FK | Is Identity | Is Computed  | Default value | Default sequence
--|--
Id | 1 | bigint | 0 | 19 | 0 |  |  |  | Yes |  |  | 
UserId | 2 | bigint | 0 | 19 | 0 |  | Yes | Yes |  |  |  | 
TeamId | 3 | bigint | 0 | 19 | 0 |  | Yes | Yes |  |  |  | 
CreatedOn | 4 | datetime | 0 | 0 | 0 |  |  |  |  |  | getdate() | 
ModifiedOn | 5 | datetime | 0 | 0 | 0 |  |  |  |  |  | getdate() | 

## Foreign key constraints

#### FK_0e7569e46d2be7d0a9142edbb25

Aspect | Value
--|--
Primary key table | [dbo.Team](../dbo/Team.htm)
Delete rule | Cascade
Update rule | Cascade 

Foreign key field | Primary key field
--|--
TeamId | dbo.Team.Id

#### FK_d308f7c4770bee43783a0e53d8f

Aspect | Value
--|--
Primary key table | [dbo.User](../dbo/User.htm)
Delete rule | Cascade
Update rule | Cascade 

Foreign key field | Primary key field
--|--
UserId | dbo.User.Id

## Model elements mapped on this table

Model Element | Element type
--|--
[ManagerOfTeam](../../../EntityModel/_DefaultGroup/Entities/ManagerOfTeam.htm) | Entity
