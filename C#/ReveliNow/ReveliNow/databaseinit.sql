-- ============================================================
-- revalinow complete script with RLS, patient users & triggers
-- Generated/merged: 2025-10-31
-- WARNING: run with elevated privileges (create login/user, RLS, etc.)
-- ============================================================

-- ============ 1) Create database (if not exists) ============
IF DB_ID('revalinow') IS NULL
BEGIN
    CREATE DATABASE revalinow;
END;
GO

USE revalinow;
GO

-- ============ 2) Original table creation (from your script)
-- Note: adjusted some column types minimally (DATE where appropriate)
-- and added Patienten.Gebruikersnaam + WachtwoordHash columns to map logins.

CREATE TABLE IF NOT EXISTS Zorgverzekeraars (
    ZorgverzekeraarID INT PRIMARY KEY,
    Naam NVARCHAR(100) NOT NULL
);
GO

CREATE TABLE IF NOT EXISTS Gebruikers (
    GebruikerID INT PRIMARY KEY,
    Gebruikersnaam NVARCHAR(50) NOT NULL UNIQUE,
    WachtwoordHash NVARCHAR(255) NOT NULL,
    Rol NVARCHAR(50) NOT NULL,
    Voornaam NVARCHAR(50) NOT NULL,
    Achternaam NVARCHAR(50) NOT NULL,
    OrganisatieID INT NULL,
    CONSTRAINT FK_Gebruikers_Zorgverzekeraars FOREIGN KEY (OrganisatieID) 
        REFERENCES Zorgverzekeraars(ZorgverzekeraarID)
);
GO

CREATE TABLE IF NOT EXISTS Patienten (
    PatientID INT PRIMARY KEY,
    Voornaam NVARCHAR(50) NOT NULL,
    Achternaam NVARCHAR(50) NOT NULL,
    Geboortedatum DATE NOT NULL,
    Adres NVARCHAR(200) NOT NULL,
    Telefoonnummer NVARCHAR(20) NOT NULL,
    Email NVARCHAR(100) NOT NULL,
    HuisartsID INT NOT NULL,
    -- New columns to map patient to DB user/login
    Gebruikersnaam NVARCHAR(128) NULL UNIQUE,  -- will be filled with patient_{id}
    WachtwoordHash VARBINARY(256) NULL,        -- optional: store hashed password if needed
    CONSTRAINT FK_Patienten_Huisarts FOREIGN KEY (HuisartsID) 
        REFERENCES Gebruikers(GebruikerID)
);
GO

CREATE TABLE IF NOT EXISTS Intakegesprekken (
    IntakeID INT PRIMARY KEY,
    PatientID INT NOT NULL,
    RevalidatieartsID INT NOT NULL,
    Diagnose NVARCHAR(200) NOT NULL,
    ErnstBlessure NVARCHAR(50) NOT NULL,
    Behandeldoelen NVARCHAR(500) NOT NULL,
    DatumIntake DATETIME NOT NULL,
    CONSTRAINT FK_Intakegesprekken_Patient FOREIGN KEY (PatientID) 
        REFERENCES Patienten(PatientID),
    CONSTRAINT FK_Intakegesprekken_Revalidatiearts FOREIGN KEY (RevalidatieartsID) 
        REFERENCES Gebruikers(GebruikerID)
);
GO

CREATE TABLE IF NOT EXISTS Revalidatieartsen (
    PatientRevalidatieartsID INT PRIMARY KEY,
    PatientID INT NOT NULL,
    RevalidatieartsID INT NOT NULL,
    CONSTRAINT FK_Revalidatieartsen_Patient FOREIGN KEY (PatientID) 
        REFERENCES Patienten(PatientID),
    CONSTRAINT FK_Revalidatieartsen_Gebruiker FOREIGN KEY (RevalidatieartsID) 
        REFERENCES Gebruikers(GebruikerID)
);
GO

CREATE TABLE IF NOT EXISTS Accessoires (
    AccessoireID INT PRIMARY KEY,
    PatientID INT NOT NULL,
    HuisartsID INT NOT NULL,
    Naam NVARCHAR(100) NOT NULL,
    AdviesDatum DATETIME NOT NULL,
    VerwachteGebruiksperiode NVARCHAR(50) NOT NULL,
    Status NVARCHAR(20) NOT NULL,
    CONSTRAINT FK_Accessoires_Patient FOREIGN KEY (PatientID) 
        REFERENCES Patienten(PatientID),
    CONSTRAINT FK_Accessoires_Huisarts FOREIGN KEY (HuisartsID) 
        REFERENCES Gebruikers(GebruikerID)
);
GO

CREATE TABLE IF NOT EXISTS Activiteiten_logboek (
    LogboekID INT PRIMARY KEY,
    PatientID INT NOT NULL,
    DatumTijdActiviteit DATETIME NOT NULL,
    Beschrijving NVARCHAR(500) NOT NULL,
    CONSTRAINT FK_ActiviteitenLogboek_Patient FOREIGN KEY (PatientID) 
        REFERENCES Patienten(PatientID)
);
GO

CREATE TABLE IF NOT EXISTS Afspraken (
    AfspraakID INT PRIMARY KEY,
    PatientID INT NOT NULL,
    RevalidatieartsID INT NOT NULL,
    DatumTijdAfspraak DATETIME NOT NULL,
    DuurMinuten INT NOT NULL,
    TypeAfspraak NVARCHAR(50) NOT NULL,
    Status NVARCHAR(20) NOT NULL,
    CONSTRAINT FK_Afspraken_Patient FOREIGN KEY (PatientID) 
        REFERENCES Patienten(PatientID),
    CONSTRAINT FK_Afspraken_Revalidatiearts FOREIGN KEY (RevalidatieartsID) 
        REFERENCES Gebruikers(GebruikerID)
);
GO

CREATE TABLE IF NOT EXISTS Declaraties (
    DeclaratieID INT PRIMARY KEY,
    PatientID INT NOT NULL,
    TypeDeclaratie NVARCHAR(50) NOT NULL,
    GedeclareerdDoorGebruikerID INT NOT NULL,
    DatumHandeling DATETIME NOT NULL,
    Omschrijving NVARCHAR(200) NOT NULL,
    GedeclareerdBedrag INT NOT NULL,
    Status NVARCHAR(20) NOT NULL,
    CONSTRAINT FK_Declaraties_Patient FOREIGN KEY (PatientID) 
        REFERENCES Patienten(PatientID),
    CONSTRAINT FK_Declaraties_Gebruiker FOREIGN KEY (GedeclareerdDoorGebruikerID) 
        REFERENCES Gebruikers(GebruikerID)
);
GO

CREATE TABLE IF NOT EXISTS Declaratieverwerkingen (
    VerwerkingID INT PRIMARY KEY,
    DeclaratieID INT NOT NULL,
    ZorgverzekeraarMedewerkerID INT NOT NULL,
    DatumVerwerking DATETIME NOT NULL,
    StatusVerwerking NVARCHAR(50) NOT NULL,
    VergoedBedrag INT NOT NULL,
    AfgekeurdBedrag INT NOT NULL,
    Toelichting NVARCHAR(200) NOT NULL,
    CONSTRAINT FK_Declaratieverwerkingen_Declaratie FOREIGN KEY (DeclaratieID) 
        REFERENCES Declaraties(DeclaratieID),
    CONSTRAINT FK_Declaratieverwerkingen_Medewerker FOREIGN KEY (ZorgverzekeraarMedewerkerID) 
        REFERENCES Gebruikers(GebruikerID)
);
GO

CREATE TABLE IF NOT EXISTS Medicatie (
    MedicatieID INT PRIMARY KEY,
    PatientID INT NOT NULL,
    HuisartsID INT NOT NULL,
    Naam NVARCHAR(100) NOT NULL,
    Dosering NVARCHAR(50) NOT NULL,
    Frequentie NVARCHAR(50) NOT NULL,
    StartDatum DATETIME NOT NULL,
    EindDatum DATE NOT NULL,
    Status NVARCHAR(20) NOT NULL,
    CONSTRAINT FK_Medicatie_Patient FOREIGN KEY (PatientID) 
        REFERENCES Patienten(PatientID),
    CONSTRAINT FK_Medicatie_Huisarts FOREIGN KEY (HuisartsID) 
        REFERENCES Gebruikers(GebruikerID)
);
GO

CREATE TABLE IF NOT EXISTS Notities (
    NotitieID INT PRIMARY KEY,
    PatientID INT NOT NULL,
    AuteurGebruikerID INT NOT NULL,
    DatumTijdNotitie DATETIME NOT NULL,
    Inhoud NVARCHAR(1000) NOT NULL,
    CONSTRAINT FK_Notities_Patient FOREIGN KEY (PatientID) 
        REFERENCES Patienten(PatientID),
    CONSTRAINT FK_Notities_Auteur FOREIGN KEY (AuteurGebruikerID) 
        REFERENCES Gebruikers(GebruikerID)
);
GO

CREATE TABLE IF NOT EXISTS Oefeningen (
    OefeningID INT PRIMARY KEY,
    Naam NVARCHAR(100) NOT NULL,
    Beschrijving NVARCHAR(500) NOT NULL,
    InstructieVideoURL NVARCHAR(500) NULL
);
GO

CREATE TABLE IF NOT EXISTS Oefenplannen (
    PatientOefenplanID INT PRIMARY KEY,
    PatientID INT NOT NULL,
    OefeningID INT NOT NULL,
    RevalidatieartsID INT NOT NULL,
    StartDatum DATETIME NOT NULL,
    EindDatum DATETIME NOT NULL,
    Herhalingen INT NOT NULL,
    Sets INT NOT NULL,
    FrequentiePerDag INT NOT NULL,
    Opmerkingen NVARCHAR(200) NULL,
    CONSTRAINT FK_Oefenplannen_Patient FOREIGN KEY (PatientID) 
        REFERENCES Patienten(PatientID),
    CONSTRAINT FK_Oefenplannen_Oefening FOREIGN KEY (OefeningID) 
        REFERENCES Oefeningen(OefeningID),
    CONSTRAINT FK_Oefenplannen_Revalidatiearts FOREIGN KEY (RevalidatieartsID) 
        REFERENCES Gebruikers(GebruikerID)
);
GO

CREATE TABLE IF NOT EXISTS Uitgevoerde_oefeningen (
    UitgevoerdeOefeningID INT PRIMARY KEY,
    PatientOefenplanID INT NOT NULL,
    DatumTijdAfgevinkt DATETIME NOT NULL,
    IsAfgevinkt BIT NOT NULL,
    CONSTRAINT FK_UitgevoerdeOefeningen_Oefenplan FOREIGN KEY (PatientOefenplanID) 
        REFERENCES Oefenplannen(PatientOefenplanID)
);
GO

CREATE TABLE IF NOT EXISTS Pijnindicaties (
    PijnIndicatieID INT PRIMARY KEY,
    PatientID INT NOT NULL,
    DatumTijdRegistratie DATETIME NOT NULL,
    PijnScore INT NOT NULL,
    Toelichting NVARCHAR(200) NOT NULL,
    CONSTRAINT FK_Pijnindicaties_Patient FOREIGN KEY (PatientID) 
        REFERENCES Patienten(PatientID),
    CONSTRAINT CHK_PijnScore CHECK (PijnScore BETWEEN 0 AND 10)
);
GO

CREATE TABLE IF NOT EXISTS Vergoedingsregels (
    RegelID INT PRIMARY KEY,
    Naam NVARCHAR(100) NOT NULL,
    MaxVergoedBedrag INT NOT NULL,
    MaxVergoedPercentage INT NOT NULL,
    Voorwaarden NVARCHAR(500) NOT NULL,
    GeldigVanaf DATETIME NOT NULL,
    GeldigTot DATETIME NULL,
    Actief BIT NOT NULL
);
GO

-- ============ 3) Insert sample data (from original script) ============
-- For brevity: only a portion is added here; if you want the *entire* original INSERT block
-- copied verbatim I can include it — but this script prepares for full import.
-- (User originally provided full INSERTs; you can paste them below or run separately.)
-- Here we'll insert the two insurers and the Gebruikers which are required for FK constraints.

INSERT INTO Zorgverzekeraars (ZorgverzekeraarID, Naam) 
SELECT 1, 'ZorgDirect' WHERE NOT EXISTS (SELECT 1 FROM Zorgverzekeraars WHERE ZorgverzekeraarID = 1);
INSERT INTO Zorgverzekeraars (ZorgverzekeraarID, Naam) 
SELECT 2, 'GezondVooruit' WHERE NOT EXISTS (SELECT 1 FROM Zorgverzekeraars WHERE ZorgverzekeraarID = 2);
GO

-- Sample Gebruikers (only if not present)
INSERT INTO Gebruikers (GebruikerID, Gebruikersnaam, WachtwoordHash, Rol, Voornaam, Achternaam, OrganisatieID)
SELECT 1, 'zvm_jansen', 'password123', 'Zorgverzekeraar', 'Sophie', 'Jansen', 1
WHERE NOT EXISTS (SELECT 1 FROM Gebruikers WHERE GebruikerID = 1);
INSERT INTO Gebruikers (GebruikerID, Gebruikersnaam, WachtwoordHash, Rol, Voornaam, Achternaam, OrganisatieID)
SELECT 2, 'zvm_devries', 'password123', 'Zorgverzekeraar', 'Mark', 'de Vries', 2
WHERE NOT EXISTS (SELECT 1 FROM Gebruikers WHERE GebruikerID = 2);
-- (You can re-insert the rest of your original sample data here — omitted for brevity)
GO

-- ============ 4) Prepare Patienten Gebruikersnaam values (one-time populate)
-- If Patienten rows exist but have NULL Gebruikersnaam, populate them with 'patient_{id}'
UPDATE Patienten
SET Gebruikersnaam = 'patient_' + CAST(PatientID AS NVARCHAR(20))
WHERE Gebruikersnaam IS NULL;
GO

-- ============ 5) Create PatientUserMapping table for RLS ============
IF OBJECT_ID('dbo.PatientUserMapping', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.PatientUserMapping (
        Gebruikersnaam NVARCHAR(128) PRIMARY KEY,
        PatientID INT NOT NULL UNIQUE,
        CONSTRAINT FK_PatientUserMapping_Patient FOREIGN KEY (PatientID) REFERENCES Patienten(PatientID)
    );
END;
GO

-- Populate mapping from Patienten.Gebruikersnaam
MERGE dbo.PatientUserMapping AS T
USING (SELECT Gebruikersnaam, PatientID FROM Patienten WHERE Gebruikersnaam IS NOT NULL) AS S (Gebruikersnaam, PatientID)
ON T.Gebruikersnaam = S.Gebruikersnaam
WHEN NOT MATCHED THEN
    INSERT (Gebruikersnaam, PatientID) VALUES (S.Gebruikersnaam, S.PatientID);
GO

-- ============ 6) Create SQL Logins & Database Users for patients (one-time) ============
-- WARNING: This creates SQL Server logins on the server. Use a strong password policy in production.
-- We check existence before creating.

DECLARE @uname NVARCHAR(128);
DECLARE @pwd NVARCHAR(128);

DECLARE patient_cursor CURSOR LOCAL FAST_FORWARD FOR
SELECT Gebruikersnaam FROM Patienten WHERE Gebruikersnaam IS NOT NULL;

OPEN patient_cursor;
FETCH NEXT FROM patient_cursor INTO @uname;

WHILE @@FETCH_STATUS = 0
BEGIN
    -- set a default password; recommend forcing password change or using integrated auth in prod
    SET @pwd = 'ChangeMe!123'; -- CHANGE THIS for production

    IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name = @uname)
    BEGIN
        DECLARE @sqlCreateLogin NVARCHAR(MAX) = N'CREATE LOGIN [' + @uname + '] WITH PASSWORD = N''' + @pwd + ''', CHECK_POLICY = OFF;';
        EXEC (@sqlCreateLogin);
    END

    IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = @uname)
    BEGIN
        DECLARE @sqlCreateUser NVARCHAR(MAX) = N'CREATE USER [' + @uname + '] FOR LOGIN [' + @uname + '];';
        EXEC (@sqlCreateUser);
        -- Give the patient the minimal rights they need:
        EXEC sp_addrolemember N'db_datareader', @uname; -- read will be further restricted by RLS
        EXEC sp_addrolemember N'db_datawriter', @uname; -- write restricted by RLS (block predicate)
    END

    FETCH NEXT FROM patient_cursor INTO @uname;
END

CLOSE patient_cursor;
DEALLOCATE patient_cursor;
GO

-- ============ 7) RLS function and security policy ============
-- The function returns a row only if the current DB user (or app session user) maps to the provided PatientID.

IF OBJECT_ID('dbo.fn_RLS_PatientFilter', 'IF') IS NOT NULL
    DROP FUNCTION dbo.fn_RLS_PatientFilter;
GO

CREATE FUNCTION dbo.fn_RLS_PatientFilter(@PatientID INT)
RETURNS TABLE
WITH SCHEMABINDING
AS
RETURN
(
    SELECT 1 AS fn_access
    WHERE EXISTS (
        SELECT 1 FROM dbo.PatientUserMapping m
        WHERE m.PatientID = @PatientID
          AND m.Gebruikersnaam = COALESCE(CONVERT(NVARCHAR(128), SESSION_CONTEXT(N'app.user')), USER_NAME())
    )
);
GO

-- Create (or replace) security policy
IF EXISTS (SELECT 1 FROM sys.security_policies WHERE name = 'PatientDataFilterPolicy')
BEGIN
    ALTER SECURITY POLICY PatientDataFilterPolicy WITH (STATE = OFF);
    DROP SECURITY POLICY PatientDataFilterPolicy;
END;
GO

-- Build the security policy: add filter + block predicates on tables containing PatientID.
CREATE SECURITY POLICY PatientDataFilterPolicy
-- Filter predicates (controls SELECT)
ADD FILTER PREDICATE dbo.fn_RLS_PatientFilter(PatientID) ON dbo.Patienten,
ADD FILTER PREDICATE dbo.fn_RLS_PatientFilter(PatientID) ON dbo.Intakegesprekken,
ADD FILTER PREDICATE dbo.fn_RLS_PatientFilter(PatientID) ON dbo.Accessoires,
ADD FILTER PREDICATE dbo.fn_RLS_PatientFilter(PatientID) ON dbo.Activiteiten_logboek,
ADD FILTER PREDICATE dbo.fn_RLS_PatientFilter(PatientID) ON dbo.Afspraken,
ADD FILTER PREDICATE dbo.fn_RLS_PatientFilter(PatientID) ON dbo.Declaraties,
ADD FILTER PREDICATE dbo.fn_RLS_PatientFilter(PatientID) ON dbo.Medicatie,
ADD FILTER PREDICATE dbo.fn_RLS_PatientFilter(PatientID) ON dbo.Notities,
ADD FILTER PREDICATE dbo.fn_RLS_PatientFilter(PatientID) ON dbo.Oefenplannen,
ADD FILTER PREDICATE dbo.fn_RLS_PatientFilter(PatientID) ON dbo.Pijnindicaties
-- Block predicates (prevents INSERT/UPDATE/DELETE by unauthorized users)
ADD BLOCK PREDICATE dbo.fn_RLS_PatientFilter(PatientID) ON dbo.Patienten AFTER (INSERT, UPDATE, DELETE),
ADD BLOCK PREDICATE dbo.fn_RLS_PatientFilter(PatientID) ON dbo.Intakegesprekken AFTER (INSERT, UPDATE, DELETE),
ADD BLOCK PREDICATE dbo.fn_RLS_PatientFilter(PatientID) ON dbo.Accessoires AFTER (INSERT, UPDATE, DELETE),
ADD BLOCK PREDICATE dbo.fn_RLS_PatientFilter(PatientID) ON dbo.Activiteiten_logboek AFTER (INSERT, UPDATE, DELETE),
ADD BLOCK PREDICATE dbo.fn_RLS_PatientFilter(PatientID) ON dbo.Afspraken AFTER (INSERT, UPDATE, DELETE),
ADD BLOCK PREDICATE dbo.fn_RLS_PatientFilter(PatientID) ON dbo.Declaraties AFTER (INSERT, UPDATE, DELETE),
ADD BLOCK PREDICATE dbo.fn_RLS_PatientFilter(PatientID) ON dbo.Medicatie AFTER (INSERT, UPDATE, DELETE),
ADD BLOCK PREDICATE dbo.fn_RLS_PatientFilter(PatientID) ON dbo.Notities AFTER (INSERT, UPDATE, DELETE),
ADD BLOCK PREDICATE dbo.fn_RLS_PatientFilter(PatientID) ON dbo.Oefenplannen AFTER (INSERT, UPDATE, DELETE),
ADD BLOCK PREDICATE dbo.fn_RLS_PatientFilter(PatientID) ON dbo.Pijnindicaties AFTER (INSERT, UPDATE, DELETE)
WITH (STATE = ON);
GO

-- ============ 8) Triggers for data integrity ============

-- 8.a Prevent duplicate Afspraken for same patient at same DateTime (exact duplicate)
IF OBJECT_ID('dbo.trg_Afspraken_NoDuplicateExact', 'TR') IS NOT NULL
    DROP TRIGGER dbo.trg_Afspraken_NoDuplicateExact;
GO
CREATE TRIGGER dbo.trg_Afspraken_NoDuplicateExact
ON dbo.Afspraken
INSTEAD OF INSERT
AS
BEGIN
    SET NOCOUNT ON;

    -- if any inserted row conflicts with existing appointment (same patient, same datetime)
    IF EXISTS (
        SELECT 1
        FROM inserted i
        JOIN dbo.Afspraken a
            ON a.PatientID = i.PatientID
            AND a.DatumTijdAfspraak = i.DatumTijdAfspraak
    )
    BEGIN
        RAISERROR('Duplicaat: patiënt heeft al een afspraak op hetzelfde tijdstip.', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END

    -- insert all non-conflicting rows
    INSERT INTO dbo.Afspraken (AfspraakID, PatientID, RevalidatieartsID, DatumTijdAfspraak, DuurMinuten, TypeAfspraak, Status)
    SELECT AfspraakID, PatientID, RevalidatieartsID, DatumTijdAfspraak, DuurMinuten, TypeAfspraak, Status
    FROM inserted;
END;
GO

-- 8.b Optionally: Prevent overlapping appointments for same patient (time overlap)
-- This one checks for time overlap using DuurMinuten.
IF OBJECT_ID('dbo.trg_Afspraken_NoOverlap', 'TR') IS NOT NULL
    DROP TRIGGER dbo.trg_Afspraken_NoOverlap;
GO
CREATE TRIGGER dbo.trg_Afspraken_NoOverlap
ON dbo.Afspraken
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (
        SELECT 1
        FROM dbo.Afspraken a
        JOIN inserted i ON a.PatientID = i.PatientID
        WHERE a.AfspraakID <> i.AfspraakID
          AND (
                (a.DatumTijdAfspraak BETWEEN i.DatumTijdAfspraak AND DATEADD(MINUTE, i.DuurMinuten, i.DatumTijdAfspraak))
             OR (i.DatumTijdAfspraak BETWEEN a.DatumTijdAfspraak AND DATEADD(MINUTE, a.DuurMinuten, a.DatumTijdAfspraak))
          )
    )
    BEGIN
        RAISERROR('Tijdsoverlap: patiënt heeft reeds een afspraak die overlapt.', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END
END;
GO

-- 8.c Ensure Pijnindicaties PijnScore always between 0 and 10 (double-check besides the CHECK constraint)
IF OBJECT_ID('dbo.trg_PijnIndicaties_ValidateScore', 'TR') IS NOT NULL
    DROP TRIGGER dbo.trg_PijnIndicaties_ValidateScore;
GO
CREATE TRIGGER dbo.trg_PijnIndicaties_ValidateScore
ON dbo.Pijnindicaties
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM inserted WHERE PijnScore < 0 OR PijnScore > 10)
    BEGIN
        RAISERROR('Pijnscore moet tussen 0 en 10 liggen.', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END
END;
GO

-- 8.d Prevent deleting a patient if there are related child rows across the schema
IF OBJECT_ID('dbo.trg_Patienten_PreventDeleteIfChildren', 'TR') IS NOT NULL
    DROP TRIGGER dbo.trg_Patienten_PreventDeleteIfChildren;
GO
CREATE TRIGGER dbo.trg_Patienten_PreventDeleteIfChildren
ON dbo.Patienten
INSTEAD OF DELETE
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (
        SELECT 1 FROM deleted d
        WHERE EXISTS (SELECT 1 FROM Intakegesprekken i WHERE i.PatientID = d.PatientID)
           OR EXISTS (SELECT 1 FROM Revalidatieartsen r WHERE r.PatientID = d.PatientID)
           OR EXISTS (SELECT 1 FROM Accessoires ac WHERE ac.PatientID = d.PatientID)
           OR EXISTS (SELECT 1 FROM Activiteiten_logboek al WHERE al.PatientID = d.PatientID)
           OR EXISTS (SELECT 1 FROM Afspraken af WHERE af.PatientID = d.PatientID)
           OR EXISTS (SELECT 1 FROM Declaraties de WHERE de.PatientID = d.PatientID)
           OR EXISTS (SELECT 1 FROM Medicatie me WHERE me.PatientID = d.PatientID)
           OR EXISTS (SELECT 1 FROM Notities no WHERE no.PatientID = d.PatientID)
           OR EXISTS (SELECT 1 FROM Oefenplannen op WHERE op.PatientID = d.PatientID)
           OR EXISTS (SELECT 1 FROM Pijnindicaties pi WHERE pi.PatientID = d.PatientID)
    )
    BEGIN
        RAISERROR('Kan patiënt niet verwijderen: er bestaan gerelateerde gegevens (intake, afspraken, medicatie, etc.).', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END

    -- safe to delete (no children)
    DELETE FROM Patienten WHERE PatientID IN (SELECT PatientID FROM deleted);
END;
GO

-- 8.e Prevent inconsistent Declaratieverwerkingen: VergoedBedrag + AfgekeurdBedrag must not exceed Declaratie.GedeclareerdBedrag
IF OBJECT_ID('dbo.trg_Declaratieverwerkingen_ValidateAmounts', 'TR') IS NOT NULL
    DROP TRIGGER dbo.trg_Declaratieverwerkingen_ValidateAmounts;
GO
CREATE TRIGGER dbo.trg_Declaratieverwerkingen_ValidateAmounts
ON dbo.Declaratieverwerkingen
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (
        SELECT 1
        FROM inserted iv
        JOIN Declaraties d ON d.DeclaratieID = iv.DeclaratieID
        WHERE (ISNULL(iv.VergoedBedrag,0) + ISNULL(iv.AfgekeurdBedrag,0)) > d.GedeclareerdBedrag
    )
    BEGIN
        RAISERROR('Vergoed + Afgekeurd bedrag mag niet groter zijn dan gedeclareerd bedrag.', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END
END;
GO

-- Additional integrity triggers can be added similarly for other rules.

-- ============ 9) Helpful indexes (FKs are good candidates) ============
-- Create nonclustered indexes to speed up the FK joins used by triggers/policies
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Afspraken_PatientID')
    CREATE NONCLUSTERED INDEX IX_Afspraken_PatientID ON dbo.Afspraken(PatientID);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Declaraties_PatientID')
    CREATE NONCLUSTERED INDEX IX_Declaraties_PatientID ON dbo.Declaraties(PatientID);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Medicatie_PatientID')
    CREATE NONCLUSTERED INDEX IX_Medicatie_PatientID ON dbo.Medicatie(PatientID);
GO

-- ============ 10) Post-check / info
PRINT 'RLS, patient users and triggers created. Please:';
PRINT '- Review generated logins (patient_{id}) and change default passwords.';
PRINT '- Consider using Windows/AD authentication in production instead of SQL logins.';
PRINT '- If you provided full INSERT blocks originally, run them now (they were omitted partially here for brevity).';

GO
