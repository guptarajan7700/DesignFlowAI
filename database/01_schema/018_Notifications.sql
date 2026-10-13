/*
    Notifications — in-app alerts. Validation failures insert one row per run.
*/
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID(N'dbo.Notifications', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Notifications
    (
        NotificationId    UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Notifications PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        UserId            UNIQUEIDENTIFIER NOT NULL,
        ProjectId         UNIQUEIDENTIFIER NULL,
        Title             NVARCHAR(200)    NOT NULL,
        Message           NVARCHAR(1000)   NOT NULL,
        Type              NVARCHAR(50)     NOT NULL,
        IsRead            BIT              NOT NULL CONSTRAINT DF_Notifications_IsRead DEFAULT (0),
        RelatedEntityType NVARCHAR(80)     NULL,
        RelatedEntityId   UNIQUEIDENTIFIER NULL,
        CreatedAt         DATETIME2        NOT NULL CONSTRAINT DF_Notifications_CreatedAt DEFAULT (SYSUTCDATETIME()),
        UpdatedAt         DATETIME2        NOT NULL CONSTRAINT DF_Notifications_UpdatedAt DEFAULT (SYSUTCDATETIME()),
        CreatedBy         UNIQUEIDENTIFIER NULL
    );
END
GO
