Venue
============

## Fields

Field name | Ordinal | Native type | Length | Precision | Scale | Is Nullable | Is PK | Is FK | Is Identity | Is Computed  | Default value | Default sequence
--|--
Id | 1 | bigint | 0 | 19 | 0 |  | Yes |  | Yes |  |  | 
Name | 2 | nvarchar | 255 | 0 | 0 |  |  |  |  |  | '' | 
Extension | 3 | nvarchar | 255 | 0 | 0 |  |  |  |  |  | '' | 
Street | 4 | nvarchar | 255 | 0 | 0 |  |  |  |  |  | '' | 
PostalCode | 5 | nvarchar | 10 | 0 | 0 |  |  |  |  |  | '' | 
City | 6 | nvarchar | 255 | 0 | 0 |  |  |  |  |  | '' | 
Longitude | 7 | float | 0 | 38 | 0 | Yes |  |  |  |  |  | 
Latitude | 8 | float | 0 | 38 | 0 | Yes |  |  |  |  |  | 
PrecisePosition | 9 | bit | 0 | 0 | 0 |  |  |  |  |  | (0) | 
Direction | 10 | nvarchar | 2147483647 | 0 | 0 |  |  |  |  |  | '' | 
CreatedOn | 11 | datetime | 0 | 0 | 0 |  |  |  |  |  | getdate() | 
ModifiedOn | 12 | datetime | 0 | 0 | 0 |  |  |  |  |  | getdate() | 

## Model elements mapped on this table

Model Element | Element type
--|--
[Venue](../../../EntityModel/_DefaultGroup/Entities/Venue.htm) | Entity
