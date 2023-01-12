IdentityUserLogin
============

## Fields

Field name | Ordinal | Native type | Length | Precision | Scale | Is Nullable | Is PK | Is FK | Is Identity | Is Computed  | Default value | Default sequence
--|--
Id | 1 | bigint | 0 | 19 | 0 |  |  |  | Yes |  |  | 
UserId | 2 | bigint | 0 | 19 | 0 |  | Yes | Yes |  |  |  | 
LoginProvider | 3 | nvarchar | 128 | 0 | 0 |  | Yes |  |  |  | '' | 
ProviderKey | 4 | nvarchar | 128 | 0 | 0 |  | Yes |  |  |  | '' | 
ProviderDisplayName | 5 | nvarchar | 128 | 0 | 0 |  |  |  |  |  | '' | 

## Foreign key constraints

#### Fk_UserLogin_User

Aspect | Value
--|--
Primary key table | [dbo.User](../dbo/User.htm)
Delete rule | Cascade
Update rule | Cascade 

Foreign key field | Primary key field
--|--
UserId | dbo.User.Id

## Unique constraints

Constraint name | Fields
--|--
UserLogin_uq | Id


## Model elements mapped on this table

Model Element | Element type
--|--
[IdentityUserLogin](../../../EntityModel/_DefaultGroup/Entities/IdentityUserLogin.htm) | Entity
