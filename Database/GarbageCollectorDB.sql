USE GarbageCollectorDB;
GO

 IF OBJECT_ID('dbo.UserRoles', 'U') IS NOT NULL
        DROP TABLE dbo.UserRoles;
    IF OBJECT_ID('dbo.Roles', 'U') IS NOT NULL
        DROP TABLE dbo.Roles;
    IF OBJECT_ID('dbo.Users', 'U') IS NOT NULL
        DROP TABLE dbo.Users;

		
CREATE TABLE Users (
    UserId INT PRIMARY KEY IDENTITY(1,1),  -- Primärschlüssel mit Auto-Inkrement
    Username NVARCHAR(100) NOT NULL,  -- Benutzername
    PasswordHash NVARCHAR(255) NOT NULL,  -- Passwort-Hash
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE()  -- Erstellungsdatum mit Standardwert als aktuelles Datum
);
GO

CREATE TABLE Roles (
    RoleId INT PRIMARY KEY IDENTITY(1,1),  -- Primärschlüssel mit Auto-Inkrement
    RoleName NVARCHAR(50) NOT NULL UNIQUE  -- Rollenname, muss eindeutig sein
);

CREATE TABLE UserRoles (
    UserRoleId INT PRIMARY KEY IDENTITY(1,1),  -- Primärschlüssel mit Auto-Inkrement
    UserId INT FOREIGN KEY REFERENCES Users(UserId),  -- Verweist auf die Users-Tabelle
    RoleId INT FOREIGN KEY REFERENCES Roles(RoleId)   -- Verweist auf die Roles-Tabelle
);

INSERT INTO Roles (RoleName) VALUES ('Admin');
INSERT INTO Roles (RoleName) VALUES ('User');

INSERT INTO UserRoles (UserId, RoleId)
VALUES (
    (SELECT UserId FROM Users WHERE Username = 'admin'), 
    (SELECT RoleId FROM Roles WHERE RoleName = 'Admin')
);


