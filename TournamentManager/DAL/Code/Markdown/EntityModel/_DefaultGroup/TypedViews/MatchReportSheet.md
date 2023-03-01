MatchReportSheet
================

## Fields

The following fields are defined in the MatchReportSheet typed view 

Name | Type | Optional | Read-only | Max. length | Precision | Scale
--|--
BestOf | `bool (System.Boolean)` |  |  | 0 | 0 | 0
ChangeSerial | `long (System.Int64)` |  |  | 0 | 19 | 0
GuestTeamId | `long (System.Int64)` |  |  | 0 | 19 | 0
GuestTeamNameForRound | `string (System.String)` | Yes |  | 255 | 0 | 0
HomeTeamId | `long (System.Int64)` |  |  | 0 | 19 | 0
HomeTeamNameForRound | `string (System.String)` | Yes |  | 255 | 0 | 0
Id | `long (System.Int64)` |  |  | 0 | 19 | 0
MaxSubstitutions | `int (System.Int32)` |  |  | 0 | 10 | 0
MaxTimeouts | `int (System.Int32)` |  |  | 0 | 10 | 0
ModifiedOn | `datetime (System.DateTime)` |  |  | 0 | 0 | 0
NumOfPointsToWinRegular | `int (System.Int32)` |  |  | 0 | 10 | 0
NumOfPointsToWinTieBreak | `int (System.Int32)` |  |  | 0 | 10 | 0
NumOfSets | `int (System.Int32)` |  |  | 0 | 10 | 0
OrigPlannedStart | `datetime (System.DateTime)` | Yes |  | 0 | 0 | 0
PlannedStart | `datetime (System.DateTime)` | Yes |  | 0 | 0 | 0
Remarks | `string (System.String)` | Yes |  | 2147483647 | 0 | 0
RoundDescription | `string (System.String)` |  |  | 255 | 0 | 0
RoundLegDescription | `string (System.String)` | Yes |  | 255 | 0 | 0
RoundLegSequenceNo | `int (System.Int32)` |  |  | 0 | 10 | 0
RoundName | `string (System.String)` |  |  | 255 | 0 | 0
RoundTypeDescription | `string (System.String)` |  |  | 255 | 0 | 0
RoundTypeName | `string (System.String)` |  |  | 255 | 0 | 0
TournamentId | `long (System.Int64)` | Yes |  | 0 | 19 | 0
TournamentName | `string (System.String)` |  |  | 255 | 0 | 0

## Mappings

#### [TestOrg.dbo.MatchReportSheet](../../../SQL_Server_SqlClient/TestOrg/dbo/MatchReportSheet.htm) (SQL Server (SqlClient))

Type of target: View

Typed View Field | Target field | Nullable | Type | Length | Precision | Scale | Type converter
--|--
BestOf | BestOf |  | bit | 0 | 0 | 0 | 
ChangeSerial | ChangeSerial |  | bigint | 0 | 19 | 0 | 
GuestTeamId | GuestTeamId |  | bigint | 0 | 19 | 0 | 
GuestTeamNameForRound | GuestTeamNameForRound | Yes | nvarchar | 255 | 0 | 0 | 
HomeTeamId | HomeTeamId |  | bigint | 0 | 19 | 0 | 
HomeTeamNameForRound | HomeTeamNameForRound | Yes | nvarchar | 255 | 0 | 0 | 
Id | Id |  | bigint | 0 | 19 | 0 | 
MaxSubstitutions | MaxSubstitutions |  | int | 0 | 10 | 0 | 
MaxTimeouts | MaxTimeouts |  | int | 0 | 10 | 0 | 
ModifiedOn | ModifiedOn |  | datetime | 0 | 0 | 0 | 
NumOfPointsToWinRegular | NumOfPointsToWinRegular |  | int | 0 | 10 | 0 | 
NumOfPointsToWinTieBreak | NumOfPointsToWinTieBreak |  | int | 0 | 10 | 0 | 
NumOfSets | NumOfSets |  | int | 0 | 10 | 0 | 
OrigPlannedStart | OrigPlannedStart | Yes | datetime | 0 | 0 | 0 | 
PlannedStart | PlannedStart | Yes | datetime | 0 | 0 | 0 | 
Remarks | Remarks | Yes | nvarchar | 2147483647 | 0 | 0 | 
RoundDescription | RoundDescription |  | nvarchar | 255 | 0 | 0 | 
RoundLegDescription | RoundLegDescription | Yes | nvarchar | 255 | 0 | 0 | 
RoundLegSequenceNo | RoundLegSequenceNo |  | int | 0 | 10 | 0 | 
RoundName | RoundName |  | nvarchar | 255 | 0 | 0 | 
RoundTypeDescription | RoundTypeDescription |  | nvarchar | 255 | 0 | 0 | 
RoundTypeName | RoundTypeName |  | nvarchar | 255 | 0 | 0 | 
TournamentId | TournamentId | Yes | bigint | 0 | 19 | 0 | 
TournamentName | TournamentName |  | nvarchar | 255 | 0 | 0 | 


## Code generation information

### Setting values
#### MatchReportSheet (TypedView)
Setting name | Value
--|--
Typed view row base class name | 
Output type | PocoWithQuerySpecQuery

#### BestOf (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

#### ChangeSerial (TypedViewField)
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

#### MaxSubstitutions (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

#### MaxTimeouts (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

#### ModifiedOn (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

#### NumOfPointsToWinRegular (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

#### NumOfPointsToWinTieBreak (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

#### NumOfSets (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

#### OrigPlannedStart (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

#### PlannedStart (TypedViewField)
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
 