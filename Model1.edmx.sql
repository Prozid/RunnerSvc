
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, and Azure
-- --------------------------------------------------
-- Date Created: 11/27/2014 16:22:25
-- Generated from EDMX file: C:\Users\pbio\Documents\GitHub\RunnerSvc\Model1.edmx
-- --------------------------------------------------

SET QUOTED_IDENTIFIER OFF;
GO
USE [webappDB];
GO
IF SCHEMA_ID(N'dbo') IS NULL EXECUTE(N'CREATE SCHEMA [dbo]');
GO

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[FK_Archivo_Carpeta]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Archivo] DROP CONSTRAINT [FK_Archivo_Carpeta];
GO
IF OBJECT_ID(N'[dbo].[FK_Carpeta_Padre]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Carpeta] DROP CONSTRAINT [FK_Carpeta_Padre];
GO
IF OBJECT_ID(N'[dbo].[FK_Proyecto_Carpeta]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Proyecto] DROP CONSTRAINT [FK_Proyecto_Carpeta];
GO
IF OBJECT_ID(N'[dbo].[FK_Resultado_Simulacion]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Resultado] DROP CONSTRAINT [FK_Resultado_Simulacion];
GO
IF OBJECT_ID(N'[dbo].[FK_Simulacion_EstadoSimulacion]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Simulacion] DROP CONSTRAINT [FK_Simulacion_EstadoSimulacion];
GO
IF OBJECT_ID(N'[dbo].[FK_Simulacion_Log]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Simulacion] DROP CONSTRAINT [FK_Simulacion_Log];
GO
IF OBJECT_ID(N'[dbo].[FK_Simulacion_MetodoClasificacion]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Simulacion] DROP CONSTRAINT [FK_Simulacion_MetodoClasificacion];
GO
IF OBJECT_ID(N'[dbo].[FK_Simulacion_MetodoSeleccion]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Simulacion] DROP CONSTRAINT [FK_Simulacion_MetodoSeleccion];
GO
IF OBJECT_ID(N'[dbo].[FK_Simulacion_Proyecto]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Simulacion] DROP CONSTRAINT [FK_Simulacion_Proyecto];
GO

-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[Archivo]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Archivo];
GO
IF OBJECT_ID(N'[dbo].[Carpeta]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Carpeta];
GO
IF OBJECT_ID(N'[dbo].[EstadoSimulacion]', 'U') IS NOT NULL
    DROP TABLE [dbo].[EstadoSimulacion];
GO
IF OBJECT_ID(N'[dbo].[Log]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Log];
GO
IF OBJECT_ID(N'[dbo].[MetodoClasificacion]', 'U') IS NOT NULL
    DROP TABLE [dbo].[MetodoClasificacion];
GO
IF OBJECT_ID(N'[dbo].[MetodoSeleccion]', 'U') IS NOT NULL
    DROP TABLE [dbo].[MetodoSeleccion];
GO
IF OBJECT_ID(N'[dbo].[Proyecto]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Proyecto];
GO
IF OBJECT_ID(N'[dbo].[Resultado]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Resultado];
GO
IF OBJECT_ID(N'[dbo].[Simulacion]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Simulacion];
GO
IF OBJECT_ID(N'[dbo].[sysdiagrams]', 'U') IS NOT NULL
    DROP TABLE [dbo].[sysdiagrams];
GO

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'Archivo'
CREATE TABLE [dbo].[Archivo] (
    [IdArchivo] uniqueidentifier  NOT NULL,
    [IdCarpeta] uniqueidentifier  NOT NULL,
    [Usuario] varchar(50)  NOT NULL,
    [Publico] bit  NOT NULL,
    [NombreArchivo] varchar(50)  NOT NULL,
    [ContentType] varchar(50)  NOT NULL,
    [FechaCreacion] datetime  NOT NULL,
    [Descripcion] varchar(256)  NULL,
    [BaseDatos] bit  NOT NULL,
    [Datos] varchar(max)  NULL
);
GO

-- Creating table 'Carpeta'
CREATE TABLE [dbo].[Carpeta] (
    [IdCarpeta] uniqueidentifier  NOT NULL,
    [Nombre] varchar(50)  NOT NULL,
    [IdCarpetaPadre] uniqueidentifier  NULL,
    [Usuario] varchar(50)  NOT NULL,
    [FechaCreac_on] datetime  NOT NULL
);
GO

-- Creating table 'EstadoSimulacion'
CREATE TABLE [dbo].[EstadoSimulacion] (
    [IdEstadoSimulacion] uniqueidentifier  NOT NULL,
    [Nombre] varchar(50)  NOT NULL,
    [Descripcion] varchar(256)  NULL,
    [NombreCorto] varchar(5)  NOT NULL
);
GO

-- Creating table 'Log'
CREATE TABLE [dbo].[Log] (
    [IdLog] uniqueidentifier  NOT NULL,
    [FechaLog] datetime  NOT NULL,
    [Texto] varchar(256)  NOT NULL
);
GO

-- Creating table 'MetodoClasificacion'
CREATE TABLE [dbo].[MetodoClasificacion] (
    [IdMetodoClasificacion] uniqueidentifier  NOT NULL,
    [Nombre] varchar(50)  NOT NULL,
    [Descripcion] varchar(256)  NULL,
    [ParametrosXDefecto] varchar(max)  NOT NULL
);
GO

-- Creating table 'MetodoSeleccion'
CREATE TABLE [dbo].[MetodoSeleccion] (
    [IdMetodoSeleccion] uniqueidentifier  NOT NULL,
    [Nombre] varchar(50)  NOT NULL,
    [Descripcion] varchar(256)  NULL,
    [ParametrosXDefecto] varchar(max)  NOT NULL
);
GO

-- Creating table 'Proyecto'
CREATE TABLE [dbo].[Proyecto] (
    [IdProyecto] uniqueidentifier  NOT NULL,
    [Nombre] varchar(150)  NOT NULL,
    [Descripcion] varchar(256)  NULL,
    [FechaLanzUltSimulacion] datetime  NULL,
    [FechaCreacionProyecto] datetime  NULL,
    [IdCarpeta] uniqueidentifier  NOT NULL
);
GO

-- Creating table 'Resultado'
CREATE TABLE [dbo].[Resultado] (
    [IdResultado] uniqueidentifier  NOT NULL,
    [NumGenes] int  NOT NULL,
    [NombreGenes] varchar(max)  NOT NULL,
    [IdGenes] varchar(max)  NOT NULL,
    [Accuracy_Media] float  NOT NULL,
    [Accuracy_Std] float  NOT NULL,
    [Sensitivity_Media] float  NOT NULL,
    [Sensitivity_Std] float  NOT NULL,
    [Specificity_Media] float  NOT NULL,
    [Specificity_Std] float  NOT NULL,
    [FechaLanzamiento] datetime  NULL,
    [FechaFinalizacion] datetime  NULL,
    [Duracion] datetime  NULL,
    [AccuracyXGenes] varchar(max)  NOT NULL,
    [Mediana] float  NULL,
    [IdSimulacion] uniqueidentifier  NULL,
    [NombreGenesSolucion] varchar(max)  NOT NULL,
    [IdGenesSolucion] varchar(max)  NULL
);
GO

-- Creating table 'Simulacion'
CREATE TABLE [dbo].[Simulacion] (
    [IdSimulacion] uniqueidentifier  NOT NULL,
    [IdProyecto] uniqueidentifier  NOT NULL,
    [Nombre] varchar(150)  NOT NULL,
    [Descripcion] varchar(256)  NULL,
    [FechaCreacionSimulacion] datetime  NOT NULL,
    [IdMetodoSeleccion] uniqueidentifier  NOT NULL,
    [IdMetodoClasificacion] uniqueidentifier  NOT NULL,
    [IdEstadoSimulacion] uniqueidentifier  NOT NULL,
    [ParametrosSeleccion] varchar(max)  NOT NULL,
    [ParametrosClasificacion] varchar(max)  NOT NULL,
    [Usuario] varchar(50)  NOT NULL,
    [IdLog] uniqueidentifier  NULL,
    [IdArchivo] uniqueidentifier  NOT NULL,
    [IdCarpeta] uniqueidentifier  NOT NULL
);
GO

-- Creating table 'sysdiagrams'
CREATE TABLE [dbo].[sysdiagrams] (
    [name] nvarchar(128)  NOT NULL,
    [principal_id] int  NOT NULL,
    [diagram_id] int IDENTITY(1,1) NOT NULL,
    [version] int  NULL,
    [definition] varbinary(max)  NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [IdArchivo] in table 'Archivo'
ALTER TABLE [dbo].[Archivo]
ADD CONSTRAINT [PK_Archivo]
    PRIMARY KEY CLUSTERED ([IdArchivo] ASC);
GO

-- Creating primary key on [IdCarpeta] in table 'Carpeta'
ALTER TABLE [dbo].[Carpeta]
ADD CONSTRAINT [PK_Carpeta]
    PRIMARY KEY CLUSTERED ([IdCarpeta] ASC);
GO

-- Creating primary key on [IdEstadoSimulacion] in table 'EstadoSimulacion'
ALTER TABLE [dbo].[EstadoSimulacion]
ADD CONSTRAINT [PK_EstadoSimulacion]
    PRIMARY KEY CLUSTERED ([IdEstadoSimulacion] ASC);
GO

-- Creating primary key on [IdLog] in table 'Log'
ALTER TABLE [dbo].[Log]
ADD CONSTRAINT [PK_Log]
    PRIMARY KEY CLUSTERED ([IdLog] ASC);
GO

-- Creating primary key on [IdMetodoClasificacion] in table 'MetodoClasificacion'
ALTER TABLE [dbo].[MetodoClasificacion]
ADD CONSTRAINT [PK_MetodoClasificacion]
    PRIMARY KEY CLUSTERED ([IdMetodoClasificacion] ASC);
GO

-- Creating primary key on [IdMetodoSeleccion] in table 'MetodoSeleccion'
ALTER TABLE [dbo].[MetodoSeleccion]
ADD CONSTRAINT [PK_MetodoSeleccion]
    PRIMARY KEY CLUSTERED ([IdMetodoSeleccion] ASC);
GO

-- Creating primary key on [IdProyecto] in table 'Proyecto'
ALTER TABLE [dbo].[Proyecto]
ADD CONSTRAINT [PK_Proyecto]
    PRIMARY KEY CLUSTERED ([IdProyecto] ASC);
GO

-- Creating primary key on [IdResultado] in table 'Resultado'
ALTER TABLE [dbo].[Resultado]
ADD CONSTRAINT [PK_Resultado]
    PRIMARY KEY CLUSTERED ([IdResultado] ASC);
GO

-- Creating primary key on [IdSimulacion] in table 'Simulacion'
ALTER TABLE [dbo].[Simulacion]
ADD CONSTRAINT [PK_Simulacion]
    PRIMARY KEY CLUSTERED ([IdSimulacion] ASC);
GO

-- Creating primary key on [diagram_id] in table 'sysdiagrams'
ALTER TABLE [dbo].[sysdiagrams]
ADD CONSTRAINT [PK_sysdiagrams]
    PRIMARY KEY CLUSTERED ([diagram_id] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on [IdCarpeta] in table 'Archivo'
ALTER TABLE [dbo].[Archivo]
ADD CONSTRAINT [FK_Archivo_Carpeta]
    FOREIGN KEY ([IdCarpeta])
    REFERENCES [dbo].[Carpeta]
        ([IdCarpeta])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_Archivo_Carpeta'
CREATE INDEX [IX_FK_Archivo_Carpeta]
ON [dbo].[Archivo]
    ([IdCarpeta]);
GO

-- Creating foreign key on [IdCarpetaPadre] in table 'Carpeta'
ALTER TABLE [dbo].[Carpeta]
ADD CONSTRAINT [FK_Carpeta_Padre]
    FOREIGN KEY ([IdCarpetaPadre])
    REFERENCES [dbo].[Carpeta]
        ([IdCarpeta])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_Carpeta_Padre'
CREATE INDEX [IX_FK_Carpeta_Padre]
ON [dbo].[Carpeta]
    ([IdCarpetaPadre]);
GO

-- Creating foreign key on [IdCarpeta] in table 'Proyecto'
ALTER TABLE [dbo].[Proyecto]
ADD CONSTRAINT [FK_Proyecto_Carpeta]
    FOREIGN KEY ([IdCarpeta])
    REFERENCES [dbo].[Carpeta]
        ([IdCarpeta])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_Proyecto_Carpeta'
CREATE INDEX [IX_FK_Proyecto_Carpeta]
ON [dbo].[Proyecto]
    ([IdCarpeta]);
GO

-- Creating foreign key on [IdEstadoSimulacion] in table 'Simulacion'
ALTER TABLE [dbo].[Simulacion]
ADD CONSTRAINT [FK_Simulacion_EstadoSimulacion]
    FOREIGN KEY ([IdEstadoSimulacion])
    REFERENCES [dbo].[EstadoSimulacion]
        ([IdEstadoSimulacion])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_Simulacion_EstadoSimulacion'
CREATE INDEX [IX_FK_Simulacion_EstadoSimulacion]
ON [dbo].[Simulacion]
    ([IdEstadoSimulacion]);
GO

-- Creating foreign key on [IdLog] in table 'Simulacion'
ALTER TABLE [dbo].[Simulacion]
ADD CONSTRAINT [FK_Simulacion_Log]
    FOREIGN KEY ([IdLog])
    REFERENCES [dbo].[Log]
        ([IdLog])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_Simulacion_Log'
CREATE INDEX [IX_FK_Simulacion_Log]
ON [dbo].[Simulacion]
    ([IdLog]);
GO

-- Creating foreign key on [IdMetodoClasificacion] in table 'Simulacion'
ALTER TABLE [dbo].[Simulacion]
ADD CONSTRAINT [FK_Simulacion_MetodoClasificacion]
    FOREIGN KEY ([IdMetodoClasificacion])
    REFERENCES [dbo].[MetodoClasificacion]
        ([IdMetodoClasificacion])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_Simulacion_MetodoClasificacion'
CREATE INDEX [IX_FK_Simulacion_MetodoClasificacion]
ON [dbo].[Simulacion]
    ([IdMetodoClasificacion]);
GO

-- Creating foreign key on [IdMetodoSeleccion] in table 'Simulacion'
ALTER TABLE [dbo].[Simulacion]
ADD CONSTRAINT [FK_Simulacion_MetodoSeleccion]
    FOREIGN KEY ([IdMetodoSeleccion])
    REFERENCES [dbo].[MetodoSeleccion]
        ([IdMetodoSeleccion])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_Simulacion_MetodoSeleccion'
CREATE INDEX [IX_FK_Simulacion_MetodoSeleccion]
ON [dbo].[Simulacion]
    ([IdMetodoSeleccion]);
GO

-- Creating foreign key on [IdProyecto] in table 'Simulacion'
ALTER TABLE [dbo].[Simulacion]
ADD CONSTRAINT [FK_Simulacion_Proyecto]
    FOREIGN KEY ([IdProyecto])
    REFERENCES [dbo].[Proyecto]
        ([IdProyecto])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_Simulacion_Proyecto'
CREATE INDEX [IX_FK_Simulacion_Proyecto]
ON [dbo].[Simulacion]
    ([IdProyecto]);
GO

-- Creating foreign key on [IdSimulacion] in table 'Resultado'
ALTER TABLE [dbo].[Resultado]
ADD CONSTRAINT [FK_Resultado_Simulacion]
    FOREIGN KEY ([IdSimulacion])
    REFERENCES [dbo].[Simulacion]
        ([IdSimulacion])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_Resultado_Simulacion'
CREATE INDEX [IX_FK_Resultado_Simulacion]
ON [dbo].[Resultado]
    ([IdSimulacion]);
GO

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------