IdentityUserRole
================

## Inheritance hierarchy

--|--
Hierarchy type | None
Is abstract | False

## Relationships

The IdentityUserRole entity is part of the following relationships 

Related Entity | Full description 
--|--
[IdentityRole](../../_DefaultGroup/Entities/IdentityRole.htm) | IdentityUserRole.IdentityRole - IdentityRole.IdentityUserRoles (m:1) 
[User](../../_DefaultGroup/Entities/User.htm) | IdentityUserRole.User - User.IdentityUserRoles (m:1) 

## Fields

The following fields are defined in the IdentityUserRole entity 

Name | Type | Is PK | Is FK | Optional | Read-only | Max. length | Precision | Scale
--|--
Id | `long (System.Int64)` |   |  |  | Yes | 0 | 19 | 0
RoleId | `long (System.Int64)` |  Yes | Yes |  |  | 0 | 19 | 0
UserId | `long (System.Int64)` |  Yes | Yes |  |  | 0 | 11 | 0

### Unique Constraints
None.

### Fields mapped onto related fields
None.

## Mappings

#### [TestOrg.dbo.IdentityUserRole](../../../SQL_Server_SqlClient/TestOrg/dbo/IdentityUserRole.htm) (SQL Server (SqlClient))

Aspect | Value
--|--
Type of target | Table
Actions allowed | Create / Retrieve / Update / Delete

Entity Field | Target field | Nullable | Type | Length | Precision | Scale | Sequence | Type converter
--|--
Id | Id |  | bigint | 0 | 19 | 0 | SCOPE_IDENTITY() | 
RoleId | RoleId |  | bigint | 0 | 19 | 0 |  | 
UserId | UserId |  | bigint | 0 | 19 | 0 |  | 

## Code generation information

### Setting values
#### IdentityUserRole (Entity)
Setting name | Value
--|--
Entity base class name | `CommonEntityBase`

#### Id (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True

#### RoleId (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True

#### UserId (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True

#### IdentityRole (NavigatorSingleValue)
Setting name | Value
--|--
Navigator property is public | True

#### User (NavigatorSingleValue)
Setting name | Value
--|--
Navigator property is public | True

### Attribute definitions per element

#### IdentityRole (NavigatorSingleValue)

* `Browsable($false)`

#### User (NavigatorSingleValue)

* `Browsable($false)`


### Additional interface definitions per element

None.

### Additional namespace definitions per element

None.
