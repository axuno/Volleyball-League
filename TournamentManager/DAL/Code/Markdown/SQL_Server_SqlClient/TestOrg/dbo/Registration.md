Registration
============

## Fields

Field name | Ordinal | Native type | Length | Precision | Scale | Is Nullable | Is PK | Is FK | Is Identity | Is Computed  | Default value | Default sequence
--|--
Id | 1 | bigint | 0 | 19 | 0 |  | Yes |  | Yes |  |  | 
TournamentId | 2 | bigint | 0 | 19 | 0 |  |  | Yes |  |  |  | 
TeamId | 3 | bigint | 0 | 19 | 0 |  |  | Yes |  |  |  | 
ManagerId | 4 | bigint | 0 | 19 | 0 |  |  | Yes |  |  |  | 
Guid | 5 | nvarchar | 50 | 0 | 0 |  |  |  |  |  | '' | 
AppliedWithMessage | 6 | nvarchar | 4000 | 0 | 0 | Yes |  |  |  |  | '' | 
AppliedOn | 7 | datetime | 0 | 0 | 0 |  |  |  |  |  | getdate() | 

## Foreign key constraints

#### FK_4462bad40269915324df04e014d

Aspect | Value
--|--
Primary key table | [dbo.User](../dbo/User.htm)
Delete rule | Cascade
Update rule | Cascade 

Foreign key field | Primary key field
--|--
ManagerId | dbo.User.Id

#### FK_7bab7f542cbbe91cec71043643e

Aspect | Value
--|--
Primary key table | [dbo.Team](../dbo/Team.htm)
Delete rule | Cascade
Update rule | Cascade 

Foreign key field | Primary key field
--|--
TeamId | dbo.Team.Id

#### FK_b3d49004f0692a3e027b9b68624

Aspect | Value
--|--
Primary key table | [dbo.Tournament](../dbo/Tournament.htm)
Delete rule | Cascade
Update rule | Cascade 

Foreign key field | Primary key field
--|--
TournamentId | dbo.Tournament.Id

## Model elements mapped on this table

Model Element | Element type
--|--
[Registration](../../../EntityModel/_DefaultGroup/Entities/Registration.htm) | Entity
