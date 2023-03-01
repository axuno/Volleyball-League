Set
============

## Fields

Field name | Ordinal | Native type | Length | Precision | Scale | Is Nullable | Is PK | Is FK | Is Identity | Is Computed  | Default value | Default sequence
--|--
Id | 1 | bigint | 0 | 19 | 0 |  | Yes |  | Yes |  |  | 
MatchId | 2 | bigint | 0 | 19 | 0 |  |  | Yes |  |  |  | 
SequenceNo | 3 | int | 0 | 10 | 0 |  |  |  |  |  | (1) | 
HomeBallPoints | 4 | int | 0 | 10 | 0 |  |  |  |  |  | (0) | 
GuestBallPoints | 5 | int | 0 | 10 | 0 |  |  |  |  |  | (0) | 
HomeSetPoints | 6 | int | 0 | 10 | 0 |  |  |  |  |  | (0) | 
GuestSetPoints | 7 | int | 0 | 10 | 0 |  |  |  |  |  | (0) | 
HomeTimeout | 8 | int | 0 | 10 | 0 |  |  |  |  |  | (0) | 
GuestTimeout | 9 | int | 0 | 10 | 0 |  |  |  |  |  | (0) | 
IsOverruled | 10 | bit | 0 | 0 | 0 |  |  |  |  |  | (0) | 
IsTieBreak | 11 | bit | 0 | 0 | 0 |  |  |  |  |  | (0) | 
RealStart | 12 | datetime | 0 | 0 | 0 | Yes |  |  |  |  |  | 
RealEnd | 13 | datetime | 0 | 0 | 0 | Yes |  |  |  |  |  | 
Remarks | 14 | nvarchar | 2147483647 | 0 | 0 | Yes |  |  |  |  |  | 
CreatedOn | 15 | datetime | 0 | 0 | 0 |  |  |  |  |  | getdate() | 
ModifiedOn | 16 | datetime | 0 | 0 | 0 |  |  |  |  |  | getdate() | 

## Foreign key constraints

#### FK_a5d0fa848aab17137435c5e2c47

Aspect | Value
--|--
Primary key table | [dbo.Match](../dbo/Match.htm)
Delete rule | Cascade
Update rule | Cascade 

Foreign key field | Primary key field
--|--
MatchId | dbo.Match.Id

## Model elements mapped on this table

Model Element | Element type
--|--
[Set](../../../EntityModel/_DefaultGroup/Entities/Set.htm) | Entity
