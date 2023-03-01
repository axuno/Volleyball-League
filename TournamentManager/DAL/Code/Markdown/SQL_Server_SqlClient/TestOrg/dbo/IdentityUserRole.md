IdentityUserRole
============

## Fields

Field name | Ordinal | Native type | Length | Precision | Scale | Is Nullable | Is PK | Is FK | Is Identity | Is Computed  | Default value | Default sequence
--|--
Id | 1 | bigint | 0 | 19 | 0 |  |  |  | Yes |  |  | 
UserId | 2 | bigint | 0 | 19 | 0 |  | Yes | Yes |  |  |  | 
RoleId | 3 | bigint | 0 | 19 | 0 |  | Yes | Yes |  |  |  | 

## Foreign key constraints

#### Fk_UserRole_Role

Aspect | Value
--|--
Primary key table | [dbo.IdentityRole](../dbo/IdentityRole.htm)
Delete rule | Cascade
Update rule | Cascade 

Foreign key field | Primary key field
--|--
RoleId | dbo.IdentityRole.Id

#### Fk_UserRole_User

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
[IdentityUserRole](../../../EntityModel/_DefaultGroup/Entities/IdentityUserRole.htm) | Entity
