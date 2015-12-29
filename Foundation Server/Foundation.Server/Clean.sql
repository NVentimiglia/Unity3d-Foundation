-- Clean up old data

IF OBJECT_ID('SocialProfiles', 'U') IS NOT NULL
BEGIN
DROP TABLE SocialProfiles;
END

IF OBJECT_ID('AppObjects', 'U') IS NOT NULL
BEGIN
DROP TABLE AppObjects;
END

IF OBJECT_ID('AppUsers', 'U') IS NOT NULL
BEGIN
DROP TABLE AppUsers;
END

IF OBJECT_ID('_MigrationHistory', 'U') IS NOT NULL
BEGIN
DROP TABLE _MigrationHistory;
END

GO