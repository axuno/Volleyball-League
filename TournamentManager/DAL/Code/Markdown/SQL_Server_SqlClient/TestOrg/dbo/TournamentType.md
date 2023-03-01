TournamentType
============

## Fields

Field name | Ordinal | Native type | Length | Precision | Scale | Is Nullable | Is PK | Is FK | Is Identity | Is Computed  | Default value | Default sequence
--|--
Id | 1 | bigint | 0 | 19 | 0 |  | Yes |  | Yes |  |  | 
Name | 2 | nvarchar | 255 | 0 | 0 |  |  |  |  |  | '' | 
Description | 3 | nvarchar | 255 | 0 | 0 |  |  |  |  |  | '' | 
CreatedOn | 4 | datetime | 0 | 0 | 0 |  |  |  |  |  | getdate() | 
ModifiedOn | 5 | datetime | 0 | 0 | 0 |  |  |  |  |  | getdate() | 

## Model elements mapped on this table

Model Element | Element type
--|--
[TournamentType](../../../EntityModel/_DefaultGroup/Entities/TournamentType.htm) | Entity
