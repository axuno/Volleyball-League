Team
================

## Inheritance hierarchy

--|--
Hierarchy type | None
Is abstract | False

## Relationships

The Team entity is part of the following relationships 

Related Entity | Full description 
--|--
[AvailableMatchDate](../../_DefaultGroup/Entities/AvailableMatchDate.htm) | AvailableMatchDate.Team - Team.AvailableMatchDates (m:1) 
[ExcludeMatchDate](../../_DefaultGroup/Entities/ExcludeMatchDate.htm) | ExcludeMatchDate.Team - Team.ExcludeMatchDates (m:1) 
[ManagerOfTeam](../../_DefaultGroup/Entities/ManagerOfTeam.htm) | ManagerOfTeam.Team - Team.ManagerOfTeams (m:1) 
[Match](../../_DefaultGroup/Entities/Match.htm) | Match.GuestTeam - Team.MatchGuestTeam (m:1) 
[Match](../../_DefaultGroup/Entities/Match.htm) | Match.HomeTeam - Team.MatchHomeTeam (m:1) 
[Match](../../_DefaultGroup/Entities/Match.htm) | Match.RefereeTeam - Team.MatchReferee (m:1) 
[PlayerInTeam](../../_DefaultGroup/Entities/PlayerInTeam.htm) | PlayerInTeam.Team - Team.PlayerInTeams (m:1) 
[Ranking](../../_DefaultGroup/Entities/Ranking.htm) | Ranking.Team - Team.Rankings (m:1) 
[Registration](../../_DefaultGroup/Entities/Registration.htm) | Registration.Team - Team.Registration (m:1) 
[Round](../../_DefaultGroup/Entities/Round.htm) | Team.RoundCollectionViaTeamsInRounds - Round.TeamCollectionViaTeamInRound (m:n) (via TeamInRound) 
[TeamInRound](../../_DefaultGroup/Entities/TeamInRound.htm) | TeamInRound.Team - Team.TeamInRounds (m:1) 
[Venue](../../_DefaultGroup/Entities/Venue.htm) | Team.Venue - Venue.Teams (m:1) 

## Fields

The following fields are defined in the Team entity 

Name | Type | Is PK | Is FK | Optional | Read-only | Max. length | Precision | Scale
--|--
Id | `long (System.Int64)` |  Yes |  |  | Yes | 0 | 11 | 0
VenueId | `long (System.Int64)` |   | Yes | Yes |  | 0 | 11 | 0
Name | `string (System.String)` |   |  |  |  | 255 | 0 | 0
MatchDayOfWeek | `int (System.Int32)` |   |  | Yes |  | 0 | 6 | 0
MatchTime | `timespan (System.TimeSpan)` |   |  | Yes |  | 0 | 0 | 0
ClubName | `string (System.String)` |   |  | Yes |  | 255 | 0 | 0
CreatedOn | `datetime (System.DateTime)` |   |  |  |  | 0 | 0 | 0
ModifiedOn | `datetime (System.DateTime)` |   |  |  |  | 0 | 0 | 0

### Unique Constraints
None.

### Fields mapped onto related fields
None.

## Mappings

#### [TestOrg.dbo.Team](../../../SQL_Server_SqlClient/TestOrg/dbo/Team.htm) (SQL Server (SqlClient))

Aspect | Value
--|--
Type of target | Table
Actions allowed | Create / Retrieve / Update / Delete

Entity Field | Target field | Nullable | Type | Length | Precision | Scale | Sequence | Type converter
--|--
ClubName | ClubName | Yes | nvarchar | 255 | 0 | 0 |  | 
CreatedOn | CreatedOn |  | datetime | 0 | 0 | 0 |  | 
Id | Id |  | bigint | 0 | 19 | 0 | SCOPE_IDENTITY() | 
MatchDayOfWeek | MatchDayOfWeek | Yes | int | 0 | 10 | 0 |  | 
MatchTime | MatchTime | Yes | time | 0 | 0 | 0 |  | 
ModifiedOn | ModifiedOn |  | datetime | 0 | 0 | 0 |  | 
Name | Name |  | nvarchar | 255 | 0 | 0 |  | 
VenueId | VenueId | Yes | bigint | 0 | 19 | 0 |  | 

## Code generation information

### Setting values
#### Team (Entity)
Setting name | Value
--|--
Entity base class name | `CommonEntityBase`

#### ClubName (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True
Field property has private setter | False

#### CreatedOn (NormalField)
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

#### MatchDayOfWeek (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True
Field property has private setter | False

#### MatchTime (NormalField)
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

#### Name (NormalField)
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

#### AvailableMatchDates (NavigatorCollection)
Setting name | Value
--|--
Navigator property is public | True

#### ExcludeMatchDates (NavigatorCollection)
Setting name | Value
--|--
Navigator property is public | True

#### ManagerOfTeams (NavigatorCollection)
Setting name | Value
--|--
Navigator property is public | True

#### MatchGuestTeam (NavigatorCollection)
Setting name | Value
--|--
Navigator property is public | True

#### MatchHomeTeam (NavigatorCollection)
Setting name | Value
--|--
Navigator property is public | True

#### MatchReferee (NavigatorCollection)
Setting name | Value
--|--
Navigator property is public | True

#### PlayerInTeams (NavigatorCollection)
Setting name | Value
--|--
Navigator property is public | True

#### Rankings (NavigatorCollection)
Setting name | Value
--|--
Navigator property is public | True

#### Registration (NavigatorCollection)
Setting name | Value
--|--
Navigator property is public | True

#### RoundCollectionViaTeamsInRounds (NavigatorCollection)
Setting name | Value
--|--
Navigator property is public | True

#### TeamInRounds (NavigatorCollection)
Setting name | Value
--|--
Navigator property is public | True

#### Venue (NavigatorSingleValue)
Setting name | Value
--|--
Navigator property is public | True

### Attribute definitions per element

#### Venue (NavigatorSingleValue)

* `Browsable($false)`


### Additional interface definitions per element

None.

### Additional namespace definitions per element

None.
