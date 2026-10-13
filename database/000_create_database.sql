/*
    DesignFlow AI — create the application database.
    Run this script against master (do not pass -d DesignFlowAI).
*/
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

USE master;
GO

IF DB_ID(N'DesignFlowAI') IS NULL
BEGIN
    CREATE DATABASE DesignFlowAI;
END
GO

IF EXISTS (
    SELECT 1
    FROM sys.databases
    WHERE name = N'DesignFlowAI'
      AND is_read_committed_snapshot_on = 0
)
BEGIN
    ALTER DATABASE DesignFlowAI SET READ_COMMITTED_SNAPSHOT ON;
END
GO
