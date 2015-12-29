
/* EF Stuff */

CREATE TABLE [dbo].[__MigrationHistory] (
    [MigrationId]    NVARCHAR (150)  NOT NULL,
    [ContextKey]     NVARCHAR (300)  NOT NULL,
    [Model]          VARBINARY (MAX) NOT NULL,
    [ProductVersion] NVARCHAR (32)   NOT NULL,
    CONSTRAINT [PK_dbo.__MigrationHistory] PRIMARY KEY CLUSTERED ([MigrationId] ASC, [ContextKey] ASC)
);



/* Core Models */

CREATE TABLE [dbo].[UserAccounts] (
    [Id]                         NVARCHAR (128)  NOT NULL,
    [Email]                      NVARCHAR (MAX)  NULL,
    [PhoneNumber]                NVARCHAR (MAX)  NULL,
    [EmailPassword_Salt]         VARBINARY (MAX) NULL,
    [EmailPassword_PasswordHash] VARBINARY (MAX) NULL,
    [EmailPassword_Expiration]   DATETIME        NULL,
    [PhonePassword_Salt]         VARBINARY (MAX) NULL,
    [PhonePassword_PasswordHash] VARBINARY (MAX) NULL,
    [PhonePassword_Expiration]   DATETIME        NULL,
    [ModifiedOn]                 DATETIME2 (7)   NOT NULL,
    [CreatedOn]                  DATETIME2 (7)   NOT NULL,
    CONSTRAINT [PK_dbo.UserAccounts] PRIMARY KEY CLUSTERED ([Id] ASC)
);

CREATE TABLE [dbo].[UserFacebookClaims] (
    [Id]          NVARCHAR (128) NOT NULL,
    [UserId]      NVARCHAR (128) NOT NULL,
    [FacebookId]  NVARCHAR (MAX) NULL,
    [AccessToken] NVARCHAR (MAX) NULL,
    [Email]       NVARCHAR (MAX) NULL,
    [FirstName]   NVARCHAR (MAX) NULL,
    [LastName]    NVARCHAR (MAX) NULL,
    [Picture]     NVARCHAR (MAX) NULL,
    [Link]        NVARCHAR (MAX) NULL,
    [Gender]      NVARCHAR (MAX) NULL,
    [Local]       NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_dbo.UserFacebookClaims] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_dbo.UserFacebookClaims_dbo.UserAccounts_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[UserAccounts] ([Id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_UserId]
    ON [dbo].[UserFacebookClaims]([UserId] ASC);

CREATE TABLE [dbo].[StorageObjects] (
    [ObjectId]    NVARCHAR (128) NOT NULL,
    [AclType]     INT            NOT NULL,
    [AclParam]    NVARCHAR (MAX) NULL,
    [ModifiedOn]  DATETIME2 (7)  NOT NULL,
    [CreatedOn]   DATETIME2 (7)  NOT NULL,
    [ObjectType]  NVARCHAR (MAX) NOT NULL,
    [ObjectScore] REAL           NOT NULL,
    [ObjectData]  NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_dbo.StorageObjects] PRIMARY KEY CLUSTERED ([ObjectId] ASC)
);


/*Custom objects*/

CREATE TABLE [dbo].[GameScores] (
    [UserId]    NVARCHAR (128) NOT NULL,
    [UserName]  NVARCHAR (MAX) NULL,
    [Score]     INT            NOT NULL,
    [CreatedOn] DATETIME2 (7)  NOT NULL,
    CONSTRAINT [PK_dbo.GameScores] PRIMARY KEY CLUSTERED ([UserId] ASC)
);

