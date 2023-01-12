ManagerOfTeam
================

## Inheritance hierarchy

--|--
Hierarchy type | None
Is abstract | False

## Relationships

The ManagerOfTeam entity is part of the following relationships 

Related Entity | Full description 
--|--
[Team](../../_DefaultGroup/Entities/Team.htm) | ManagerOfTeam.Team - Team.ManagerOfTeams (m:1) 
[User](../../_DefaultGroup/Entities/User.htm) | ManagerOfTeam.User - User.ManagerOfTeams (m:1) 

## Fields

The following fields are defined in the ManagerOfTeam entity 

Name | Type | Is PK | Is FK | Optional | Read-only | Max. length | Precision | Scale
--|--
Id | `long (System.Int64)` |   |  |  | Yes | 0 | 11 | 0
UserId | `long (System.Int64)` |  Yes | Yes |  |  | 0 | 11 | 0
TeamId | `long (System.Int64)` |  Yes | Yes |  |  | 0 | 11 | 0
CreatedOn | `datetime (System.DateTime)` |   |  |  |  | 0 | 0 | 0
ModifiedOn | `datetime (System.DateTime)` |   |  |  |  | 0 | 0 | 0

### Unique Constraints
None.

### Fields mapped onto related fields
None.

## Mappings

#### [TestOrg.dbo.ManagerOfTeam](../../../SQL_Server_SqlClient/TestOrg/dbo/ManagerOfTeam.htm) (SQL Server (SqlClient))

Aspect | Value
--|--
Type of target | Table
Actions allowed | Create / Retrieve / Update / Delete

Entity Field | Target field | Nullable | Type | Length | Precision | Scale | Sequence | Type converter
--|--
CreatedOn | CreatedOn |  | datetime | 0 | 0 | 0 |  | 
Id | Id |  | bigint | 0 | 19 | 0 | SCOPE_IDENTITY() | 
ModifiedOn | ModifiedOn |  | datetime | 0 | 0 | 0 |  | 
TeamId | TeamId |  | bigint | 0 | 19 | 0 |  | 
UserId | UserId |  | bigint | 0 | 19 | 0 |  | 

## Code generation information

### Setting values
#### ManagerOfTeam (Entity)
Setting name | Value
--|--
Entity base class name | `CommonEntityBase`

#### CreatedOn (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True

#### Id (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True

#### ModifiedOn (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True

#### TeamId (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True

#### UserId (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True

#### Team (NavigatorSingleValue)
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

#### User (NavigatorSingleValue)

* `Browsable($false)`


### Additional interface definitions per element

None.

### Additional namespace definitions per element

None.
