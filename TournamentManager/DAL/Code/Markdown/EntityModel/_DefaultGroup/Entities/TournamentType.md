TournamentType
================

## Inheritance hierarchy

--|--
Hierarchy type | None
Is abstract | False

## Relationships

The TournamentType entity is part of the following relationships 

Related Entity | Full description 
--|--
[Tournament](../../_DefaultGroup/Entities/Tournament.htm) | Tournament.TournamentType - TournamentType.Tournaments (m:1) 

## Fields

The following fields are defined in the TournamentType entity 

Name | Type | Is PK | Is FK | Optional | Read-only | Max. length | Precision | Scale
--|--
Id | `long (System.Int64)` |  Yes |  |  | Yes | 0 | 11 | 0
Name | `string (System.String)` |   |  |  |  | 255 | 0 | 0
Description | `string (System.String)` |   |  |  |  | 255 | 0 | 0
CreatedOn | `datetime (System.DateTime)` |   |  |  |  | 0 | 0 | 0
ModifiedOn | `datetime (System.DateTime)` |   |  |  |  | 0 | 0 | 0

### Unique Constraints
None.

### Fields mapped onto related fields
None.

## Mappings

#### [TestOrg.dbo.TournamentType](../../../SQL_Server_SqlClient/TestOrg/dbo/TournamentType.htm) (SQL Server (SqlClient))

Aspect | Value
--|--
Type of target | Table
Actions allowed | Create / Retrieve / Update / Delete

Entity Field | Target field | Nullable | Type | Length | Precision | Scale | Sequence | Type converter
--|--
CreatedOn | CreatedOn |  | datetime | 0 | 0 | 0 |  | 
Description | Description |  | nvarchar | 255 | 0 | 0 |  | 
Id | Id |  | bigint | 0 | 19 | 0 | SCOPE_IDENTITY() | 
ModifiedOn | ModifiedOn |  | datetime | 0 | 0 | 0 |  | 
Name | Name |  | nvarchar | 255 | 0 | 0 |  | 

## Code generation information

### Setting values
#### TournamentType (Entity)
Setting name | Value
--|--
Entity base class name | `CommonEntityBase`

#### CreatedOn (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True

#### Description (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True

#### Id (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True

#### ModifiedOn (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True

#### Name (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True

#### Tournaments (NavigatorCollection)
Setting name | Value
--|--
Navigator property is public | True

### Attribute definitions per element

None.

### Additional interface definitions per element

None.

### Additional namespace definitions per element

None.
