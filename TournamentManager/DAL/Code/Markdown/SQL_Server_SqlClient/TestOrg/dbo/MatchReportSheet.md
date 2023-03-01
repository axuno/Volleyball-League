MatchReportSheet
============

## Fields

Field name | Ordinal | Native type | Length | Precision | Scale | Is Nullable | Is Identity
--|--
Id | 1 | bigint | 0 | 19 | 0 |  | 
TournamentId | 2 | bigint | 0 | 19 | 0 | Yes | 
TournamentName | 3 | nvarchar | 255 | 0 | 0 |  | 
RoundName | 4 | nvarchar | 255 | 0 | 0 |  | 
RoundDescription | 5 | nvarchar | 255 | 0 | 0 |  | 
RoundTypeName | 6 | nvarchar | 255 | 0 | 0 |  | 
RoundTypeDescription | 7 | nvarchar | 255 | 0 | 0 |  | 
RoundLegSequenceNo | 8 | int | 0 | 10 | 0 |  | 
RoundLegDescription | 9 | nvarchar | 255 | 0 | 0 | Yes | 
HomeTeamId | 10 | bigint | 0 | 19 | 0 |  | 
HomeTeamNameForRound | 11 | nvarchar | 255 | 0 | 0 | Yes | 
GuestTeamId | 12 | bigint | 0 | 19 | 0 |  | 
GuestTeamNameForRound | 13 | nvarchar | 255 | 0 | 0 | Yes | 
PlannedStart | 14 | datetime | 0 | 0 | 0 | Yes | 
OrigPlannedStart | 15 | datetime | 0 | 0 | 0 | Yes | 
NumOfSets | 16 | int | 0 | 10 | 0 |  | 
BestOf | 17 | bit | 0 | 0 | 0 |  | 
NumOfPointsToWinRegular | 18 | int | 0 | 10 | 0 |  | 
NumOfPointsToWinTieBreak | 19 | int | 0 | 10 | 0 |  | 
MaxTimeouts | 20 | int | 0 | 10 | 0 |  | 
MaxSubstitutions | 21 | int | 0 | 10 | 0 |  | 
ChangeSerial | 22 | bigint | 0 | 19 | 0 |  | 
Remarks | 23 | nvarchar | 2147483647 | 0 | 0 | Yes | 
ModifiedOn | 24 | datetime | 0 | 0 | 0 |  | 

## Model elements mapped on this view

Model Element | Element type
--|--
[MatchReportSheet](../../../EntityModel/_DefaultGroup/TypedViews/MatchReportSheet.htm) | Typed View
