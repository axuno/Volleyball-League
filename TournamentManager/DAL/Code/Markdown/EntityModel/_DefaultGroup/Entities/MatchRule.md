MatchRule
================

## Inheritance hierarchy

--|--
Hierarchy type | None
Is abstract | False

## Relationships

The MatchRule entity is part of the following relationships 

Related Entity | Full description 
--|--
[Round](../../_DefaultGroup/Entities/Round.htm) | Round.MatchRule - MatchRule.Rounds (m:1) 

## Fields

The following fields are defined in the MatchRule entity 

Name | Type | Is PK | Is FK | Optional | Read-only | Max. length | Precision | Scale
--|--
BestOf | `bool (System.Boolean)` |   |  |  |  | 0 | 0 | 0
CreatedOn | `datetime (System.DateTime)` |   |  |  |  | 0 | 0 | 0
Id | `long (System.Int64)` |  Yes |  |  | Yes | 0 | 19 | 0
ModifiedOn | `datetime (System.DateTime)` |   |  |  |  | 0 | 0 | 0
Name | `string (System.String)` |   |  |  |  | 255 | 0 | 0
NumOfSets | `int (System.Int32)` |   |  |  |  | 0 | 10 | 0
PointsMatchLost | `int (System.Int32)` |   |  |  |  | 0 | 10 | 0
PointsMatchLostAfterTieBreak | `int (System.Int32)` |   |  |  |  | 0 | 10 | 0
PointsMatchTie | `int (System.Int32)` |   |  |  |  | 0 | 10 | 0
PointsMatchWon | `int (System.Int32)` |   |  |  |  | 0 | 10 | 0
PointsMatchWonAfterTieBreak | `int (System.Int32)` |   |  |  |  | 0 | 10 | 0
RankComparer | `int (System.Int32)` |   |  |  |  | 0 | 10 | 0

### Unique Constraints
None.

### Fields mapped onto related fields
None.

## Mappings

#### [TestOrg.dbo.MatchRule](../../../SQL_Server_SqlClient/TestOrg/dbo/MatchRule.htm) (SQL Server (SqlClient))

Aspect | Value
--|--
Type of target | Table
Actions allowed | Create / Retrieve / Update / Delete

Entity Field | Target field | Nullable | Type | Length | Precision | Scale | Sequence | Type converter
--|--
BestOf | BestOf |  | bit | 0 | 0 | 0 |  | 
CreatedOn | CreatedOn |  | datetime | 0 | 0 | 0 |  | 
Id | Id |  | bigint | 0 | 19 | 0 | SCOPE_IDENTITY() | 
ModifiedOn | ModifiedOn |  | datetime | 0 | 0 | 0 |  | 
Name | Name |  | nvarchar | 255 | 0 | 0 |  | 
NumOfSets | NumOfSets |  | int | 0 | 10 | 0 |  | 
PointsMatchLost | PointsMatchLost |  | int | 0 | 10 | 0 |  | 
PointsMatchLostAfterTieBreak | PointsMatchLostAfterTieBreak |  | int | 0 | 10 | 0 |  | 
PointsMatchTie | PointsMatchTie |  | int | 0 | 10 | 0 |  | 
PointsMatchWon | PointsMatchWon |  | int | 0 | 10 | 0 |  | 
PointsMatchWonAfterTieBreak | PointsMatchWonAfterTieBreak |  | int | 0 | 10 | 0 |  | 
RankComparer | RankComparer |  | int | 0 | 10 | 0 |  | 

## Code generation information

### Setting values
#### MatchRule (Entity)
Setting name | Value
--|--
Entity base class name | `CommonEntityBase`

#### BestOf (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True

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

#### NumOfSets (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True

#### PointsMatchLost (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True

#### PointsMatchLostAfterTieBreak (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True

#### PointsMatchTie (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True

#### PointsMatchWon (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True

#### PointsMatchWonAfterTieBreak (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True

#### RankComparer (NormalField)
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
