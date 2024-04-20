ExcludeMatchDate
================

## Inheritance hierarchy

--|--
Hierarchy type | None
Is abstract | False

## Relationships

The ExcludeMatchDate entity is part of the following relationships 

Related Entity | Full description 
--|--
[Round](../../_DefaultGroup/Entities/Round.htm) | ExcludeMatchDate.Round - Round.ExcludeMatchDates (m:1) 
[Team](../../_DefaultGroup/Entities/Team.htm) | ExcludeMatchDate.Team - Team.ExcludeMatchDates (m:1) 
[Tournament](../../_DefaultGroup/Entities/Tournament.htm) | ExcludeMatchDate.Tournament - Tournament.ExcludeMatchDates (m:1) 

## Fields

The following fields are defined in the ExcludeMatchDate entity 

Name | Type | Is PK | Is FK | Optional | Read-only | Max. length | Precision | Scale
--|--
Id | `long (System.Int64)` |  Yes |  |  | Yes | 0 | 11 | 0
TournamentId | `long (System.Int64)` |   | Yes |  |  | 0 | 11 | 0
RoundId | `long (System.Int64)` |   | Yes | Yes |  | 0 | 11 | 0
TeamId | `long (System.Int64)` |   | Yes | Yes |  | 0 | 11 | 0
DateFrom | `datetime (System.DateTime)` |   |  |  |  | 0 | 0 | 0
DateTo | `datetime (System.DateTime)` |   |  |  |  | 0 | 0 | 0
Reason | `string (System.String)` |   |  | Yes |  | 60 | 0 | 0
CreatedOn | `datetime (System.DateTime)` |   |  |  |  | 0 | 0 | 0
ModifiedOn | `datetime (System.DateTime)` |   |  |  |  | 0 | 0 | 0

### Unique Constraints
None.

### Fields mapped onto related fields
None.

## Mappings

#### [TestOrg.dbo.ExcludeMatchDate](../../../SQL_Server_SqlClient/TestOrg/dbo/ExcludeMatchDate.htm) (SQL Server (SqlClient))

Aspect | Value
--|--
Type of target | Table
Actions allowed | Create / Retrieve / Update / Delete

Entity Field | Target field | Nullable | Type | Length | Precision | Scale | Sequence | Type converter
--|--
CreatedOn | CreatedOn |  | datetime | 0 | 0 | 0 |  | 
DateFrom | DateFrom |  | datetime | 0 | 0 | 0 |  | 
DateTo | DateTo |  | datetime | 0 | 0 | 0 |  | 
Id | Id |  | bigint | 0 | 19 | 0 | SCOPE_IDENTITY() | 
ModifiedOn | ModifiedOn |  | datetime | 0 | 0 | 0 |  | 
Reason | Reason | Yes | nvarchar | 60 | 0 | 0 |  | 
RoundId | RoundId | Yes | bigint | 0 | 19 | 0 |  | 
TeamId | TeamId | Yes | bigint | 0 | 19 | 0 |  | 
TournamentId | TournamentId |  | bigint | 0 | 19 | 0 |  | 

## Code generation information

### Setting values
#### ExcludeMatchDate (Entity)
Setting name | Value
--|--
Entity base class name | `CommonEntityBase`

#### CreatedOn (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True
Field property has private setter | False

#### DateFrom (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True
Field property has private setter | False

#### DateTo (NormalField)
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

#### Reason (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True
Field property has private setter | False

#### RoundId (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True
Field property has private setter | False

#### TeamId (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True
Field property has private setter | False

#### TournamentId (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True
Field property has private setter | False

#### Round (NavigatorSingleValue)
Setting name | Value
--|--
Navigator property is public | True

#### Team (NavigatorSingleValue)
Setting name | Value
--|--
Navigator property is public | True

#### Tournament (NavigatorSingleValue)
Setting name | Value
--|--
Navigator property is public | True

### Attribute definitions per element

#### Round (NavigatorSingleValue)

* `Browsable($false)`

#### Team (NavigatorSingleValue)

* `Browsable($false)`

#### Tournament (NavigatorSingleValue)

* `Browsable($false)`


### Additional interface definitions per element

None.

### Additional namespace definitions per element

None.
