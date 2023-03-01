RoundLegPeriod
================

## Fields

The following fields are defined in the RoundLegPeriod typed view 

Name | Type | Optional | Read-only | Max. length | Precision | Scale
--|--
EndDateTime | `datetime (System.DateTime)` |  |  | 0 | 0 | 0
LegId | `long (System.Int64)` |  |  | 0 | 19 | 0
RoundId | `long (System.Int64)` |  |  | 0 | 19 | 0
SequenceNo | `int (System.Int32)` |  |  | 0 | 10 | 0
StartDateTime | `datetime (System.DateTime)` |  |  | 0 | 0 | 0
TournamentId | `long (System.Int64)` | Yes |  | 0 | 19 | 0

## Mappings

#### [TestOrg.dbo.RoundLegPeriod](../../../SQL_Server_SqlClient/TestOrg/dbo/RoundLegPeriod.htm) (SQL Server (SqlClient))

Type of target: View

Typed View Field | Target field | Nullable | Type | Length | Precision | Scale | Type converter
--|--
EndDateTime | EndDateTime |  | datetime | 0 | 0 | 0 | 
LegId | LegId |  | bigint | 0 | 19 | 0 | 
RoundId | RoundId |  | bigint | 0 | 19 | 0 | 
SequenceNo | SequenceNo |  | int | 0 | 10 | 0 | 
StartDateTime | StartDateTime |  | datetime | 0 | 0 | 0 | 
TournamentId | TournamentId | Yes | bigint | 0 | 19 | 0 | 


## Code generation information

### Setting values
#### RoundLegPeriod (TypedView)
Setting name | Value
--|--
Typed view row base class name | 
Output type | PocoWithQuerySpecQuery

#### EndDateTime (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

#### LegId (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

#### RoundId (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

#### SequenceNo (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

#### StartDateTime (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

#### TournamentId (TypedViewField)
Setting name | Value
--|--
Generate as nullable type | True

### Attribute definitions per element

None.

### Additional interface definitions per element

None.

### Additional namespace definitions per element

None.
 