IdentityUserClaim
================

## Inheritance hierarchy

--|--
Hierarchy type | None
Is abstract | False

## Relationships

The IdentityUserClaim entity is part of the following relationships 

Related Entity | Full description 
--|--
[User](../../_DefaultGroup/Entities/User.htm) | IdentityUserClaim.User - User.IdentityUserClaims (m:1) 

## Fields

The following fields are defined in the IdentityUserClaim entity 

Name | Type | Is PK | Is FK | Optional | Read-only | Max. length | Precision | Scale
--|--
ClaimType | `string (System.String)` |   |  |  |  | 255 | 0 | 0
ClaimValue | `string (System.String)` |   |  |  |  | 2147483647 | 0 | 0
Id | `long (System.Int64)` |  Yes |  |  | Yes | 0 | 19 | 0
Issuer | `string (System.String)` |   |  | Yes |  | 255 | 0 | 0
UserId | `long (System.Int64)` |   | Yes |  |  | 0 | 11 | 0
ValueType | `string (System.String)` |   |  | Yes |  | 255 | 0 | 0

### Unique Constraints
None.

### Fields mapped onto related fields
None.

## Mappings

#### [TestOrg.dbo.IdentityUserClaim](../../../SQL_Server_SqlClient/TestOrg/dbo/IdentityUserClaim.htm) (SQL Server (SqlClient))

Aspect | Value
--|--
Type of target | Table
Actions allowed | Create / Retrieve / Update / Delete

Entity Field | Target field | Nullable | Type | Length | Precision | Scale | Sequence | Type converter
--|--
ClaimType | ClaimType |  | nvarchar | 255 | 0 | 0 |  | 
ClaimValue | ClaimValue |  | nvarchar | 2147483647 | 0 | 0 |  | 
Id | Id |  | bigint | 0 | 19 | 0 | SCOPE_IDENTITY() | 
Issuer | Issuer | Yes | nvarchar | 255 | 0 | 0 |  | 
UserId | UserId |  | bigint | 0 | 19 | 0 |  | 
ValueType | ValueType | Yes | nvarchar | 255 | 0 | 0 |  | 

## Code generation information

### Setting values
#### IdentityUserClaim (Entity)
Setting name | Value
--|--
Entity base class name | `CommonEntityBase`

#### ClaimType (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True
Field property has private setter | False

#### ClaimValue (NormalField)
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

#### Issuer (NormalField)
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

#### ValueType (NormalField)
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
