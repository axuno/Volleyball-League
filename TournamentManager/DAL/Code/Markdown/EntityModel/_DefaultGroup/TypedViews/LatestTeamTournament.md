LatestTeamTournament
================

## Fields

The following fields are defined in the LatestTeamTournament typed view 

Name | Type | Optional | Read-only | Max. length | Precision | Scale
--|--
RoundDescription | `string (System.String)` |  |  | 255 | 0 | 0
RoundId | `long (System.Int64)` |  |  | 0 | 19 | 0
RoundName | `string (System.String)` |  |  | 255 | 0 | 0
RoundTypeDescription | `string (System.String)` |  |  | 255 | 0 | 0
RoundTypeName | `string (System.String)` |  |  | 255 | 0 | 0
TeamClubName | `string (System.String)` | Yes |  | 255 | 0 | 0
TeamId | `long (System.Int64)` |  |  | 0 | 19 | 0
TeamMatchDayOfWeek | `int (System.Int32)` | Yes |  | 0 | 10 | 0
TeamMatchTime | `timespan (System.TimeSpan)` | Yes |  | 0 | 0 | 0
TeamName | `string (System.String)` |  |  | 255 | 0 | 0
TeamNameForRound | `string (System.String)` | Yes |  | 255 | 0 | 0
TournamentDescription | `string (System.String)` | Yes |  | 255 | 0 | 0
TournamentId | `long (System.Int64)` |  |  | 0 | 19 | 0
TournamentName | `string (System.String)` |  |  | 255 | 0 | 0

## Mappings

#### [TestOrg.dbo.LatestTeamTournament](../../../SQL_Server_SqlClient/TestOrg/dbo/LatestTeamTournament.htm) (SQL Server (SqlClient))

Type of target: View

Typed View Field | Target field | Nullable | Type | Length | Precision | Scale | Type converter
--|--
RoundDescription | RoundDescription |  | nvarchar | 255 | 0 | 0 | 
RoundId | RoundId |  | bigint | 0 | 19 | 0 | 
RoundName | RoundName |  | nvarchar | 255 | 0 | 0 | 
RoundTypeDescription | RoundTypeDescription |  | nvarchar | 255 | 0 | 0 | 
RoundTypeName | RoundTypeName |  | nvarchar | 255 | 0 | 0 | 
TeamClubName | TeamClubName | Yes | nvarchar | 255 | 0 | 0 | 
TeamId | TeamId |  | bigint | 0 | 19 | 0 | 
TeamMatchDayOfWeek | TeamMatchDayOfWeek | Yes | int | 0 | 10 | 0 | 
TeamMatchTime | TeamMatchTime | Yes | time | 0 | 0 | 0 | 
TeamName | TeamName |  | nvarchar | 255 | 0 | 0 | 
TeamNameForRound | TeamNameForRound | Yes | nvarchar | 255 | 0 | 0 | 
TournamentDescription | TournamentDescription | Yes | nvarchar | 255 | 0 | 0 | 
TournamentId | TournamentId |  | bigint | 0 | 19 | 0 | 
TournamentName | TournamentName |  | nvarchar | 255 | 0 | 0 | 


## Code generation information

### Setting values
#### LatestTeamTournament (TypedView)
Setting name | Value
--|--
Typed view row base class name | 
Output type | PocoWithQuerySpecQuery

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

#### TeamClubName (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

#### TeamId (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

#### TeamMatchDayOfWeek (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

#### TeamMatchTime (TypedViewField)
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

#### TournamentName (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

### Attribute definitions per element

None.

### Additional interface definitions per element

None.

### Additional namespace definitions per element

None.
 