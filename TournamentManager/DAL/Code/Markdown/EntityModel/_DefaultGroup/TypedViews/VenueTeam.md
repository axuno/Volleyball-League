VenueTeam
================

## Fields

The following fields are defined in the VenueTeam typed view 

Name | Type | Optional | Read-only | Max. length | Precision | Scale
--|--
City | `string (System.String)` |  |  | 255 | 0 | 0
Direction | `string (System.String)` |  |  | 2147483647 | 0 | 0
Latitude | `double (System.Double)` | Yes |  | 0 | 38 | 0
Longitude | `double (System.Double)` | Yes |  | 0 | 38 | 0
PostalCode | `string (System.String)` |  |  | 10 | 0 | 0
PrecisePosition | `bool (System.Boolean)` |  |  | 0 | 0 | 0
RoundId | `long (System.Int64)` |  |  | 0 | 19 | 0
Street | `string (System.String)` |  |  | 255 | 0 | 0
TeamClubName | `string (System.String)` | Yes |  | 255 | 0 | 0
TeamId | `long (System.Int64)` |  |  | 0 | 19 | 0
TeamName | `string (System.String)` |  |  | 255 | 0 | 0
TeamNameForRound | `string (System.String)` | Yes |  | 255 | 0 | 0
TournamentId | `long (System.Int64)` |  |  | 0 | 19 | 0
VenueExtension | `string (System.String)` |  |  | 255 | 0 | 0
VenueId | `long (System.Int64)` |  |  | 0 | 19 | 0
VenueName | `string (System.String)` |  |  | 255 | 0 | 0

## Mappings

#### [TestOrg.dbo.VenueTeam](../../../SQL_Server_SqlClient/TestOrg/dbo/VenueTeam.htm) (SQL Server (SqlClient))

Type of target: View

Typed View Field | Target field | Nullable | Type | Length | Precision | Scale | Type converter
--|--
City | City |  | nvarchar | 255 | 0 | 0 | 
Direction | Direction |  | nvarchar | 2147483647 | 0 | 0 | 
Latitude | Latitude | Yes | float | 0 | 38 | 0 | 
Longitude | Longitude | Yes | float | 0 | 38 | 0 | 
PostalCode | PostalCode |  | nvarchar | 10 | 0 | 0 | 
PrecisePosition | PrecisePosition |  | bit | 0 | 0 | 0 | 
RoundId | RoundId |  | bigint | 0 | 19 | 0 | 
Street | Street |  | nvarchar | 255 | 0 | 0 | 
TeamClubName | TeamClubName | Yes | nvarchar | 255 | 0 | 0 | 
TeamId | TeamId |  | bigint | 0 | 19 | 0 | 
TeamName | TeamName |  | nvarchar | 255 | 0 | 0 | 
TeamNameForRound | TeamNameForRound | Yes | nvarchar | 255 | 0 | 0 | 
TournamentId | TournamentId |  | bigint | 0 | 19 | 0 | 
VenueExtension | VenueExtension |  | nvarchar | 255 | 0 | 0 | 
VenueId | VenueId |  | bigint | 0 | 19 | 0 | 
VenueName | VenueName |  | nvarchar | 255 | 0 | 0 | 


## Code generation information

### Setting values
#### VenueTeam (TypedView)
Setting name | Value
--|--
Typed view row base class name | 
Output type | PocoWithQuerySpecQuery

#### City (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

#### Direction (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

#### Latitude (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

#### Longitude (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

#### PostalCode (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

#### PrecisePosition (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

#### RoundId (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

#### Street (TypedViewField)
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

#### VenueExtension (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

#### VenueId (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

#### VenueName (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

### Attribute definitions per element

None.

### Additional interface definitions per element

None.

### Additional namespace definitions per element

None.
 