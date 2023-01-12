RankingList
================

## Fields

The following fields are defined in the RankingList typed view 

Name | Type | Optional | Read-only | Max. length | Precision | Scale
--|--
BallPointsLost | `int (System.Int32)` | Yes |  | 0 | 10 | 0
BallPointsWon | `int (System.Int32)` | Yes |  | 0 | 10 | 0
ClubName | `string (System.String)` | Yes |  | 255 | 0 | 0
MatchesPlayed | `int (System.Int32)` |  |  | 0 | 10 | 0
MatchesToPlay | `int (System.Int32)` |  |  | 0 | 10 | 0
MatchPointsLost | `int (System.Int32)` | Yes |  | 0 | 10 | 0
MatchPointsWon | `int (System.Int32)` | Yes |  | 0 | 10 | 0
ModifiedOn | `datetime (System.DateTime)` |  |  | 0 | 0 | 0
Rank | `int (System.Int32)` |  |  | 0 | 10 | 0
RoundDescription | `string (System.String)` |  |  | 255 | 0 | 0
RoundId | `long (System.Int64)` |  |  | 0 | 19 | 0
RoundName | `string (System.String)` |  |  | 255 | 0 | 0
RoundTypeDescription | `string (System.String)` |  |  | 255 | 0 | 0
RoundTypeName | `string (System.String)` |  |  | 255 | 0 | 0
SetPointsLost | `int (System.Int32)` | Yes |  | 0 | 10 | 0
SetPointsWon | `int (System.Int32)` | Yes |  | 0 | 10 | 0
TeamId | `long (System.Int64)` |  |  | 0 | 19 | 0
TeamName | `string (System.String)` |  |  | 255 | 0 | 0
TeamNameForRound | `string (System.String)` | Yes |  | 255 | 0 | 0
TournamentDescription | `string (System.String)` | Yes |  | 255 | 0 | 0
TournamentId | `long (System.Int64)` |  |  | 0 | 19 | 0
TournamentIsComplete | `bool (System.Boolean)` |  |  | 0 | 0 | 0
TournamentName | `string (System.String)` |  |  | 255 | 0 | 0
ValuationDate | `datetime (System.DateTime)` |  |  | 0 | 0 | 0

## Mappings

#### [TestOrg.dbo.RankingList](../../../SQL_Server_SqlClient/TestOrg/dbo/RankingList.htm) (SQL Server (SqlClient))

Type of target: View

Typed View Field | Target field | Nullable | Type | Length | Precision | Scale | Type converter
--|--
BallPointsLost | BallPointsLost | Yes | int | 0 | 10 | 0 | 
BallPointsWon | BallPointsWon | Yes | int | 0 | 10 | 0 | 
ClubName | ClubName | Yes | nvarchar | 255 | 0 | 0 | 
MatchesPlayed | MatchesPlayed |  | int | 0 | 10 | 0 | 
MatchesToPlay | MatchesToPlay |  | int | 0 | 10 | 0 | 
MatchPointsLost | MatchPointsLost | Yes | int | 0 | 10 | 0 | 
MatchPointsWon | MatchPointsWon | Yes | int | 0 | 10 | 0 | 
ModifiedOn | ModifiedOn |  | datetime | 0 | 0 | 0 | 
Rank | Rank |  | int | 0 | 10 | 0 | 
RoundDescription | RoundDescription |  | nvarchar | 255 | 0 | 0 | 
RoundId | RoundId |  | bigint | 0 | 19 | 0 | 
RoundName | RoundName |  | nvarchar | 255 | 0 | 0 | 
RoundTypeDescription | RoundTypeDescription |  | nvarchar | 255 | 0 | 0 | 
RoundTypeName | RoundTypeName |  | nvarchar | 255 | 0 | 0 | 
SetPointsLost | SetPointsLost | Yes | int | 0 | 10 | 0 | 
SetPointsWon | SetPointsWon | Yes | int | 0 | 10 | 0 | 
TeamId | TeamId |  | bigint | 0 | 19 | 0 | 
TeamName | TeamName |  | nvarchar | 255 | 0 | 0 | 
TeamNameForRound | TeamNameForRound | Yes | nvarchar | 255 | 0 | 0 | 
TournamentDescription | TournamentDescription | Yes | nvarchar | 255 | 0 | 0 | 
TournamentId | TournamentId |  | bigint | 0 | 19 | 0 | 
TournamentIsComplete | TournamentIsComplete |  | bit | 0 | 0 | 0 | 
TournamentName | TournamentName |  | nvarchar | 255 | 0 | 0 | 
ValuationDate | ValuationDate |  | datetime | 0 | 0 | 0 | 


## Code generation information

### Setting values
#### RankingList (TypedView)
Setting name | Value
--|--
Typed view row base class name | 
Output type | PocoWithQuerySpecQuery

#### BallPointsLost (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

#### BallPointsWon (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

#### ClubName (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

#### MatchesPlayed (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

#### MatchesToPlay (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

#### MatchPointsLost (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

#### MatchPointsWon (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

#### ModifiedOn (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

#### Rank (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

#### RoundDescription (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

#### RoundId (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

#### RoundName (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

#### RoundTypeDescription (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

#### RoundTypeName (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

#### SetPointsLost (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

#### SetPointsWon (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

#### TeamId (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

#### TeamName (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

#### TeamNameForRound (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

#### TournamentDescription (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

#### TournamentId (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

#### TournamentIsComplete (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

#### TournamentName (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

#### ValuationDate (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

### Attribute definitions per element

None.

### Additional interface definitions per element

None.

### Additional namespace definitions per element

None.
 