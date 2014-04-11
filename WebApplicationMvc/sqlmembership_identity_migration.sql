IF OBJECT_ID('AspNetUserRoles', 'U') IS NOT NULL
BEGIN
DROP TABLE AspNetUserRoles;
END

IF OBJECT_ID('AspNetUserClaims', 'U') IS NOT NULL
BEGIN
DROP TABLE AspNetUserClaims;
END

IF OBJECT_ID('AspNetUserLogins', 'U') IS NOT NULL
BEGIN
DROP TABLE AspNetUserLogins;
END

IF OBJECT_ID('AspNetRoles', 'U') IS NOT NULL
BEGIN
DROP TABLE AspNetRoles;
END

IF OBJECT_ID('AspNetUsers', 'U') IS NOT NULL
BEGIN
DROP TABLE AspNetUsers;
END

CREATE TABLE [dbo].[AspNetUsers] (
    [Id]           							 UNIQUEIDENTIFIER NOT NULL,
    [UserName]      						 NVARCHAR (256)   NULL,
    [PasswordHash]  						 NVARCHAR (MAX)   NULL,
    [SecurityStamp] 						 NVARCHAR (MAX)   NULL,
    [Email]                                  NVARCHAR (256)   NULL,
	[EmailConfirmed]						 BIT			  NOT NULL,
	[PhoneNumber]							 NVARCHAR (256)	  NULL,
	[PhoneNumberConfirmed]					 BIT			  NOT NULL,
	[IsApproved]							 BIT			  NOT NULL,
	[TwoFactorEnabled]						 BIT			  NOT NULL,
    [LockoutEnabled]                         BIT              NOT NULL,
    [LockoutEndDateUtc]                      DATETIME2        NULL,
    [CreateDate]                             DATETIME2	      NOT NULL,
    [AccessFailedCount]			             INT              NOT NULL,
    [Comment]                                NVARCHAR (MAX)   NULL,
	[DisplayName]							 NVARCHAR (100)   NULL,
	[CountryId]								 INT			  NULL,
	[StateId]								 INT			  NULL,
	[InIndustry]							 BIT			  NOT NULL,
	[Credits]								 INT			  NOT NULL,
	[DefaultProfileId]						 INT			  NULL,
	[IsBad]									 BIT			  NOT NULL,
	[NotifyDirectMessages]					 BIT			  NOT NULL,
	[NotifyAskAndAnswer]					 BIT			  NOT NULL,
    CONSTRAINT [PK_dbo.AspNetUsers] PRIMARY KEY CLUSTERED ([Id] ASC)
);

INSERT INTO AspNetUsers
(
	Id,
	UserName,
	PasswordHash,
	SecurityStamp,
	IsApproved,
	Email,
	EmailConfirmed,
	PhoneNumber,
	PhoneNumberConfirmed,
	TwoFactorEnabled,
	LockoutEnabled,
	LockoutEndDateUtc,
	AccessFailedCount,
	CreateDate,
	Comment,
	DisplayName,
	CountryId,
	StateId,
	InIndustry,
	Credits,
	DefaultProfileId,
	IsBad,
	NotifyDirectMessages,
	NotifyAskAndAnswer
)
SELECT
	u.UserId,
	u.UserName,
	(m.Password+'|'+CAST(m.PasswordFormat as varchar)+'|'+m.PasswordSalt),
	NewID(),
	m.IsApproved,
	m.LoweredEmail,
	1,
	NULL,
	0,
	0,
	m.IsLockedOut,
	m.LastLockoutDate,
	m.FailedPasswordAttemptCount,
	m.CreateDate,
	m.Comment,
	p.DisplayName,
	p.CountryID,
	p.StateID,
	ISNULL(p.InIndustry, 0),
	ISNULL(p.Credits, 0),
	p.DefaultProfileId,
	ISNULL(p.IsBad, 0),
	ISNULL(c.PrivateMessages, 1),
	ISNULL(c.AskAndAnswer, 1)
FROM
	aspnet_Users u
LEFT OUTER JOIN
	aspnet_Membership m ON u.UserId = m.UserId
LEFT OUTER JOIN
	aspnet_CustomProfile p ON u.UserId = p.UserID
LEFT OUTER JOIN
	Contact_Preferences c ON u.UserId = c.UserID

CREATE TABLE [dbo].[AspNetRoles] (
    [Id]   UNIQUEIDENTIFIER NOT NULL,
    [Name] NVARCHAR (MAX) NOT NULL,
    PRIMARY KEY NONCLUSTERED ([Id] ASC),
);

INSERT INTO AspNetRoles(Id,Name)
SELECT RoleId,RoleName
FROM aspnet_Roles;

CREATE TABLE [dbo].[AspNetUserRoles] (
    [UserId] UNIQUEIDENTIFIER NOT NULL,
    [RoleId] UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT [PK_dbo.AspNetUserRoles] PRIMARY KEY CLUSTERED ([UserId] ASC, [RoleId] ASC),
    CONSTRAINT [FK_dbo.AspNetUserRoles_dbo.AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [dbo].[AspNetRoles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_dbo.AspNetUserRoles_dbo.AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE
);

INSERT INTO AspNetUserRoles(UserId,RoleId)
SELECT UserId,RoleId
FROM aspnet_UsersInRoles;

CREATE TABLE [dbo].[AspNetUserClaims] (
    [Id]         INT            IDENTITY (1, 1) NOT NULL,
    [ClaimType]  NVARCHAR (MAX) NULL,
    [ClaimValue] NVARCHAR (MAX) NULL,
    [UserId]    UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT [PK_dbo.AspNetUserClaims] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_dbo.AspNetUserClaims_dbo.AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE
);

GO
CREATE NONCLUSTERED INDEX [IX_UserId]
    ON [dbo].[AspNetUserClaims]([UserId] ASC);

CREATE TABLE [dbo].[AspNetUserLogins] (
    [UserId]        UNIQUEIDENTIFIER NOT NULL,
    [LoginProvider] NVARCHAR (128) NOT NULL,
    [ProviderKey]   NVARCHAR (128) NOT NULL,
    CONSTRAINT [PK_dbo.AspNetUserLogins] PRIMARY KEY CLUSTERED ([UserId] ASC, [LoginProvider] ASC, [ProviderKey] ASC),
    CONSTRAINT [FK_dbo.AspNetUserLogins_dbo.AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE
);

GO
CREATE NONCLUSTERED INDEX [IX_UserId]
    ON [dbo].[AspNetUserLogins]([UserId] ASC);
