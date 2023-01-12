Ranking
================

## Inheritance hierarchy

--|--
Hierarchy type | None
Is abstract | False

## Relationships

The Ranking entity is part of the following relationships 

Related Entity | Full description 
--|--
[Round](../../_DefaultGroup/Entities/Round.htm) | Ranking.Round - Round.Rankings (m:1) 
[Team](../../_DefaultGroup/Entities/Team.htm) | Ranking.Team - Team.Rankings (m:1) 
[Tournament](../../_DefaultGroup/Entities/Tournament.htm) | Ranking.Tournament - Tournament.Rankings (m:1) 

## Fields

The following fields are defined in the Ranking entity 

Name | Type | Is PK | Is FK | Optional | Read-only | Max. length | Precision | Scale
--|--
Id | `long (System.Int64)` |  Yes |  |  | Yes | 0 | 11 | 0
TournamentId | `long (System.Int64)` |   | Yes |  |  | 0 | 11 | 0
RoundId | `long (System.Int64)` |   | Yes |  |  | 0 | 11 | 0
TeamId | `long (System.Int64)` |   | Yes |  |  | 0 | 11 | 0
ValuationDate | `datetime (System.DateTime)` |   |  |  |  | 0 | 0 | 0
Rank | `int (System.Int32)` |   |  |  |  | 0 | 4 | 0
MatchPointsWon | `int (System.Int32)` |   |  | Yes |  | 0 | 0 | 0
MatchPointsLost | `int (System.Int32)` |   |  | Yes |  | 0 | 0 | 0
SetPointsWon | `int (System.Int32)` |   |  | Yes |  | 0 | 0 | 0
SetPointsLost | `int (System.Int32)` |   |  | Yes |  | 0 | 0 | 0
BallPointsWon | `int (System.Int32)` |   |  | Yes |  | 0 | 0 | 0
BallPointsLost | `int (System.Int32)` |   |  | Yes |  | 0 | 0 | 0
MatchesPlayed | `int (System.Int32)` |   |  |  |  | 0 | 0 | 0
MatchesToPlay | `int (System.Int32)` |   |  |  |  | 0 | 4 | 0
CreatedOn | `datetime (System.DateTime)` |   |  |  |  | 0 | 0 | 0
ModifiedOn | `datetime (System.DateTime)` |   |  |  |  | 0 | 0 | 0

### Unique Constraints
None.

### Fields mapped onto related fields
None.

## Mappings

#### [TestOrg.dbo.Ranking](../../../SQL_Server_SqlClient/TestOrg/dbo/Ranking.htm) (SQL Server (SqlClient))

Aspect | Value
--|--
Type of target | Table
Actions allowed | Create / Retrieve / Update / Delete

Entity Field | Target field | Nullable | Type | Length | Precision | Scale | Sequence | Type converter
--|--
BallPointsLost | BallPointsLost | Yes | int | 0 | 10 | 0 |  | 
BallPointsWon | BallPointsWon | Yes | int | 0 | 10 | 0 |  | 
CreatedOn | CreatedOn |  | datetime | 0 | 0 | 0 |  | 
Id | Id |  | bigint | 0 | 19 | 0 | SCOPE_IDENTITY() | 
MatchesPlayed | MatchesPlayed |  | int | 0 | 10 | 0 |  | 
MatchesToPlay | MatchesToPlay |  | int | 0 | 10 | 0 |  | 
MatchPointsLost | MatchPointsLost | Yes | int | 0 | 10 | 0 |  | 
MatchPointsWon | MatchPointsWon | Yes | int | 0 | 10 | 0 |  | 
ModifiedOn | ModifiedOn |  | datetime | 0 | 0 | 0 |  | 
Rank | Rank |  | int | 0 | 10 | 0 |  | 
RoundId | RoundId |  | bigint | 0 | 19 | 0 |  | 
SetPointsLost | SetPointsLost | Yes | int | 0 | 10 | 0 |  | 
SetPointsWon | SetPointsWon | Yes | int | 0 | 10 | 0 |  | 
TeamId | TeamId |  | bigint | 0 | 19 | 0 |  | 
TournamentId | TournamentId |  | bigint | 0 | 19 | 0 |  | 
ValuationDate | ValuationDate |  | datetime | 0 | 0 | 0 |  | 

## Code generation information

### Setting values
#### Ranking (Entity)
Setting name | Value
--|--
Entity base class name | `CommonEntityBase`

#### BallPointsLost (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True

#### BallPointsWon (NormalField)
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

#### MatchesPlayed (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True

#### MatchesToPlay (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True

#### MatchPointsLost (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True

#### MatchPointsWon (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True

#### ModifiedOn (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True

#### Rank (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True

#### RoundId (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True

#### SetPointsLost (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True

#### SetPointsWon (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True

#### TeamId (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True

#### TournamentId (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True

#### ValuationDate (NormalField)
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
