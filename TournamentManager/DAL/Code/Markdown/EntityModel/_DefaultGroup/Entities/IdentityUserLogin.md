IdentityUserLogin
================

## Inheritance hierarchy

--|--
Hierarchy type | None
Is abstract | False

## Relationships

The IdentityUserLogin entity is part of the following relationships 

Related Entity | Full description 
--|--
[User](../../_DefaultGroup/Entities/User.htm) | IdentityUserLogin.User - User.IdentityUserLogins (m:1) 

## Fields

The following fields are defined in the IdentityUserLogin entity 

Name | Type | Is PK | Is FK | Optional | Read-only | Max. length | Precision | Scale
--|--
Id | `long (System.Int64)` |   |  |  | Yes | 0 | 19 | 0
LoginProvider | `string (System.String)` |  Yes |  |  |  | 128 | 0 | 0
ProviderDisplayName | `string (System.String)` |   |  |  |  | 128 | 0 | 0
ProviderKey | `string (System.String)` |  Yes |  |  |  | 128 | 0 | 0
UserId | `long (System.Int64)` |  Yes | Yes |  |  | 0 | 11 | 0

### Unique Constraints

The following unique constraints are defined at the entity level

Name | Fields 
--|--
UserLoginUq | Id

### Fields mapped onto related fields
None.

## Mappings

#### [TestOrg.dbo.IdentityUserLogin](../../../SQL_Server_SqlClient/TestOrg/dbo/IdentityUserLogin.htm) (SQL Server (SqlClient))

Aspect | Value
--|--
Type of target | Table
Actions allowed | Create / Retrieve / Update / Delete

Entity Field | Target field | Nullable | Type | Length | Precision | Scale | Sequence | Type converter
--|--
Id | Id |  | bigint | 0 | 19 | 0 | SCOPE_IDENTITY() | 
LoginProvider | LoginProvider |  | nvarchar | 128 | 0 | 0 |  | 
ProviderDisplayName | ProviderDisplayName |  | nvarchar | 128 | 0 | 0 |  | 
ProviderKey | ProviderKey |  | nvarchar | 128 | 0 | 0 |  | 
UserId | UserId |  | bigint | 0 | 19 | 0 |  | 

## Code generation information

### Setting values
#### IdentityUserLogin (Entity)
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

#### ProviderDisplayName (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True
Field property has private setter | False

#### ProviderKey (NormalField)
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
