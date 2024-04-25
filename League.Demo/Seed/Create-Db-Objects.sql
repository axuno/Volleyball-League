/****** Object:  UserDefinedFunction [dbo].[GetSetsAsText]    Script Date: 06.08.2023 20:51:06 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[GetSetsAsText] (@matchId bigint
)
RETURNS nvarchar(50)
AS
BEGIN

DECLARE @allsetresults varchar(50);
DECLARE @setresult varchar(50);

DECLARE SetsOfMatch CURSOR READ_ONLY
FOR 
SELECT CONCAT(HomeBallPoints,':',GuestBallPoints) AS setresult FROM [Set] WHERE [Set].[MatchId] = @MatchId ORDER BY [SequenceNo];
OPEN SetsOfMatch;
FETCH NEXT FROM SetsOfMatch INTO @setresult;

WHILE @@FETCH_STATUS = 0
BEGIN
    SET @allsetresults = CONCAT(@allsetresults, @setresult, ' ');
	FETCH NEXT FROM SetsOfMatch INTO @setresult;
END

CLOSE SetsOfMatch;
DEALLOCATE SetsOfMatch;

RETURN RTRIM(@allsetresults);
END
GO
/****** Object:  UserDefinedFunction [dbo].[SpatialBearingAngle]    Script Date: 06.08.2023 20:51:06 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[SpatialBearingAngle]
( 
@lat1 float,
@lon1 float,
@lat2 float,
@lon2 float)
RETURNS float
AS
BEGIN

DECLARE 
	@Bearing float = NULL, 
	@x float, 
	@y float, 
	@East float, 
	@North float, 
	@radLat1 float,
	@radLat2 float

IF @lat1 IS NULL OR @lon1 IS NULL OR @lat2 IS NULL OR @lon2 IS NULL
	RETURN NULL;
ELSE
	BEGIN
		SET @East = RADIANS(@lat2 - @lat1);
		SET @North = RADIANS(@lon2 - @lon1);
		SET @radLat1 = RADIANS(@lat1);
		SET @radLat2 = RADIANS(@lat2);
		SET @y = SIN(@North)*COS(@radLat2);

		SET @x = COS(@radLat1)*SIN(@radLat2)-SIN(@radLat1)*COS(@radLat2)*COS(@North);
		IF (@x = 0 AND @y = 0)
			RETURN NULL;
		ELSE
			SET @Bearing = ROUND(CAST((DEGREES(ATN2(@y,@x)) + 360) AS DECIMAL(18,12)) % 360, 1)
	END

RETURN @Bearing

END
GO
/****** Object:  UserDefinedFunction [dbo].[SpatialDistance]    Script Date: 06.08.2023 20:51:06 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[SpatialDistance] (@lat1 float, @lon1 float, @lat2 float, @lon2 float )
RETURNS float
AS
BEGIN
  IF (@lat1 IS NULL OR @lon1 IS NULL OR @lat2 IS NULL or @lon2 IS NULL) RETURN NULL;
  DECLARE @geo1 GEOGRAPHY = GEOGRAPHY::Point(@lat1, @lon1, 4326);
  DECLARE @geo2 GEOGRAPHY = GEOGRAPHY::Point(@lat2, @lon2, 4326);
  RETURN ROUND(@geo1.STDistance(@geo2)/1000, 3); /* result in kilometers */
END
GO
/****** Object:  UserDefinedFunction [dbo].[Weekday]    Script Date: 25.04.2024 20:45:00 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION dbo.[Weekday] (@dow int)
RETURNS nvarchar(15)
WITH EXECUTE AS CALLER
AS
BEGIN
	-- NOTE: To return the name in a specific language, use: SET LANGUAGE '<name>' in the caller. You cannet set it in a UDF.
    DECLARE @weekday nvarchar(15)

	IF @dow >= 0 AND @dow <=6
		SET @weekday = DATENAME(WEEKDAY, DATEADD(DAY, @dow, '2023-10-01')); /* 2023-10-01 is a Sunday */
    ELSE
		SET @weekday = '';
        
    RETURN @weekday
END
GO
/****** Object:  UserDefinedFunction [dbo].[UtcToLocal]    Script Date: 25.04.2024 20:45:00 ******/
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION UtcToLocal (@UtcDateTime datetime2, @TimeZoneIdentifier nvarchar(100) = 'W. Europe Standard Time')
RETURNS datetime2
AS
BEGIN
    DECLARE @LocalDateTime datetime2;

    SET @LocalDateTime = @UtcDateTime AT TIME ZONE 'UTC' AT TIME ZONE @TimeZoneIdentifier;

    RETURN @LocalDateTime;
END
GO
/****** Object:  Table [dbo].[Match]    Script Date: 06.08.2023 20:51:06 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Match](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[HomeTeamId] [bigint] NOT NULL,
	[GuestTeamId] [bigint] NOT NULL,
	[RefereeId] [bigint] NULL,
	[RoundId] [bigint] NOT NULL,
	[VenueId] [bigint] NULL,
	[OrigVenueId] [bigint] NULL,
	[LegSequenceNo] [int] NULL,
	[HomePoints] [int] NULL,
	[GuestPoints] [int] NULL,
	[IsComplete] [bit] NOT NULL,
	[IsOverruled] [bit] NOT NULL,
	[PlannedStart] [datetime] NULL,
	[PlannedEnd] [datetime] NULL,
	[OrigPlannedStart] [datetime] NULL,
	[OrigPlannedEnd] [datetime] NULL,
	[RealStart] [datetime] NULL,
	[RealEnd] [datetime] NULL,
	[Remarks] [nvarchar](max) NULL,
	[ChangeSerial] [bigint] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[ModifiedOn] [datetime] NOT NULL,
 CONSTRAINT [PK_cdb30d74095824096fb340c68d6] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Round]    Script Date: 06.08.2023 20:51:06 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Round](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[TournamentId] [bigint] NULL,
	[Name] [nvarchar](255) NOT NULL,
	[Description] [nvarchar](255) NOT NULL,
	[TypeId] [bigint] NOT NULL,
	[NumOfLegs] [int] NOT NULL,
	[MatchRuleId] [bigint] NOT NULL,
	[SetRuleId] [bigint] NOT NULL,
	[IsComplete] [bit] NOT NULL,
	[NextRoundId] [bigint] NULL,
	[CreatedOn] [datetime] NOT NULL,
	[ModifiedOn] [datetime] NOT NULL,
 CONSTRAINT [PK_ff808a54bd291274117f80c10df] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RoundType]    Script Date: 06.08.2023 20:51:06 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RoundType](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](255) NOT NULL,
	[Description] [nvarchar](255) NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[ModifiedOn] [datetime] NOT NULL,
 CONSTRAINT [PK_872525b4019ba9fd7b6b23f9ab5] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Team]    Script Date: 06.08.2023 20:51:06 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Team](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[VenueId] [bigint] NULL,
	[Name] [nvarchar](255) NOT NULL,
	[MatchDayOfWeek] [int] NULL,
	[MatchTime] [time](7) NULL,
	[ClubName] [nvarchar](255) NULL,
	[CreatedOn] [datetime] NOT NULL,
	[ModifiedOn] [datetime] NOT NULL,
 CONSTRAINT [PK_e4f33494c6d8484f20aabdbf101] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[TeamInRound]    Script Date: 06.08.2023 20:51:06 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TeamInRound](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[RoundId] [bigint] NOT NULL,
	[TeamId] [bigint] NOT NULL,
	[TeamNameForRound] [nvarchar](255) NULL,
	[CreatedOn] [datetime] NOT NULL,
	[ModifiedOn] [datetime] NOT NULL,
 CONSTRAINT [TeamRound_idx] PRIMARY KEY CLUSTERED 
(
	[RoundId] ASC,
	[TeamId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = ON, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Tournament]    Script Date: 06.08.2023 20:51:06 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Tournament](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](255) NOT NULL,
	[Description] [nvarchar](255) NULL,
	[TypeId] [bigint] NULL,
	[IsComplete] [bit] NOT NULL,
	[IsPlanningMode] [bit] NOT NULL,
	[NextTournamentId] [bigint] NULL,
	[CreatedOn] [datetime] NOT NULL,
	[ModifiedOn] [datetime] NOT NULL,
 CONSTRAINT [PK_f407daf4107a2d31aa47471145c] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Venue]    Script Date: 06.08.2023 20:51:06 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Venue](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](255) NOT NULL,
	[Extension] [nvarchar](255) NOT NULL,
	[Street] [nvarchar](255) NOT NULL,
	[PostalCode] [nvarchar](10) NOT NULL,
	[City] [nvarchar](255) NOT NULL,
	[Longitude] [float] NULL,
	[Latitude] [float] NULL,
	[PrecisePosition] [bit] NOT NULL,
	[Direction] [nvarchar](max) NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[ModifiedOn] [datetime] NOT NULL,
 CONSTRAINT [PK_26cdf024cef8161d4090ee30111] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  View [dbo].[Calendar]    Script Date: 06.08.2023 20:51:06 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[Calendar]
AS
SELECT TOP 100 PERCENT
  m.[Id] Id,
  r.[TournamentId] TournamentId,
  [Tournament].[Name] TournamentName,
  r.[Id] RoundId,
  r.[Name] RoundName,
  r.[Description] RoundDescription,
  rt.[Name] RoundTypeName,
  rt.[Description] RoundTypeDescription,
  hteam.[Id] HomeTeamId,
  htir.[TeamNameForRound] HomeTeamNameForRound,
  gteam.[Id] GuestTeamId,
  gtir.[TeamNameForRound] GuestTeamNameForRound,
  m.[PlannedStart],
  m.[PlannedEnd],
  v.[Id] VenueId,
  v.[Name] VenueName,
  v.[Extension] VenueExtension,
  v.[Street] VenueStreet,
  v.[PostalCode] VenuePostalCode,
  v.[City] VenueCity,
  v.[Longitude] VenueLongitude,
  v.[Latitude] VenueLatitude,
  v.[Direction] VenueDirection,
  m.[ChangeSerial] ChangeSerial,
  m.[ModifiedOn] ModifiedOn
FROM
  [Match] m
  JOIN [Team] hteam  ON (m.HomeTeamId = hteam.Id)
  JOIN [Team] gteam ON (m.[GuestTeamId] = gteam.[Id])
  LEFT JOIN [Venue] v ON (m.[VenueId] = v.[Id])
  JOIN [Round] r ON (m.[RoundId] = r.Id)
  /* Exclude matches where a team is not (i.e. no more) part of a round */
  JOIN [TeamInRound] htir ON (htir.[TeamId] = hteam.[Id] AND htir.[RoundId] = r.[Id])
  JOIN [TeamInRound] gtir ON (gtir.[TeamId] = gteam.[Id] AND gtir.[RoundId] = r.[Id])
  JOIN [RoundType] rt ON (r.[TypeId] = rt.[Id])
  JOIN [Tournament] ON (r.[TournamentId] = [Tournament].[Id])
  WHERE m.[IsComplete] = 0 AND NOT (m.[PlannedStart] IS NULL OR m.[PlannedEnd] IS NULL OR v.[Id] IS NULL)
ORDER BY r.[TournamentId], m.[PlannedStart];
GO
/****** Object:  Table [dbo].[RoundLeg]    Script Date: 06.08.2023 20:51:06 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RoundLeg](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[RoundId] [bigint] NOT NULL,
	[SequenceNo] [int] NOT NULL,
	[Description] [nvarchar](255) NULL,
	[StartDateTime] [datetime] NOT NULL,
	[EndDateTime] [datetime] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[ModifiedOn] [datetime] NOT NULL,
 CONSTRAINT [PK_a0718ea42c3be684e9e944c4033] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Set]    Script Date: 06.08.2023 20:51:06 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Set](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[MatchId] [bigint] NOT NULL,
	[SequenceNo] [int] NOT NULL,
	[HomeBallPoints] [int] NOT NULL,
	[GuestBallPoints] [int] NOT NULL,
	[HomeSetPoints] [int] NOT NULL,
	[GuestSetPoints] [int] NOT NULL,
	[HomeTimeout] [int] NOT NULL,
	[GuestTimeout] [int] NOT NULL,
	[IsOverruled] [bit] NOT NULL,
	[IsTieBreak] [bit] NOT NULL,
	[RealStart] [datetime] NULL,
	[RealEnd] [datetime] NULL,
	[Remarks] [nvarchar](max) NULL,
	[CreatedOn] [datetime] NOT NULL,
	[ModifiedOn] [datetime] NOT NULL,
 CONSTRAINT [PK_b4138ea4b60b7ab2bf5de64a148] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  View [dbo].[CompletedMatch]    Script Date: 06.08.2023 20:51:06 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[CompletedMatch]
AS
SELECT DISTINCT TOP 100 PERCENT
  m.[Id] Id,
  r.[TournamentId] TournamentId,
  [Tournament].[Name] TournamentName,
  r.[Id] RoundId,
  r.[Name] RoundName,
  r.[Description] RoundDescription,
  rt.[Name] RoundTypeName,
  rt.[Description] RoundTypeDescription,
  rl.[SequenceNo] RoundLegSequenceNo,
  rl.[Description] RoundLegDescription,
  hteam.[Id] HomeTeamId,
  htir.[TeamNameForRound] HomeTeamNameForRound,
  gteam.[Id] GuestTeamId,
  gtir.[TeamNameForRound] GuestTeamNameForRound,
  m.[VenueId],
  IIF(m.[RealStart] IS NULL, m.[PlannedStart], m.[RealStart]) MatchDate,
  m.[HomePoints] HomeMatchPoints,
  m.[GuestPoints] GuestMatchPoints,
  SetsOfMatch.HomeSetPoints HomeSetPoints,
  SetsOfMatch.GuestSetPoints GuestSetPoints,
  SetsOfMatch.HomeBallPoints HomeBallPoints,
  SetsOfMatch.GuestBallPoints GuestBallPoints,
  /* STUFF replaces use of UDF: IIF(m.IsComplete = 1, GetSetsAsText(m.Id), NULL) SetResult, */
  /* From MSSQL >= 2017 STUFF can be replaced with STRING_AGG */
  (STUFF(
      (SELECT ', ' + CONCAT(s.HomeBallPoints,':',s.GuestBallPoints)
        FROM [Set] s
        WHERE s.MatchId = m.[Id]
        ORDER BY s.[SequenceNo] ASC
        FOR XML PATH('')
      )
  ,1,2,'')) SetResults,
  m.[IsOverruled],
  m.[Remarks],
  m.[ModifiedOn]
FROM
  [Match] m
  JOIN [Team] hteam  ON (m.[HomeTeamId] = hteam.[Id])
  JOIN [Team] gteam ON (m.[GuestTeamId] = gteam.[Id])
  JOIN [Round] r ON (m.[RoundId] = r.Id)
  /* Exclude matches where a team is not (i.e. no more) part of a round */
  JOIN [TeamInRound] htir ON (htir.[TeamId] = hteam.[Id] AND htir.[RoundId] = r.[Id])
  JOIN [TeamInRound] gtir ON (gtir.[TeamId] = gteam.[Id] AND gtir.[RoundId] = r.[Id])
  JOIN [RoundType] rt ON (r.[TypeId] = rt.[Id])
  JOIN [RoundLeg] rl ON (r.[Id] = rl.[RoundId] AND  m.[LegSequenceNo] = rl.[SequenceNo])
  JOIN [Tournament] ON (r.[TournamentId] = [Tournament].[Id])
  LEFT OUTER JOIN (
    (SELECT 
	   MAX([MatchId]) MatchId,
       SUM([HomeBallPoints]) HomeBallPoints,
       SUM([GuestBallPoints]) GuestBallPoints,
       SUM([HomeSetPoints]) HomeSetPoints,
       SUM([GuestSetPoints]) GuestSetPoints
     FROM [Set]
     GROUP BY [MatchId])
    ) SetsOfMatch ON SetsOfMatch.[MatchId] = m.[Id]
    WHERE m.[IsComplete] = 1
ORDER BY r.[TournamentId], r.[Name], rl.[SequenceNo], [MatchDate]
GO
/****** Object:  View [dbo].[LatestTeamTournament]    Script Date: 06.08.2023 20:51:06 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[LatestTeamTournament]
AS
WITH cte AS
(
  /* Select teams, which did not participate in the next tournament */
  SELECT
	tour.[Id] TournamentId,
    tour.[Name] TournamentName,
    tour.[Description] TournamentDescription,
    r.[Id] RoundId,
    r.[Name] RoundName,
    r.[Description] RoundDescription,
    rt.[Name] RoundTypeName,
    rt.[Description] RoundTypeDescription,
    t.[Id] TeamId,
    t.[Name] TeamName,
    tir.[TeamNameForRound] TeamNameForRound,
    t.[ClubName] TeamClubName,
    t.[MatchDayOfWeek] TeamMatchDayOfWeek,
    t.[MatchTime] TeamMatchTime,
    /* The latest tournament is in the first row */
    ROW_NUMBER() OVER (PARTITION BY t.[Id] ORDER BY tour.[Id] DESC) AS rowNo
  FROM [TeamInRound] tir
    JOIN [Team] t ON (tir.[TeamId] = t.[Id])
    JOIN [Round] r ON (tir.[RoundId] = r.[Id])
    JOIN [RoundType] rt ON (r.TypeId = rt.[Id])
	JOIN [Tournament] tour ON (r.[TournamentId] = tour.[Id])
  WHERE NOT EXISTS
  (
  	SELECT whereTir.[TeamId]
    FROM [TeamInRound] whereTir
    JOIN [Round] whereRound ON (whereTir.[RoundId] = whereRound.[Id])
	JOIN [Tournament] stour ON (whereRound.[TournamentId] = tour.[NextTournamentId])
    WHERE whereTir.[TeamId] = tir.[TeamId]
  )
)
SELECT
	[TournamentId], 
    [TournamentName], 
    [TournamentDescription], 
    [RoundId], 
    [RoundName],
    [RoundDescription],
    [RoundTypeName],
    [RoundTypeDescription],
    [TeamId],
    [TeamName],
    [TeamNameForRound],
    [TeamClubName],
    [TeamMatchDayOfWeek],
    CAST([TeamMatchTime] AS time(0)) TeamMatchTime
FROM cte
WHERE rowNo = 1
GO
/****** Object:  Table [dbo].[ManagerOfTeam]    Script Date: 06.08.2023 20:51:06 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ManagerOfTeam](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[UserId] [bigint] NOT NULL,
	[TeamId] [bigint] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[ModifiedOn] [datetime] NOT NULL,
 CONSTRAINT [PK_61281d44c57bcda8f8ca6d2d85a] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC,
	[TeamId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[User]    Script Date: 06.08.2023 20:51:06 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[User](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Guid] [nvarchar](50) NOT NULL,
	[UserName] [nvarchar](255) NOT NULL,
	[PasswordHash] [nvarchar](max) NOT NULL,
	[Email] [nvarchar](255) NOT NULL,
	[EmailConfirmedOn] [datetime] NULL,
	[PhoneNumber] [nvarchar](40) NOT NULL,
	[PhoneNumberConfirmedOn] [datetime] NULL,
	[LastLoginOn] [datetime] NULL,
	[AccessFailedCount] [int] NOT NULL,
	[LockoutEndDateUtc] [datetime] NULL,
	[Gender] [nvarchar](1) NOT NULL,
	[Title] [nvarchar](255) NOT NULL,
	[FirstName] [nvarchar](255) NOT NULL,
	[MiddleName] [nvarchar](255) NOT NULL,
	[LastName] [nvarchar](255) NOT NULL,
	[Nickname] [nvarchar](255) NOT NULL,
	[PhoneNumber2] [nvarchar](40) NOT NULL,
	[Email2] [nvarchar](100) NOT NULL,
	[Birthday] [datetime] NULL,
	[Remarks] [nvarchar](4000) NULL,
	[CreatedOn] [datetime] NOT NULL,
	[ModifiedOn] [datetime] NOT NULL,
 CONSTRAINT [PK_05646114ed0b268cc9236c4f656] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [UC_5ee8deb40f8bca844c8745f8722] UNIQUE NONCLUSTERED 
(
	[Email] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [UC_9d0e7fb4df4865699f58d126a42] UNIQUE NONCLUSTERED 
(
	[UserName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  View [dbo].[m_AllTournamentData]    Script Date: 06.08.2023 20:51:06 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[m_AllTournamentData]
AS
SELECT DISTINCT TOP 10000 tournament.Id, round.Name AS RoundName, round.Description AS RoundDescription, Team.Id AS TeamId, Team.Name AS TeamName, tir.TeamNameForRound, dbo.Weekday(Team.MatchDayOfWeek) AS Weekday, Team.MatchTime, 
                         Team.ClubName, [user].Id AS UserId, [user].Guid, [user].UserName, [user].Email, [user].EmailConfirmedOn, [user].PhoneNumber, [user].PhoneNumberConfirmedOn, 
                         [user].LastLoginOn, [user].AccessFailedCount, [user].LockoutEndDateUtc, [user].Gender, [user].Title, [user].FirstName, [user].MiddleName, [user].LastName, [user].Nickname,
                         [user].PhoneNumber2, [user].Email2, [user].Birthday, [user].Remarks, 
                         tournament.Name AS TournamentName, tournament.Description AS TournamentDescription, 
                         venue.Name AS VenueName, venue.Street AS VenueStreet, venue.PostalCode AS VenuePostalCode, venue.City AS VenueCity, venue.Extension AS VenueExtension, venue.Direction AS VenueDirection, 
                         venue.Latitude AS VenueLatitude, venue.Longitude AS VenueLongitude
FROM            Team INNER JOIN
                         TeamInRound AS tir ON Team.Id = tir.TeamId INNER JOIN
                         Round AS round ON tir.RoundId = round.Id INNER JOIN
                         Tournament AS tournament ON round.TournamentId = tournament.Id INNER JOIN
                         Venue AS venue ON Team.VenueId = venue.Id INNER JOIN
                         ManagerOfTeam AS mot ON tir.TeamId = mot.TeamId INNER JOIN
                         [User] AS [user] ON mot.UserId = [user].Id
ORDER BY tournament.Id, RoundName, tir.TeamNameForRound, [user].LastName
GO
/****** Object:  View [dbo].[m_TeamMatchesByDate]    Script Date: 06.08.2023 20:51:06 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[m_TeamMatchesByDate]
AS
select
  match.Id AS MatchId,
  round.TournamentId AS TournamentId,
  match.RoundId AS RoundId,
  round.Description AS RoundDescription,
  RoundLeg.SequenceNo AS LegSequenceNo,
  roundleg.Description AS LegDescription,
  team.Id AS TeamId,
  team.Name AS TeamName,
  teamguest.Id AS OpponentId,
  teamguest.Name AS OpponentName,
  match.PlannedStart AS PlannedStart,
  match.VenueId AS VenueId,
  venue.Name AS Venue,
  venue.City AS City,
  match.IsComplete AS IsComplete,
  'X'  AS HomeMatch
from
  ((((match
  join team on ((match.HomeTeamId = team.Id)))
  join team teamguest on ((match.GuestTeamId = teamguest.Id)))
  join venue on ((match.VenueId = venue.Id)))
  join round on ((match.RoundId = round.Id))
  join roundleg on (match.RoundId = RoundLeg.RoundId and match.LegSequenceNo = roundleg.SequenceNo))
union
select
  match.Id AS MatchId,
  round.TournamentId AS TournamentId,
  match.RoundId AS RoundId,
  round.Description AS RoundDescription,
  RoundLeg.SequenceNo AS LegSequenceNo,
  roundleg.Description AS LegDescription,
  teamguest.Id AS TeamId,
  teamguest.Name AS TeamName,
  team.Id AS OpponentId,
  team.Name AS OpponentName,
  match.PlannedStart AS PlannedStart,
  match.VenueId AS VenueId,
  venue.Name AS Venue,
  venue.City AS City,
  match.IsComplete AS IsComplete,
  ''  AS HomeMatch
from
  ((((match
  join team on ((match.HomeTeamId = team.Id)))
  join team teamguest on ((match.GuestTeamId = teamguest.Id)))
  join venue on ((match.VenueId = venue.Id)))
  join round on ((match.RoundId = round.Id))
  join roundleg on (match.RoundId = RoundLeg.RoundId and match.LegSequenceNo = roundleg.SequenceNo))
GO
/****** Object:  View [dbo].[m_TeamsNotInLastRound]    Script Date: 25.04.2024 20:45:00 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[m_TeamsNotInLastRound]
AS
select distinct 
  team.Name AS TeamName,
  [user].Id AS UserId,
  [user].Email AS Email,
  [user].FirstName AS FirstName,
  [user].Nickname AS Nickname,
  [user].Gender AS Gender,
  [user].LastName AS LastName,
  [user].Title AS Title,
  [user].PhoneNumber AS PhoneNumber,
  [user].UserName AS UserName,
  [user].Id AS Id
from
  ((((teaminround
  join team on ((teaminround.TeamId = team.Id)))
  join round on ((teaminround.RoundId = round.Id)))
  join managerofteam on ((team.Id = managerofteam.TeamId)))
  join [user] on ((managerofteam.UserId = [user].Id)))
where
  ((round.TournamentId <> 2) and
  (not (team.Id in (
                            select
                              distinct t2.Id
                            from
                              ((teaminround tir2
                              join team t2 on ((tir2.TeamId = t2.Id)))
                              join round r2 on ((tir2.RoundId = r2.Id)))
                            where
                              (r2.TournamentId = 22)
  ))));
GO
/****** Object:  View [dbo].[MatchCompleteRaw]    Script Date: 06.08.2023 20:51:06 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[MatchCompleteRaw]
AS
SELECT DISTINCT TOP 100 PERCENT
  m.[Id] Id,
  r.[TournamentId] TournamentId,
  r.[Id] RoundId,
  hteam.[Id] HomeTeamId,
  gteam.[Id] GuestTeamId,
  m.[VenueId],
  IIF(m.[RealStart] IS NULL, m.[PlannedStart], m.[RealStart]) MatchDate,
  m.[HomePoints] HomeMatchPoints,
  m.[GuestPoints] GuestMatchPoints,
  SetsOfMatch.HomeSetPoints HomeSetPoints,
  SetsOfMatch.GuestSetPoints GuestSetPoints,
  SetsOfMatch.HomeBallPoints HomeBallPoints,
  SetsOfMatch.GuestBallPoints GuestBallPoints,
  m.[IsOverruled],
  m.[ModifiedOn]
FROM
  [Match] m
  JOIN [Team] hteam  ON (m.[HomeTeamId] = hteam.[Id])
  JOIN [Team] gteam ON (m.[GuestTeamId] = gteam.[Id])
  JOIN [Round] r ON (m.[RoundId] = r.Id)
  /* Exclude matches where a team is not (i.e. no more) part of a round */
  JOIN [TeamInRound] htir ON (htir.[TeamId] = hteam.[Id] AND htir.[RoundId] = r.[Id])
  JOIN [TeamInRound] gtir ON (gtir.[TeamId] = gteam.[Id] AND gtir.[RoundId] = r.[Id])
  LEFT OUTER JOIN (
    (SELECT 
	   MAX([MatchId]) MatchId,
       SUM([HomeBallPoints]) HomeBallPoints,
       SUM([GuestBallPoints]) GuestBallPoints,
       SUM([HomeSetPoints]) HomeSetPoints,
       SUM([GuestSetPoints]) GuestSetPoints
     FROM [Set]
     GROUP BY [MatchId])
    ) SetsOfMatch ON SetsOfMatch.[MatchId] = m.[Id]
    WHERE m.[IsComplete] = 1
ORDER BY r.[TournamentId], r.[Id], [MatchDate]
GO
/****** Object:  Table [dbo].[SetRule]    Script Date: 06.08.2023 20:51:06 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SetRule](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](255) NOT NULL,
	[NumOfPointsToWinRegular] [int] NOT NULL,
	[NumOfPointsToWinTiebreak] [int] NOT NULL,
	[PointsDiffToWinRegular] [int] NOT NULL,
	[PointsDiffToWinTiebreak] [int] NOT NULL,
	[PointsSetLost] [int] NOT NULL,
	[PointsSetTie] [int] NOT NULL,
	[PointsSetWon] [int] NOT NULL,
	[MaxTimeouts] [int] NOT NULL,
	[MaxSubstitutions] [int] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[ModifiedOn] [datetime] NOT NULL,
 CONSTRAINT [PK_23e33af454a87a76a066050c891] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[MatchRule]    Script Date: 06.08.2023 20:51:06 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MatchRule](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](255) NOT NULL,
	[NumOfSets] [int] NOT NULL,
	[BestOf] [bit] NOT NULL,
	[PointsMatchWon] [int] NOT NULL,
	[PointsMatchLost] [int] NOT NULL,
	[PointsMatchWonAfterTieBreak] [int] NOT NULL,
	[PointsMatchLostAfterTieBreak] [int] NOT NULL,
	[PointsMatchTie] [int] NOT NULL,
	[RankComparer] [int] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[ModifiedOn] [datetime] NOT NULL,
 CONSTRAINT [PK_086d14b497db1fbf2242367f70b] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  View [dbo].[MatchReportSheet]    Script Date: 06.08.2023 20:51:06 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[MatchReportSheet]
AS
SELECT TOP 100 PERCENT
  m.[Id] Id,
  r.[TournamentId] TournamentId,
  [Tournament].[Name] TournamentName,
  r.[Name] RoundName,
  r.[Description] RoundDescription,
  rt.[Name] RoundTypeName,
  rt.[Description] RoundTypeDescription,
  rl.[SequenceNo] RoundLegSequenceNo,
  rl.[Description] RoundLegDescription,
  hteam.[Id] HomeTeamId,
  htir.[TeamNameForRound] HomeTeamNameForRound,
  gteam.[Id] GuestTeamId,
  gtir.[TeamNameForRound] GuestTeamNameForRound,
  m.[PlannedStart],
  m.[OrigPlannedStart],
  mrr.[NumOfSets] NumOfSets,
  mrr.[BestOf] BestOf,
  srr.[NumOfPointsToWinRegular] NumOfPointsToWinRegular,
  srr.[NumOfPointsToWinTieBreak] NumOfPointsToWinTieBreak,
  srr.[MaxTimeouts] MaxTimeouts,
  srr.[MaxSubstitutions] MaxSubstitutions,
  m.[ChangeSerial] ChangeSerial,
  m.[Remarks] Remarks,
  m.[ModifiedOn] ModifiedOn
FROM
  [Match] m
  JOIN [Round] r ON (m.[RoundId] = r.Id)
  JOIN [Team] hteam  ON (m.HomeTeamId = hteam.Id)
  JOIN [Team] gteam ON (m.[GuestTeamId] = gteam.[Id])
  JOIN [TeamInRound] htir ON (m.HomeTeamId = htir.[TeamId] AND htir.[RoundId] = r.[Id])
  JOIN [TeamInRound] gtir ON (m.[GuestTeamId] = gtir.[TeamId] AND gtir.[RoundId] = r.[Id])
  JOIN [RoundType] rt ON (r.[TypeId] = rt.[Id])
  JOIN [RoundLeg] rl ON (r.[Id] = rl.[RoundId] AND  m.[LegSequenceNo] = rl.[SequenceNo])
  JOIN [Tournament] ON (r.[TournamentId] = [Tournament].[Id])
  JOIN [MatchRule] mrr ON (r.[MatchRuleId] = mrr.[Id])
  JOIN [SetRule] srr ON (r.[SetRuleId] = srr.[Id])
GO
/****** Object:  View [dbo].[MatchToPlayRaw]    Script Date: 06.08.2023 20:51:06 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[MatchToPlayRaw]
AS
SELECT DISTINCT TOP 100 PERCENT
  m.[Id] Id,
  r.[TournamentId] TournamentId,
  r.[Id] RoundId,
  hteam.[Id] HomeTeamId,
  gteam.[Id] GuestTeamId,
  m.[VenueId],
  m.[PlannedStart] MatchDate,
  m.[ModifiedOn]
FROM
  [Match] m
  JOIN [Team] hteam  ON (m.[HomeTeamId] = hteam.[Id])
  JOIN [Team] gteam ON (m.[GuestTeamId] = gteam.[Id])
  JOIN [Round] r ON (m.[RoundId] = r.Id)
  /* Exclude matches where a team is not (i.e. no more) part of a round */
  JOIN [TeamInRound] htir ON (htir.[TeamId] = hteam.[Id] AND htir.[RoundId] = r.[Id])
  JOIN [TeamInRound] gtir ON (gtir.[TeamId] = gteam.[Id] AND gtir.[RoundId] = r.[Id])
WHERE m.[IsComplete] = 0
ORDER BY r.[TournamentId], r.[Id], [MatchDate]
GO
/****** Object:  View [dbo].[PlannedMatch]    Script Date: 06.08.2023 20:51:06 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[PlannedMatch]
AS
SELECT TOP 100 PERCENT
  m.[Id] Id,
  r.[TournamentId] TournamentId,
  [Tournament].[Name] TournamentName,
  r.[Id] RoundId,
  r.[Name] RoundName,
  r.[Description] RoundDescription,
  rt.[Name] RoundTypeName,
  rt.[Description] RoundTypeDescription,
  rl.[SequenceNo] RoundLegSequenceNo,
  rl.[Description] RoundLegDescription,
  hteam.[Id] HomeTeamId,
  htir.[TeamNameForRound] HomeTeamNameForRound,
  gteam.[Id] GuestTeamId,
  gtir.[TeamNameForRound] GuestTeamNameForRound,
  m.[PlannedStart],
  m.[PlannedEnd],
  m.[OrigPlannedStart],
  m.[OrigPlannedEnd],
  v.[Id] VenueId,
  v.[Name] VenueName,
  v.[Extension] VenueExtension,
  vo.[Id] OrigVenueId,
  vo.[Name] OrigVenueName,
  vo.[Extension] OrigVenueExtension,
  m.[ChangeSerial] ChangeSerial,
  m.[Remarks] Remarks,
  m.[ModifiedOn] ModifiedOn
FROM
  [Match] m
  JOIN [Team] hteam  ON (m.HomeTeamId = hteam.Id)
  JOIN [Team] gteam ON (m.[GuestTeamId] = gteam.[Id])
  LEFT JOIN [Venue] v ON (m.[VenueId] = v.[Id])
  LEFT JOIN [Venue] vo ON (m.[OrigVenueId] = vo.[Id])
  JOIN [Round] r ON (m.[RoundId] = r.Id)
  /* Exclude matches where a team is not (i.e. no more) part of a round */
  JOIN [TeamInRound] htir ON (htir.[TeamId] = hteam.[Id] AND htir.[RoundId] = r.[Id])
  JOIN [TeamInRound] gtir ON (gtir.[TeamId] = gteam.[Id] AND gtir.[RoundId] = r.[Id])
  JOIN [RoundType] rt ON (r.[TypeId] = rt.[Id])
  JOIN [RoundLeg] rl ON (r.[Id] = rl.[RoundId] AND  m.[LegSequenceNo] = rl.[SequenceNo])
  JOIN [Tournament] ON (r.[TournamentId] = [Tournament].[Id])
  WHERE m.[IsComplete] = 0
ORDER BY r.[TournamentId], r.[Name], rl.[SequenceNo], [PlannedStart]
GO
/****** Object:  Table [dbo].[Ranking]    Script Date: 06.08.2023 20:51:06 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Ranking](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[TournamentId] [bigint] NOT NULL,
	[RoundId] [bigint] NOT NULL,
	[TeamId] [bigint] NOT NULL,
	[ValuationDate] [datetime] NOT NULL,
	[Rank] [int] NOT NULL,
	[MatchPointsWon] [int] NULL,
	[MatchPointsLost] [int] NULL,
	[SetPointsWon] [int] NULL,
	[SetPointsLost] [int] NULL,
	[BallPointsWon] [int] NULL,
	[BallPointsLost] [int] NULL,
	[MatchesPlayed] [int] NOT NULL,
	[MatchesToPlay] [int] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[ModifiedOn] [datetime] NOT NULL,
 CONSTRAINT [PK_d28bac344c795e61c21b8e6d9b9] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  View [dbo].[RankingList]    Script Date: 06.08.2023 20:51:06 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[RankingList]
AS
SELECT
  [Tournament].[Id] TournamentId,
  [Tournament].[Name] TournamentName,
  [Tournament].[Description] TournamentDescription,
  [Tournament].[IsComplete] TournamentIsComplete,
  [Round].[Id] RoundId,
  [Round].[Name] RoundName,
  [Round].[Description] RoundDescription,
  [RoundType].[Name] RoundTypeName,
  [RoundType].[Description] RoundTypeDescription,
  [Team].[Id] TeamId,
  [Team].[Name] TeamName,
  [TeamInRound].[TeamNameForRound] TeamNameForRound,
  [Team].[ClubName] ClubName,
  [Ranking].[ValuationDate] ValuationDate,
  [Ranking].[Rank] Rank,
  [Ranking].[MatchPointsWon] MatchPointsWon,
  [Ranking].[MatchPointsLost] MatchPointsLost,
  [Ranking].[SetPointsWon] SetPointsWon,
  [Ranking].[SetPointsLost] SetPointsLost,
  [Ranking].[BallPointsWon] BallPointsWon,
  [Ranking].[BallPointsLost] BallPointsLost,
  [Ranking].[MatchesPlayed] MatchesPlayed,
  [Ranking].[MatchesToPlay] MatchesToPlay,
  [Ranking].[ModifiedOn] ModifiedOn
FROM
  [Ranking]
  JOIN [Round] ON ([Ranking].[RoundId] = [Round].[Id])
  JOIN [Tournament] ON ([Round].[TournamentId] = [Tournament].[Id])
  JOIN [RoundType] ON ([RoundType].[Id] = [Round].[TypeId])
  JOIN [Team] ON ([Ranking].[TeamId] = [Team].[Id])
  JOIN [TeamInRound] ON ([TeamInRound].[RoundId] = [Round].[Id] AND [TeamInRound].[TeamId] = [Team].[Id])
GO
/****** Object:  View [dbo].[RoundLegPeriod]    Script Date: 06.08.2023 20:51:06 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[RoundLegPeriod]
AS
  SELECT TOP 100 PERCENT
   r.TournamentId, r.Id RoundId, leg.Id LegId, leg.SequenceNo, leg.StartDateTime, leg.EndDateTime
  FROM [Round] r
  	INNER JOIN [RoundLeg] leg ON (r.Id = leg.RoundId)
  ORDER BY r.Id, leg.SequenceNo
GO
/****** Object:  View [dbo].[RoundTeam]    Script Date: 06.08.2023 20:51:06 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[RoundTeam]
AS
  /* Round info with team info */
  SELECT 
    r.[Id] RoundId,
    r.[Name] RoundName,
    r.[Description] RoundDescription,
    rt.[Name] RoundTypeName,
    rt.[Description] RoundTypeDescription,
    t.[Id] TeamId,
    t.[Name] TeamName,
    tir.[TeamNameForRound] TeamNameForRound,
    t.[ClubName] TeamClubName,
    t.[MatchDayOfWeek] TeamMatchDayOfWeek,
    t.[MatchTime] TeamMatchTime,
    t.[ModifiedOn] TeamModifiedOn,
    tour.[Id] TournamentId
  FROM [TeamInRound] tir
    JOIN [Team] t ON (tir.[TeamId] = t.[Id])
    JOIN [Round] r ON (tir.[RoundId] = r.[Id])
    JOIN [RoundType] rt ON (rt.[Id] = r.[TypeId])
	JOIN [Tournament] tour ON (r.[TournamentId] = tour.[Id])
GO
/****** Object:  Table [dbo].[PlayerInTeam]    Script Date: 06.08.2023 20:51:06 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PlayerInTeam](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[UserId] [bigint] NOT NULL,
	[TeamId] [bigint] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[ModifiedOn] [datetime] NOT NULL,
 CONSTRAINT [PK_813c96e4d12bbb271cfd2e6f609] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC,
	[TeamId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  View [dbo].[TeamUserRound]    Script Date: 06.08.2023 20:51:06 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[TeamUserRound]
AS
SELECT
  t.[Id] TeamId,
  t.[Name] TeamName,
  tir.[TeamNameForRound] TeamNameForRound,
  t.[MatchDayOfWeek],
  FORMAT(CAST('2019-01-20T12:00:00' AS datetime) + t.[MatchDayOfWeek], 'dddd', 'de-DE') MatchWeekday,
  t.[MatchTime] MatchTime,
  t.[ClubName] ClubName,
  t.[ModifiedOn] TeamModifiedOn,
  u.[Id] UserId,
  u.[Gender] Gender,
  u.[Title] Title,
  u.[FirstName] FirstName,
  u.[MiddleName] MiddleName,
  u.[LastName] LastName,
  u.[Nickname] Nickname,
  u.[PhoneNumber] PhoneNumber,
  u.[PhoneNumber2] PhoneNumber2,
  u.[Email] Email,
  u.[Email2] Email2,
  u.[ModifiedOn] UserModifiedOn,
  motPit.IsManager,
  motPit.IsPlayer,
  r.[Id] RoundId,
  r.[Name] RoundName,
  r.[Description] RoundDescription,
  r.TournamentId TournamentId
FROM
      /* Select users who are managers or players, returning one user per team  */
      /* ISNULL test ensures the column is NOT NULL */
      (SELECT p.UserId, p.TeamId, ISNULL(CAST(SUM(p.IsManager) AS bit),0) IsManager, ISNULL(CAST(SUM(p.IsPlayer) AS bit),0) IsPlayer
        FROM
        (SELECT mot.[UserId] UserId, mot.[TeamId] TeamId, 1 IsManager, 0 IsPlayer
        	FROM [ManagerOfTeam] mot
        	UNION ALL
        SELECT pit.[UserId] UserId, pit.[TeamId] TeamId, 0 IsManager, 1 IsPlayer
        	FROM [PlayerInTeam] pit) p
      GROUP BY p.UserId, p.TeamId) motPit
  JOIN [User] u ON (motPit.[UserId] = u.[Id])
  JOIN [Team] t ON (motPit.[TeamId] = t.[Id])
  JOIN [TeamInRound] tir ON (motPit.[TeamId] = tir.[TeamId]) /* only teams in a round */
  JOIN [Round] r ON (tir.[RoundId] = r.[Id])
GO
/****** Object:  View [dbo].[TeamVenueRound]    Script Date: 06.08.2023 20:51:06 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[TeamVenueRound]
AS
  /* Team with venue and round information */
  SELECT 
    t.[Id] TeamId,
    t.[Name] TeamName,
    tir.[TeamNameForRound] TeamNameForRound,
    t.[ClubName] TeamClubName,
    t.[MatchDayOfWeek] MatchDayOfWeek,
    FORMAT(CAST('2019-01-20T12:00:00' AS datetime) + t.[MatchDayOfWeek], 'dddd', 'de-DE') MatchWeekday,
    t.[MatchTime] MatchTime,
    t.[ModifiedOn] TeamModifiedOn,
    v.[Id] VenueId,
    v.[Name] VenueName,
    v.[Extension] VenueExtension,
    v.[Street] VenueStreet,
    v.[PostalCode] VenuePostalCode,
    v.[City] VenueCity,
    v.[Direction] VenueDirection,
    v.[Longitude] VenueLongitude,
    v.[Latitude] VenueLatitude,
    v.[PrecisePosition] VenuePrecisePosition,
    v.[ModifiedOn] VenueModifiedOn,
    r.[Id] RoundId,
    r.[Name] RoundName,
    r.[Description] RoundDescription,
    rt.[Name] RoundTypeName,
    rt.[Description] RoundTypeDescription,
    tour.[Id] TournamentId
  FROM [TeamInRound] tir
    JOIN [Team] t ON (tir.[TeamId] = t.[Id])
    LEFT JOIN [Venue] v ON (t.[VenueId] = v.[Id])
    JOIN [Round] r ON (tir.[RoundId] = r.[Id])
    JOIN [RoundType] rt ON (rt.[Id] = r.[TypeId])
	JOIN [Tournament] tour ON (r.[TournamentId] = tour.[Id])
GO
/****** Object:  View [dbo].[VenueTeam]    Script Date: 06.08.2023 20:51:06 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[VenueTeam]
AS
  /* Team of a tournament which have a venue */
  SELECT 
    v.[Id] VenueId,
    v.[Name] VenueName,
    v.[Extension] VenueExtension,
    v.[Street] Street,
    v.[PostalCode] PostalCode,
    v.[City] City,
    v.[Direction] Direction,
    v.[Longitude] Longitude,
    v.[Latitude] Latitude,
    v.[PrecisePosition] PrecisePosition,
    t.[Id] TeamId,
    t.[Name] TeamName,
    tir.[TeamNameForRound] TeamNameForRound,
    t.[ClubName] TeamClubName,
    tour.[Id] TournamentId, 
    r.[Id] RoundId
  FROM [TeamInRound] tir
    JOIN [Team] t ON (tir.[TeamId] = t.[Id])
    JOIN [Venue] v ON (t.[VenueId] = v.[Id])
    JOIN [Round] r ON (tir.[RoundId] = r.[Id])
    JOIN [Tournament] tour ON (r.[TournamentId] = tour.[Id])
GO
/****** Object:  Table [dbo].[AvailableMatchDate]    Script Date: 06.08.2023 20:51:06 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AvailableMatchDate](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[TournamentId] [bigint] NOT NULL,
	[HomeTeamId] [bigint] NOT NULL,
	[VenueId] [bigint] NOT NULL,
	[MatchStartTime] [datetime] NOT NULL,
	[MatchEndTime] [datetime] NOT NULL,
	[IsGenerated] [bit] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[ModifiedOn] [datetime] NOT NULL,
 CONSTRAINT [PK_378f3f14a1c871b25041c380344] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ExcludeMatchDate]    Script Date: 06.08.2023 20:51:06 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ExcludeMatchDate](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[TournamentId] [bigint] NOT NULL,
	[RoundId] [bigint] NULL,
	[TeamId] [bigint] NULL,
	[DateFrom] [datetime] NOT NULL,
	[DateTo] [datetime] NOT NULL,
	[Reason] [nvarchar](60) NULL,
	[CreatedOn] [datetime] NOT NULL,
	[ModifiedOn] [datetime] NOT NULL,
 CONSTRAINT [PK_31fdaf2445a85a5094198491bb0] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[IdentityRole]    Script Date: 06.08.2023 20:51:06 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[IdentityRole](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](255) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [Role_uq] UNIQUE NONCLUSTERED 
(
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[IdentityRoleClaim]    Script Date: 06.08.2023 20:51:06 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[IdentityRoleClaim](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[RoleId] [bigint] NOT NULL,
	[ClaimType] [nvarchar](255) NOT NULL,
	[ClaimValue] [nvarchar](max) NOT NULL,
	[ValueType] [nvarchar](255) NULL,
	[Issuer] [nvarchar](255) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[IdentityUserClaim]    Script Date: 06.08.2023 20:51:06 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[IdentityUserClaim](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[UserId] [bigint] NOT NULL,
	[ClaimType] [nvarchar](255) NOT NULL,
	[ClaimValue] [nvarchar](max) NOT NULL,
	[ValueType] [nvarchar](255) NULL,
	[Issuer] [nvarchar](255) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[IdentityUserLogin]    Script Date: 06.08.2023 20:51:06 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[IdentityUserLogin](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[UserId] [bigint] NOT NULL,
	[LoginProvider] [nvarchar](128) NOT NULL,
	[ProviderKey] [nvarchar](128) NOT NULL,
	[ProviderDisplayName] [nvarchar](128) NOT NULL,
 CONSTRAINT [PK_IdentityUserLogins] PRIMARY KEY CLUSTERED 
(
	[LoginProvider] ASC,
	[ProviderKey] ASC,
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [UserLogin_uq] UNIQUE NONCLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[IdentityUserRole]    Script Date: 06.08.2023 20:51:06 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[IdentityUserRole](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[UserId] [bigint] NOT NULL,
	[RoleId] [bigint] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[UserId] ASC,
	[RoleId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[IdentityUserToken]    Script Date: 06.08.2023 20:51:06 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[IdentityUserToken](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[UserId] [bigint] NOT NULL,
	[LoginProvider] [nvarchar](128) NOT NULL,
	[Name] [nvarchar](128) NOT NULL,
	[Value] [nvarchar](max) NULL,
 CONSTRAINT [PK_UserTokens] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC,
	[LoginProvider] ASC,
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [UserTokens_uq] UNIQUE NONCLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Registration]    Script Date: 06.08.2023 20:51:06 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Registration](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[TournamentId] [bigint] NOT NULL,
	[TeamId] [bigint] NOT NULL,
	[ManagerId] [bigint] NOT NULL,
	[Guid] [nvarchar](50) NOT NULL,
	[AppliedWithMessage] [nvarchar](4000) NULL,
	[AppliedOn] [datetime] NOT NULL,
 CONSTRAINT [PK_5a14f5d4dc690066c854d5d8685] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[TournamentType]    Script Date: 06.08.2023 20:51:06 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TournamentType](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](255) NOT NULL,
	[Description] [nvarchar](255) NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[ModifiedOn] [datetime] NOT NULL,
 CONSTRAINT [PK_848db044eef87f3bbcc96868650] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IdentityUserClaim_idx]    Script Date: 06.08.2023 20:51:06 ******/
CREATE NONCLUSTERED INDEX [IdentityUserClaim_idx] ON [dbo].[IdentityUserClaim]
(
	[ClaimType] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = OFF, ALLOW_PAGE_LOCKS = OFF) ON [PRIMARY]
GO
/****** Object:  Index [UserRole_idx]    Script Date: 06.08.2023 20:51:06 ******/
CREATE UNIQUE NONCLUSTERED INDEX [UserRole_idx] ON [dbo].[IdentityUserRole]
(
	[UserId] ASC,
	[RoleId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = ON, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [UserTeam_idx]    Script Date: 06.08.2023 20:51:06 ******/
CREATE UNIQUE NONCLUSTERED INDEX [UserTeam_idx] ON [dbo].[ManagerOfTeam]
(
	[UserId] ASC,
	[TeamId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = ON, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [PlayerTeam_idx]    Script Date: 06.08.2023 20:51:06 ******/
CREATE UNIQUE NONCLUSTERED INDEX [PlayerTeam_idx] ON [dbo].[PlayerInTeam]
(
	[UserId] ASC,
	[TeamId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = ON, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE [dbo].[AvailableMatchDate] ADD  CONSTRAINT [DF_AvailableMatchDate_CreatedOn]  DEFAULT (getdate()) FOR [CreatedOn]
GO
ALTER TABLE [dbo].[AvailableMatchDate] ADD  CONSTRAINT [DF_AvailableMatchDate_ModifiedOn]  DEFAULT (getdate()) FOR [ModifiedOn]
GO
ALTER TABLE [dbo].[ExcludeMatchDate] ADD  CONSTRAINT [DF_ExcludeMatchDate_CreatedOn]  DEFAULT (getdate()) FOR [CreatedOn]
GO
ALTER TABLE [dbo].[ExcludeMatchDate] ADD  CONSTRAINT [DF_ExcludeMatchDate_ModifiedOn]  DEFAULT (getdate()) FOR [ModifiedOn]
GO
ALTER TABLE [dbo].[IdentityRole] ADD  DEFAULT ('') FOR [Name]
GO
ALTER TABLE [dbo].[IdentityRoleClaim] ADD  DEFAULT ('') FOR [ClaimType]
GO
ALTER TABLE [dbo].[IdentityRoleClaim] ADD  DEFAULT ('') FOR [ClaimValue]
GO
ALTER TABLE [dbo].[IdentityUserClaim] ADD  DEFAULT ('') FOR [ClaimType]
GO
ALTER TABLE [dbo].[IdentityUserClaim] ADD  DEFAULT ('') FOR [ClaimValue]
GO
ALTER TABLE [dbo].[IdentityUserLogin] ADD  DEFAULT ('') FOR [LoginProvider]
GO
ALTER TABLE [dbo].[IdentityUserLogin] ADD  DEFAULT ('') FOR [ProviderKey]
GO
ALTER TABLE [dbo].[IdentityUserLogin] ADD  DEFAULT ('') FOR [ProviderDisplayName]
GO
ALTER TABLE [dbo].[IdentityUserToken] ADD  DEFAULT ('') FOR [LoginProvider]
GO
ALTER TABLE [dbo].[IdentityUserToken] ADD  DEFAULT ('') FOR [Name]
GO
ALTER TABLE [dbo].[ManagerOfTeam] ADD  CONSTRAINT [DF_ManagerOfTeam_CreatedOn]  DEFAULT (getdate()) FOR [CreatedOn]
GO
ALTER TABLE [dbo].[ManagerOfTeam] ADD  CONSTRAINT [DF_ManagerOfTeam_ModifiedOn]  DEFAULT (getdate()) FOR [ModifiedOn]
GO
ALTER TABLE [dbo].[Match] ADD  DEFAULT ((1)) FOR [IsComplete]
GO
ALTER TABLE [dbo].[Match] ADD  DEFAULT ((0)) FOR [IsOverruled]
GO
ALTER TABLE [dbo].[Match] ADD  DEFAULT ((0)) FOR [ChangeSerial]
GO
ALTER TABLE [dbo].[Match] ADD  CONSTRAINT [DF_Match_CreatedOn]  DEFAULT (getdate()) FOR [CreatedOn]
GO
ALTER TABLE [dbo].[Match] ADD  CONSTRAINT [DF_Match_ModifiedOn]  DEFAULT (getdate()) FOR [ModifiedOn]
GO
ALTER TABLE [dbo].[MatchRule] ADD  DEFAULT ('') FOR [Name]
GO
ALTER TABLE [dbo].[MatchRule] ADD  DEFAULT ((1)) FOR [NumOfSets]
GO
ALTER TABLE [dbo].[MatchRule] ADD  DEFAULT ((0)) FOR [BestOf]
GO
ALTER TABLE [dbo].[MatchRule] ADD  DEFAULT ((0)) FOR [PointsMatchWon]
GO
ALTER TABLE [dbo].[MatchRule] ADD  DEFAULT ((0)) FOR [PointsMatchLost]
GO
ALTER TABLE [dbo].[MatchRule] ADD  DEFAULT ((0)) FOR [PointsMatchWonAfterTieBreak]
GO
ALTER TABLE [dbo].[MatchRule] ADD  DEFAULT ((0)) FOR [PointsMatchLostAfterTieBreak]
GO
ALTER TABLE [dbo].[MatchRule] ADD  DEFAULT ((0)) FOR [PointsMatchTie]
GO
ALTER TABLE [dbo].[MatchRule] ADD  DEFAULT ((1)) FOR [RankComparer]
GO
ALTER TABLE [dbo].[MatchRule] ADD  CONSTRAINT [DF_MatchResultRule_CreatedOn]  DEFAULT (getdate()) FOR [CreatedOn]
GO
ALTER TABLE [dbo].[MatchRule] ADD  CONSTRAINT [DF_MatchResultRule_ModifiedOn]  DEFAULT (getdate()) FOR [ModifiedOn]
GO
ALTER TABLE [dbo].[PlayerInTeam] ADD  CONSTRAINT [DF_PlayerInTeam_CreatedOn]  DEFAULT (getdate()) FOR [CreatedOn]
GO
ALTER TABLE [dbo].[PlayerInTeam] ADD  CONSTRAINT [DF_PlayerInTeam_ModifiedOn]  DEFAULT (getdate()) FOR [ModifiedOn]
GO
ALTER TABLE [dbo].[Ranking] ADD  DEFAULT ((0)) FOR [MatchesPlayed]
GO
ALTER TABLE [dbo].[Ranking] ADD  DEFAULT ((0)) FOR [MatchesToPlay]
GO
ALTER TABLE [dbo].[Ranking] ADD  CONSTRAINT [DF_Ranking_CreatedOn]  DEFAULT (getdate()) FOR [CreatedOn]
GO
ALTER TABLE [dbo].[Ranking] ADD  CONSTRAINT [DF_Ranking_ModifiedOn]  DEFAULT (getdate()) FOR [ModifiedOn]
GO
ALTER TABLE [dbo].[Registration] ADD  DEFAULT ('') FOR [Guid]
GO
ALTER TABLE [dbo].[Registration] ADD  DEFAULT ('') FOR [AppliedWithMessage]
GO
ALTER TABLE [dbo].[Registration] ADD  DEFAULT (getdate()) FOR [AppliedOn]
GO
ALTER TABLE [dbo].[Round] ADD  DEFAULT ('') FOR [Name]
GO
ALTER TABLE [dbo].[Round] ADD  DEFAULT ('') FOR [Description]
GO
ALTER TABLE [dbo].[Round] ADD  DEFAULT ((1)) FOR [NumOfLegs]
GO
ALTER TABLE [dbo].[Round] ADD  DEFAULT ((0)) FOR [IsComplete]
GO
ALTER TABLE [dbo].[Round] ADD  CONSTRAINT [DF_Round_CreatedOn]  DEFAULT (getdate()) FOR [CreatedOn]
GO
ALTER TABLE [dbo].[Round] ADD  CONSTRAINT [DF_Round_ModifiedOn]  DEFAULT (getdate()) FOR [ModifiedOn]
GO
ALTER TABLE [dbo].[RoundLeg] ADD  DEFAULT ((1)) FOR [SequenceNo]
GO
ALTER TABLE [dbo].[RoundLeg] ADD  DEFAULT (getdate()) FOR [StartDateTime]
GO
ALTER TABLE [dbo].[RoundLeg] ADD  DEFAULT (getdate()) FOR [EndDateTime]
GO
ALTER TABLE [dbo].[RoundLeg] ADD  CONSTRAINT [DF_RoundLeg_CreatedOn]  DEFAULT (getdate()) FOR [CreatedOn]
GO
ALTER TABLE [dbo].[RoundLeg] ADD  CONSTRAINT [DF_RoundLeg_ModifiedOn]  DEFAULT (getdate()) FOR [ModifiedOn]
GO
ALTER TABLE [dbo].[RoundType] ADD  DEFAULT ('') FOR [Name]
GO
ALTER TABLE [dbo].[RoundType] ADD  DEFAULT ('') FOR [Description]
GO
ALTER TABLE [dbo].[RoundType] ADD  CONSTRAINT [DF_RoundType_CreatedOn]  DEFAULT (getdate()) FOR [CreatedOn]
GO
ALTER TABLE [dbo].[RoundType] ADD  CONSTRAINT [DF_RoundType_ModifiedOn]  DEFAULT (getdate()) FOR [ModifiedOn]
GO
ALTER TABLE [dbo].[Set] ADD  DEFAULT ((1)) FOR [SequenceNo]
GO
ALTER TABLE [dbo].[Set] ADD  DEFAULT ((0)) FOR [HomeBallPoints]
GO
ALTER TABLE [dbo].[Set] ADD  DEFAULT ((0)) FOR [GuestBallPoints]
GO
ALTER TABLE [dbo].[Set] ADD  DEFAULT ((0)) FOR [HomeSetPoints]
GO
ALTER TABLE [dbo].[Set] ADD  DEFAULT ((0)) FOR [GuestSetPoints]
GO
ALTER TABLE [dbo].[Set] ADD  DEFAULT ((0)) FOR [HomeTimeout]
GO
ALTER TABLE [dbo].[Set] ADD  DEFAULT ((0)) FOR [GuestTimeout]
GO
ALTER TABLE [dbo].[Set] ADD  DEFAULT ((0)) FOR [IsOverruled]
GO
ALTER TABLE [dbo].[Set] ADD  DEFAULT ((0)) FOR [IsTieBreak]
GO
ALTER TABLE [dbo].[Set] ADD  CONSTRAINT [DF_Set_CreatedOn]  DEFAULT (getdate()) FOR [CreatedOn]
GO
ALTER TABLE [dbo].[Set] ADD  CONSTRAINT [DF_Set_ModifiedOn]  DEFAULT (getdate()) FOR [ModifiedOn]
GO
ALTER TABLE [dbo].[SetRule] ADD  DEFAULT ('') FOR [Name]
GO
ALTER TABLE [dbo].[SetRule] ADD  DEFAULT ((0)) FOR [NumOfPointsToWinRegular]
GO
ALTER TABLE [dbo].[SetRule] ADD  DEFAULT ((0)) FOR [NumOfPointsToWinTiebreak]
GO
ALTER TABLE [dbo].[SetRule] ADD  DEFAULT ((0)) FOR [PointsDiffToWinRegular]
GO
ALTER TABLE [dbo].[SetRule] ADD  DEFAULT ((0)) FOR [PointsDiffToWinTiebreak]
GO
ALTER TABLE [dbo].[SetRule] ADD  DEFAULT ((0)) FOR [PointsSetLost]
GO
ALTER TABLE [dbo].[SetRule] ADD  DEFAULT ((0)) FOR [PointsSetTie]
GO
ALTER TABLE [dbo].[SetRule] ADD  DEFAULT ((0)) FOR [PointsSetWon]
GO
ALTER TABLE [dbo].[SetRule] ADD  DEFAULT ((0)) FOR [MaxTimeouts]
GO
ALTER TABLE [dbo].[SetRule] ADD  DEFAULT ((0)) FOR [MaxSubstitutions]
GO
ALTER TABLE [dbo].[SetRule] ADD  CONSTRAINT [DF_SetResultRule_CreatedOn]  DEFAULT (getdate()) FOR [CreatedOn]
GO
ALTER TABLE [dbo].[SetRule] ADD  CONSTRAINT [DF_SetResultRule_ModifiedOn]  DEFAULT (getdate()) FOR [ModifiedOn]
GO
ALTER TABLE [dbo].[Team] ADD  DEFAULT ('') FOR [Name]
GO
ALTER TABLE [dbo].[Team] ADD  DEFAULT ('') FOR [ClubName]
GO
ALTER TABLE [dbo].[Team] ADD  CONSTRAINT [DF_Team_CreatedOn]  DEFAULT (getdate()) FOR [CreatedOn]
GO
ALTER TABLE [dbo].[Team] ADD  CONSTRAINT [DF_Team_ModifiedOn]  DEFAULT (getdate()) FOR [ModifiedOn]
GO
ALTER TABLE [dbo].[TeamInRound] ADD  DEFAULT ('') FOR [TeamNameForRound]
GO
ALTER TABLE [dbo].[TeamInRound] ADD  CONSTRAINT [DF_TeamInRound_CreatedOn]  DEFAULT (getdate()) FOR [CreatedOn]
GO
ALTER TABLE [dbo].[TeamInRound] ADD  CONSTRAINT [DF_TeamInRound_ModifiedOn]  DEFAULT (getdate()) FOR [ModifiedOn]
GO
ALTER TABLE [dbo].[Tournament] ADD  DEFAULT ('') FOR [Name]
GO
ALTER TABLE [dbo].[Tournament] ADD  DEFAULT ('') FOR [Description]
GO
ALTER TABLE [dbo].[Tournament] ADD  DEFAULT ((0)) FOR [IsComplete]
GO
ALTER TABLE [dbo].[Tournament] ADD  DEFAULT ((0)) FOR [IsPlanningMode]
GO
ALTER TABLE [dbo].[Tournament] ADD  CONSTRAINT [DF_Tournament_CreatedOn]  DEFAULT (getdate()) FOR [CreatedOn]
GO
ALTER TABLE [dbo].[Tournament] ADD  CONSTRAINT [DF_Tournament_ModifiedOn]  DEFAULT (getdate()) FOR [ModifiedOn]
GO
ALTER TABLE [dbo].[TournamentType] ADD  DEFAULT ('') FOR [Name]
GO
ALTER TABLE [dbo].[TournamentType] ADD  DEFAULT ('') FOR [Description]
GO
ALTER TABLE [dbo].[TournamentType] ADD  CONSTRAINT [DF_TournamentType_CreatedOn]  DEFAULT (getdate()) FOR [CreatedOn]
GO
ALTER TABLE [dbo].[TournamentType] ADD  CONSTRAINT [DF_TournamentType_ModifiedOn]  DEFAULT (getdate()) FOR [ModifiedOn]
GO
ALTER TABLE [dbo].[User] ADD  DEFAULT ('') FOR [Guid]
GO
ALTER TABLE [dbo].[User] ADD  CONSTRAINT [DF__User__UserName__28B808A7]  DEFAULT ('') FOR [UserName]
GO
ALTER TABLE [dbo].[User] ADD  CONSTRAINT [DF__User__PasswordHa__29AC2CE0]  DEFAULT ('') FOR [PasswordHash]
GO
ALTER TABLE [dbo].[User] ADD  CONSTRAINT [DF__User__Email__2AA05119]  DEFAULT ('') FOR [Email]
GO
ALTER TABLE [dbo].[User] ADD  DEFAULT ('') FOR [PhoneNumber]
GO
ALTER TABLE [dbo].[User] ADD  DEFAULT ((0)) FOR [AccessFailedCount]
GO
ALTER TABLE [dbo].[User] ADD  DEFAULT ('u') FOR [Gender]
GO
ALTER TABLE [dbo].[User] ADD  DEFAULT ('') FOR [Title]
GO
ALTER TABLE [dbo].[User] ADD  DEFAULT ('') FOR [FirstName]
GO
ALTER TABLE [dbo].[User] ADD  DEFAULT ('') FOR [MiddleName]
GO
ALTER TABLE [dbo].[User] ADD  DEFAULT ('') FOR [LastName]
GO
ALTER TABLE [dbo].[User] ADD  DEFAULT ('') FOR [Nickname]
GO
ALTER TABLE [dbo].[User] ADD  DEFAULT ('') FOR [PhoneNumber2]
GO
ALTER TABLE [dbo].[User] ADD  DEFAULT ('') FOR [Email2]
GO
ALTER TABLE [dbo].[User] ADD  DEFAULT ('') FOR [Remarks]
GO
ALTER TABLE [dbo].[User] ADD  CONSTRAINT [DF_User_CreatedOn]  DEFAULT (getdate()) FOR [CreatedOn]
GO
ALTER TABLE [dbo].[User] ADD  CONSTRAINT [DF_User_ModifiedOn]  DEFAULT (getdate()) FOR [ModifiedOn]
GO
ALTER TABLE [dbo].[Venue] ADD  DEFAULT ('') FOR [Name]
GO
ALTER TABLE [dbo].[Venue] ADD  DEFAULT ('') FOR [Extension]
GO
ALTER TABLE [dbo].[Venue] ADD  DEFAULT ('') FOR [Street]
GO
ALTER TABLE [dbo].[Venue] ADD  DEFAULT ('') FOR [PostalCode]
GO
ALTER TABLE [dbo].[Venue] ADD  DEFAULT ('') FOR [City]
GO
ALTER TABLE [dbo].[Venue] ADD  DEFAULT ((0)) FOR [PrecisePosition]
GO
ALTER TABLE [dbo].[Venue] ADD  DEFAULT ('') FOR [Direction]
GO
ALTER TABLE [dbo].[Venue] ADD  CONSTRAINT [DF_Venue_CreatedOn]  DEFAULT (getdate()) FOR [CreatedOn]
GO
ALTER TABLE [dbo].[Venue] ADD  CONSTRAINT [DF_Venue_ModifiedOn]  DEFAULT (getdate()) FOR [ModifiedOn]
GO
ALTER TABLE [dbo].[AvailableMatchDate]  WITH NOCHECK ADD  CONSTRAINT [FK_4d472ac4c12adb65ea0b4d3ff01] FOREIGN KEY([HomeTeamId])
REFERENCES [dbo].[Team] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AvailableMatchDate] CHECK CONSTRAINT [FK_4d472ac4c12adb65ea0b4d3ff01]
GO
ALTER TABLE [dbo].[AvailableMatchDate]  WITH NOCHECK ADD  CONSTRAINT [FK_74cccb8440e8cf75d4dd871747b] FOREIGN KEY([VenueId])
REFERENCES [dbo].[Venue] ([Id])
GO
ALTER TABLE [dbo].[AvailableMatchDate] CHECK CONSTRAINT [FK_74cccb8440e8cf75d4dd871747b]
GO
ALTER TABLE [dbo].[ExcludeMatchDate]  WITH NOCHECK ADD  CONSTRAINT [ExcludeMatchDate_TournamentId_fk] FOREIGN KEY([TournamentId])
REFERENCES [dbo].[Tournament] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[ExcludeMatchDate] CHECK CONSTRAINT [ExcludeMatchDate_TournamentId_fk]
GO
ALTER TABLE [dbo].[ExcludeMatchDate]  WITH NOCHECK ADD  CONSTRAINT [FK_839b5ab411ba7bce29b0d4782c7] FOREIGN KEY([RoundId])
REFERENCES [dbo].[Round] ([Id])
ON UPDATE CASCADE
ON DELETE SET NULL
GO
ALTER TABLE [dbo].[ExcludeMatchDate] CHECK CONSTRAINT [FK_839b5ab411ba7bce29b0d4782c7]
GO
ALTER TABLE [dbo].[ExcludeMatchDate]  WITH NOCHECK ADD  CONSTRAINT [FK_e059c2f4a6fbdd3147184f47ed1] FOREIGN KEY([TeamId])
REFERENCES [dbo].[Team] ([Id])
ON UPDATE CASCADE
ON DELETE SET NULL
GO
ALTER TABLE [dbo].[ExcludeMatchDate] CHECK CONSTRAINT [FK_e059c2f4a6fbdd3147184f47ed1]
GO
ALTER TABLE [dbo].[IdentityRoleClaim]  WITH NOCHECK ADD  CONSTRAINT [IdentityRoleClaim_Role_fk] FOREIGN KEY([RoleId])
REFERENCES [dbo].[IdentityRole] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[IdentityRoleClaim] CHECK CONSTRAINT [IdentityRoleClaim_Role_fk]
GO
ALTER TABLE [dbo].[IdentityUserClaim]  WITH NOCHECK ADD  CONSTRAINT [IdentityUserClaim_User_fk] FOREIGN KEY([UserId])
REFERENCES [dbo].[User] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[IdentityUserClaim] CHECK CONSTRAINT [IdentityUserClaim_User_fk]
GO
ALTER TABLE [dbo].[IdentityUserLogin]  WITH NOCHECK ADD  CONSTRAINT [Fk_UserLogin_User] FOREIGN KEY([UserId])
REFERENCES [dbo].[User] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[IdentityUserLogin] CHECK CONSTRAINT [Fk_UserLogin_User]
GO
ALTER TABLE [dbo].[IdentityUserRole]  WITH NOCHECK ADD  CONSTRAINT [Fk_UserRole_Role] FOREIGN KEY([RoleId])
REFERENCES [dbo].[IdentityRole] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[IdentityUserRole] CHECK CONSTRAINT [Fk_UserRole_Role]
GO
ALTER TABLE [dbo].[IdentityUserRole]  WITH NOCHECK ADD  CONSTRAINT [Fk_UserRole_User] FOREIGN KEY([UserId])
REFERENCES [dbo].[User] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[IdentityUserRole] CHECK CONSTRAINT [Fk_UserRole_User]
GO
ALTER TABLE [dbo].[IdentityUserToken]  WITH NOCHECK ADD  CONSTRAINT [UserTokens_fk] FOREIGN KEY([UserId])
REFERENCES [dbo].[User] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[IdentityUserToken] CHECK CONSTRAINT [UserTokens_fk]
GO
ALTER TABLE [dbo].[ManagerOfTeam]  WITH NOCHECK ADD  CONSTRAINT [FK_0e7569e46d2be7d0a9142edbb25] FOREIGN KEY([TeamId])
REFERENCES [dbo].[Team] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[ManagerOfTeam] CHECK CONSTRAINT [FK_0e7569e46d2be7d0a9142edbb25]
GO
ALTER TABLE [dbo].[ManagerOfTeam]  WITH NOCHECK ADD  CONSTRAINT [FK_d308f7c4770bee43783a0e53d8f] FOREIGN KEY([UserId])
REFERENCES [dbo].[User] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[ManagerOfTeam] CHECK CONSTRAINT [FK_d308f7c4770bee43783a0e53d8f]
GO
ALTER TABLE [dbo].[Match]  WITH NOCHECK ADD  CONSTRAINT [FK_5c03c0c403195643ad89303cc4b] FOREIGN KEY([RoundId])
REFERENCES [dbo].[Round] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Match] CHECK CONSTRAINT [FK_5c03c0c403195643ad89303cc4b]
GO
ALTER TABLE [dbo].[Match]  WITH NOCHECK ADD  CONSTRAINT [FK_cb028e742dbbcefcdc18bc33d8c] FOREIGN KEY([VenueId])
REFERENCES [dbo].[Venue] ([Id])
GO
ALTER TABLE [dbo].[Match] CHECK CONSTRAINT [FK_cb028e742dbbcefcdc18bc33d8c]
GO
ALTER TABLE [dbo].[Match]  WITH NOCHECK ADD  CONSTRAINT [FK_ce641dd44a79b6a0536029e5095] FOREIGN KEY([GuestTeamId])
REFERENCES [dbo].[Team] ([Id])
GO
ALTER TABLE [dbo].[Match] CHECK CONSTRAINT [FK_ce641dd44a79b6a0536029e5095]
GO
ALTER TABLE [dbo].[Match]  WITH NOCHECK ADD  CONSTRAINT [FK_ef1387a4716a86685ca94abfe03] FOREIGN KEY([HomeTeamId])
REFERENCES [dbo].[Team] ([Id])
GO
ALTER TABLE [dbo].[Match] CHECK CONSTRAINT [FK_ef1387a4716a86685ca94abfe03]
GO
ALTER TABLE [dbo].[Match]  WITH NOCHECK ADD  CONSTRAINT [FK_f7bed0a4ecbbddad75d0016be12] FOREIGN KEY([OrigVenueId])
REFERENCES [dbo].[Venue] ([Id])
GO
ALTER TABLE [dbo].[Match] CHECK CONSTRAINT [FK_f7bed0a4ecbbddad75d0016be12]
GO
ALTER TABLE [dbo].[Match]  WITH NOCHECK ADD  CONSTRAINT [FK_fc8dfc04f2d86db9b7c1f85ab55] FOREIGN KEY([RefereeId])
REFERENCES [dbo].[Team] ([Id])
GO
ALTER TABLE [dbo].[Match] CHECK CONSTRAINT [FK_fc8dfc04f2d86db9b7c1f85ab55]
GO
ALTER TABLE [dbo].[PlayerInTeam]  WITH NOCHECK ADD  CONSTRAINT [FK_1f583a545568a2d84dc9b3c3e85] FOREIGN KEY([UserId])
REFERENCES [dbo].[User] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[PlayerInTeam] CHECK CONSTRAINT [FK_1f583a545568a2d84dc9b3c3e85]
GO
ALTER TABLE [dbo].[PlayerInTeam]  WITH NOCHECK ADD  CONSTRAINT [FK_5de6c0349c79218e00c605092ae] FOREIGN KEY([TeamId])
REFERENCES [dbo].[Team] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[PlayerInTeam] CHECK CONSTRAINT [FK_5de6c0349c79218e00c605092ae]
GO
ALTER TABLE [dbo].[Ranking]  WITH NOCHECK ADD  CONSTRAINT [FK_2074dbd428a8d48582fdeaa4b8c] FOREIGN KEY([TeamId])
REFERENCES [dbo].[Team] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Ranking] CHECK CONSTRAINT [FK_2074dbd428a8d48582fdeaa4b8c]
GO
ALTER TABLE [dbo].[Ranking]  WITH NOCHECK ADD  CONSTRAINT [FK_ba19e0b4cd4a24b99b094d481f7] FOREIGN KEY([RoundId])
REFERENCES [dbo].[Round] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Ranking] CHECK CONSTRAINT [FK_ba19e0b4cd4a24b99b094d481f7]
GO
ALTER TABLE [dbo].[Ranking]  WITH NOCHECK ADD  CONSTRAINT [FK_c8e677848abac5f1b8bf13ea775] FOREIGN KEY([TournamentId])
REFERENCES [dbo].[Tournament] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Ranking] CHECK CONSTRAINT [FK_c8e677848abac5f1b8bf13ea775]
GO
ALTER TABLE [dbo].[Registration]  WITH NOCHECK ADD  CONSTRAINT [FK_4462bad40269915324df04e014d] FOREIGN KEY([ManagerId])
REFERENCES [dbo].[User] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Registration] CHECK CONSTRAINT [FK_4462bad40269915324df04e014d]
GO
ALTER TABLE [dbo].[Registration]  WITH NOCHECK ADD  CONSTRAINT [FK_7bab7f542cbbe91cec71043643e] FOREIGN KEY([TeamId])
REFERENCES [dbo].[Team] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Registration] CHECK CONSTRAINT [FK_7bab7f542cbbe91cec71043643e]
GO
ALTER TABLE [dbo].[Registration]  WITH NOCHECK ADD  CONSTRAINT [FK_b3d49004f0692a3e027b9b68624] FOREIGN KEY([TournamentId])
REFERENCES [dbo].[Tournament] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Registration] CHECK CONSTRAINT [FK_b3d49004f0692a3e027b9b68624]
GO
ALTER TABLE [dbo].[Round]  WITH NOCHECK ADD  CONSTRAINT [Round_MatchRule] FOREIGN KEY([MatchRuleId])
REFERENCES [dbo].[MatchRule] ([Id])
ON UPDATE CASCADE
GO
ALTER TABLE [dbo].[Round] CHECK CONSTRAINT [Round_MatchRule]
GO
ALTER TABLE [dbo].[Round]  WITH NOCHECK ADD  CONSTRAINT [Round_NextRound] FOREIGN KEY([NextRoundId])
REFERENCES [dbo].[Round] ([Id])
GO
ALTER TABLE [dbo].[Round] CHECK CONSTRAINT [Round_NextRound]
GO
ALTER TABLE [dbo].[Round]  WITH NOCHECK ADD  CONSTRAINT [Round_RoundType] FOREIGN KEY([TypeId])
REFERENCES [dbo].[RoundType] ([Id])
ON UPDATE CASCADE
GO
ALTER TABLE [dbo].[Round] CHECK CONSTRAINT [Round_RoundType]
GO
ALTER TABLE [dbo].[Round]  WITH NOCHECK ADD  CONSTRAINT [Round_SetRule] FOREIGN KEY([SetRuleId])
REFERENCES [dbo].[SetRule] ([Id])
ON UPDATE CASCADE
GO
ALTER TABLE [dbo].[Round] CHECK CONSTRAINT [Round_SetRule]
GO
ALTER TABLE [dbo].[Round]  WITH NOCHECK ADD  CONSTRAINT [Round_Tournament] FOREIGN KEY([TournamentId])
REFERENCES [dbo].[Tournament] ([Id])
GO
ALTER TABLE [dbo].[Round] CHECK CONSTRAINT [Round_Tournament]
GO
ALTER TABLE [dbo].[RoundLeg]  WITH NOCHECK ADD  CONSTRAINT [FK_b06fad141949b946948a4b7b238] FOREIGN KEY([RoundId])
REFERENCES [dbo].[Round] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[RoundLeg] CHECK CONSTRAINT [FK_b06fad141949b946948a4b7b238]
GO
ALTER TABLE [dbo].[Set]  WITH NOCHECK ADD  CONSTRAINT [FK_a5d0fa848aab17137435c5e2c47] FOREIGN KEY([MatchId])
REFERENCES [dbo].[Match] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Set] CHECK CONSTRAINT [FK_a5d0fa848aab17137435c5e2c47]
GO
ALTER TABLE [dbo].[Team]  WITH NOCHECK ADD  CONSTRAINT [FK_e13a4a24741b2e5a717f9c696a8] FOREIGN KEY([VenueId])
REFERENCES [dbo].[Venue] ([Id])
GO
ALTER TABLE [dbo].[Team] CHECK CONSTRAINT [FK_e13a4a24741b2e5a717f9c696a8]
GO
ALTER TABLE [dbo].[TeamInRound]  WITH NOCHECK ADD  CONSTRAINT [FK_144927a424e8f34c15e8854f3c9] FOREIGN KEY([RoundId])
REFERENCES [dbo].[Round] ([Id])
ON UPDATE CASCADE
GO
ALTER TABLE [dbo].[TeamInRound] CHECK CONSTRAINT [FK_144927a424e8f34c15e8854f3c9]
GO
ALTER TABLE [dbo].[TeamInRound]  WITH NOCHECK ADD  CONSTRAINT [FK_b5d92d5411191fa54e97410ad0f] FOREIGN KEY([TeamId])
REFERENCES [dbo].[Team] ([Id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[TeamInRound] CHECK CONSTRAINT [FK_b5d92d5411191fa54e97410ad0f]
GO
ALTER TABLE [dbo].[Tournament]  WITH NOCHECK ADD  CONSTRAINT [FK_7fdd8614f9d8df5c5df69e60586] FOREIGN KEY([NextTournamentId])
REFERENCES [dbo].[Tournament] ([Id])
GO
ALTER TABLE [dbo].[Tournament] CHECK CONSTRAINT [FK_7fdd8614f9d8df5c5df69e60586]
GO
ALTER TABLE [dbo].[Tournament]  WITH NOCHECK ADD  CONSTRAINT [FK_b1c85dc49a0ad94cf65182015cb] FOREIGN KEY([TypeId])
REFERENCES [dbo].[TournamentType] ([Id])
ON UPDATE CASCADE
ON DELETE SET NULL
GO
ALTER TABLE [dbo].[Tournament] CHECK CONSTRAINT [FK_b1c85dc49a0ad94cf65182015cb]
GO
ALTER TABLE [dbo].[User]  WITH NOCHECK ADD  CONSTRAINT [User_ck_Gender] CHECK  (([Gender]='m' OR [Gender]='f' OR [Gender]='u'))
GO
ALTER TABLE [dbo].[User] CHECK CONSTRAINT [User_ck_Gender]
GO
/****** Object:  StoredProcedure [dbo].[VenueDistance]    Script Date: 06.08.2023 20:51:06 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[VenueDistance]
	@MaxDistance float=22000,
    @Lat float = NULL,
    @Lng float = NULL
AS
BEGIN
  /* Selects all records if all latitude/longitude values are NOT NULL */
  SELECT *, ISNULL(dbo.[SpatialDistance](@Lat, @Lng, [Latitude], [Longitude]),0) Distance 
  FROM [dbo].[Venue]
  WHERE dbo.[SpatialDistance](@Lat, @Lng, [Latitude], [Longitude]) <= @MaxDistance
END
GO
/****** Object:  Trigger [dbo].[Team_InsteadOf_Delete_tr]    Script Date: 06.08.2023 20:51:06 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TRIGGER [dbo].[Team_InsteadOf_Delete_tr] ON [dbo].[Team]
WITH EXECUTE AS OWNER
INSTEAD OF DELETE
AS
BEGIN
    -- use inserted for insert or update trigger, deleted for update or delete trigger
    DECLARE @rowsAffected int = (SELECT COUNT(*) FROM DELETED);

    --no need to continue on if no rows affected
    IF @rowsAffected = 0 RETURN;

    SET NOCOUNT ON; --to avoid the rowcount messages
    SET ROWCOUNT 0; --in case the client has modified the rowcount

    BEGIN TRY

        -- implement multi-path cascade delete in trigger
        DELETE FROM [Match]
        WHERE [HomeTeamId] IN (SELECT [Id] FROM DELETED);
        
        DELETE FROM [Match]
        WHERE [GuestTeamId] IN (SELECT [Id] FROM DELETED);
        
        DELETE FROM [Match]
        WHERE [RefereeId] IN (SELECT [Id] FROM DELETED);

        -- perform action
        DELETE FROM [Team]
        WHERE [Id] IN (SELECT [Id] FROM DELETED);

    END TRY

    BEGIN CATCH
        IF @@trancount > 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH;

END
GO
ALTER TABLE [dbo].[Team] ENABLE TRIGGER [Team_InsteadOf_Delete_tr]
GO
/****** Object:  Trigger [dbo].[Venue_InsteadOf_Delete_tr]    Script Date: 06.08.2023 20:51:07 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TRIGGER [dbo].[Venue_InsteadOf_Delete_tr] ON [dbo].[Venue]
WITH EXECUTE AS OWNER
INSTEAD OF DELETE
AS
BEGIN
    -- use inserted for insert or update trigger, deleted for update or delete trigger
    DECLARE @rowsAffected int = (SELECT COUNT(*) FROM DELETED);

    --no need to continue on if no rows affected
    IF @rowsAffected = 0 RETURN;

    SET NOCOUNT ON; --to avoid the rowcount messages
    SET ROWCOUNT 0; --in case the client has modified the rowcount

    BEGIN TRY

        -- implement multi-path cascade delete in trigger
        UPDATE match
        SET [VenueId] = NULL
        FROM [Match] AS match
        INNER JOIN [DELETED] AS d 
        ON match.VenueId = d.Id;
        
    	UPDATE match
        SET [OrigVenueId] = NULL
        FROM [Match] AS match
        INNER JOIN [DELETED] AS d 
        ON match.OrigVenueId = d.Id;
        
    	UPDATE team
        SET [VenueId] = NULL
        FROM [Team] AS team
        INNER JOIN [DELETED] AS d 
        ON team.VenueId = d.Id;
        
        DELETE FROM [AvailableMatchDate]
        WHERE [VenueId] IN (SELECT [Id] FROM DELETED);

        -- perform action
        DELETE FROM [Venue]
        WHERE [Id] IN (SELECT [Id] FROM DELETED);

    END TRY
    
    BEGIN CATCH
        IF @@trancount > 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH;

END
GO
ALTER TABLE [dbo].[Venue] ENABLE TRIGGER [Venue_InsteadOf_Delete_tr]
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Invitations for new users, players or managers' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'AvailableMatchDate'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Match dates which cannot be used' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ExcludeMatchDate'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Identity Role' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'IdentityRole'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Identity RoleClaim' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'IdentityRoleClaim'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Identity UserClaim' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'IdentityUserClaim'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Store for login providers of user' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'IdentityUserLogin'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'User Roles' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'IdentityUserRole'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Store for login tokens of users' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'IdentityUserToken'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Teams and their Managers' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ManagerOfTeam'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Matches of the tournaments' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Match'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Match result rules' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'MatchRule'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Players in Teams' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PlayerInTeam'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Ranking history for all tournaments' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Ranking'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Registration of teams for a tournament' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Registration'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Rounds of a tournament' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Round'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Legs of a round' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'RoundLeg'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'The type of a round, e.g. number of female/male players' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'RoundType'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Sets of the matches' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Set'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Set result rules' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SetRule'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Time for home matches in local (!) time' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Team', @level2type=N'COLUMN',@level2name=N'MatchTime'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Team basic data' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Team'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Which teams play in which round' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TeamInRound'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Tournament basic data' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Tournament'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Tournament types' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TournamentType'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'User incl. Managers (similar to IdentityUser of Microsoft.AspNet.Identity)' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'User'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Venue teams are using for matches' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Venue'
GO
