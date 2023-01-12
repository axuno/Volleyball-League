Match
================

## Inheritance hierarchy

--|--
Hierarchy type | None
Is abstract | False

## Relationships

The Match entity is part of the following relationships 

Related Entity | Full description 
--|--
[Round](../../_DefaultGroup/Entities/Round.htm) | Match.Round - Round.Matches (m:1) 
[Set](../../_DefaultGroup/Entities/Set.htm) | Set.Match - Match.Sets (m:1) 
[Team](../../_DefaultGroup/Entities/Team.htm) | Match.GuestTeam - Team.MatchGuestTeam (m:1) 
[Team](../../_DefaultGroup/Entities/Team.htm) | Match.HomeTeam - Team.MatchHomeTeam (m:1) 
[Team](../../_DefaultGroup/Entities/Team.htm) | Match.RefereeTeam - Team.MatchReferee (m:1) 
[Venue](../../_DefaultGroup/Entities/Venue.htm) | Match.OrigVenue - Venue.OrigVenueMatches (m:1) 
[Venue](../../_DefaultGroup/Entities/Venue.htm) | Match.Venue - Venue.CurrentVenueMatches (m:1) 

## Fields

The following fields are defined in the Match entity 

Name | Type | Is PK | Is FK | Optional | Read-only | Max. length | Precision | Scale
--|--
Id | `long (System.Int64)` |  Yes |  |  | Yes | 0 | 11 | 0
HomeTeamId | `long (System.Int64)` |   | Yes |  |  | 0 | 11 | 0
GuestTeamId | `long (System.Int64)` |   | Yes |  |  | 0 | 11 | 0
RefereeId | `long (System.Int64)` |   | Yes | Yes |  | 0 | 11 | 0
RoundId | `long (System.Int64)` |   | Yes |  |  | 0 | 11 | 0
VenueId | `long (System.Int64)` |   | Yes | Yes |  | 0 | 11 | 0
OrigVenueId | `long (System.Int64)` |   | Yes | Yes |  | 0 | 11 | 0
LegSequenceNo | `int (System.Int32)` |   |  | Yes |  | 0 | 2 | 0
HomePoints | `int (System.Int32)` |   |  | Yes |  | 0 | 2 | 0
GuestPoints | `int (System.Int32)` |   |  | Yes |  | 0 | 2 | 0
IsComplete | `bool (System.Boolean)` |   |  |  |  | 0 | 1 | 0
IsOverruled | `bool (System.Boolean)` |   |  |  |  | 0 | 1 | 0
PlannedStart | `datetime (System.DateTime)` |   |  | Yes |  | 0 | 0 | 0
PlannedEnd | `datetime (System.DateTime)` |   |  | Yes |  | 0 | 0 | 0
OrigPlannedStart | `datetime (System.DateTime)` |   |  | Yes |  | 0 | 0 | 0
OrigPlannedEnd | `datetime (System.DateTime)` |   |  | Yes |  | 0 | 0 | 0
RealStart | `datetime (System.DateTime)` |   |  | Yes |  | 0 | 0 | 0
RealEnd | `datetime (System.DateTime)` |   |  | Yes |  | 0 | 0 | 0
Remarks | `string (System.String)` |   |  | Yes |  | 2147483647 | 0 | 0
ChangeSerial | `long (System.Int64)` |   |  |  |  | 0 | 2 | 0
CreatedOn | `datetime (System.DateTime)` |   |  |  |  | 0 | 0 | 0
ModifiedOn | `datetime (System.DateTime)` |   |  |  |  | 0 | 0 | 0

### Unique Constraints
None.

### Fields mapped onto related fields
None.

## Mappings

#### [TestOrg.dbo.Match](../../../SQL_Server_SqlClient/TestOrg/dbo/Match.htm) (SQL Server (SqlClient))

Aspect | Value
--|--
Type of target | Table
Actions allowed | Create / Retrieve / Update / Delete

Entity Field | Target field | Nullable | Type | Length | Precision | Scale | Sequence | Type converter
--|--
ChangeSerial | ChangeSerial |  | bigint | 0 | 19 | 0 |  | 
CreatedOn | CreatedOn |  | datetime | 0 | 0 | 0 |  | 
GuestPoints | GuestPoints | Yes | int | 0 | 10 | 0 |  | 
GuestTeamId | GuestTeamId |  | bigint | 0 | 19 | 0 |  | 
HomePoints | HomePoints | Yes | int | 0 | 10 | 0 |  | 
HomeTeamId | HomeTeamId |  | bigint | 0 | 19 | 0 |  | 
Id | Id |  | bigint | 0 | 19 | 0 | SCOPE_IDENTITY() | 
IsComplete | IsComplete |  | bit | 0 | 0 | 0 |  | 
IsOverruled | IsOverruled |  | bit | 0 | 0 | 0 |  | 
LegSequenceNo | LegSequenceNo | Yes | int | 0 | 10 | 0 |  | 
ModifiedOn | ModifiedOn |  | datetime | 0 | 0 | 0 |  | 
OrigPlannedEnd | OrigPlannedEnd | Yes | datetime | 0 | 0 | 0 |  | 
OrigPlannedStart | OrigPlannedStart | Yes | datetime | 0 | 0 | 0 |  | 
OrigVenueId | OrigVenueId | Yes | bigint | 0 | 19 | 0 |  | 
PlannedEnd | PlannedEnd | Yes | datetime | 0 | 0 | 0 |  | 
PlannedStart | PlannedStart | Yes | datetime | 0 | 0 | 0 |  | 
RealEnd | RealEnd | Yes | datetime | 0 | 0 | 0 |  | 
RealStart | RealStart | Yes | datetime | 0 | 0 | 0 |  | 
RefereeId | RefereeId | Yes | bigint | 0 | 19 | 0 |  | 
Remarks | Remarks | Yes | nvarchar | 2147483647 | 0 | 0 |  | 
RoundId | RoundId |  | bigint | 0 | 19 | 0 |  | 
VenueId | VenueId | Yes | bigint | 0 | 19 | 0 |  | 

## Code generation information

### Setting values
#### Match (Entity)
Setting name | Value
--|--
Entity base class name | `CommonEntityBase`

#### ChangeSerial (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True

#### CreatedOn (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True

#### GuestPoints (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True

#### GuestTeamId (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True

#### HomePoints (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True

#### HomeTeamId (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True

#### Id (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True

#### IsComplete (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True

#### IsOverruled (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True

#### LegSequenceNo (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True

#### ModifiedOn (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True

#### OrigPlannedEnd (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True

#### OrigPlannedStart (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True

#### OrigVenueId (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True

#### PlannedEnd (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True

#### PlannedStart (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True

#### RealEnd (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True

#### RealStart (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True

#### RefereeId (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True

#### Remarks (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True

#### RoundId (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True

#### VenueId (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True

#### GuestTeam (NavigatorSingleValue)
Setting name | Value
--|--
Navigator property is public | True

#### HomeTeam (NavigatorSingleValue)
Setting name | Value
--|--
Navigator property is public | True

#### OrigVenue (NavigatorSingleValue)
Setting name | Value
--|--
Navigator property is public | True

#### RefereeTeam (NavigatorSingleValue)
Setting name | Value
--|--
Navigator property is public | True

#### Round (NavigatorSingleValue)
Setting name | Value
--|--
Navigator property is public | True

#### Sets (NavigatorCollection)
Setting name | Value
--|--
Navigator property is public | True

#### Venue (NavigatorSingleValue)
Setting name | Value
--|--
Navigator property is public | True

### Attribute definitions per element

#### GuestTeam (NavigatorSingleValue)

* `Browsable($false)`

#### HomeTeam (NavigatorSingleValue)

* `Browsable($false)`

#### OrigVenue (NavigatorSingleValue)

* `Browsable($false)`

#### RefereeTeam (NavigatorSingleValue)

* `Browsable($false)`

#### Round (NavigatorSingleValue)

* `Browsable($false)`

#### Venue (NavigatorSingleValue)

* `Browsable($false)`


### Additional interface definitions per element

None.

### Additional namespace definitions per element

None.
