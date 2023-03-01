TeamVenueRound
================

## Fields

The following fields are defined in the TeamVenueRound typed view 

Name | Type | Optional | Read-only | Max. length | Precision | Scale
--|--
MatchDayOfWeek | `int (System.Int32)` | Yes |  | 0 | 10 | 0
MatchTime | `timespan (System.TimeSpan)` | Yes |  | 0 | 0 | 0
MatchWeekday | `string (System.String)` | Yes |  | 4000 | 0 | 0
RoundDescription | `string (System.String)` |  |  | 255 | 0 | 0
RoundId | `long (System.Int64)` |  |  | 0 | 19 | 0
RoundName | `string (System.String)` |  |  | 255 | 0 | 0
RoundTypeDescription | `string (System.String)` |  |  | 255 | 0 | 0
RoundTypeName | `string (System.String)` |  |  | 255 | 0 | 0
TeamClubName | `string (System.String)` | Yes |  | 255 | 0 | 0
TeamId | `long (System.Int64)` |  |  | 0 | 19 | 0
TeamModifiedOn | `datetime (System.DateTime)` |  |  | 0 | 0 | 0
TeamName | `string (System.String)` |  |  | 255 | 0 | 0
TeamNameForRound | `string (System.String)` | Yes |  | 255 | 0 | 0
TournamentId | `long (System.Int64)` |  |  | 0 | 19 | 0
VenueCity | `string (System.String)` | Yes |  | 255 | 0 | 0
VenueDirection | `string (System.String)` | Yes |  | 2147483647 | 0 | 0
VenueExtension | `string (System.String)` | Yes |  | 255 | 0 | 0
VenueId | `long (System.Int64)` | Yes |  | 0 | 19 | 0
VenueLatitude | `double (System.Double)` | Yes |  | 0 | 38 | 0
VenueLongitude | `double (System.Double)` | Yes |  | 0 | 38 | 0
VenueModifiedOn | `datetime (System.DateTime)` | Yes |  | 0 | 0 | 0
VenueName | `string (System.String)` | Yes |  | 255 | 0 | 0
VenuePostalCode | `string (System.String)` | Yes |  | 10 | 0 | 0
VenuePrecisePosition | `bool (System.Boolean)` | Yes |  | 0 | 0 | 0
VenueStreet | `string (System.String)` | Yes |  | 255 | 0 | 0

## Mappings

#### [TestOrg.dbo.TeamVenueRound](../../../SQL_Server_SqlClient/TestOrg/dbo/TeamVenueRound.htm) (SQL Server (SqlClient))

Type of target: View

Typed View Field | Target field | Nullable | Type | Length | Precision | Scale | Type converter
--|--
MatchDayOfWeek | MatchDayOfWeek | Yes | int | 0 | 10 | 0 | 
MatchTime | MatchTime | Yes | time | 0 | 0 | 0 | 
MatchWeekday | MatchWeekday | Yes | nvarchar | 4000 | 0 | 0 | 
RoundDescription | RoundDescription |  | nvarchar | 255 | 0 | 0 | 
RoundId | RoundId |  | bigint | 0 | 19 | 0 | 
RoundName | RoundName |  | nvarchar | 255 | 0 | 0 | 
RoundTypeDescription | RoundTypeDescription |  | nvarchar | 255 | 0 | 0 | 
RoundTypeName | RoundTypeName |  | nvarchar | 255 | 0 | 0 | 
TeamClubName | TeamClubName | Yes | nvarchar | 255 | 0 | 0 | 
TeamId | TeamId |  | bigint | 0 | 19 | 0 | 
TeamModifiedOn | TeamModifiedOn |  | datetime | 0 | 0 | 0 | 
TeamName | TeamName |  | nvarchar | 255 | 0 | 0 | 
TeamNameForRound | TeamNameForRound | Yes | nvarchar | 255 | 0 | 0 | 
TournamentId | TournamentId |  | bigint | 0 | 19 | 0 | 
VenueCity | VenueCity | Yes | nvarchar | 255 | 0 | 0 | 
VenueDirection | VenueDirection | Yes | nvarchar | 2147483647 | 0 | 0 | 
VenueExtension | VenueExtension | Yes | nvarchar | 255 | 0 | 0 | 
VenueId | VenueId | Yes | bigint | 0 | 19 | 0 | 
VenueLatitude | VenueLatitude | Yes | float | 0 | 38 | 0 | 
VenueLongitude | VenueLongitude | Yes | float | 0 | 38 | 0 | 
VenueModifiedOn | VenueModifiedOn | Yes | datetime | 0 | 0 | 0 | 
VenueName | VenueName | Yes | nvarchar | 255 | 0 | 0 | 
VenuePostalCode | VenuePostalCode | Yes | nvarchar | 10 | 0 | 0 | 
VenuePrecisePosition | VenuePrecisePosition | Yes | bit | 0 | 0 | 0 | 
VenueStreet | VenueStreet | Yes | nvarchar | 255 | 0 | 0 | 


## Code generation information

### Setting values
#### TeamVenueRound (TypedView)
Setting name | Value
--|--
Typed view row base class name | 
Output type | PocoWithQuerySpecQuery

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

#### TournamentId (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

#### VenueCity (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

#### VenueDirection (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

#### VenueExtension (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

#### VenueId (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

#### VenueLatitude (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

#### VenueLongitude (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

#### VenueModifiedOn (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

#### VenueName (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

#### VenuePostalCode (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

#### VenuePrecisePosition (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

#### VenueStreet (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

### Attribute definitions per element

None.

### Additional interface definitions per element

None.

### Additional namespace definitions per element

None.
 