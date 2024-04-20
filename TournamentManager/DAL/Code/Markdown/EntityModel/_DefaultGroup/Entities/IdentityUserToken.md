IdentityUserToken
================

## Inheritance hierarchy

--|--
Hierarchy type | None
Is abstract | False

## Relationships

The IdentityUserToken entity is part of the following relationships 

Related Entity | Full description 
--|--
[User](../../_DefaultGroup/Entities/User.htm) | IdentityUserToken.User - User.IdentityUserTokens (m:1) 

## Fields

The following fields are defined in the IdentityUserToken entity 

Name | Type | Is PK | Is FK | Optional | Read-only | Max. length | Precision | Scale
--|--
Id | `long (System.Int64)` |   |  |  | Yes | 0 | 19 | 0
LoginProvider | `string (System.String)` |  Yes |  |  |  | 128 | 0 | 0
Name | `string (System.String)` |  Yes |  |  |  | 128 | 0 | 0
UserId | `long (System.Int64)` |  Yes | Yes |  |  | 0 | 11 | 0
Value | `string (System.String)` |   |  | Yes |  | 2147483647 | 0 | 0

### Unique Constraints

The following unique constraints are defined at the entity level

Name | Fields 
--|--
UserTokensUq | Id

### Fields mapped onto related fields
None.

## Mappings

#### [TestOrg.dbo.IdentityUserToken](../../../SQL_Server_SqlClient/TestOrg/dbo/IdentityUserToken.htm) (SQL Server (SqlClient))

Aspect | Value
--|--
Type of target | Table
Actions allowed | Create / Retrieve / Update / Delete

Entity Field | Target field | Nullable | Type | Length | Precision | Scale | Sequence | Type converter
--|--
Id | Id |  | bigint | 0 | 19 | 0 | SCOPE_IDENTITY() | 
LoginProvider | LoginProvider |  | nvarchar | 128 | 0 | 0 |  | 
Name | Name |  | nvarchar | 128 | 0 | 0 |  | 
UserId | UserId |  | bigint | 0 | 19 | 0 |  | 
Value | Value | Yes | nvarchar | 2147483647 | 0 | 0 |  | 

## Code generation information

### Setting values
#### IdentityUserToken (Entity)
Setting name | Value
--|--
Entity base class name | `CommonEntityBase`

#### Id (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True
Field property has private setter | False

#### LoginProvider (NormalField)
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

#### UserId (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True
Field property has private setter | False

#### Value (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True
Field property has private setter | False

#### User (NavigatorSingleValue)
Setting name | Value
--|--
Navigator property is public | True

### Attribute definitions per element

#### User (NavigatorSingleValue)

* `Browsable($false)`


### Additional interface definitions per element

None.

### Additional namespace definitions per element

None.
