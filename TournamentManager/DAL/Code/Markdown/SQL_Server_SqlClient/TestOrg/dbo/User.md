User
============

## Fields

Field name | Ordinal | Native type | Length | Precision | Scale | Is Nullable | Is PK | Is FK | Is Identity | Is Computed  | Default value | Default sequence
--|--
Id | 1 | bigint | 0 | 19 | 0 |  | Yes |  | Yes |  |  | 
Guid | 2 | nvarchar | 50 | 0 | 0 |  |  |  |  |  | '' | 
UserName | 3 | nvarchar | 255 | 0 | 0 |  |  |  |  |  | '' | 
PasswordHash | 4 | nvarchar | 2147483647 | 0 | 0 |  |  |  |  |  | '' | 
Email | 5 | nvarchar | 255 | 0 | 0 |  |  |  |  |  | '' | 
EmailConfirmedOn | 6 | datetime | 0 | 0 | 0 | Yes |  |  |  |  |  | 
PhoneNumber | 7 | nvarchar | 40 | 0 | 0 |  |  |  |  |  | '' | 
PhoneNumberConfirmedOn | 8 | datetime | 0 | 0 | 0 | Yes |  |  |  |  |  | 
LastLoginOn | 9 | datetime | 0 | 0 | 0 | Yes |  |  |  |  |  | 
AccessFailedCount | 10 | int | 0 | 10 | 0 |  |  |  |  |  | (0) | 
LockoutEndDateUtc | 11 | datetime | 0 | 0 | 0 | Yes |  |  |  |  |  | 
Gender | 12 | nvarchar | 1 | 0 | 0 |  |  |  |  |  | 'u' | 
Title | 13 | nvarchar | 255 | 0 | 0 |  |  |  |  |  | '' | 
FirstName | 14 | nvarchar | 255 | 0 | 0 |  |  |  |  |  | '' | 
MiddleName | 15 | nvarchar | 255 | 0 | 0 |  |  |  |  |  | '' | 
LastName | 16 | nvarchar | 255 | 0 | 0 |  |  |  |  |  | '' | 
Nickname | 17 | nvarchar | 255 | 0 | 0 |  |  |  |  |  | '' | 
PhoneNumber2 | 18 | nvarchar | 40 | 0 | 0 |  |  |  |  |  | '' | 
Email2 | 19 | nvarchar | 100 | 0 | 0 |  |  |  |  |  | '' | 
Birthday | 20 | datetime | 0 | 0 | 0 | Yes |  |  |  |  |  | 
Remarks | 21 | nvarchar | 4000 | 0 | 0 | Yes |  |  |  |  | '' | 
CreatedOn | 22 | datetime | 0 | 0 | 0 |  |  |  |  |  | getdate() | 
ModifiedOn | 23 | datetime | 0 | 0 | 0 |  |  |  |  |  | getdate() | 

## Unique constraints

Constraint name | Fields
--|--
UC_5ee8deb40f8bca844c8745f8722 | Email

Constraint name | Fields
--|--
UC_9d0e7fb4df4865699f58d126a42 | UserName


## Model elements mapped on this table

Model Element | Element type
--|--
[User](../../../EntityModel/_DefaultGroup/Entities/User.htm) | Entity
