SetRule
================

## Inheritance hierarchy

--|--
Hierarchy type | None
Is abstract | False

## Relationships

The SetRule entity is part of the following relationships 

Related Entity | Full description 
--|--
[Round](../../_DefaultGroup/Entities/Round.htm) | Round.SetRule - SetRule.Rounds (m:1) 

## Fields

The following fields are defined in the SetRule entity 

Name | Type | Is PK | Is FK | Optional | Read-only | Max. length | Precision | Scale
--|--
CreatedOn | `datetime (System.DateTime)` |   |  |  |  | 0 | 0 | 0
Id | `long (System.Int64)` |  Yes |  |  | Yes | 0 | 19 | 0
MaxSubstitutions | `int (System.Int32)` |   |  |  |  | 0 | 10 | 0
MaxTimeouts | `int (System.Int32)` |   |  |  |  | 0 | 10 | 0
ModifiedOn | `datetime (System.DateTime)` |   |  |  |  | 0 | 0 | 0
Name | `string (System.String)` |   |  |  |  | 255 | 0 | 0
NumOfPointsToWinRegular | `int (System.Int32)` |   |  |  |  | 0 | 10 | 0
NumOfPointsToWinTiebreak | `int (System.Int32)` |   |  |  |  | 0 | 10 | 0
PointsDiffToWinRegular | `int (System.Int32)` |   |  |  |  | 0 | 10 | 0
PointsDiffToWinTiebreak | `int (System.Int32)` |   |  |  |  | 0 | 10 | 0
PointsSetLost | `int (System.Int32)` |   |  |  |  | 0 | 10 | 0
PointsSetTie | `int (System.Int32)` |   |  |  |  | 0 | 10 | 0
PointsSetWon | `int (System.Int32)` |   |  |  |  | 0 | 10 | 0

### Unique Constraints
None.

### Fields mapped onto related fields
None.

## Mappings

#### [TestOrg.dbo.SetRule](../../../SQL_Server_SqlClient/TestOrg/dbo/SetRule.htm) (SQL Server (SqlClient))

Aspect | Value
--|--
Type of target | Table
Actions allowed | Create / Retrieve / Update / Delete

Entity Field | Target field | Nullable | Type | Length | Precision | Scale | Sequence | Type converter
--|--
CreatedOn | CreatedOn |  | datetime | 0 | 0 | 0 |  | 
Id | Id |  | bigint | 0 | 19 | 0 | SCOPE_IDENTITY() | 
MaxSubstitutions | MaxSubstitutions |  | int | 0 | 10 | 0 |  | 
MaxTimeouts | MaxTimeouts |  | int | 0 | 10 | 0 |  | 
ModifiedOn | ModifiedOn |  | datetime | 0 | 0 | 0 |  | 
Name | Name |  | nvarchar | 255 | 0 | 0 |  | 
NumOfPointsToWinRegular | NumOfPointsToWinRegular |  | int | 0 | 10 | 0 |  | 
NumOfPointsToWinTiebreak | NumOfPointsToWinTiebreak |  | int | 0 | 10 | 0 |  | 
PointsDiffToWinRegular | PointsDiffToWinRegular |  | int | 0 | 10 | 0 |  | 
PointsDiffToWinTiebreak | PointsDiffToWinTiebreak |  | int | 0 | 10 | 0 |  | 
PointsSetLost | PointsSetLost |  | int | 0 | 10 | 0 |  | 
PointsSetTie | PointsSetTie |  | int | 0 | 10 | 0 |  | 
PointsSetWon | PointsSetWon |  | int | 0 | 10 | 0 |  | 

## Code generation information

### Setting values
#### SetRule (Entity)
Setting name | Value
--|--
Entity base class name | `CommonEntityBase`

#### CreatedOn (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True

#### Id (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True

#### MaxSubstitutions (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True

#### MaxTimeouts (NormalField)
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

#### NumOfPointsToWinRegular (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True

#### NumOfPointsToWinTiebreak (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True

#### PointsDiffToWinRegular (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True

#### PointsDiffToWinTiebreak (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True

#### PointsSetLost (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True

#### PointsSetTie (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True

#### PointsSetWon (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True

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
