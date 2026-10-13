/*
    Promotes AI task plans from Proposed to ToDo.
    Caller must be Admin or ProjectManager. Proposed rows are not active work before this runs.
*/
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

CREATE OR ALTER PROCEDURE dbo.usp_ApproveProposedTasks
    @ProjectId       UNIQUEIDENTIFIER,
    @ReviewerUserId  UNIQUEIDENTIFIER,
    @Comments        NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF @ProjectId IS NULL OR @ReviewerUserId IS NULL
    BEGIN
        THROW 50011, 'ProjectId and ReviewerUserId are required.', 1;
    END

    IF NOT EXISTS (SELECT 1 FROM dbo.Projects WHERE ProjectId = @ProjectId)
    BEGIN
        THROW 50012, 'Project was not found.', 1;
    END

    IF NOT EXISTS
    (
        SELECT 1
        FROM dbo.UserRoles AS ur
        INNER JOIN dbo.Roles AS r ON r.RoleId = ur.RoleId
        WHERE ur.UserId = @ReviewerUserId
          AND r.Name IN (N'Admin', N'ProjectManager')
    )
    BEGIN
        THROW 50013, 'Only an Admin or ProjectManager can approve proposed tasks.', 1;
    END

    DECLARE @ApprovedCount INT;

    BEGIN TRY
        BEGIN TRANSACTION;

        UPDATE dbo.Tasks
        SET Status = N'ToDo',
            ApprovedBy = @ReviewerUserId,
            ApprovedAt = SYSUTCDATETIME(),
            UpdatedAt = SYSUTCDATETIME()
        WHERE ProjectId = @ProjectId
          AND Status = N'Proposed'
          AND Source = N'AiProposed';

        SET @ApprovedCount = @@ROWCOUNT;

        IF @ApprovedCount = 0
        BEGIN
            THROW 50014, 'This project has no proposed AI tasks to approve.', 1;
        END

        UPDATE dbo.Approvals
        SET Status = N'Approved',
            ReviewedById = @ReviewerUserId,
            ReviewedAt = SYSUTCDATETIME(),
            Comments = COALESCE(@Comments, Comments),
            UpdatedAt = SYSUTCDATETIME()
        WHERE ProjectId = @ProjectId
          AND EntityType = N'TaskPlan'
          AND EntityId = @ProjectId
          AND Status = N'Pending';

        IF @@ROWCOUNT = 0
        BEGIN
            INSERT INTO dbo.Approvals
            (
                ApprovalId, ProjectId, EntityType, EntityId, Status,
                RequestedById, ReviewedById, Comments, RequestedAt, ReviewedAt,
                CreatedAt, UpdatedAt, CreatedBy
            )
            VALUES
            (
                NEWID(), @ProjectId, N'TaskPlan', @ProjectId, N'Approved',
                @ReviewerUserId, @ReviewerUserId, @Comments, SYSUTCDATETIME(), SYSUTCDATETIME(),
                SYSUTCDATETIME(), SYSUTCDATETIME(), @ReviewerUserId
            );
        END

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH;

    RETURN @ApprovedCount;
END
GO
