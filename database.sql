-- ============================================================================
-- Georgia Transit Intelligence - University Midterm Database Initialization
-- Course: Object-Oriented Programming (C#) - Kutaisi International University
-- Topic 13: Bus Timetable (BusRoutes)
-- Database: BusTimetableDb
-- Table: dbo.BusRoutes
-- ============================================================================

-- Step 1: Create database if it does not exist
IF DB_ID(N'BusTimetableDb') IS NULL
BEGIN
    CREATE DATABASE BusTimetableDb;
END
GO

USE BusTimetableDb;
GO

-- Step 2: Create BusRoutes table if it does not exist
IF OBJECT_ID(N'dbo.BusRoutes', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.BusRoutes
    (
        Id              INT IDENTITY(1,1) PRIMARY KEY,
        RouteNumber     NVARCHAR(20)  NOT NULL,
        Origin          NVARCHAR(100) NOT NULL,
        Destination     NVARCHAR(100) NOT NULL,
        DepartureTime   TIME          NOT NULL,
        DurationMinutes INT           NOT NULL,
        Price           DECIMAL(8,2)  NOT NULL,
        IsExpress       BIT           NOT NULL
    );
END
GO

-- Step 3: Insert realistic seed data (guarded so re-execution does not duplicate rows)
IF NOT EXISTS (SELECT 1 FROM dbo.BusRoutes)
BEGIN
    INSERT INTO dbo.BusRoutes (RouteNumber, Origin, Destination, DepartureTime, DurationMinutes, Price, IsExpress)
    VALUES
        (N'GEO-101', N'Tbilisi (Ortachala)',   N'Batumi (New Station)',   '07:00:00', 330, 40.00, 1),
        (N'GEO-102', N'Tbilisi (Didube)',      N'Kutaisi (Chavchavadze)', '08:00:00', 220, 20.00, 1),
        (N'GEO-103', N'Kutaisi (Chavchavadze)',N'Batumi (Central)',       '08:30:00', 140, 15.00, 1),
        (N'GEO-104', N'Tbilisi (Ortachala)',   N'Kutaisi (Chavchavadze)', '09:00:00', 240, 18.00, 0),
        (N'GEO-105', N'Batumi (Central)',      N'Kutaisi (Chavchavadze)', '09:15:00', 150, 15.00, 0),
        (N'GEO-106', N'Kutaisi (Chavchavadze)',N'Tbilisi (Didube)',       '10:00:00', 215, 20.00, 1),
        (N'GEO-107', N'Tbilisi (Didube)',      N'Zugdidi (Bus Station)',  '10:30:00', 360, 30.00, 0),
        (N'GEO-108', N'Kutaisi (Chavchavadze)',N'Zugdidi (Bus Station)',  '11:00:00', 120, 12.00, 1),
        (N'GEO-109', N'Tbilisi (Ortachala)',   N'Telavi (Central)',       '11:30:00', 130, 14.00, 0),
        (N'GEO-110', N'Kutaisi (Chavchavadze)',N'Poti (Bus Station)',     '12:00:00', 95,  10.00, 0),
        (N'GEO-111', N'Tbilisi (Didube)',      N'Akhaltsikhe (Central)',  '13:00:00', 190, 18.00, 0),
        (N'GEO-112', N'Batumi (Central)',      N'Tbilisi (Ortachala)',    '14:00:00', 340, 40.00, 1),
        (N'GEO-113', N'Kutaisi (Chavchavadze)',N'Gori (Comfort Bus)',     '14:30:00', 150, 16.00, 0),
        (N'GEO-114', N'Tbilisi (Didube)',      N'Kazbegi (Stepantsminda)','15:00:00', 180, 17.00, 0),
        (N'GEO-115', N'Kutaisi Airport (KUT)', N'Tbilisi (Pushkin Sq)',   '16:00:00', 240, 25.00, 1),
        (N'GEO-116', N'Kutaisi Airport (KUT)', N'Batumi (Radisson)',      '16:30:00', 130, 20.00, 1),
        (N'GEO-117', N'Tbilisi (Didube)',      N'Borjomi (Park Station)', '17:15:00', 150, 15.00, 0),
        (N'GEO-118', N'Kutaisi (Chavchavadze)',N'Mestia (Seti Square)',   '07:30:00', 310, 35.00, 0);
END
GO
