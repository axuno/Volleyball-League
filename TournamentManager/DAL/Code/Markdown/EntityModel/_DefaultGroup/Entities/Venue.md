Venue
================

## Inheritance hierarchy

--|--
Hierarchy type | None
Is abstract | False

## Relationships

The Venue entity is part of the following relationships 

Related Entity | Full description 
--|--
[AvailableMatchDate](../../_DefaultGroup/Entities/AvailableMatchDate.htm) | AvailableMatchDate.Venue - Venue.AvailableMatchDates (m:1) 
[Match](../../_DefaultGroup/Entities/Match.htm) | Match.OrigVenue - Venue.OrigVenueMatches (m:1) 
[Match](../../_DefaultGroup/Entities/Match.htm) | Match.Venue - Venue.CurrentVenueMatches (m:1) 
[Team](../../_DefaultGroup/Entities/Team.htm) | Team.Venue - Venue.Teams (m:1) 

## Fields

The following fields are defined in the Venue entity 

Name | Type | Is PK | Is FK | Optional | Read-only | Max. length | Precision | Scale
--|--
Id | `long (System.Int64)` |  Yes |  |  | Yes | 0 | 11 | 0
Name | `string (System.String)` |   |  |  |  | 255 | 0 | 0
Extension | `string (System.String)` |   |  |  |  | 255 | 0 | 0
Street | `string (System.String)` |   |  |  |  | 255 | 0 | 0
PostalCode | `string (System.String)` |   |  |  |  | 10 | 0 | 0
City | `string (System.String)` |   |  |  |  | 255 | 0 | 0
Longitude | `double (System.Double)` |   |  | Yes |  | 0 | 0 | 0
Latitude | `double (System.Double)` |   |  | Yes |  | 0 | 0 | 0
Direction | `string (System.String)` |   |  |  |  | 2147483647 | 0 | 0
CreatedOn | `datetime (System.DateTime)` |   |  |  |  | 0 | 0 | 0
ModifiedOn | `datetime (System.DateTime)` |   |  |  |  | 0 | 0 | 0
PrecisePosition | `bool (System.Boolean)` |   |  |  |  | 0 | 0 | 0

### Unique Constraints
None.

### Fields mapped onto related fields
None.

## Mappings

#### [TestOrg.dbo.Venue](../../../SQL_Server_SqlClient/TestOrg/dbo/Venue.htm) (SQL Server (SqlClient))

Aspect | Value
--|--
Type of target | Table
Actions allowed | Create / Retrieve / Update / Delete

Entity Field | Target field | Nullable | Type | Length | Precision | Scale | Sequence | Type converter
--|--
City | City |  | nvarchar | 255 | 0 | 0 |  | 
CreatedOn | CreatedOn |  | datetime | 0 | 0 | 0 |  | 
Direction | Direction |  | nvarchar | 2147483647 | 0 | 0 |  | 
Extension | Extension |  | nvarchar | 255 | 0 | 0 |  | 
Id | Id |  | bigint | 0 | 19 | 0 | SCOPE_IDENTITY() | 
Latitude | Latitude | Yes | float | 0 | 38 | 0 |  | 
Longitude | Longitude | Yes | float | 0 | 38 | 0 |  | 
ModifiedOn | ModifiedOn |  | datetime | 0 | 0 | 0 |  | 
Name | Name |  | nvarchar | 255 | 0 | 0 |  | 
PostalCode | PostalCode |  | nvarchar | 10 | 0 | 0 |  | 
PrecisePosition | PrecisePosition |  | bit | 0 | 0 | 0 |  | 
Street | Street |  | nvarchar | 255 | 0 | 0 |  | 

## Code generation information

### Setting values
#### Venue (Entity)
Setting name | Value
--|--
Entity base class name | `CommonEntityBase`

#### City (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True
Field property has private setter | False

#### CreatedOn (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True
Field property has private setter | False

#### Direction (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True
Field property has private setter | False

#### Extension (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True
Field property has private setter | False

#### Id (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True
Field property has private setter | False

#### Latitude (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True
Field property has private setter | False

#### Longitude (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True
Field property has private setter | False

#### ModifiedOn (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True
Field property has private setter | False

#### Name (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True
Field property has private setter | False

#### PostalCode (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True
Field property has private setter | False

#### PrecisePosition (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True
Field property has private setter | False

#### Street (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True
Field property has private setter | False

#### AvailableMatchDates (NavigatorCollection)
Setting name | Value
--|--
Navigator property is public | True

#### CurrentVenueMatches (NavigatorCollection)
Setting name | Value
--|--
Navigator property is public | True

#### OrigVenueMatches (NavigatorCollection)
Setting name | Value
--|--
Navigator property is public | True

#### Teams (NavigatorCollection)
Setting name | Value
--|--
Navigator property is public | True

### Attribute definitions per element

None.

### Additional interface definitions per element

None.

### Additional namespace definitions per element

None.
