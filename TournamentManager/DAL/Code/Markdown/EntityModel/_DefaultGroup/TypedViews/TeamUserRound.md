TeamUserRound
================

## Fields

The following fields are defined in the TeamUserRound typed view 

Name | Type | Optional | Read-only | Max. length | Precision | Scale
--|--
ClubName | `string (System.String)` | Yes |  | 255 | 0 | 0
Email | `string (System.String)` |  |  | 255 | 0 | 0
Email2 | `string (System.String)` |  |  | 100 | 0 | 0
FirstName | `string (System.String)` |  |  | 255 | 0 | 0
Gender | `string (System.String)` |  |  | 1 | 0 | 0
IsManager | `bool (System.Boolean)` |  |  | 0 | 0 | 0
IsPlayer | `bool (System.Boolean)` |  |  | 0 | 0 | 0
LastName | `string (System.String)` |  |  | 255 | 0 | 0
MatchDayOfWeek | `int (System.Int32)` | Yes |  | 0 | 10 | 0
MatchTime | `timespan (System.TimeSpan)` | Yes |  | 0 | 0 | 0
MatchWeekday | `string (System.String)` | Yes |  | 4000 | 0 | 0
MiddleName | `string (System.String)` |  |  | 255 | 0 | 0
Nickname | `string (System.String)` |  |  | 255 | 0 | 0
PhoneNumber | `string (System.String)` |  |  | 40 | 0 | 0
PhoneNumber2 | `string (System.String)` |  |  | 40 | 0 | 0
RoundDescription | `string (System.String)` |  |  | 255 | 0 | 0
RoundId | `long (System.Int64)` |  |  | 0 | 19 | 0
RoundName | `string (System.String)` |  |  | 255 | 0 | 0
TeamId | `long (System.Int64)` |  |  | 0 | 19 | 0
TeamModifiedOn | `datetime (System.DateTime)` |  |  | 0 | 0 | 0
TeamName | `string (System.String)` |  |  | 255 | 0 | 0
TeamNameForRound | `string (System.String)` | Yes |  | 255 | 0 | 0
Title | `string (System.String)` |  |  | 255 | 0 | 0
TournamentId | `long (System.Int64)` | Yes |  | 0 | 19 | 0
UserId | `long (System.Int64)` |  |  | 0 | 19 | 0
UserModifiedOn | `datetime (System.DateTime)` |  |  | 0 | 0 | 0

## Mappings

#### [TestOrg.dbo.TeamUserRound](../../../SQL_Server_SqlClient/TestOrg/dbo/TeamUserRound.htm) (SQL Server (SqlClient))

Type of target: View

Typed View Field | Target field | Nullable | Type | Length | Precision | Scale | Type converter
--|--
ClubName | ClubName | Yes | nvarchar | 255 | 0 | 0 | 
Email | Email |  | nvarchar | 255 | 0 | 0 | 
Email2 | Email2 |  | nvarchar | 100 | 0 | 0 | 
FirstName | FirstName |  | nvarchar | 255 | 0 | 0 | 
Gender | Gender |  | nvarchar | 1 | 0 | 0 | 
IsManager | IsManager |  | bit | 0 | 0 | 0 | 
IsPlayer | IsPlayer |  | bit | 0 | 0 | 0 | 
LastName | LastName |  | nvarchar | 255 | 0 | 0 | 
MatchDayOfWeek | MatchDayOfWeek | Yes | int | 0 | 10 | 0 | 
MatchTime | MatchTime | Yes | time | 0 | 0 | 0 | 
MatchWeekday | MatchWeekday | Yes | nvarchar | 4000 | 0 | 0 | 
MiddleName | MiddleName |  | nvarchar | 255 | 0 | 0 | 
Nickname | Nickname |  | nvarchar | 255 | 0 | 0 | 
PhoneNumber | PhoneNumber |  | nvarchar | 40 | 0 | 0 | 
PhoneNumber2 | PhoneNumber2 |  | nvarchar | 40 | 0 | 0 | 
RoundDescription | RoundDescription |  | nvarchar | 255 | 0 | 0 | 
RoundId | RoundId |  | bigint | 0 | 19 | 0 | 
RoundName | RoundName |  | nvarchar | 255 | 0 | 0 | 
TeamId | TeamId |  | bigint | 0 | 19 | 0 | 
TeamModifiedOn | TeamModifiedOn |  | datetime | 0 | 0 | 0 | 
TeamName | TeamName |  | nvarchar | 255 | 0 | 0 | 
TeamNameForRound | TeamNameForRound | Yes | nvarchar | 255 | 0 | 0 | 
Title | Title |  | nvarchar | 255 | 0 | 0 | 
TournamentId | TournamentId | Yes | bigint | 0 | 19 | 0 | 
UserId | UserId |  | bigint | 0 | 19 | 0 | 
UserModifiedOn | UserModifiedOn |  | datetime | 0 | 0 | 0 | 


## Code generation information

### Setting values
#### TeamUserRound (TypedView)
Setting name | Value
--|--
Typed view row base class name | 
Output type | PocoWithQuerySpecQuery

#### ClubName (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

#### Email (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

#### Email2 (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

#### FirstName (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

#### Gender (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

#### IsManager (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

#### IsPlayer (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

#### LastName (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

#### MatchDayOfWeek (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

#### MatchTime (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

#### MatchWeekday (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

#### MiddleName (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

#### Nickname (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

#### PhoneNumber (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

#### PhoneNumber2 (TypedViewField)
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

#### TeamId (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

#### TeamModifiedOn (TypedViewField)
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

#### Title (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

#### TournamentId (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

#### UserId (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

#### UserModifiedOn (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

### Attribute definitions per element

None.

### Additional interface definitions per element

None.

### Additional namespace definitions per element

None.
 