CREATE DATABASE GarbageCollectorDB;
GO

USE GarbageCollectorDB;
GO

CREATE TABLE Users (
    UserId INT PRIMARY KEY IDENTITY(1,1),  -- Primärschlüssel mit Auto-Inkrement
    Username NVARCHAR(100) NOT NULL,  -- Benutzername
    PasswordHash NVARCHAR(255) NOT NULL,  -- Passwort-Hash
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE()  -- Erstellungsdatum mit Standardwert als aktuelles Datum
);
GO
