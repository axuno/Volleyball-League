IdentityUserToken
============

## Fields

Field name | Ordinal | Native type | Length | Precision | Scale | Is Nullable | Is PK | Is FK | Is Identity | Is Computed  | Default value | Default sequence
--|--
Id | 1 | bigint | 0 | 19 | 0 |  |  |  | Yes |  |  | 
UserId | 2 | bigint | 0 | 19 | 0 |  | Yes | Yes |  |  |  | 
LoginProvider | 3 | nvarchar | 128 | 0 | 0 |  | Yes |  |  |  | '' | 
Name | 4 | nvarchar | 128 | 0 | 0 |  | Yes |  |  |  | '' | 
Value | 5 | nvarchar | 2147483647 | 0 | 0 | Yes |  |  |  |  |  | 

## Foreign key constraints

#### UserTokens_fk

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
UserTokens_uq | Id


## Model elements mapped on this table

Model Element | Element type
--|--
[IdentityUserToken](../../../EntityModel/_DefaultGroup/Entities/IdentityUserToken.htm) | Entity
