
/* EF Stuff */

CREATE TABLE [dbo].[__MigrationHistory] (
    [MigrationId]    NVARCHAR (150)  NOT NULL,
    [ContextKey]     NVARCHAR (300)  NOT NULL,
    [Model]          VARBINARY (MAX) NOT NULL,
    [ProductVersion] NVARCHAR (32)   NOT NULL,
    CONSTRAINT [PK_dbo.__MigrationHistory] PRIMARY KEY CLUSTERED ([MigrationId] ASC, [ContextKey] ASC)
);

/* Core Models */

CREATE TABLE [dbo].[AppUsers] (
    [Id]                NVARCHAR (128) NOT NULL,
    [Email]             NVARCHAR (MAX) NULL,
    [PasswordHash]      NVARCHAR (MAX) NULL,
    [ResetToken]        NVARCHAR (MAX) NULL,
    [ResetTokenExpires] DATETIME2 (7)  NOT NULL,
    [ModifiedOn]        DATETIME2 (7)  NOT NULL,
    [CreatedOn]         DATETIME2 (7)  NOT NULL,
    CONSTRAINT [PK_dbo.AppUsers] PRIMARY KEY CLUSTERED ([Id] ASC)
);

CREATE TABLE [dbo].[SocialProfiles] (
    [Id]          NVARCHAR (128) NOT NULL,
    [UserId]      NVARCHAR (128) NOT NULL,
    [Provider]    NVARCHAR (MAX) NULL,
    [AccessToken] NVARCHAR (MAX) NULL,
    [Email]       NVARCHAR (MAX) NULL,
    [FirstName]   NVARCHAR (MAX) NULL,
    [LastName]    NVARCHAR (MAX) NULL,
    [Picture]     NVARCHAR (MAX) NULL,
    [Link]        NVARCHAR (MAX) NULL,
    [Gender]      NVARCHAR (MAX) NULL,
    [Local]       NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_dbo.SocialProfiles] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_dbo.SocialProfiles_dbo.AppUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AppUsers] ([Id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_UserId]
    ON [dbo].[SocialProfiles]([UserId] ASC);

CREATE TABLE [dbo].[AppObjects] (
    [ObjectId]    NVARCHAR (128) NOT NULL,
    [AppId]       NVARCHAR (MAX) NOT NULL,
    [AclType]     INT            NOT NULL,
    [AclParam]    NVARCHAR (MAX) NULL,
    [ModifiedOn]  DATETIME2 (7)  NOT NULL,
    [CreatedOn]   DATETIME2 (7)  NOT NULL,
    [ObjectType]  NVARCHAR (MAX) NOT NULL,
    [ObjectScore] REAL           NOT NULL,
    [ObjectData]  NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_dbo.AppObjects] PRIMARY KEY CLUSTERED ([ObjectId] ASC)
);

/* Custom Models */

CREATE TABLE [dbo].[GameScore] (
    [UserId]    NVARCHAR (128) NOT NULL,
    [UserName]       NVARCHAR (MAX) NOT NULL,
    [Score]     INT            NOT NULL,
    [CreatedOn]  DATETIME2 (7)  NOT NULL,
    CONSTRAINT [PK_dbo.GameScore] PRIMARY KEY CLUSTERED ([UserId] ASC)
);