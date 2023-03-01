CompletedMatch
================

## Fields

The following fields are defined in the CompletedMatch typed view 

Name | Type | Optional | Read-only | Max. length | Precision | Scale
--|--
GuestBallPoints | `int (System.Int32)` | Yes |  | 0 | 10 | 0
GuestMatchPoints | `int (System.Int32)` | Yes |  | 0 | 10 | 0
GuestSetPoints | `int (System.Int32)` | Yes |  | 0 | 10 | 0
GuestTeamId | `long (System.Int64)` |  |  | 0 | 19 | 0
GuestTeamNameForRound | `string (System.String)` | Yes |  | 255 | 0 | 0
HomeBallPoints | `int (System.Int32)` | Yes |  | 0 | 10 | 0
HomeMatchPoints | `int (System.Int32)` | Yes |  | 0 | 10 | 0
HomeSetPoints | `int (System.Int32)` | Yes |  | 0 | 10 | 0
HomeTeamId | `long (System.Int64)` |  |  | 0 | 19 | 0
HomeTeamNameForRound | `string (System.String)` | Yes |  | 255 | 0 | 0
Id | `long (System.Int64)` |  |  | 0 | 19 | 0
IsOverruled | `bool (System.Boolean)` |  |  | 0 | 0 | 0
MatchDate | `datetime (System.DateTime)` | Yes |  | 0 | 0 | 0
ModifiedOn | `datetime (System.DateTime)` |  |  | 0 | 0 | 0
Remarks | `string (System.String)` | Yes |  | 2147483647 | 0 | 0
RoundDescription | `string (System.String)` |  |  | 255 | 0 | 0
RoundId | `long (System.Int64)` |  |  | 0 | 19 | 0
RoundLegDescription | `string (System.String)` | Yes |  | 255 | 0 | 0
RoundLegSequenceNo | `int (System.Int32)` |  |  | 0 | 10 | 0
RoundName | `string (System.String)` |  |  | 255 | 0 | 0
RoundTypeDescription | `string (System.String)` |  |  | 255 | 0 | 0
RoundTypeName | `string (System.String)` |  |  | 255 | 0 | 0
SetResults | `string (System.String)` | Yes |  | 2147483647 | 0 | 0
TournamentId | `long (System.Int64)` | Yes |  | 0 | 19 | 0
TournamentName | `string (System.String)` |  |  | 255 | 0 | 0
VenueId | `long (System.Int64)` | Yes |  | 0 | 19 | 0

## Mappings

#### [TestOrg.dbo.CompletedMatch](../../../SQL_Server_SqlClient/TestOrg/dbo/CompletedMatch.htm) (SQL Server (SqlClient))

Type of target: View

Typed View Field | Target field | Nullable | Type | Length | Precision | Scale | Type converter
--|--
GuestBallPoints | GuestBallPoints | Yes | int | 0 | 10 | 0 | 
GuestMatchPoints | GuestMatchPoints | Yes | int | 0 | 10 | 0 | 
GuestSetPoints | GuestSetPoints | Yes | int | 0 | 10 | 0 | 
GuestTeamId | GuestTeamId |  | bigint | 0 | 19 | 0 | 
GuestTeamNameForRound | GuestTeamNameForRound | Yes | nvarchar | 255 | 0 | 0 | 
HomeBallPoints | HomeBallPoints | Yes | int | 0 | 10 | 0 | 
HomeMatchPoints | HomeMatchPoints | Yes | int | 0 | 10 | 0 | 
HomeSetPoints | HomeSetPoints | Yes | int | 0 | 10 | 0 | 
HomeTeamId | HomeTeamId |  | bigint | 0 | 19 | 0 | 
HomeTeamNameForRound | HomeTeamNameForRound | Yes | nvarchar | 255 | 0 | 0 | 
Id | Id |  | bigint | 0 | 19 | 0 | 
IsOverruled | IsOverruled |  | bit | 0 | 0 | 0 | 
MatchDate | MatchDate | Yes | datetime | 0 | 0 | 0 | 
ModifiedOn | ModifiedOn |  | datetime | 0 | 0 | 0 | 
Remarks | Remarks | Yes | nvarchar | 2147483647 | 0 | 0 | 
RoundDescription | RoundDescription |  | nvarchar | 255 | 0 | 0 | 
RoundId | RoundId |  | bigint | 0 | 19 | 0 | 
RoundLegDescription | RoundLegDescription | Yes | nvarchar | 255 | 0 | 0 | 
RoundLegSequenceNo | RoundLegSequenceNo |  | int | 0 | 10 | 0 | 
RoundName | RoundName |  | nvarchar | 255 | 0 | 0 | 
RoundTypeDescription | RoundTypeDescription |  | nvarchar | 255 | 0 | 0 | 
RoundTypeName | RoundTypeName |  | nvarchar | 255 | 0 | 0 | 
SetResults | SetResults | Yes | nvarchar | 2147483647 | 0 | 0 | 
TournamentId | TournamentId | Yes | bigint | 0 | 19 | 0 | 
TournamentName | TournamentName |  | nvarchar | 255 | 0 | 0 | 
VenueId | VenueId | Yes | bigint | 0 | 19 | 0 | 


## Code generation information

### Setting values
#### CompletedMatch (TypedView)
Setting name | Value
--|--
Typed view row base class name | 
Output type | PocoWithQuerySpecQuery

#### GuestBallPoints (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

#### GuestMatchPoints (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

#### GuestSetPoints (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

#### GuestTeamId (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

#### GuestTeamNameForRound (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

#### HomeBallPoints (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

#### HomeMatchPoints (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

#### HomeSetPoints (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

#### HomeTeamId (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

#### HomeTeamNameForRound (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

#### Id (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

#### IsOverruled (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

#### MatchDate (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

#### ModifiedOn (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

#### Remarks (TypedViewField)
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

#### RoundLegDescription (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

#### RoundLegSequenceNo (TypedViewField)
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

#### SetResults (TypedViewField)
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

#### VenueId (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

### Attribute definitions per element

None.

### Additional interface definitions per element

None.

### Additional namespace definitions per element

None.
 