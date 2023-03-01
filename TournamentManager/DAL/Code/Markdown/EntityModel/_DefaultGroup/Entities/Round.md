Round
================

## Inheritance hierarchy

--|--
Hierarchy type | None
Is abstract | False

## Relationships

The Round entity is part of the following relationships 

Related Entity | Full description 
--|--
[ExcludeMatchDate](../../_DefaultGroup/Entities/ExcludeMatchDate.htm) | ExcludeMatchDate.Round - Round.ExcludeMatchDates (m:1) 
[Match](../../_DefaultGroup/Entities/Match.htm) | Match.Round - Round.Matches (m:1) 
[MatchRule](../../_DefaultGroup/Entities/MatchRule.htm) | Round.MatchRule - MatchRule.Rounds (m:1) 
[Ranking](../../_DefaultGroup/Entities/Ranking.htm) | Ranking.Round - Round.Rankings (m:1) 
[Round](../../_DefaultGroup/Entities/Round.htm) | Round.PreviousRound - Round.NextRound (1:n) 
[RoundLeg](../../_DefaultGroup/Entities/RoundLeg.htm) | RoundLeg.Round - Round.RoundLegs (m:1) 
[RoundType](../../_DefaultGroup/Entities/RoundType.htm) | Round.RoundType - RoundType.Rounds (m:1) 
[SetRule](../../_DefaultGroup/Entities/SetRule.htm) | Round.SetRule - SetRule.Rounds (m:1) 
[Team](../../_DefaultGroup/Entities/Team.htm) | Team.RoundCollectionViaTeamsInRounds - Round.TeamCollectionViaTeamInRound (m:n) (via TeamInRound) 
[TeamInRound](../../_DefaultGroup/Entities/TeamInRound.htm) | TeamInRound.Round - Round.TeamInRounds (m:1) 
[Tournament](../../_DefaultGroup/Entities/Tournament.htm) | Round.Tournament - Tournament.Rounds (m:1) 

## Fields

The following fields are defined in the Round entity 

Name | Type | Is PK | Is FK | Optional | Read-only | Max. length | Precision | Scale
--|--
Id | `long (System.Int64)` |  Yes |  |  | Yes | 0 | 11 | 0
TournamentId | `long (System.Int64)` |   | Yes | Yes |  | 0 | 11 | 0
Name | `string (System.String)` |   |  |  |  | 255 | 0 | 0
Description | `string (System.String)` |   |  |  |  | 255 | 0 | 0
TypeId | `long (System.Int64)` |   | Yes |  |  | 0 | 11 | 0
NumOfLegs | `int (System.Int32)` |   |  |  |  | 0 | 11 | 0
MatchRuleId | `long (System.Int64)` |   | Yes |  |  | 0 | 19 | 0
SetRuleId | `long (System.Int64)` |   | Yes |  |  | 0 | 19 | 0
IsComplete | `bool (System.Boolean)` |   |  |  |  | 0 | 1 | 0
NextRoundId | `long (System.Int64)` |   | Yes | Yes |  | 0 | 11 | 0
CreatedOn | `datetime (System.DateTime)` |   |  |  |  | 0 | 0 | 0
ModifiedOn | `datetime (System.DateTime)` |   |  |  |  | 0 | 0 | 0

### Unique Constraints
None.

### Fields mapped onto related fields
None.

## Mappings

#### [TestOrg.dbo.Round](../../../SQL_Server_SqlClient/TestOrg/dbo/Round.htm) (SQL Server (SqlClient))

Aspect | Value
--|--
Type of target | Table
Actions allowed | Create / Retrieve / Update / Delete

Entity Field | Target field | Nullable | Type | Length | Precision | Scale | Sequence | Type converter
--|--
CreatedOn | CreatedOn |  | datetime | 0 | 0 | 0 |  | 
Description | Description |  | nvarchar | 255 | 0 | 0 |  | 
Id | Id |  | bigint | 0 | 19 | 0 | SCOPE_IDENTITY() | 
IsComplete | IsComplete |  | bit | 0 | 0 | 0 |  | 
MatchRuleId | MatchRuleId |  | bigint | 0 | 19 | 0 |  | 
ModifiedOn | ModifiedOn |  | datetime | 0 | 0 | 0 |  | 
Name | Name |  | nvarchar | 255 | 0 | 0 |  | 
NextRoundId | NextRoundId | Yes | bigint | 0 | 19 | 0 |  | 
NumOfLegs | NumOfLegs |  | int | 0 | 10 | 0 |  | 
SetRuleId | SetRuleId |  | bigint | 0 | 19 | 0 |  | 
TournamentId | TournamentId | Yes | bigint | 0 | 19 | 0 |  | 
TypeId | TypeId |  | bigint | 0 | 19 | 0 |  | 

## Code generation information

### Setting values
#### Round (Entity)
Setting name | Value
--|--
Entity base class name | `CommonEntityBase`

#### CreatedOn (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True

#### Description (NormalField)
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

#### MatchRuleId (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True

#### ModifiedOn (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True

#### Name (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True

#### NextRoundId (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True

#### NumOfLegs (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True

#### SetRuleId (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True

#### TournamentId (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True

#### TypeId (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True

#### ExcludeMatchDates (NavigatorCollection)
Setting name | Value
--|--
Navigator property is public | True

#### Matches (NavigatorCollection)
Setting name | Value
--|--
Navigator property is public | True

#### MatchRule (NavigatorSingleValue)
Setting name | Value
--|--
Navigator property is public | True

#### NextRound (NavigatorSingleValue)
Setting name | Value
--|--
Navigator property is public | True

#### PreviousRound (NavigatorCollection)
Setting name | Value
--|--
Navigator property is public | True

#### Rankings (NavigatorCollection)
Setting name | Value
--|--
Navigator property is public | True

#### RoundLegs (NavigatorCollection)
Setting name | Value
--|--
Navigator property is public | True

#### RoundType (NavigatorSingleValue)
Setting name | Value
--|--
Navigator property is public | True

#### SetRule (NavigatorSingleValue)
Setting name | Value
--|--
Navigator property is public | True

#### TeamCollectionViaTeamInRound (NavigatorCollection)
Setting name | Value
--|--
Navigator property is public | True

#### TeamInRounds (NavigatorCollection)
Setting name | Value
--|--
Navigator property is public | True

#### Tournament (NavigatorSingleValue)
Setting name | Value
--|--
Navigator property is public | True

### Attribute definitions per element

#### MatchRule (NavigatorSingleValue)

* `Browsable($false)`

#### NextRound (NavigatorSingleValue)

* `Browsable($false)`

#### RoundType (NavigatorSingleValue)

* `Browsable($false)`

#### SetRule (NavigatorSingleValue)

* `Browsable($false)`

#### Tournament (NavigatorSingleValue)

* `Browsable($false)`


### Additional interface definitions per element

None.

### Additional namespace definitions per element

None.
