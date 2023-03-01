RoundLeg
============

## Fields

Field name | Ordinal | Native type | Length | Precision | Scale | Is Nullable | Is PK | Is FK | Is Identity | Is Computed  | Default value | Default sequence
--|--
Id | 1 | bigint | 0 | 19 | 0 |  | Yes |  | Yes |  |  | 
RoundId | 2 | bigint | 0 | 19 | 0 |  |  | Yes |  |  |  | 
SequenceNo | 3 | int | 0 | 10 | 0 |  |  |  |  |  | (1) | 
Description | 4 | nvarchar | 255 | 0 | 0 | Yes |  |  |  |  |  | 
StartDateTime | 5 | datetime | 0 | 0 | 0 |  |  |  |  |  | getdate() | 
EndDateTime | 6 | datetime | 0 | 0 | 0 |  |  |  |  |  | getdate() | 
CreatedOn | 7 | datetime | 0 | 0 | 0 |  |  |  |  |  | getdate() | 
ModifiedOn | 8 | datetime | 0 | 0 | 0 |  |  |  |  |  | getdate() | 

## Foreign key constraints

#### FK_b06fad141949b946948a4b7b238

Aspect | Value
--|--
Primary key table | [dbo.Round](../dbo/Round.htm)
Delete rule | Cascade
Update rule | Cascade 

Foreign key field | Primary key field
--|--
RoundId | dbo.Round.Id

## Model elements mapped on this table

Model Element | Element type
--|--
[RoundLeg](../../../EntityModel/_DefaultGroup/Entities/RoundLeg.htm) | Entity
