AvailableMatchDate
================

## Inheritance hierarchy

--|--
Hierarchy type | None
Is abstract | False

## Relationships

The AvailableMatchDate entity is part of the following relationships 

Related Entity | Full description 
--|--
[Team](../../_DefaultGroup/Entities/Team.htm) | AvailableMatchDate.Team - Team.AvailableMatchDates (m:1) 
[Venue](../../_DefaultGroup/Entities/Venue.htm) | AvailableMatchDate.Venue - Venue.AvailableMatchDates (m:1) 

## Fields

The following fields are defined in the AvailableMatchDate entity 

Name | Type | Is PK | Is FK | Optional | Read-only | Max. length | Precision | Scale
--|--
Id | `long (System.Int64)` |  Yes |  |  | Yes | 0 | 11 | 0
TournamentId | `long (System.Int64)` |   |  |  |  | 0 | 11 | 0
HomeTeamId | `long (System.Int64)` |   | Yes |  |  | 0 | 11 | 0
VenueId | `long (System.Int64)` |   | Yes |  |  | 0 | 11 | 0
MatchStartTime | `datetime (System.DateTime)` |   |  |  |  | 0 | 0 | 0
MatchEndTime | `datetime (System.DateTime)` |   |  |  |  | 0 | 0 | 0
IsGenerated | `bool (System.Boolean)` |   |  |  |  | 0 | 4 | 0
CreatedOn | `datetime (System.DateTime)` |   |  |  |  | 0 | 0 | 0
ModifiedOn | `datetime (System.DateTime)` |   |  |  |  | 0 | 0 | 0

### Unique Constraints
None.

### Fields mapped onto related fields
None.

## Mappings

#### [TestOrg.dbo.AvailableMatchDate](../../../SQL_Server_SqlClient/TestOrg/dbo/AvailableMatchDate.htm) (SQL Server (SqlClient))

Aspect | Value
--|--
Type of target | Table
Actions allowed | Create / Retrieve / Update / Delete

Entity Field | Target field | Nullable | Type | Length | Precision | Scale | Sequence | Type converter
--|--
CreatedOn | CreatedOn |  | datetime | 0 | 0 | 0 |  | 
HomeTeamId | HomeTeamId |  | bigint | 0 | 19 | 0 |  | 
Id | Id |  | bigint | 0 | 19 | 0 | SCOPE_IDENTITY() | 
IsGenerated | IsGenerated |  | bit | 0 | 0 | 0 |  | 
MatchEndTime | MatchEndTime |  | datetime | 0 | 0 | 0 |  | 
MatchStartTime | MatchStartTime |  | datetime | 0 | 0 | 0 |  | 
ModifiedOn | ModifiedOn |  | datetime | 0 | 0 | 0 |  | 
TournamentId | TournamentId |  | bigint | 0 | 19 | 0 |  | 
VenueId | VenueId |  | bigint | 0 | 19 | 0 |  | 

## Code generation information

### Setting values
#### AvailableMatchDate (Entity)
Setting name | Value
--|--
Entity base class name | `CommonEntityBase`

#### CreatedOn (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True
Field property has private setter | False

#### HomeTeamId (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True
Field property has private setter | False

#### Id (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True
Field property has private setter | False

#### IsGenerated (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True
Field property has private setter | False

#### MatchEndTime (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True
Field property has private setter | False

#### MatchStartTime (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True
Field property has private setter | False

#### ModifiedOn (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True
Field property has private setter | False

#### TournamentId (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True
Field property has private setter | False

#### VenueId (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True
Field property has private setter | False

#### Team (NavigatorSingleValue)
Setting name | Value
--|--
Navigator property is public | True

#### Venue (NavigatorSingleValue)
Setting name | Value
--|--
Navigator property is public | True

### Attribute definitions per element

#### Team (NavigatorSingleValue)

* `Browsable($false)`

#### Venue (NavigatorSingleValue)

* `Browsable($false)`


### Additional interface definitions per element

None.

### Additional namespace definitions per element

None.
