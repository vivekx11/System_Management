-- College Event Management Portal - Database Setup Script
-- This script creates the database and applies initial configuration

-- Create Database
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'CollegeEventPortalDb')
BEGIN
    CREATE DATABASE CollegeEventPortalDb;
END
GO

USE CollegeEventPortalDb;
GO

-- After running migrations, execute these additional configurations

-- Add indexes for performance
CREATE NONCLUSTERED INDEX IX_Events_Status_StartDate 
ON Events(Status, StartDate) INCLUDE (Name, EventType);

CREATE NONCLUSTERED INDEX IX_Registrations_Status 
ON Registrations(Status) INCLUDE (EventId, UserId);

CREATE NONCLUSTERED INDEX IX_Scores_EventId_Round 
ON Scores(EventId, Round) INCLUDE (Points, ParticipantId, TeamId);

CREATE NONCLUSTERED INDEX IX_Notifications_UserId_IsRead 
ON Notifications(UserId, IsRead) INCLUDE (CreatedAt, Title);

CREATE NONCLUSTERED INDEX IX_ActivityLogs_CreatedAt 
ON ActivityLogs(CreatedAt DESC) INCLUDE (UserId, Action, Entity);

-- Add check constraints
ALTER TABLE Events 
ADD CONSTRAINT CK_Events_Dates CHECK (StartDate >= RegistrationDeadline AND EndDate >= StartDate);

ALTER TABLE Events 
ADD CONSTRAINT CK_Events_MaxParticipants CHECK (MaxParticipants > 0);

ALTER TABLE Events 
ADD CONSTRAINT CK_Events_MaxTeamSize CHECK (MaxTeamSize > 0 AND MaxTeamSize <= 10);

ALTER TABLE Scores 
ADD CONSTRAINT CK_Scores_Points CHECK (Points >= 0);

-- Create views for reporting

-- View: Event Statistics
CREATE OR ALTER VIEW vw_EventStatistics AS
SELECT 
    e.Id AS EventId,
    e.Name AS EventName,
    e.EventType,
    e.Status,
    e.StartDate,
    COUNT(DISTINCT r.Id) AS TotalRegistrations,
    COUNT(DISTINCT CASE WHEN r.Status = 1 THEN r.Id END) AS ApprovedRegistrations,
    COUNT(DISTINCT t.Id) AS TotalTeams,
    COUNT(DISTINCT s.Id) AS TotalScores
FROM Events e
LEFT JOIN Registrations r ON e.Id = r.EventId
LEFT JOIN Teams t ON e.Id = t.EventId
LEFT JOIN Scores s ON e.Id = s.EventId
GROUP BY e.Id, e.Name, e.EventType, e.Status, e.StartDate;
GO

-- View: User Participation Summary
CREATE OR ALTER VIEW vw_UserParticipation AS
SELECT 
    u.Id AS UserId,
    u.FullName,
    u.Email,
    u.Department,
    COUNT(DISTINCT r.EventId) AS EventsRegistered,
    COUNT(DISTINCT c.Id) AS CertificatesEarned,
    SUM(CASE WHEN s.Points IS NOT NULL THEN s.Points ELSE 0 END) AS TotalPoints
FROM AspNetUsers u
LEFT JOIN Registrations r ON u.Id = r.UserId
LEFT JOIN Certificates c ON u.Id = c.UserId
LEFT JOIN Scores s ON u.Id = s.ParticipantId
GROUP BY u.Id, u.FullName, u.Email, u.Department;
GO

-- View: Leaderboard
CREATE OR ALTER VIEW vw_Leaderboard AS
SELECT 
    e.Id AS EventId,
    e.Name AS EventName,
    CASE 
        WHEN e.AllowTeamRegistration = 1 THEN t.Name
        ELSE u.FullName
    END AS ParticipantName,
    SUM(s.Points) AS TotalScore,
    COUNT(DISTINCT s.Round) AS RoundsCompleted,
    RANK() OVER (PARTITION BY e.Id ORDER BY SUM(s.Points) DESC) AS Rank
FROM Events e
INNER JOIN Scores s ON e.Id = s.EventId
LEFT JOIN Teams t ON s.TeamId = t.Id
LEFT JOIN AspNetUsers u ON s.ParticipantId = u.Id
GROUP BY e.Id, e.Name, e.AllowTeamRegistration, t.Name, u.FullName;
GO

-- Stored Procedure: Get Event Dashboard Data
CREATE OR ALTER PROCEDURE sp_GetEventDashboard
    @EventId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Event Details
    SELECT * FROM Events WHERE Id = @EventId;
    
    -- Registration Statistics
    SELECT 
        Status,
        COUNT(*) AS Count
    FROM Registrations
    WHERE EventId = @EventId
    GROUP BY Status;
    
    -- Top Performers
    SELECT TOP 10
        CASE 
            WHEN e.AllowTeamRegistration = 1 THEN t.Name
            ELSE u.FullName
        END AS Name,
        SUM(s.Points) AS TotalScore
    FROM Scores s
    INNER JOIN Events e ON s.EventId = e.Id
    LEFT JOIN Teams t ON s.TeamId = t.Id
    LEFT JOIN AspNetUsers u ON s.ParticipantId = u.Id
    WHERE s.EventId = @EventId
    GROUP BY e.AllowTeamRegistration, t.Name, u.FullName
    ORDER BY TotalScore DESC;
END;
GO

-- Stored Procedure: Generate Certificate Batch
CREATE OR ALTER PROCEDURE sp_GenerateCertificateBatch
    @EventId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @CertTable TABLE (
        UserId NVARCHAR(450),
        EventId INT,
        Rank INT
    );
    
    -- Get top performers
    INSERT INTO @CertTable (UserId, EventId, Rank)
    SELECT 
        s.ParticipantId,
        s.EventId,
        RANK() OVER (ORDER BY SUM(s.Points) DESC) AS Rank
    FROM Scores s
    WHERE s.EventId = @EventId AND s.ParticipantId IS NOT NULL
    GROUP BY s.ParticipantId, s.EventId;
    
    -- Return certificate data
    SELECT 
        ct.UserId,
        ct.EventId,
        ct.Rank,
        u.FullName,
        e.Name AS EventName
    FROM @CertTable ct
    INNER JOIN AspNetUsers u ON ct.UserId = u.Id
    INNER JOIN Events e ON ct.EventId = e.Id
    WHERE ct.Rank <= 3;
END;
GO

-- Function: Calculate User Score for Event
CREATE OR ALTER FUNCTION fn_GetUserEventScore
(
    @UserId NVARCHAR(450),
    @EventId INT
)
RETURNS DECIMAL(18,2)
AS
BEGIN
    DECLARE @TotalScore DECIMAL(18,2);
    
    SELECT @TotalScore = SUM(Points)
    FROM Scores
    WHERE ParticipantId = @UserId AND EventId = @EventId;
    
    RETURN ISNULL(@TotalScore, 0);
END;
GO

-- Sample data for testing (optional)
-- Uncomment to insert sample events

/*
INSERT INTO Events (Name, Description, EventType, StartDate, EndDate, RegistrationDeadline, MaxParticipants, MaxTeamSize, AllowSoloRegistration, AllowTeamRegistration, Venue, Status, CurrentRound, TotalRounds, CreatedAt)
VALUES 
('Debugging Championship 2026', 'Find and fix bugs in real-world code scenarios', 1, '2026-05-15', '2026-05-15', '2026-05-10', 100, 1, 1, 0, 'Computer Lab A', 2, 1, 3, GETUTCDATE()),
('Code in the Dark Challenge', 'Write HTML/CSS without preview in complete darkness', 2, '2026-06-01', '2026-06-01', '2026-05-25', 50, 1, 1, 0, 'Auditorium', 2, 1, 1, GETUTCDATE()),
('UI/UX Design Hackathon', 'Design innovative user interfaces for mobile apps', 3, '2026-06-20', '2026-06-21', '2026-06-15', 80, 4, 0, 1, 'Design Studio', 2, 1, 2, GETUTCDATE());
*/

PRINT 'Database setup completed successfully!';
PRINT 'Run: dotnet ef database update';
PRINT 'Then run this script for additional configurations.';
GO
