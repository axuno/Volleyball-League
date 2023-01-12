MatchToPlayRaw
================

## Fields

The following fields are defined in the MatchToPlayRaw typed view 

Name | Type | Optional | Read-only | Max. length | Precision | Scale
--|--
GuestTeamId | `long (System.Int64)` |  |  | 0 | 19 | 0
HomeTeamId | `long (System.Int64)` |  |  | 0 | 19 | 0
Id | `long (System.Int64)` |  |  | 0 | 19 | 0
MatchDate | `datetime (System.DateTime)` | Yes |  | 0 | 0 | 0
ModifiedOn | `datetime (System.DateTime)` |  |  | 0 | 0 | 0
RoundId | `long (System.Int64)` |  |  | 0 | 19 | 0
TournamentId | `long (System.Int64)` | Yes |  | 0 | 19 | 0
VenueId | `long (System.Int64)` | Yes |  | 0 | 19 | 0

## Mappings

#### [TestOrg.dbo.MatchToPlayRaw](../../../SQL_Server_SqlClient/TestOrg/dbo/MatchToPlayRaw.htm) (SQL Server (SqlClient))

Type of target: View

Typed View Field | Target field | Nullable | Type | Length | Precision | Scale | Type converter
--|--
GuestTeamId | GuestTeamId |  | bigint | 0 | 19 | 0 | 
HomeTeamId | HomeTeamId |  | bigint | 0 | 19 | 0 | 
Id | Id |  | bigint | 0 | 19 | 0 | 
MatchDate | MatchDate | Yes | datetime | 0 | 0 | 0 | 
ModifiedOn | ModifiedOn |  | datetime | 0 | 0 | 0 | 
RoundId | RoundId |  | bigint | 0 | 19 | 0 | 
TournamentId | TournamentId | Yes | bigint | 0 | 19 | 0 | 
VenueId | VenueId | Yes | bigint | 0 | 19 | 0 | 


## Code generation information

### Setting values
#### MatchToPlayRaw (TypedView)
Setting name | Value
--|--
Typed view row base class name | 
Output type | PocoWithQuerySpecQuery

#### GuestTeamId (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

#### HomeTeamId (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

#### Id (TypedViewField)
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

#### RoundId (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

#### TournamentId (TypedViewField)
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
 