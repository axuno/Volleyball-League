IdentityRole
================

## Inheritance hierarchy

--|--
Hierarchy type | None
Is abstract | False

## Relationships

The IdentityRole entity is part of the following relationships 

Related Entity | Full description 
--|--
[IdentityRoleClaim](../../_DefaultGroup/Entities/IdentityRoleClaim.htm) | IdentityRoleClaim.IdentityRole - IdentityRole.IdentityRoleClaims (m:1) 
[IdentityUserRole](../../_DefaultGroup/Entities/IdentityUserRole.htm) | IdentityUserRole.IdentityRole - IdentityRole.IdentityUserRoles (m:1) 

## Fields

The following fields are defined in the IdentityRole entity 

Name | Type | Is PK | Is FK | Optional | Read-only | Max. length | Precision | Scale
--|--
Id | `long (System.Int64)` |  Yes |  |  | Yes | 0 | 19 | 0
Name | `string (System.String)` |   |  |  |  | 255 | 0 | 0

### Unique Constraints

The following unique constraints are defined at the entity level

Name | Fields 
--|--
RoleUq | Name

### Fields mapped onto related fields
None.

## Mappings

#### [TestOrg.dbo.IdentityRole](../../../SQL_Server_SqlClient/TestOrg/dbo/IdentityRole.htm) (SQL Server (SqlClient))

Aspect | Value
--|--
Type of target | Table
Actions allowed | Create / Retrieve / Update / Delete

Entity Field | Target field | Nullable | Type | Length | Precision | Scale | Sequence | Type converter
--|--
Id | Id |  | bigint | 0 | 19 | 0 | SCOPE_IDENTITY() | 
Name | Name |  | nvarchar | 255 | 0 | 0 |  | 

## Code generation information

### Setting values
#### IdentityRole (Entity)
Setting name | Value
--|--
Entity base class name | `CommonEntityBase`

#### Id (NormalField)
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

#### IdentityRoleClaims (NavigatorCollection)
Setting name | Value
--|--
Navigator property is public | True

#### IdentityUserRoles (NavigatorCollection)
Setting name | Value
--|--
Navigator property is public | True

### Attribute definitions per element

None.

### Additional interface definitions per element

None.

### Additional namespace definitions per element

None.
