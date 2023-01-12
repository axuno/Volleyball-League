Round
============

## Fields

Field name | Ordinal | Native type | Length | Precision | Scale | Is Nullable | Is PK | Is FK | Is Identity | Is Computed  | Default value | Default sequence
--|--
Id | 1 | bigint | 0 | 19 | 0 |  | Yes |  | Yes |  |  | 
TournamentId | 2 | bigint | 0 | 19 | 0 | Yes |  | Yes |  |  |  | 
Name | 3 | nvarchar | 255 | 0 | 0 |  |  |  |  |  | '' | 
Description | 4 | nvarchar | 255 | 0 | 0 |  |  |  |  |  | '' | 
TypeId | 5 | bigint | 0 | 19 | 0 |  |  | Yes |  |  |  | 
NumOfLegs | 6 | int | 0 | 10 | 0 |  |  |  |  |  | (1) | 
MatchRuleId | 7 | bigint | 0 | 19 | 0 |  |  | Yes |  |  |  | 
SetRuleId | 8 | bigint | 0 | 19 | 0 |  |  | Yes |  |  |  | 
IsComplete | 9 | bit | 0 | 0 | 0 |  |  |  |  |  | (0) | 
NextRoundId | 10 | bigint | 0 | 19 | 0 | Yes |  | Yes |  |  |  | 
CreatedOn | 11 | datetime | 0 | 0 | 0 |  |  |  |  |  | getdate() | 
ModifiedOn | 12 | datetime | 0 | 0 | 0 |  |  |  |  |  | getdate() | 

## Foreign key constraints

#### Round_MatchRule

Aspect | Value
--|--
Primary key table | [dbo.MatchRule](../dbo/MatchRule.htm)
Delete rule | NoAction
Update rule | Cascade 

Foreign key field | Primary key field
--|--
MatchRuleId | dbo.MatchRule.Id

#### Round_NextRound

Aspect | Value
--|--
Primary key table | [dbo.Round](../dbo/Round.htm)
Delete rule | NoAction
Update rule | NoAction 

Foreign key field | Primary key field
--|--
NextRoundId | dbo.Round.Id

#### Round_RoundType

Aspect | Value
--|--
Primary key table | [dbo.RoundType](../dbo/RoundType.htm)
Delete rule | NoAction
Update rule | Cascade 

Foreign key field | Primary key field
--|--
TypeId | dbo.RoundType.Id

#### Round_SetRule

Aspect | Value
--|--
Primary key table | [dbo.SetRule](../dbo/SetRule.htm)
Delete rule | NoAction
Update rule | Cascade 

Foreign key field | Primary key field
--|--
SetRuleId | dbo.SetRule.Id

#### Round_Tournament

Aspect | Value
--|--
Primary key table | [dbo.Tournament](../dbo/Tournament.htm)
Delete rule | NoAction
Update rule | NoAction 

Foreign key field | Primary key field
--|--
TournamentId | dbo.Tournament.Id

## Model elements mapped on this table

Model Element | Element type
--|--
[Round](../../../EntityModel/_DefaultGroup/Entities/Round.htm) | Entity
