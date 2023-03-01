IdentityUserClaim
============

## Fields

Field name | Ordinal | Native type | Length | Precision | Scale | Is Nullable | Is PK | Is FK | Is Identity | Is Computed  | Default value | Default sequence
--|--
Id | 1 | bigint | 0 | 19 | 0 |  | Yes |  | Yes |  |  | 
UserId | 2 | bigint | 0 | 19 | 0 |  |  | Yes |  |  |  | 
ClaimType | 3 | nvarchar | 255 | 0 | 0 |  |  |  |  |  | '' | 
ClaimValue | 4 | nvarchar | 2147483647 | 0 | 0 |  |  |  |  |  | '' | 
ValueType | 5 | nvarchar | 255 | 0 | 0 | Yes |  |  |  |  |  | 
Issuer | 6 | nvarchar | 255 | 0 | 0 | Yes |  |  |  |  |  | 

## Foreign key constraints

#### IdentityUserClaim_User_fk

Aspect | Value
--|--
Primary key table | [dbo.User](../dbo/User.htm)
Delete rule | Cascade
Update rule | Cascade 

Foreign key field | Primary key field
--|--
UserId | dbo.User.Id

## Model elements mapped on this table

Model Element | Element type
--|--
[IdentityUserClaim](../../../EntityModel/_DefaultGroup/Entities/IdentityUserClaim.htm) | Entity
