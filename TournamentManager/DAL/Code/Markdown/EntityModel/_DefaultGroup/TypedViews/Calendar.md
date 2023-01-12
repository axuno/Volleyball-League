Calendar
================

## Fields

The following fields are defined in the Calendar typed view 

Name | Type | Optional | Read-only | Max. length | Precision | Scale
--|--
ChangeSerial | `long (System.Int64)` |  |  | 0 | 19 | 0
GuestTeamId | `long (System.Int64)` |  |  | 0 | 19 | 0
GuestTeamNameForRound | `string (System.String)` | Yes |  | 255 | 0 | 0
HomeTeamId | `long (System.Int64)` |  |  | 0 | 19 | 0
HomeTeamNameForRound | `string (System.String)` | Yes |  | 255 | 0 | 0
Id | `long (System.Int64)` |  |  | 0 | 19 | 0
ModifiedOn | `datetime (System.DateTime)` |  |  | 0 | 0 | 0
PlannedEnd | `datetime (System.DateTime)` | Yes |  | 0 | 0 | 0
PlannedStart | `datetime (System.DateTime)` | Yes |  | 0 | 0 | 0
RoundDescription | `string (System.String)` |  |  | 255 | 0 | 0
RoundId | `long (System.Int64)` |  |  | 0 | 19 | 0
RoundName | `string (System.String)` |  |  | 255 | 0 | 0
RoundTypeDescription | `string (System.String)` |  |  | 255 | 0 | 0
RoundTypeName | `string (System.String)` |  |  | 255 | 0 | 0
TournamentId | `long (System.Int64)` | Yes |  | 0 | 19 | 0
TournamentName | `string (System.String)` |  |  | 255 | 0 | 0
VenueCity | `string (System.String)` | Yes |  | 255 | 0 | 0
VenueDirection | `string (System.String)` | Yes |  | 2147483647 | 0 | 0
VenueExtension | `string (System.String)` | Yes |  | 255 | 0 | 0
VenueId | `long (System.Int64)` | Yes |  | 0 | 19 | 0
VenueLatitude | `double (System.Double)` | Yes |  | 0 | 38 | 0
VenueLongitude | `double (System.Double)` | Yes |  | 0 | 38 | 0
VenueName | `string (System.String)` | Yes |  | 255 | 0 | 0
VenuePostalCode | `string (System.String)` | Yes |  | 10 | 0 | 0
VenueStreet | `string (System.String)` | Yes |  | 255 | 0 | 0

## Mappings

#### [TestOrg.dbo.Calendar](../../../SQL_Server_SqlClient/TestOrg/dbo/Calendar.htm) (SQL Server (SqlClient))

Type of target: View

Typed View Field | Target field | Nullable | Type | Length | Precision | Scale | Type converter
--|--
ChangeSerial | ChangeSerial |  | bigint | 0 | 19 | 0 | 
GuestTeamId | GuestTeamId |  | bigint | 0 | 19 | 0 | 
GuestTeamNameForRound | GuestTeamNameForRound | Yes | nvarchar | 255 | 0 | 0 | 
HomeTeamId | HomeTeamId |  | bigint | 0 | 19 | 0 | 
HomeTeamNameForRound | HomeTeamNameForRound | Yes | nvarchar | 255 | 0 | 0 | 
Id | Id |  | bigint | 0 | 19 | 0 | 
ModifiedOn | ModifiedOn |  | datetime | 0 | 0 | 0 | 
PlannedEnd | PlannedEnd | Yes | datetime | 0 | 0 | 0 | 
PlannedStart | PlannedStart | Yes | datetime | 0 | 0 | 0 | 
RoundDescription | RoundDescription |  | nvarchar | 255 | 0 | 0 | 
RoundId | RoundId |  | bigint | 0 | 19 | 0 | 
RoundName | RoundName |  | nvarchar | 255 | 0 | 0 | 
RoundTypeDescription | RoundTypeDescription |  | nvarchar | 255 | 0 | 0 | 
RoundTypeName | RoundTypeName |  | nvarchar | 255 | 0 | 0 | 
TournamentId | TournamentId | Yes | bigint | 0 | 19 | 0 | 
TournamentName | TournamentName |  | nvarchar | 255 | 0 | 0 | 
VenueCity | VenueCity | Yes | nvarchar | 255 | 0 | 0 | 
VenueDirection | VenueDirection | Yes | nvarchar | 2147483647 | 0 | 0 | 
VenueExtension | VenueExtension | Yes | nvarchar | 255 | 0 | 0 | 
VenueId | VenueId | Yes | bigint | 0 | 19 | 0 | 
VenueLatitude | VenueLatitude | Yes | float | 0 | 38 | 0 | 
VenueLongitude | VenueLongitude | Yes | float | 0 | 38 | 0 | 
VenueName | VenueName | Yes | nvarchar | 255 | 0 | 0 | 
VenuePostalCode | VenuePostalCode | Yes | nvarchar | 10 | 0 | 0 | 
VenueStreet | VenueStreet | Yes | nvarchar | 255 | 0 | 0 | 


## Code generation information

### Setting values
#### Calendar (TypedView)
Setting name | Value
--|--
Typed view row base class name | 
Output type | PocoWithQuerySpecQuery

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

#### ModifiedOn (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

#### PlannedEnd (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

#### PlannedStart (TypedViewField)
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

#### TournamentId (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

#### TournamentName (TypedViewField)
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

#### VenueName (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

#### VenuePostalCode (TypedViewField)
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
 