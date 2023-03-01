IdentityRoleClaim
============

## Fields

Field name | Ordinal | Native type | Length | Precision | Scale | Is Nullable | Is PK | Is FK | Is Identity | Is Computed  | Default value | Default sequence
--|--
Id | 1 | bigint | 0 | 19 | 0 |  | Yes |  | Yes |  |  | 
RoleId | 2 | bigint | 0 | 19 | 0 |  |  | Yes |  |  |  | 
ClaimType | 3 | nvarchar | 255 | 0 | 0 |  |  |  |  |  | '' | 
ClaimValue | 4 | nvarchar | 2147483647 | 0 | 0 |  |  |  |  |  | '' | 
ValueType | 5 | nvarchar | 255 | 0 | 0 | Yes |  |  |  |  |  | 
Issuer | 6 | nvarchar | 255 | 0 | 0 | Yes |  |  |  |  |  | 

## Foreign key constraints

#### IdentityRoleClaim_Role_fk

Aspect | Value
--|--
Primary key table | [dbo.IdentityRole](../dbo/IdentityRole.htm)
Delete rule | Cascade
Update rule | Cascade 

Foreign key field | Primary key field
--|--
RoleId | dbo.IdentityRole.Id

## Model elements mapped on this table

Model Element | Element type
--|--
[IdentityRoleClaim](../../../EntityModel/_DefaultGroup/Entities/IdentityRoleClaim.htm) | Entity
