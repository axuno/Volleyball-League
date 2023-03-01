SetRule
============

## Fields

Field name | Ordinal | Native type | Length | Precision | Scale | Is Nullable | Is PK | Is FK | Is Identity | Is Computed  | Default value | Default sequence
--|--
Id | 1 | bigint | 0 | 19 | 0 |  | Yes |  | Yes |  |  | 
Name | 2 | nvarchar | 255 | 0 | 0 |  |  |  |  |  | '' | 
NumOfPointsToWinRegular | 3 | int | 0 | 10 | 0 |  |  |  |  |  | (0) | 
NumOfPointsToWinTiebreak | 4 | int | 0 | 10 | 0 |  |  |  |  |  | (0) | 
PointsDiffToWinRegular | 5 | int | 0 | 10 | 0 |  |  |  |  |  | (0) | 
PointsDiffToWinTiebreak | 6 | int | 0 | 10 | 0 |  |  |  |  |  | (0) | 
PointsSetLost | 7 | int | 0 | 10 | 0 |  |  |  |  |  | (0) | 
PointsSetTie | 8 | int | 0 | 10 | 0 |  |  |  |  |  | (0) | 
PointsSetWon | 9 | int | 0 | 10 | 0 |  |  |  |  |  | (0) | 
MaxTimeouts | 10 | int | 0 | 10 | 0 |  |  |  |  |  | (0) | 
MaxSubstitutions | 11 | int | 0 | 10 | 0 |  |  |  |  |  | (0) | 
CreatedOn | 12 | datetime | 0 | 0 | 0 |  |  |  |  |  | getdate() | 
ModifiedOn | 13 | datetime | 0 | 0 | 0 |  |  |  |  |  | getdate() | 

## Model elements mapped on this table

Model Element | Element type
--|--
[SetRule](../../../EntityModel/_DefaultGroup/Entities/SetRule.htm) | Entity
