﻿/*
Deployment script for Northwind

This code was generated by a tool.
Changes to this file may cause incorrect behavior and will be lost if
the code is regenerated.
*/

GO
SET ANSI_NULLS, ANSI_PADDING, ANSI_WARNINGS, ARITHABORT, CONCAT_NULL_YIELDS_NULL, QUOTED_IDENTIFIER ON;

SET NUMERIC_ROUNDABORT OFF;

GO
USE [Northwind];


GO
PRINT N'Dropping [dbo].[Categories].[CategoryName]...';


GO
DROP INDEX [CategoryName]
    ON [dbo].[Categories];


GO
PRINT N'Dropping [dbo].[FK_Employees_Employees]...';


GO
ALTER TABLE [dbo].[Employees] DROP CONSTRAINT [FK_Employees_Employees];


GO
PRINT N'Dropping [dbo].[FK_Orders_Employees]...';


GO
ALTER TABLE [dbo].[Orders] DROP CONSTRAINT [FK_Orders_Employees];


GO
PRINT N'Dropping [dbo].[FK_EmployeeTerritories_Employees]...';


GO
ALTER TABLE [dbo].[EmployeeTerritories] DROP CONSTRAINT [FK_EmployeeTerritories_Employees];


GO
PRINT N'Dropping [dbo].[FK_Products_Suppliers]...';


GO
ALTER TABLE [dbo].[Products] DROP CONSTRAINT [FK_Products_Suppliers];


GO
PRINT N'Altering [dbo].[Categories]...';


GO
ALTER TABLE [dbo].[Categories] ALTER COLUMN [CategoryName] NVARCHAR (20) NOT NULL;


GO
PRINT N'Creating [dbo].[Categories].[CategoryName]...';


GO
CREATE NONCLUSTERED INDEX [CategoryName]
    ON [dbo].[Categories]([CategoryName] ASC);


GO
PRINT N'Starting rebuilding table [dbo].[Employees]...';


GO
BEGIN TRANSACTION;

SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;

SET XACT_ABORT ON;

CREATE TABLE [dbo].[tmp_ms_xx_Employees] (
    [EmployeeID]      INT            IDENTITY (1, 1) NOT NULL,
    [LastName]        NVARCHAR (30)  NOT NULL,
    [FirstName]       NVARCHAR (20)  NOT NULL,
    [MiddleName]      NVARCHAR (30)  NULL,
    [Title]           NVARCHAR (30)  NULL,
    [TitleOfCourtesy] NVARCHAR (25)  NULL,
    [BirthDate]       DATETIME       NULL,
    [HireDate]        DATETIME       NULL,
    [Address]         NVARCHAR (60)  NULL,
    [City]            NVARCHAR (15)  NULL,
    [Region]          NVARCHAR (15)  NULL,
    [PostalCode]      NVARCHAR (10)  NULL,
    [Country]         NVARCHAR (15)  NULL,
    [HomePhone]       NVARCHAR (24)  NULL,
    [Extension]       NVARCHAR (4)   NULL,
    [Photo]           IMAGE          NULL,
    [Notes]           NTEXT          NULL,
    [ReportsTo]       INT            NULL,
    [PhotoPath]       NVARCHAR (255) NULL,
    CONSTRAINT [tmp_ms_xx_constraint_PK_Employees1] PRIMARY KEY CLUSTERED ([EmployeeID] ASC)
);

IF EXISTS (SELECT TOP 1 1 
           FROM   [dbo].[Employees])
    BEGIN
        SET IDENTITY_INSERT [dbo].[tmp_ms_xx_Employees] ON;
        INSERT INTO [dbo].[tmp_ms_xx_Employees] ([EmployeeID], [LastName], [FirstName], [Title], [TitleOfCourtesy], [BirthDate], [HireDate], [Address], [City], [Region], [PostalCode], [Country], [HomePhone], [Extension], [Photo], [Notes], [ReportsTo], [PhotoPath])
        SELECT   [EmployeeID],
                 [LastName],
                 [FirstName],
                 [Title],
                 [TitleOfCourtesy],
                 [BirthDate],
                 [HireDate],
                 [Address],
                 [City],
                 [Region],
                 [PostalCode],
                 [Country],
                 [HomePhone],
                 [Extension],
                 [Photo],
                 [Notes],
                 [ReportsTo],
                 [PhotoPath]
        FROM     [dbo].[Employees]
        ORDER BY [EmployeeID] ASC;
        SET IDENTITY_INSERT [dbo].[tmp_ms_xx_Employees] OFF;
    END

DROP TABLE [dbo].[Employees];

EXECUTE sp_rename N'[dbo].[tmp_ms_xx_Employees]', N'Employees';

EXECUTE sp_rename N'[dbo].[tmp_ms_xx_constraint_PK_Employees1]', N'PK_Employees', N'OBJECT';

COMMIT TRANSACTION;

SET TRANSACTION ISOLATION LEVEL READ COMMITTED;


GO
PRINT N'Creating [dbo].[Employees].[LastName]...';


GO
CREATE NONCLUSTERED INDEX [LastName]
    ON [dbo].[Employees]([LastName] ASC);


GO
PRINT N'Creating [dbo].[Employees].[PostalCode]...';


GO
CREATE NONCLUSTERED INDEX [PostalCode]
    ON [dbo].[Employees]([PostalCode] ASC);


GO
PRINT N'Starting rebuilding table [dbo].[Suppliers]...';


GO
BEGIN TRANSACTION;

SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;

SET XACT_ABORT ON;

CREATE TABLE [dbo].[tmp_ms_xx_Suppliers] (
    [SupplierID]   INT            IDENTITY (1, 1) NOT NULL,
    [CompanyName]  NVARCHAR (40)  NOT NULL,
    [ContactName]  NVARCHAR (30)  NULL,
    [ContactTitle] NVARCHAR (30)  NULL,
    [Address]      NVARCHAR (60)  NULL,
    [City]         NVARCHAR (15)  NULL,
    [Region]       NVARCHAR (15)  NULL,
    [PostalCode]   NVARCHAR (10)  NULL,
    [Country]      NVARCHAR (15)  NULL,
    [Phone]        NVARCHAR (24)  NULL,
    [Fax]          NVARCHAR (24)  NULL,
    [Email]        NVARCHAR (100) NULL,
    [HomePage]     NTEXT          NULL,
    CONSTRAINT [tmp_ms_xx_constraint_PK_Suppliers1] PRIMARY KEY CLUSTERED ([SupplierID] ASC)
);

IF EXISTS (SELECT TOP 1 1 
           FROM   [dbo].[Suppliers])
    BEGIN
        SET IDENTITY_INSERT [dbo].[tmp_ms_xx_Suppliers] ON;
        INSERT INTO [dbo].[tmp_ms_xx_Suppliers] ([SupplierID], [CompanyName], [ContactName], [ContactTitle], [Address], [City], [Region], [PostalCode], [Country], [Phone], [Fax], [HomePage])
        SELECT   [SupplierID],
                 [CompanyName],
                 [ContactName],
                 [ContactTitle],
                 [Address],
                 [City],
                 [Region],
                 [PostalCode],
                 [Country],
                 [Phone],
                 [Fax],
                 [HomePage]
        FROM     [dbo].[Suppliers]
        ORDER BY [SupplierID] ASC;
        SET IDENTITY_INSERT [dbo].[tmp_ms_xx_Suppliers] OFF;
    END

DROP TABLE [dbo].[Suppliers];

EXECUTE sp_rename N'[dbo].[tmp_ms_xx_Suppliers]', N'Suppliers';

EXECUTE sp_rename N'[dbo].[tmp_ms_xx_constraint_PK_Suppliers1]', N'PK_Suppliers', N'OBJECT';

COMMIT TRANSACTION;

SET TRANSACTION ISOLATION LEVEL READ COMMITTED;


GO
PRINT N'Creating [dbo].[Suppliers].[CompanyName]...';


GO
CREATE NONCLUSTERED INDEX [CompanyName]
    ON [dbo].[Suppliers]([CompanyName] ASC);


GO
PRINT N'Creating [dbo].[Suppliers].[PostalCode]...';


GO
CREATE NONCLUSTERED INDEX [PostalCode]
    ON [dbo].[Suppliers]([PostalCode] ASC);


GO
PRINT N'Creating [dbo].[FK_Employees_Employees]...';


GO
ALTER TABLE [dbo].[Employees] WITH NOCHECK
    ADD CONSTRAINT [FK_Employees_Employees] FOREIGN KEY ([ReportsTo]) REFERENCES [dbo].[Employees] ([EmployeeID]);


GO
PRINT N'Creating [dbo].[FK_Orders_Employees]...';


GO
ALTER TABLE [dbo].[Orders] WITH NOCHECK
    ADD CONSTRAINT [FK_Orders_Employees] FOREIGN KEY ([EmployeeID]) REFERENCES [dbo].[Employees] ([EmployeeID]);


GO
PRINT N'Creating [dbo].[FK_EmployeeTerritories_Employees]...';


GO
ALTER TABLE [dbo].[EmployeeTerritories] WITH NOCHECK
    ADD CONSTRAINT [FK_EmployeeTerritories_Employees] FOREIGN KEY ([EmployeeID]) REFERENCES [dbo].[Employees] ([EmployeeID]);


GO
PRINT N'Creating [dbo].[FK_Products_Suppliers]...';


GO
ALTER TABLE [dbo].[Products] WITH NOCHECK
    ADD CONSTRAINT [FK_Products_Suppliers] FOREIGN KEY ([SupplierID]) REFERENCES [dbo].[Suppliers] ([SupplierID]);


GO
PRINT N'Creating [dbo].[CK_Birthdate]...';


GO
ALTER TABLE [dbo].[Employees] WITH NOCHECK
    ADD CONSTRAINT [CK_Birthdate] CHECK ([BirthDate]<getdate());


GO
PRINT N'Refreshing [dbo].[Alphabetical list of products]...';


GO
EXECUTE sp_refreshsqlmodule N'[dbo].[Alphabetical list of products]';


GO
PRINT N'Refreshing [dbo].[Product Sales for 1997]...';


GO
EXECUTE sp_refreshsqlmodule N'[dbo].[Product Sales for 1997]';


GO
PRINT N'Refreshing [dbo].[Products by Category]...';


GO
EXECUTE sp_refreshsqlmodule N'[dbo].[Products by Category]';


GO
PRINT N'Refreshing [dbo].[Sales by Category]...';


GO
EXECUTE sp_refreshsqlmodule N'[dbo].[Sales by Category]';


GO
PRINT N'Refreshing [dbo].[Category Sales for 1997]...';


GO
EXECUTE sp_refreshsqlmodule N'[dbo].[Category Sales for 1997]';


GO
PRINT N'Refreshing [dbo].[Invoices]...';


GO
EXECUTE sp_refreshsqlmodule N'[dbo].[Invoices]';


GO
PRINT N'Refreshing [dbo].[Customer and Suppliers by City]...';


GO
EXECUTE sp_refreshsqlmodule N'[dbo].[Customer and Suppliers by City]';


GO
PRINT N'Refreshing [dbo].[SalesByCategory]...';


GO
EXECUTE sp_refreshsqlmodule N'[dbo].[SalesByCategory]';


GO
PRINT N'Refreshing [dbo].[Employee Sales by Country]...';


GO
EXECUTE sp_refreshsqlmodule N'[dbo].[Employee Sales by Country]';


GO
PRINT N'Checking existing data against newly created constraints';


GO
USE [Northwind];


GO
ALTER TABLE [dbo].[Employees] WITH CHECK CHECK CONSTRAINT [FK_Employees_Employees];

ALTER TABLE [dbo].[Orders] WITH CHECK CHECK CONSTRAINT [FK_Orders_Employees];

ALTER TABLE [dbo].[EmployeeTerritories] WITH CHECK CHECK CONSTRAINT [FK_EmployeeTerritories_Employees];

ALTER TABLE [dbo].[Products] WITH CHECK CHECK CONSTRAINT [FK_Products_Suppliers];

ALTER TABLE [dbo].[Employees] WITH CHECK CHECK CONSTRAINT [CK_Birthdate];


GO
PRINT N'Update complete.';


GO
