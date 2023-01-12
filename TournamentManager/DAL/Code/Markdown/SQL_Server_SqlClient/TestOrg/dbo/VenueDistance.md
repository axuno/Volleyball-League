VenueDistance
============

## Parameters


Parameter name | Ordinal | Native type | Length | Precision | Scale | Direction | Is result-set
--|--
@MaxDistance | 1 | float | 0 | 38 | 0 | Input | 
@Lat | 2 | float | 0 | 38 | 0 | Input | 
@Lng | 3 | float | 0 | 38 | 0 | Input | 

## Model elements mapped on this stored procedure

Model Element | Element type
--|--
[VenueDistance](../../../EntityModel/_DefaultGroup/SPCalls/VenueDistance.htm) | Stored Procedure Call


## Result sets

### Resultset1

#### Fields

Field name | Ordinal | Native type | Length | Precision | Scale | Is Nullable
--|--
Id | 1 | bigint | 0 | 19 | 0 | 
Name | 2 | nvarchar | 255 | 0 | 0 | 
Extension | 3 | nvarchar | 255 | 0 | 0 | 
Street | 4 | nvarchar | 255 | 0 | 0 | 
PostalCode | 5 | nvarchar | 10 | 0 | 0 | 
City | 6 | nvarchar | 255 | 0 | 0 | 
Longitude | 7 | float | 0 | 15 | 0 | Yes
Latitude | 8 | float | 0 | 15 | 0 | Yes
PrecisePosition | 9 | bit | 0 | 0 | 0 | 
Direction | 10 | nvarchar | 2147483647 | 0 | 0 | 
CreatedOn | 11 | datetime | 0 | 0 | 0 | 
ModifiedOn | 12 | datetime | 0 | 0 | 0 | 
Distance | 13 | float | 0 | 15 | 0 | 

##### Model elements mapped on this resultset

Model Element | Element type
--|--
[VenueDistanceResult](../../../EntityModel/_DefaultGroup/TypedViews/VenueDistanceResult.htm) | Typed View


 