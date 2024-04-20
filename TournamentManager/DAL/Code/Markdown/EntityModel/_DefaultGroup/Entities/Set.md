Set
================

## Inheritance hierarchy

--|--
Hierarchy type | None
Is abstract | False

## Relationships

The Set entity is part of the following relationships 

Related Entity | Full description 
--|--
[Match](../../_DefaultGroup/Entities/Match.htm) | Set.Match - Match.Sets (m:1) 

## Fields

The following fields are defined in the Set entity 

Name | Type | Is PK | Is FK | Optional | Read-only | Max. length | Precision | Scale
--|--
Id | `long (System.Int64)` |  Yes |  |  | Yes | 0 | 11 | 0
MatchId | `long (System.Int64)` |   | Yes |  |  | 0 | 11 | 0
SequenceNo | `int (System.Int32)` |   |  |  |  | 0 | 2 | 0
HomeBallPoints | `int (System.Int32)` |   |  |  |  | 0 | 2 | 0
GuestBallPoints | `int (System.Int32)` |   |  |  |  | 0 | 2 | 0
HomeSetPoints | `int (System.Int32)` |   |  |  |  | 0 | 2 | 0
GuestSetPoints | `int (System.Int32)` |   |  |  |  | 0 | 2 | 0
HomeTimeout | `int (System.Int32)` |   |  |  |  | 0 | 0 | 0
GuestTimeout | `int (System.Int32)` |   |  |  |  | 0 | 0 | 0
IsOverruled | `bool (System.Boolean)` |   |  |  |  | 0 | 1 | 0
IsTieBreak | `bool (System.Boolean)` |   |  |  |  | 0 | 1 | 0
RealStart | `datetime (System.DateTime)` |   |  | Yes |  | 0 | 0 | 0
RealEnd | `datetime (System.DateTime)` |   |  | Yes |  | 0 | 0 | 0
Remarks | `string (System.String)` |   |  | Yes |  | 2147483647 | 0 | 0
CreatedOn | `datetime (System.DateTime)` |   |  |  |  | 0 | 0 | 0
ModifiedOn | `datetime (System.DateTime)` |   |  |  |  | 0 | 0 | 0

### Unique Constraints
None.

### Fields mapped onto related fields
None.

## Mappings

#### [TestOrg.dbo.Set](../../../SQL_Server_SqlClient/TestOrg/dbo/Set.htm) (SQL Server (SqlClient))

Aspect | Value
--|--
Type of target | Table
Actions allowed | Create / Retrieve / Update / Delete

Entity Field | Target field | Nullable | Type | Length | Precision | Scale | Sequence | Type converter
--|--
CreatedOn | CreatedOn |  | datetime | 0 | 0 | 0 |  | 
GuestBallPoints | GuestBallPoints |  | int | 0 | 10 | 0 |  | 
GuestSetPoints | GuestSetPoints |  | int | 0 | 10 | 0 |  | 
GuestTimeout | GuestTimeout |  | int | 0 | 10 | 0 |  | 
HomeBallPoints | HomeBallPoints |  | int | 0 | 10 | 0 |  | 
HomeSetPoints | HomeSetPoints |  | int | 0 | 10 | 0 |  | 
HomeTimeout | HomeTimeout |  | int | 0 | 10 | 0 |  | 
Id | Id |  | bigint | 0 | 19 | 0 | SCOPE_IDENTITY() | 
IsOverruled | IsOverruled |  | bit | 0 | 0 | 0 |  | 
IsTieBreak | IsTieBreak |  | bit | 0 | 0 | 0 |  | 
MatchId | MatchId |  | bigint | 0 | 19 | 0 |  | 
ModifiedOn | ModifiedOn |  | datetime | 0 | 0 | 0 |  | 
RealEnd | RealEnd | Yes | datetime | 0 | 0 | 0 |  | 
RealStart | RealStart | Yes | datetime | 0 | 0 | 0 |  | 
Remarks | Remarks | Yes | nvarchar | 2147483647 | 0 | 0 |  | 
SequenceNo | SequenceNo |  | int | 0 | 10 | 0 |  | 

## Code generation information

### Setting values
#### Set (Entity)
Setting name | Value
--|--
Entity base class name | `CommonEntityBase`

#### CreatedOn (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True
Field property has private setter | False

#### GuestBallPoints (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True
Field property has private setter | False

#### GuestSetPoints (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True
Field property has private setter | False

#### GuestTimeout (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True
Field property has private setter | False

#### HomeBallPoints (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True
Field property has private setter | False

#### HomeSetPoints (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True
Field property has private setter | False

#### HomeTimeout (NormalField)
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

#### IsOverruled (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True
Field property has private setter | False

#### IsTieBreak (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True
Field property has private setter | False

#### MatchId (NormalField)
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

#### RealEnd (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True
Field property has private setter | False

#### RealStart (NormalField)
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

#### SequenceNo (NormalField)
Setting name | Value
--|--
Generate as nullable type | True
Field property is public | True
Field property has private setter | False

#### Match (NavigatorSingleValue)
Setting name | Value
--|--
Navigator property is public | True

### Attribute definitions per element

#### Match (NavigatorSingleValue)

* `Browsable($false)`


### Additional interface definitions per element

None.

### Additional namespace definitions per element

None.
