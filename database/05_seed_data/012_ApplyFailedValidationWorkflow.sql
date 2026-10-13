/*
    Demonstrates the failed-validation rule.
    Creates issues, assigns Arjun Mehta, sets NW-2BHK-001 to NeedsAttention, and notifies him.
*/
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

EXEC dbo.usp_CreateIssuesFromFailedValidation
    @ValidationRunId = 'C9000001-0000-4000-8000-000000000001',
    @ActorUserId = 'B2000001-0000-4000-8000-000000000003';
GO
