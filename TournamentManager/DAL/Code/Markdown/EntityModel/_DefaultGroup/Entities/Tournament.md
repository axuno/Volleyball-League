Tournament
================

## Inheritance hierarchy

--|--
Hierarchy type | None
Is abstract | False

## Relationships

The Tournament entity is part of the following relationships 

Related Entity | Full description 
--|--
[ExcludeMatchDate](../../_DefaultGroup/Entities/ExcludeMatchDate.htm) | ExcludeMatchDate.Tournament - Tournament.ExcludeMatchDates (m:1) 
[Ranking](../../_DefaultGroup/Entities/Ranking.htm) | Ranking.Tournament - Tournament.Rankings (m:1) 
[Registration](../../_DefaultGroup/Entities/Registration.htm) | Registration.Tournament - Tournament.Registrations (m:1) 
[Round](../../_DefaultGroup/Entities/Round.htm) | Round.Tournament - Tournament.Rounds (m:1) 
[Tournament](../../_DefaultGroup/Entities/Tournament.htm) | Tournament.Tournament - Tournament.Tournaments (m:1) 
[TournamentType](../../_DefaultGroup/Entities/TournamentType.htm) | Tournament.TournamentType - TournamentType.Tournaments (m:1) 

## Fields

The following fields are defined in the Tournament entity 

Name | Type | Is PK | Is FK | Optional | Read-only | Max. length | Precision | Scale
--|--
Id | `long (System.Int64)` |  Yes |  |  | Yes | 0 | 11 | 0
Name | `string (System.String)` |   |  |  |  | 255 | 0 | 0
Description | `string (System.String)` |   |  | Yes |  | 255 | 0 | 0
TypeId | `long (System.Int64)` |   | Yes | Yes |  | 0 | 11 | 0
IsComplete | `bool (System.Boolean)` |   |  |  |  | 0 | 1 | 0
IsPlanningMode | `bool (System.Boolean)` |   |  |  |  | 0 | 1 | 0
NextTournamentId | `long (System.Int64)` |   | Yes | Yes |  | 0 | 11 | 0
CreatedOn | `datetime (System.DateTime)` |   |  |  |  | 0 | 0 | 0
ModifiedOn | `datetime (System.DateTime)` |   |  |  |  | 0 | 0 | 0

### Unique Constraints
None.

### Fields mapped onto related fields
None.

## Mappings

#### [TestOrg.dbo.Tournament](../../../SQL_Server_SqlClient/TestOrg/dbo/Tournament.htm) (SQL Server (SqlClient))

Aspect | Value
--|--
Type of target | Table
Actions allowed | Create / Retrieve / Update / Delete

Entity Field | Target field | Nullable | Type | Length | Precision | Scale | Sequence | Type converter
--|--
CreatedOn | CreatedOn |  | datetime | 0 | 0 | 0 |  | 
Description | Description | Yes | nvarchar | 255 | 0 | 0 |  | 
Id | Id |  | bigint | 0 | 19 | 0 | SCOPE_IDENTITY() | 
IsComplete | IsComplete |  | bit | 0 | 0 | 0 |  | 
IsPlanningMode | IsPlanningMode |  | bit | 0 | 0 | 0 |  | 
ModifiedOn | ModifiedOn |  | datetime | 0 | 0 | 0 |  | 
Name | Name |  | nvarchar | 255 | 0 | 0 |  | 
NextTournamentId | NextTournamentId | Yes | bigint | 0 | 19 | 0 |  | 
TypeId | TypeId | Yes | bigint | 0 | 19 | 0 |  | 

## Code generation information

### Setting values
#### Tournament (Entity)
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

#### IsPlanningMode (NormalField)
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

#### NextTournamentId (NormalField)
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

#### Rankings (NavigatorCollection)
Setting name | Value
--|--
Navigator property is public | True

#### Registrations (NavigatorCollection)
Setting name | Value
--|--
Navigator property is public | True

#### Rounds (NavigatorCollection)
Setting name | Value
--|--
Navigator property is public | True

#### Tournament (NavigatorSingleValue)
Setting name | Value
--|--
Navigator property is public | True

#### Tournaments (NavigatorCollection)
Setting name | Value
--|--
Navigator property is public | True

#### TournamentType (NavigatorSingleValue)
Setting name | Value
--|--
Navigator property is public | True

### Attribute definitions per element

#### Tournament (NavigatorSingleValue)

* `Browsable($false)`

#### TournamentType (NavigatorSingleValue)

* `Browsable($false)`


### Additional interface definitions per element

None.

### Additional namespace definitions per element

None.
