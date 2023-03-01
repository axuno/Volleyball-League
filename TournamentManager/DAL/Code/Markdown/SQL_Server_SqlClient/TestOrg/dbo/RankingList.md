RankingList
============

## Fields

Field name | Ordinal | Native type | Length | Precision | Scale | Is Nullable | Is Identity
--|--
TournamentId | 1 | bigint | 0 | 19 | 0 |  | 
TournamentName | 2 | nvarchar | 255 | 0 | 0 |  | 
TournamentDescription | 3 | nvarchar | 255 | 0 | 0 | Yes | 
TournamentIsComplete | 4 | bit | 0 | 0 | 0 |  | 
RoundId | 5 | bigint | 0 | 19 | 0 |  | 
RoundName | 6 | nvarchar | 255 | 0 | 0 |  | 
RoundDescription | 7 | nvarchar | 255 | 0 | 0 |  | 
RoundTypeName | 8 | nvarchar | 255 | 0 | 0 |  | 
RoundTypeDescription | 9 | nvarchar | 255 | 0 | 0 |  | 
TeamId | 10 | bigint | 0 | 19 | 0 |  | 
TeamName | 11 | nvarchar | 255 | 0 | 0 |  | 
TeamNameForRound | 12 | nvarchar | 255 | 0 | 0 | Yes | 
ClubName | 13 | nvarchar | 255 | 0 | 0 | Yes | 
ValuationDate | 14 | datetime | 0 | 0 | 0 |  | 
Rank | 15 | int | 0 | 10 | 0 |  | 
MatchPointsWon | 16 | int | 0 | 10 | 0 | Yes | 
MatchPointsLost | 17 | int | 0 | 10 | 0 | Yes | 
SetPointsWon | 18 | int | 0 | 10 | 0 | Yes | 
SetPointsLost | 19 | int | 0 | 10 | 0 | Yes | 
BallPointsWon | 20 | int | 0 | 10 | 0 | Yes | 
BallPointsLost | 21 | int | 0 | 10 | 0 | Yes | 
MatchesPlayed | 22 | int | 0 | 10 | 0 |  | 
MatchesToPlay | 23 | int | 0 | 10 | 0 |  | 
ModifiedOn | 24 | datetime | 0 | 0 | 0 |  | 

## Model elements mapped on this view

Model Element | Element type
--|--
[RankingList](../../../EntityModel/_DefaultGroup/TypedViews/RankingList.htm) | Typed View
