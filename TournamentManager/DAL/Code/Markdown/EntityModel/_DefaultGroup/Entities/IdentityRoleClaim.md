IdentityRoleClaim
================

## Inheritance hierarchy

--|--
Hierarchy type | None
Is abstract | False

## Relationships

The IdentityRoleClaim entity is part of the following relationships 

Related Entity | Full description 
--|--
[IdentityRole](../../_DefaultGroup/Entities/IdentityRole.htm) | IdentityRoleClaim.IdentityRole - IdentityRole.IdentityRoleClaims (m:1) 

## Fields

The following fields are defined in the IdentityRoleClaim entity 

Name | Type | Is PK | Is FK | Optional | Read-only | Max. length | Precision | Scale
--|--
ClaimType | `string (System.String)` |   |  |  |  | 255 | 0 | 0
ClaimValue | `string (System.String)` |   |  |  |  | 2147483647 | 0 | 0
Id | `long (System.Int64)` |  Yes |  |  | Yes | 0 | 19 | 0
Issuer | `string (System.String)` |   |  | Yes |  | 255 | 0 | 0
RoleId | `long (System.Int64)` |   | Yes |  |  | 0 | 19 | 0
ValueType | `string (System.String)` |   |  | Yes |  | 255 | 0 | 0

### Unique Constraints
None.

### Fields mapped onto related fields
None.

## Mappings

#### [TestOrg.dbo.IdentityRoleClaim](../../../SQL_Server_SqlClient/TestOrg/dbo/IdentityRoleClaim.htm) (SQL Server (SqlClient))

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
RoleId | RoleId |  | bigint | 0 | 19 | 0 |  | 
ValueType | ValueType | Yes | nvarchar | 255 | 0 | 0 |  | 

## Code generation information

### Setting values
#### IdentityRoleClaim (Entity)
Setting name | Value
--|--
Entity base class name | `CommonEntityBase`

#### ClaimType (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True

#### ClaimValue (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True

#### Id (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True

#### Issuer (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True

#### RoleId (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True

#### ValueType (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True

#### IdentityRole (NavigatorSingleValue)
Setting name | Value
--|--
Navigator property is public | True

### Attribute definitions per element

#### IdentityRole (NavigatorSingleValue)

* `Browsable($false)`


### Additional interface definitions per element

None.

### Additional namespace definitions per element

None.
