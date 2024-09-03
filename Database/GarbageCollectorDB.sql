USE GarbageCollectorDB;
GO

IF OBJECT_ID('dbo.UserRoles', 'U') IS NOT NULL
    DROP TABLE dbo.UserRoles;
IF OBJECT_ID('dbo.Roles', 'U') IS NOT NULL
    DROP TABLE dbo.Roles;

-- Neue Tabelle CleanupLogs einfügen
IF OBJECT_ID('dbo.CleanupLogs', 'U') IS NOT NULL
    DROP TABLE dbo.CleanupLogs;

	IF OBJECT_ID('dbo.Users', 'U') IS NOT NULL
    DROP TABLE dbo.Users;

-- Tabelle Users erstellen
CREATE TABLE Users (
    UserId INT PRIMARY KEY IDENTITY(1,1),  -- Primärschlüssel mit Auto-Inkrement
    Username NVARCHAR(100) NOT NULL,  -- Benutzername
    PasswordHash NVARCHAR(255) NOT NULL,  -- Passwort-Hash
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE()  -- Erstellungsdatum mit Standardwert als aktuelles Datum
);
GO

-- Tabelle Roles erstellen
CREATE TABLE Roles (
    RoleId INT PRIMARY KEY IDENTITY(1,1),  -- Primärschlüssel mit Auto-Inkrement
    RoleName NVARCHAR(50) NOT NULL UNIQUE  -- Rollenname, muss eindeutig sein
);

-- Tabelle UserRoles erstellen
CREATE TABLE UserRoles (
    UserRoleId INT PRIMARY KEY IDENTITY(1,1),  -- Primärschlüssel mit Auto-Inkrement
    UserId INT FOREIGN KEY REFERENCES Users(UserId),  -- Verweist auf die Users-Tabelle
    RoleId INT FOREIGN KEY REFERENCES Roles(RoleId)   -- Verweist auf die Roles-Tabelle
);

-- Tabelle CleanupLogs erstellen
CREATE TABLE CleanupLogs (
    CleanupLogId INT PRIMARY KEY IDENTITY(1,1),   -- Primärschlüssel mit Auto-Inkrement
    UserId INT FOREIGN KEY REFERENCES Users(UserId), -- Verweist auf die Users-Tabelle
    CleanupDate DATETIME NOT NULL DEFAULT GETDATE(), -- Datum der Bereinigung
    FilesDeleted INT NOT NULL,                     -- Anzahl der gelöschten Dateien
    SpaceFreedInMB FLOAT NOT NULL,                 -- Freigegebener Speicherplatz in MB
    CleanupType NVARCHAR(50) NOT NULL DEFAULT 'Standard' -- Typ der Bereinigung (Standard, Junk, Duplicates)
);

-- Rollen einfügen
INSERT INTO Roles (RoleName) VALUES ('Admin');
INSERT INTO Roles (RoleName) VALUES ('User');

-- Admin-Benutzerrolle zuweisen
INSERT INTO UserRoles (UserId, RoleId)
VALUES (
    (SELECT UserId FROM Users WHERE Username = 'admin'), 
    (SELECT RoleId FROM Roles WHERE RoleName = 'Admin')
);
