MatchRule
============

## Fields

Field name | Ordinal | Native type | Length | Precision | Scale | Is Nullable | Is PK | Is FK | Is Identity | Is Computed  | Default value | Default sequence
--|--
Id | 1 | bigint | 0 | 19 | 0 |  | Yes |  | Yes |  |  | 
Name | 2 | nvarchar | 255 | 0 | 0 |  |  |  |  |  | '' | 
NumOfSets | 3 | int | 0 | 10 | 0 |  |  |  |  |  | (1) | 
BestOf | 4 | bit | 0 | 0 | 0 |  |  |  |  |  | (0) | 
PointsMatchWon | 5 | int | 0 | 10 | 0 |  |  |  |  |  | (0) | 
PointsMatchLost | 6 | int | 0 | 10 | 0 |  |  |  |  |  | (0) | 
PointsMatchWonAfterTieBreak | 7 | int | 0 | 10 | 0 |  |  |  |  |  | (0) | 
PointsMatchLostAfterTieBreak | 8 | int | 0 | 10 | 0 |  |  |  |  |  | (0) | 
PointsMatchTie | 9 | int | 0 | 10 | 0 |  |  |  |  |  | (0) | 
RankComparer | 10 | int | 0 | 10 | 0 |  |  |  |  |  | (1) | 
CreatedOn | 11 | datetime | 0 | 0 | 0 |  |  |  |  |  | getdate() | 
ModifiedOn | 12 | datetime | 0 | 0 | 0 |  |  |  |  |  | getdate() | 

## Model elements mapped on this table

Model Element | Element type
--|--
[MatchRule](../../../EntityModel/_DefaultGroup/Entities/MatchRule.htm) | Entity
