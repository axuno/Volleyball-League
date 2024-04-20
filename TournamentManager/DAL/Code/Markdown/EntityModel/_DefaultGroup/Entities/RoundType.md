RoundType
================

## Inheritance hierarchy

--|--
Hierarchy type | None
Is abstract | False

## Relationships

The RoundType entity is part of the following relationships 

Related Entity | Full description 
--|--
[Round](../../_DefaultGroup/Entities/Round.htm) | Round.RoundType - RoundType.Rounds (m:1) 

## Fields

The following fields are defined in the RoundType entity 

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

#### [TestOrg.dbo.RoundType](../../../SQL_Server_SqlClient/TestOrg/dbo/RoundType.htm) (SQL Server (SqlClient))

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
#### RoundType (Entity)
Setting name | Value
--|--
Entity base class name | `CommonEntityBase`

#### CreatedOn (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True
Field property has private setter | False

#### Description (NormalField)
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

#### Rounds (NavigatorCollection)
Setting name | Value
--|--
Navigator property is public | True

### Attribute definitions per element

None.

### Additional interface definitions per element

None.

### Additional namespace definitions per element

None.
