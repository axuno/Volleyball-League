User
================

## Inheritance hierarchy

--|--
Hierarchy type | None
Is abstract | False

## Relationships

The User entity is part of the following relationships 

Related Entity | Full description 
--|--
[IdentityUserClaim](../../_DefaultGroup/Entities/IdentityUserClaim.htm) | IdentityUserClaim.User - User.IdentityUserClaims (m:1) 
[IdentityUserLogin](../../_DefaultGroup/Entities/IdentityUserLogin.htm) | IdentityUserLogin.User - User.IdentityUserLogins (m:1) 
[IdentityUserRole](../../_DefaultGroup/Entities/IdentityUserRole.htm) | IdentityUserRole.User - User.IdentityUserRoles (m:1) 
[IdentityUserToken](../../_DefaultGroup/Entities/IdentityUserToken.htm) | IdentityUserToken.User - User.IdentityUserTokens (m:1) 
[ManagerOfTeam](../../_DefaultGroup/Entities/ManagerOfTeam.htm) | ManagerOfTeam.User - User.ManagerOfTeams (m:1) 
[PlayerInTeam](../../_DefaultGroup/Entities/PlayerInTeam.htm) | PlayerInTeam.User - User.PlayerInTeams (m:1) 
[Registration](../../_DefaultGroup/Entities/Registration.htm) | Registration.User - User.Registrations (m:1) 

## Fields

The following fields are defined in the User entity 

Name | Type | Is PK | Is FK | Optional | Read-only | Max. length | Precision | Scale
--|--
Id | `long (System.Int64)` |  Yes |  |  | Yes | 0 | 11 | 0
Guid | `string (System.String)` |   |  |  |  | 50 | 0 | 0
UserName | `string (System.String)` |   |  |  |  | 255 | 0 | 0
PasswordHash | `string (System.String)` |   |  |  |  | 2147483647 | 0 | 0
Email | `string (System.String)` |   |  |  |  | 255 | 0 | 0
EmailConfirmedOn | `datetime (System.DateTime)` |   |  | Yes |  | 0 | 0 | 0
PhoneNumber | `string (System.String)` |   |  |  |  | 40 | 0 | 0
PhoneNumberConfirmedOn | `datetime (System.DateTime)` |   |  | Yes |  | 0 | 0 | 0
LastLoginOn | `datetime (System.DateTime)` |   |  | Yes |  | 0 | 0 | 0
AccessFailedCount | `int (System.Int32)` |   |  |  |  | 0 | 0 | 0
LockoutEndDateUtc | `datetime (System.DateTime)` |   |  | Yes |  | 0 | 0 | 0
Gender | `string (System.String)` |   |  |  |  | 1 | 0 | 0
Title | `string (System.String)` |   |  |  |  | 255 | 0 | 0
FirstName | `string (System.String)` |   |  |  |  | 255 | 0 | 0
MiddleName | `string (System.String)` |   |  |  |  | 255 | 0 | 0
LastName | `string (System.String)` |   |  |  |  | 255 | 0 | 0
Nickname | `string (System.String)` |   |  |  |  | 255 | 0 | 0
PhoneNumber2 | `string (System.String)` |   |  |  |  | 40 | 0 | 0
Email2 | `string (System.String)` |   |  |  |  | 100 | 0 | 0
Birthday | `datetime (System.DateTime)` |   |  | Yes |  | 0 | 0 | 0
Remarks | `string (System.String)` |   |  | Yes |  | 4000 | 0 | 0
CreatedOn | `datetime (System.DateTime)` |   |  |  |  | 0 | 0 | 0
ModifiedOn | `datetime (System.DateTime)` |   |  |  |  | 0 | 0 | 0

### Unique Constraints

The following unique constraints are defined at the entity level

Name | Fields 
--|--
Email | Email
UserName | UserName

### Fields mapped onto related fields
None.

## Mappings

#### [TestOrg.dbo.User](../../../SQL_Server_SqlClient/TestOrg/dbo/User.htm) (SQL Server (SqlClient))

Aspect | Value
--|--
Type of target | Table
Actions allowed | Create / Retrieve / Update / Delete

Entity Field | Target field | Nullable | Type | Length | Precision | Scale | Sequence | Type converter
--|--
AccessFailedCount | AccessFailedCount |  | int | 0 | 10 | 0 |  | 
Birthday | Birthday | Yes | datetime | 0 | 0 | 0 |  | 
CreatedOn | CreatedOn |  | datetime | 0 | 0 | 0 |  | 
Email | Email |  | nvarchar | 255 | 0 | 0 |  | 
Email2 | Email2 |  | nvarchar | 100 | 0 | 0 |  | 
EmailConfirmedOn | EmailConfirmedOn | Yes | datetime | 0 | 0 | 0 |  | 
FirstName | FirstName |  | nvarchar | 255 | 0 | 0 |  | 
Gender | Gender |  | nvarchar | 1 | 0 | 0 |  | 
Guid | Guid |  | nvarchar | 50 | 0 | 0 |  | 
Id | Id |  | bigint | 0 | 19 | 0 | SCOPE_IDENTITY() | 
LastLoginOn | LastLoginOn | Yes | datetime | 0 | 0 | 0 |  | 
LastName | LastName |  | nvarchar | 255 | 0 | 0 |  | 
LockoutEndDateUtc | LockoutEndDateUtc | Yes | datetime | 0 | 0 | 0 |  | 
MiddleName | MiddleName |  | nvarchar | 255 | 0 | 0 |  | 
ModifiedOn | ModifiedOn |  | datetime | 0 | 0 | 0 |  | 
Nickname | Nickname |  | nvarchar | 255 | 0 | 0 |  | 
PasswordHash | PasswordHash |  | nvarchar | 2147483647 | 0 | 0 |  | 
PhoneNumber | PhoneNumber |  | nvarchar | 40 | 0 | 0 |  | 
PhoneNumber2 | PhoneNumber2 |  | nvarchar | 40 | 0 | 0 |  | 
PhoneNumberConfirmedOn | PhoneNumberConfirmedOn | Yes | datetime | 0 | 0 | 0 |  | 
Remarks | Remarks | Yes | nvarchar | 4000 | 0 | 0 |  | 
Title | Title |  | nvarchar | 255 | 0 | 0 |  | 
UserName | UserName |  | nvarchar | 255 | 0 | 0 |  | 

## Code generation information

### Setting values
#### User (Entity)
Setting name | Value
--|--
Entity base class name | `CommonEntityBase`

#### AccessFailedCount (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True
Field property has private setter | False

#### Birthday (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True
Field property has private setter | False

#### CreatedOn (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True
Field property has private setter | False

#### Email (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True
Field property has private setter | False

#### Email2 (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True
Field property has private setter | False

#### EmailConfirmedOn (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True
Field property has private setter | False

#### FirstName (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True
Field property has private setter | False

#### Gender (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True
Field property has private setter | False

#### Guid (NormalField)
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

#### LastLoginOn (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True
Field property has private setter | False

#### LastName (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True
Field property has private setter | False

#### LockoutEndDateUtc (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True
Field property has private setter | False

#### MiddleName (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True
Field property has private setter | False

#### ModifiedOn (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True
Field property has private setter | False

#### Nickname (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True
Field property has private setter | False

#### PasswordHash (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True
Field property has private setter | False

#### PhoneNumber (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True
Field property has private setter | False

#### PhoneNumber2 (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True
Field property has private setter | False

#### PhoneNumberConfirmedOn (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True
Field property has private setter | False

#### Remarks (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True
Field property has private setter | False

#### Title (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True
Field property has private setter | False

#### UserName (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True
Field property has private setter | False

#### IdentityUserClaims (NavigatorCollection)
Setting name | Value
--|--
Navigator property is public | True

#### IdentityUserLogins (NavigatorCollection)
Setting name | Value
--|--
Navigator property is public | True

#### IdentityUserRoles (NavigatorCollection)
Setting name | Value
--|--
Navigator property is public | True

#### IdentityUserTokens (NavigatorCollection)
Setting name | Value
--|--
Navigator property is public | True

#### ManagerOfTeams (NavigatorCollection)
Setting name | Value
--|--
Navigator property is public | True

#### PlayerInTeams (NavigatorCollection)
Setting name | Value
--|--
Navigator property is public | True

#### Registrations (NavigatorCollection)
Setting name | Value
--|--
Navigator property is public | True

### Attribute definitions per element

None.

### Additional interface definitions per element

None.

### Additional namespace definitions per element

None.
