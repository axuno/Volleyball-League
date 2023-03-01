VenueDistanceResult
================

## Fields

The following fields are defined in the VenueDistanceResult typed view 

Name | Type | Optional | Read-only | Max. length | Precision | Scale
--|--
City | `string (System.String)` |  |  | 255 | 0 | 0
CreatedOn | `datetime (System.DateTime)` |  |  | 0 | 0 | 0
Direction | `string (System.String)` |  |  | 2147483647 | 0 | 0
Distance | `double (System.Double)` |  |  | 0 | 15 | 0
Extension | `string (System.String)` |  |  | 255 | 0 | 0
Id | `long (System.Int64)` |  |  | 0 | 19 | 0
Latitude | `double (System.Double)` | Yes |  | 0 | 15 | 0
Longitude | `double (System.Double)` | Yes |  | 0 | 15 | 0
ModifiedOn | `datetime (System.DateTime)` |  |  | 0 | 0 | 0
Name | `string (System.String)` |  |  | 255 | 0 | 0
PostalCode | `string (System.String)` |  |  | 10 | 0 | 0
PrecisePosition | `bool (System.Boolean)` |  |  | 0 | 0 | 0
Street | `string (System.String)` |  |  | 255 | 0 | 0

## Mappings

#### [TestOrg.dbo.VenueDistance.Resultset1](../../../SQL_Server_SqlClient/TestOrg/dbo/VenueDistance.htm#resultset1) (SQL Server (SqlClient))

Type of target: Stored procedure resultset

Typed View Field | Target field | Nullable | Type | Length | Precision | Scale | Type converter
--|--
City | City |  | nvarchar | 255 | 0 | 0 | 
CreatedOn | CreatedOn |  | datetime | 0 | 0 | 0 | 
Direction | Direction |  | nvarchar | 2147483647 | 0 | 0 | 
Distance | Distance |  | float | 0 | 15 | 0 | 
Extension | Extension |  | nvarchar | 255 | 0 | 0 | 
Id | Id |  | bigint | 0 | 19 | 0 | 
Latitude | Latitude | Yes | float | 0 | 15 | 0 | 
Longitude | Longitude | Yes | float | 0 | 15 | 0 | 
ModifiedOn | ModifiedOn |  | datetime | 0 | 0 | 0 | 
Name | Name |  | nvarchar | 255 | 0 | 0 | 
PostalCode | PostalCode |  | nvarchar | 10 | 0 | 0 | 
PrecisePosition | PrecisePosition |  | bit | 0 | 0 | 0 | 
Street | Street |  | nvarchar | 255 | 0 | 0 | 


## Code generation information

### Setting values
#### VenueDistanceResult (TypedView)
Setting name | Value
--|--
Typed view row base class name | 
Output type | PocoWithQuerySpecQuery

#### City (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

#### CreatedOn (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

#### Direction (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

#### Distance (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

#### Extension (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

#### Id (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

#### Latitude (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

#### Longitude (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

#### ModifiedOn (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

#### Name (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

#### PostalCode (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

#### PrecisePosition (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

#### Street (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

### Attribute definitions per element

None.

### Additional interface definitions per element

None.

### Additional namespace definitions per element

None.
 