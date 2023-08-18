/* Data for the 'dbo.IdentityRole' table  (Records 1 - 2) */

SET IDENTITY_INSERT dbo.IdentityRole ON; 
GO
INSERT INTO dbo.IdentityRole (Id, Name)
VALUES 
  (1, N'SystemManager'),
  (2, N'TournamentManager')
GO
SET IDENTITY_INSERT dbo.IdentityRole OFF; 
GO

/* Data for the 'dbo.MatchRule' table  (Records 1 - 1) */

SET IDENTITY_INSERT dbo.MatchRule ON; 
GO
INSERT INTO dbo.MatchRule (Id, Name, NumOfSets, BestOf, PointsMatchWon, PointsMatchLost, PointsMatchWonAfterTieBreak, PointsMatchLostAfterTieBreak, PointsMatchTie, RankComparer, CreatedOn, ModifiedOn)
VALUES 
  (1, N'3 sets to win, tie-break rule', 3, 1, 3, 0, 2, 1, 0, 1, GETDATE(), GETDATE())
SET IDENTITY_INSERT dbo.MatchRule OFF; 
GO

/* Data for the 'dbo.SetRule' table  (Records 1 - 1) */

SET IDENTITY_INSERT dbo.SetRule ON; 
GO
INSERT INTO dbo.SetRule (Id, Name, NumOfPointsToWinRegular, NumOfPointsToWinTiebreak, PointsDiffToWinRegular, PointsDiffToWinTiebreak, PointsSetLost, PointsSetTie, PointsSetWon, MaxTimeouts, MaxSubstitutions, CreatedOn, ModifiedOn)
VALUES 
  (1, N'Indoor set rule', 25, 15, 2, 2, 0, 0, 1, 2, 6, GETDATE(), GETDATE()),
  (1, N'Beach set rule', 21, 15, 2, 2, 0, 0, 1, 2, 0, GETDATE(), GETDATE())
GO
SET IDENTITY_INSERT dbo.SetRule OFF;
GO

/* Data for the 'dbo.RoundType' table  (Records 1 - 1) */

SET IDENTITY_INSERT dbo.RoundType ON; 
GO
INSERT INTO dbo.RoundType (Id, Name, Description, CreatedOn, ModifiedOn)
VALUES 
  (1, N'Female', N'Female teams', GETDATE(), GETDATE()),
  (2, N'Male', N'Male teams', GETDATE(), GETDATE()),
  (3, N'Mixed', N'Mixed teams', GETDATE(), GETDATE())
GO
SET IDENTITY_INSERT dbo.RoundType OFF; 
GO


/* Data for the 'dbo.User' table  (Records 1 - 1) */

SET IDENTITY_INSERT dbo.[User] ON; 
GO
INSERT INTO dbo.[User] (Id, Guid, UserName, PasswordHash, Email, EmailConfirmedOn, PhoneNumber, PhoneNumberConfirmedOn, LastLoginOn, AccessFailedCount, LockoutEndDateUtc, Gender, Title, FirstName, MiddleName, LastName, Nickname, PhoneNumber2, Email2, Birthday, Remarks, CreatedOn, ModifiedOn)
VALUES 
  (1, N'49afc1e5-d439-40d7-bb13-ce994a825069', N'sysadmin', NULL, N'admin@axuno.net', '20230817', N'', NULL, NULL, 0, NULL, N'm', N'', N'admin', N'', N'admin', N'', N'', N'', NULL, NULL, '20230817', '20230817')
GO
SET IDENTITY_INSERT dbo.[User] OFF; 
GO

/* Data for the 'dbo.IdentityUserRole' table  (Records 1 - 1) */

SET IDENTITY_INSERT dbo.IdentityUserRole ON; 
GO
INSERT INTO dbo.IdentityUserRole (Id, UserId, RoleId)
VALUES 
  (1, 1, 1)
GO
SET IDENTITY_INSERT dbo.IdentityUserRole OFF; 
GO
