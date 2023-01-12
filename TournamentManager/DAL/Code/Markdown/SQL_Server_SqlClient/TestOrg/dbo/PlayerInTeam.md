PlayerInTeam
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

#### FK_1f583a545568a2d84dc9b3c3e85

Aspect | Value
--|--
Primary key table | [dbo.User](../dbo/User.htm)
Delete rule | Cascade
Update rule | Cascade 

Foreign key field | Primary key field
--|--
UserId | dbo.User.Id

#### FK_5de6c0349c79218e00c605092ae

Aspect | Value
--|--
Primary key table | [dbo.Team](../dbo/Team.htm)
Delete rule | Cascade
Update rule | Cascade 

Foreign key field | Primary key field
--|--
TeamId | dbo.Team.Id

## Model elements mapped on this table

Model Element | Element type
--|--
[PlayerInTeam](../../../EntityModel/_DefaultGroup/Entities/PlayerInTeam.htm) | Entity
