TeamInRound
================

## Inheritance hierarchy

--|--
Hierarchy type | None
Is abstract | False

## Relationships

The TeamInRound entity is part of the following relationships 

Related Entity | Full description 
--|--
[Round](../../_DefaultGroup/Entities/Round.htm) | TeamInRound.Round - Round.TeamInRounds (m:1) 
[Team](../../_DefaultGroup/Entities/Team.htm) | TeamInRound.Team - Team.TeamInRounds (m:1) 

## Fields

The following fields are defined in the TeamInRound entity 

Name | Type | Is PK | Is FK | Optional | Read-only | Max. length | Precision | Scale
--|--
Id | `long (System.Int64)` |   |  |  | Yes | 0 | 11 | 0
RoundId | `long (System.Int64)` |  Yes | Yes |  |  | 0 | 11 | 0
TeamId | `long (System.Int64)` |  Yes | Yes |  |  | 0 | 11 | 0
TeamNameForRound | `string (System.String)` |   |  | Yes |  | 255 | 0 | 0
CreatedOn | `datetime (System.DateTime)` |   |  |  |  | 0 | 0 | 0
ModifiedOn | `datetime (System.DateTime)` |   |  |  |  | 0 | 0 | 0

### Unique Constraints
None.

### Fields mapped onto related fields
None.

## Mappings

#### [TestOrg.dbo.TeamInRound](../../../SQL_Server_SqlClient/TestOrg/dbo/TeamInRound.htm) (SQL Server (SqlClient))

Aspect | Value
--|--
Type of target | Table
Actions allowed | Create / Retrieve / Update / Delete

Entity Field | Target field | Nullable | Type | Length | Precision | Scale | Sequence | Type converter
--|--
CreatedOn | CreatedOn |  | datetime | 0 | 0 | 0 |  | 
Id | Id |  | bigint | 0 | 19 | 0 | SCOPE_IDENTITY() | 
ModifiedOn | ModifiedOn |  | datetime | 0 | 0 | 0 |  | 
RoundId | RoundId |  | bigint | 0 | 19 | 0 |  | 
TeamId | TeamId |  | bigint | 0 | 19 | 0 |  | 
TeamNameForRound | TeamNameForRound | Yes | nvarchar | 255 | 0 | 0 |  | 

## Code generation information

### Setting values
#### TeamInRound (Entity)
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

#### ModifiedOn (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True

#### RoundId (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True

#### TeamId (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True

#### TeamNameForRound (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True

#### Round (NavigatorSingleValue)
Setting name | Value
--|--
Navigator property is public | True

#### Team (NavigatorSingleValue)
Setting name | Value
--|--
Navigator property is public | True

### Attribute definitions per element

#### Round (NavigatorSingleValue)

* `Browsable($false)`

#### Team (NavigatorSingleValue)

* `Browsable($false)`


### Additional interface definitions per element

None.

### Additional namespace definitions per element

None.
