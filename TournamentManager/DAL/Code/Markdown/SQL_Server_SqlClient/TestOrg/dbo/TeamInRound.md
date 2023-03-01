TeamInRound
============

## Fields

Field name | Ordinal | Native type | Length | Precision | Scale | Is Nullable | Is PK | Is FK | Is Identity | Is Computed  | Default value | Default sequence
--|--
Id | 1 | bigint | 0 | 19 | 0 |  |  |  | Yes |  |  | 
RoundId | 2 | bigint | 0 | 19 | 0 |  | Yes | Yes |  |  |  | 
TeamId | 3 | bigint | 0 | 19 | 0 |  | Yes | Yes |  |  |  | 
TeamNameForRound | 4 | nvarchar | 255 | 0 | 0 | Yes |  |  |  |  | '' | 
CreatedOn | 5 | datetime | 0 | 0 | 0 |  |  |  |  |  | getdate() | 
ModifiedOn | 6 | datetime | 0 | 0 | 0 |  |  |  |  |  | getdate() | 

## Foreign key constraints

#### FK_144927a424e8f34c15e8854f3c9

Aspect | Value
--|--
Primary key table | [dbo.Round](../dbo/Round.htm)
Delete rule | NoAction
Update rule | Cascade 

Foreign key field | Primary key field
--|--
RoundId | dbo.Round.Id

#### FK_b5d92d5411191fa54e97410ad0f

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
[TeamInRound](../../../EntityModel/_DefaultGroup/Entities/TeamInRound.htm) | Entity
