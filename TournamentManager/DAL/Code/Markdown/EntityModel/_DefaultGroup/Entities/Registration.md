Registration
================

## Inheritance hierarchy

--|--
Hierarchy type | None
Is abstract | False

## Relationships

The Registration entity is part of the following relationships 

Related Entity | Full description 
--|--
[Team](../../_DefaultGroup/Entities/Team.htm) | Registration.Team - Team.Registration (m:1) 
[Tournament](../../_DefaultGroup/Entities/Tournament.htm) | Registration.Tournament - Tournament.Registrations (m:1) 
[User](../../_DefaultGroup/Entities/User.htm) | Registration.User - User.Registrations (m:1) 

## Fields

The following fields are defined in the Registration entity 

Name | Type | Is PK | Is FK | Optional | Read-only | Max. length | Precision | Scale
--|--
Id | `long (System.Int64)` |  Yes |  |  | Yes | 0 | 11 | 0
TournamentId | `long (System.Int64)` |   | Yes |  |  | 0 | 11 | 0
TeamId | `long (System.Int64)` |   | Yes |  |  | 0 | 11 | 0
ManagerId | `long (System.Int64)` |   | Yes |  |  | 0 | 11 | 0
Guid | `string (System.String)` |   |  |  |  | 50 | 0 | 0
AppliedWithMessage | `string (System.String)` |   |  | Yes |  | 4000 | 0 | 0
AppliedOn | `datetime (System.DateTime)` |   |  |  |  | 0 | 0 | 0

### Unique Constraints
None.

### Fields mapped onto related fields
None.

## Mappings

#### [TestOrg.dbo.Registration](../../../SQL_Server_SqlClient/TestOrg/dbo/Registration.htm) (SQL Server (SqlClient))

Aspect | Value
--|--
Type of target | Table
Actions allowed | Create / Retrieve / Update / Delete

Entity Field | Target field | Nullable | Type | Length | Precision | Scale | Sequence | Type converter
--|--
AppliedOn | AppliedOn |  | datetime | 0 | 0 | 0 |  | 
AppliedWithMessage | AppliedWithMessage | Yes | nvarchar | 4000 | 0 | 0 |  | 
Guid | Guid |  | nvarchar | 50 | 0 | 0 |  | 
Id | Id |  | bigint | 0 | 19 | 0 | SCOPE_IDENTITY() | 
ManagerId | ManagerId |  | bigint | 0 | 19 | 0 |  | 
TeamId | TeamId |  | bigint | 0 | 19 | 0 |  | 
TournamentId | TournamentId |  | bigint | 0 | 19 | 0 |  | 

## Code generation information

### Setting values
#### Registration (Entity)
Setting name | Value
--|--
Entity base class name | `CommonEntityBase`

#### AppliedOn (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True

#### AppliedWithMessage (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True

#### Guid (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True

#### Id (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True

#### ManagerId (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True

#### TeamId (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True

#### TournamentId (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True

#### Team (NavigatorSingleValue)
Setting name | Value
--|--
Navigator property is public | True

#### Tournament (NavigatorSingleValue)
Setting name | Value
--|--
Navigator property is public | True

#### User (NavigatorSingleValue)
Setting name | Value
--|--
Navigator property is public | True

### Attribute definitions per element

#### Team (NavigatorSingleValue)

* `Browsable($false)`

#### Tournament (NavigatorSingleValue)

* `Browsable($false)`

#### User (NavigatorSingleValue)

* `Browsable($false)`


### Additional interface definitions per element

None.

### Additional namespace definitions per element

None.
