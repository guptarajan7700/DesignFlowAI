/*
    Opens issues for each failed check on a Failed validation run,
    assigns them to the project lead designer, sets the project to NeedsAttention,
    and notifies that designer (or the project manager when no designer is set).
    Safe to call again: existing issues and the notification are not duplicated.
*/
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

CREATE OR ALTER PROCEDURE dbo.usp_CreateIssuesFromFailedValidation
    @ValidationRunId UNIQUEIDENTIFIER,
    @ActorUserId     UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF @ValidationRunId IS NULL
    BEGIN
        THROW 50001, 'ValidationRunId is required.', 1;
    END

    DECLARE @ProjectId   UNIQUEIDENTIFIER;
    DECLARE @DesignerId  UNIQUEIDENTIFIER;
    DECLARE @ManagerId   UNIQUEIDENTIFIER;
    DECLARE @RunStatus   NVARCHAR(30);
    DECLARE @ModelId     UNIQUEIDENTIFIER;
    DECLARE @FailedCount INT = 0;
    DECLARE @NotifyUser  UNIQUEIDENTIFIER;

    SELECT
        @ProjectId  = p.ProjectId,
        @DesignerId = p.LeadDesignerId,
        @ManagerId  = p.ProjectManagerId,
        @RunStatus  = vr.Status,
        @ModelId    = vr.RevitModelId
    FROM dbo.ValidationRuns AS vr
    INNER JOIN dbo.Projects AS p ON p.ProjectId = vr.ProjectId
    WHERE vr.ValidationRunId = @ValidationRunId;

    IF @ProjectId IS NULL
    BEGIN
        THROW 50002, 'Validation run was not found.', 1;
    END

    IF @RunStatus <> N'Failed'
        RETURN 0;

    BEGIN TRY
        BEGIN TRANSACTION;

        INSERT INTO dbo.Issues
        (
            IssueId, ProjectId, ValidationRunId, ValidationResultId, RevitModelId,
            Title, Description, Severity, Status, ElementId, SuggestedFix,
            AssignedToId, AssignedAt, CreatedAt, UpdatedAt, CreatedBy
        )
        SELECT
            NEWID(),
            @ProjectId,
            r.ValidationRunId,
            r.ValidationResultId,
            @ModelId,
            r.CheckName,
            r.Message,
            CASE r.CheckCode
                WHEN N'DoorsMissingFireRating' THEN N'High'
                WHEN N'UnplacedRooms' THEN N'Medium'
                WHEN N'RoomsWithoutNames' THEN N'Medium'
                WHEN N'MissingSheetParameters' THEN N'Low'
                ELSE N'Medium'
            END,
            CASE WHEN @DesignerId IS NULL THEN N'Open' ELSE N'Assigned' END,
            r.ElementId,
            r.SuggestedFix,
            @DesignerId,
            CASE WHEN @DesignerId IS NULL THEN NULL ELSE SYSUTCDATETIME() END,
            SYSUTCDATETIME(),
            SYSUTCDATETIME(),
            @ActorUserId
        FROM dbo.ValidationResults AS r
        WHERE r.ValidationRunId = @ValidationRunId
          AND r.Status = N'Fail'
          AND NOT EXISTS
          (
              SELECT 1
              FROM dbo.Issues AS i
              WHERE i.ValidationResultId = r.ValidationResultId
          );

        SET @FailedCount = @@ROWCOUNT;

        UPDATE dbo.Projects
        SET Status = N'NeedsAttention',
            UpdatedAt = SYSUTCDATETIME()
        WHERE ProjectId = @ProjectId
          AND Status NOT IN (N'Completed', N'Archived');

        SET @NotifyUser = COALESCE(@DesignerId, @ManagerId);

        IF @NotifyUser IS NOT NULL
           AND NOT EXISTS
           (
               SELECT 1
               FROM dbo.Notifications AS n
               WHERE n.RelatedEntityType = N'ValidationRun'
                 AND n.RelatedEntityId = @ValidationRunId
                 AND n.Type = N'ValidationFailed'
           )
        BEGIN
            INSERT INTO dbo.Notifications
            (
                NotificationId, UserId, ProjectId, Title, Message, Type,
                IsRead, RelatedEntityType, RelatedEntityId, CreatedAt, UpdatedAt, CreatedBy
            )
            VALUES
            (
                NEWID(),
                @NotifyUser,
                @ProjectId,
                N'Validation failed',
                CONCAT(
                    N'A model validation run failed. ',
                    @FailedCount,
                    N' new issue(s) were opened and assigned for review.'
                ),
                N'ValidationFailed',
                0,
                N'ValidationRun',
                @ValidationRunId,
                SYSUTCDATETIME(),
                SYSUTCDATETIME(),
                @ActorUserId
            );
        END

        IF NOT EXISTS
        (
            SELECT 1
            FROM dbo.AuditLogs AS a
            WHERE a.EntityType = N'ValidationRun'
              AND a.EntityId = @ValidationRunId
              AND a.Action = N'CreateIssuesFromFailure'
        )
        BEGIN
            INSERT INTO dbo.AuditLogs
            (
                AuditLogId, UserId, EntityType, EntityId, Action, NewValues,
                CreatedAt, UpdatedAt, CreatedBy
            )
            VALUES
            (
                NEWID(),
                @ActorUserId,
                N'ValidationRun',
                @ValidationRunId,
                N'CreateIssuesFromFailure',
                CONCAT(N'{"issuesCreated":', @FailedCount, N'}'),
                SYSUTCDATETIME(),
                SYSUTCDATETIME(),
                @ActorUserId
            );
        END

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH;

    RETURN @FailedCount;
END
GO
