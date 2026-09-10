IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821001131_InicialSucursalesIdentidadCatalogo'
)
BEGIN
    IF SCHEMA_ID(N'catalogo') IS NULL EXEC(N'CREATE SCHEMA [catalogo];');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821001131_InicialSucursalesIdentidadCatalogo'
)
BEGIN
    IF SCHEMA_ID(N'identidad') IS NULL EXEC(N'CREATE SCHEMA [identidad];');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821001131_InicialSucursalesIdentidadCatalogo'
)
BEGIN
    IF SCHEMA_ID(N'sucursales') IS NULL EXEC(N'CREATE SCHEMA [sucursales];');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821001131_InicialSucursalesIdentidadCatalogo'
)
BEGIN
    CREATE TABLE [catalogo].[Categorias] (
        [Id] uniqueidentifier NOT NULL,
        [Nombre] nvarchar(150) NOT NULL,
        [Orden] int NOT NULL,
        [SucursalId] uniqueidentifier NOT NULL,
        [CreadoEn] datetime2 NOT NULL,
        [CreadoPorUsuarioId] uniqueidentifier NULL,
        [ActualizadoEn] datetime2 NULL,
        [ActualizadoPorUsuarioId] uniqueidentifier NULL,
        CONSTRAINT [PK_Categorias] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821001131_InicialSucursalesIdentidadCatalogo'
)
BEGIN
    CREATE TABLE [identidad].[Permisos] (
        [Id] uniqueidentifier NOT NULL,
        [Modulo] nvarchar(100) NOT NULL,
        [Accion] nvarchar(100) NOT NULL,
        CONSTRAINT [PK_Permisos] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821001131_InicialSucursalesIdentidadCatalogo'
)
BEGIN
    CREATE TABLE [catalogo].[Productos] (
        [Id] uniqueidentifier NOT NULL,
        [Nombre] nvarchar(200) NOT NULL,
        [Descripcion] nvarchar(max) NULL,
        [ImagenUrl] nvarchar(max) NULL,
        [CodigoBarra] nvarchar(450) NULL,
        [Precio] decimal(18,2) NOT NULL,
        [CategoriaId] uniqueidentifier NULL,
        [Activo] bit NOT NULL,
        [AplicaItbis] bit NOT NULL,
        [TipoProducto] int NOT NULL,
        [UnidadMedida] nvarchar(max) NOT NULL,
        [StockMinimo] decimal(18,3) NULL,
        [StockMaximo] decimal(18,3) NULL,
        [SucursalId] uniqueidentifier NOT NULL,
        [CreadoEn] datetime2 NOT NULL,
        [CreadoPorUsuarioId] uniqueidentifier NULL,
        [ActualizadoEn] datetime2 NULL,
        [ActualizadoPorUsuarioId] uniqueidentifier NULL,
        CONSTRAINT [PK_Productos] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821001131_InicialSucursalesIdentidadCatalogo'
)
BEGIN
    CREATE TABLE [identidad].[Roles] (
        [Id] uniqueidentifier NOT NULL,
        [Nombre] nvarchar(50) NOT NULL,
        CONSTRAINT [PK_Roles] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821001131_InicialSucursalesIdentidadCatalogo'
)
BEGIN
    CREATE TABLE [sucursales].[Sucursales] (
        [Id] uniqueidentifier NOT NULL,
        [Nombre] nvarchar(200) NOT NULL,
        [Rnc] nvarchar(20) NULL,
        [Direccion] nvarchar(max) NULL,
        [Telefono] nvarchar(max) NULL,
        [Activa] bit NOT NULL,
        [ModuloMeseroActivo] bit NOT NULL,
        [ModuloCocinaActivo] bit NOT NULL,
        [EcfActivo] bit NOT NULL,
        [SmtpActivo] bit NOT NULL,
        [CreadoEn] datetime2 NOT NULL,
        CONSTRAINT [PK_Sucursales] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821001131_InicialSucursalesIdentidadCatalogo'
)
BEGIN
    CREATE TABLE [identidad].[Usuarios] (
        [Id] uniqueidentifier NOT NULL,
        [NombreUsuario] nvarchar(100) NOT NULL,
        [HashPassword] nvarchar(max) NOT NULL,
        [Nombre] nvarchar(max) NOT NULL,
        [Email] nvarchar(max) NULL,
        [Activo] bit NOT NULL,
        CONSTRAINT [PK_Usuarios] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821001131_InicialSucursalesIdentidadCatalogo'
)
BEGIN
    CREATE TABLE [catalogo].[ProductoIngredientes] (
        [Id] uniqueidentifier NOT NULL,
        [ProductoId] uniqueidentifier NOT NULL,
        [InsumoId] uniqueidentifier NOT NULL,
        [CantidadUsada] decimal(18,3) NOT NULL,
        [IncluidoPorDefecto] bit NOT NULL,
        [Opcional] bit NOT NULL,
        CONSTRAINT [PK_ProductoIngredientes] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ProductoIngredientes_Productos_InsumoId] FOREIGN KEY ([InsumoId]) REFERENCES [catalogo].[Productos] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ProductoIngredientes_Productos_ProductoId] FOREIGN KEY ([ProductoId]) REFERENCES [catalogo].[Productos] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821001131_InicialSucursalesIdentidadCatalogo'
)
BEGIN
    CREATE TABLE [identidad].[RolPermisos] (
        [RolId] uniqueidentifier NOT NULL,
        [PermisoId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_RolPermisos] PRIMARY KEY ([RolId], [PermisoId]),
        CONSTRAINT [FK_RolPermisos_Permisos_PermisoId] FOREIGN KEY ([PermisoId]) REFERENCES [identidad].[Permisos] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_RolPermisos_Roles_RolId] FOREIGN KEY ([RolId]) REFERENCES [identidad].[Roles] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821001131_InicialSucursalesIdentidadCatalogo'
)
BEGIN
    CREATE TABLE [identidad].[UsuarioRoles] (
        [UsuarioId] uniqueidentifier NOT NULL,
        [RolId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_UsuarioRoles] PRIMARY KEY ([UsuarioId], [RolId]),
        CONSTRAINT [FK_UsuarioRoles_Roles_RolId] FOREIGN KEY ([RolId]) REFERENCES [identidad].[Roles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_UsuarioRoles_Usuarios_UsuarioId] FOREIGN KEY ([UsuarioId]) REFERENCES [identidad].[Usuarios] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821001131_InicialSucursalesIdentidadCatalogo'
)
BEGIN
    CREATE TABLE [identidad].[UsuarioSucursales] (
        [UsuarioId] uniqueidentifier NOT NULL,
        [SucursalId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_UsuarioSucursales] PRIMARY KEY ([UsuarioId], [SucursalId]),
        CONSTRAINT [FK_UsuarioSucursales_Usuarios_UsuarioId] FOREIGN KEY ([UsuarioId]) REFERENCES [identidad].[Usuarios] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821001131_InicialSucursalesIdentidadCatalogo'
)
BEGIN
    CREATE INDEX [IX_ProductoIngredientes_InsumoId] ON [catalogo].[ProductoIngredientes] ([InsumoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821001131_InicialSucursalesIdentidadCatalogo'
)
BEGIN
    CREATE INDEX [IX_ProductoIngredientes_ProductoId] ON [catalogo].[ProductoIngredientes] ([ProductoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821001131_InicialSucursalesIdentidadCatalogo'
)
BEGIN
    CREATE INDEX [IX_Productos_CodigoBarra] ON [catalogo].[Productos] ([CodigoBarra]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821001131_InicialSucursalesIdentidadCatalogo'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Roles_Nombre] ON [identidad].[Roles] ([Nombre]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821001131_InicialSucursalesIdentidadCatalogo'
)
BEGIN
    CREATE INDEX [IX_RolPermisos_PermisoId] ON [identidad].[RolPermisos] ([PermisoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821001131_InicialSucursalesIdentidadCatalogo'
)
BEGIN
    CREATE INDEX [IX_UsuarioRoles_RolId] ON [identidad].[UsuarioRoles] ([RolId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821001131_InicialSucursalesIdentidadCatalogo'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Usuarios_NombreUsuario] ON [identidad].[Usuarios] ([NombreUsuario]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821001131_InicialSucursalesIdentidadCatalogo'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260821001131_InicialSucursalesIdentidadCatalogo', N'10.0.11');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821004317_AgregarCajaFacturacionInventario'
)
BEGIN
    IF SCHEMA_ID(N'caja') IS NULL EXEC(N'CREATE SCHEMA [caja];');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821004317_AgregarCajaFacturacionInventario'
)
BEGIN
    IF SCHEMA_ID(N'facturacion') IS NULL EXEC(N'CREATE SCHEMA [facturacion];');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821004317_AgregarCajaFacturacionInventario'
)
BEGIN
    IF SCHEMA_ID(N'inventario') IS NULL EXEC(N'CREATE SCHEMA [inventario];');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821004317_AgregarCajaFacturacionInventario'
)
BEGIN
    ALTER TABLE [catalogo].[Productos] ADD [StockActual] decimal(18,3) NOT NULL DEFAULT 0.0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821004317_AgregarCajaFacturacionInventario'
)
BEGIN
    CREATE TABLE [caja].[Cajas] (
        [Id] uniqueidentifier NOT NULL,
        [SucursalId] uniqueidentifier NOT NULL,
        [Numero] nvarchar(50) NOT NULL,
        [Activa] bit NOT NULL,
        [IpPermitida] nvarchar(max) NULL,
        [HostnamePermitido] nvarchar(max) NULL,
        CONSTRAINT [PK_Cajas] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821004317_AgregarCajaFacturacionInventario'
)
BEGIN
    CREATE TABLE [facturacion].[Facturas] (
        [Id] uniqueidentifier NOT NULL,
        [SucursalId] uniqueidentifier NOT NULL,
        [CajaTurnoId] uniqueidentifier NOT NULL,
        [ClienteId] uniqueidentifier NULL,
        [NumeroNcf] nvarchar(20) NULL,
        [TipoComprobante] nvarchar(10) NULL,
        [Subtotal] decimal(18,2) NOT NULL,
        [Itbis] decimal(18,2) NOT NULL,
        [Descuento] decimal(18,2) NOT NULL,
        [Total] decimal(18,2) NOT NULL,
        [EstadoDgii] int NOT NULL,
        [FechaEmision] datetime2 NOT NULL,
        [CreadoPorUsuarioId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_Facturas] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821004317_AgregarCajaFacturacionInventario'
)
BEGIN
    CREATE TABLE [inventario].[MovimientosInventario] (
        [Id] uniqueidentifier NOT NULL,
        [SucursalId] uniqueidentifier NOT NULL,
        [ProductoId] uniqueidentifier NOT NULL,
        [Tipo] int NOT NULL,
        [Cantidad] decimal(18,3) NOT NULL,
        [CostoUnitario] decimal(18,2) NULL,
        [SaldoResultante] decimal(18,3) NOT NULL,
        [ReferenciaId] uniqueidentifier NULL,
        [Nota] nvarchar(max) NULL,
        [FechaHora] datetime2 NOT NULL,
        [CreadoPorUsuarioId] uniqueidentifier NULL,
        CONSTRAINT [PK_MovimientosInventario] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821004317_AgregarCajaFacturacionInventario'
)
BEGIN
    CREATE TABLE [facturacion].[SecuenciasNcf] (
        [Id] uniqueidentifier NOT NULL,
        [SucursalId] uniqueidentifier NOT NULL,
        [TipoComprobante] nvarchar(10) NOT NULL,
        [SecuenciaInicial] bigint NOT NULL,
        [SecuenciaProxima] bigint NOT NULL,
        [SecuenciaFinal] bigint NOT NULL,
        [FechaVencimiento] datetime2 NOT NULL,
        [FechaRegistro] datetime2 NOT NULL,
        [Activa] bit NOT NULL,
        CONSTRAINT [PK_SecuenciasNcf] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821004317_AgregarCajaFacturacionInventario'
)
BEGIN
    CREATE TABLE [caja].[TurnosCaja] (
        [Id] uniqueidentifier NOT NULL,
        [CajaId] uniqueidentifier NOT NULL,
        [UsuarioAperturaId] uniqueidentifier NOT NULL,
        [UsuarioCierreId] uniqueidentifier NULL,
        [FechaHoraApertura] datetime2 NOT NULL,
        [FechaHoraCierre] datetime2 NULL,
        [MontoAperturaEfectivo] decimal(18,2) NOT NULL,
        [Estado] int NOT NULL,
        CONSTRAINT [PK_TurnosCaja] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_TurnosCaja_Cajas_CajaId] FOREIGN KEY ([CajaId]) REFERENCES [caja].[Cajas] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821004317_AgregarCajaFacturacionInventario'
)
BEGIN
    CREATE TABLE [facturacion].[FacturaDetalles] (
        [Id] uniqueidentifier NOT NULL,
        [FacturaId] uniqueidentifier NOT NULL,
        [ProductoId] uniqueidentifier NOT NULL,
        [NombreProducto] nvarchar(200) NOT NULL,
        [Cantidad] decimal(18,3) NOT NULL,
        [PrecioUnitario] decimal(18,2) NOT NULL,
        [Descuento] decimal(18,2) NOT NULL,
        [Itbis] decimal(18,2) NOT NULL,
        [Total] decimal(18,2) NOT NULL,
        CONSTRAINT [PK_FacturaDetalles] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_FacturaDetalles_Facturas_FacturaId] FOREIGN KEY ([FacturaId]) REFERENCES [facturacion].[Facturas] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821004317_AgregarCajaFacturacionInventario'
)
BEGIN
    CREATE TABLE [caja].[DenominacionesCierre] (
        [Id] uniqueidentifier NOT NULL,
        [TurnoCajaId] uniqueidentifier NOT NULL,
        [FormaPago] int NOT NULL,
        [Denominacion] decimal(18,2) NULL,
        [Cantidad] int NOT NULL,
        [Subtotal] decimal(18,2) NOT NULL,
        CONSTRAINT [PK_DenominacionesCierre] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_DenominacionesCierre_TurnosCaja_TurnoCajaId] FOREIGN KEY ([TurnoCajaId]) REFERENCES [caja].[TurnosCaja] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821004317_AgregarCajaFacturacionInventario'
)
BEGIN
    CREATE TABLE [caja].[MovimientosCaja] (
        [Id] uniqueidentifier NOT NULL,
        [TurnoCajaId] uniqueidentifier NOT NULL,
        [Tipo] int NOT NULL,
        [FacturaId] uniqueidentifier NULL,
        [FormaPago] int NOT NULL,
        [Monto] decimal(18,2) NOT NULL,
        [Descripcion] nvarchar(max) NULL,
        [CreadoEn] datetime2 NOT NULL,
        CONSTRAINT [PK_MovimientosCaja] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_MovimientosCaja_TurnosCaja_TurnoCajaId] FOREIGN KEY ([TurnoCajaId]) REFERENCES [caja].[TurnosCaja] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821004317_AgregarCajaFacturacionInventario'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Cajas_SucursalId_Numero] ON [caja].[Cajas] ([SucursalId], [Numero]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821004317_AgregarCajaFacturacionInventario'
)
BEGIN
    CREATE INDEX [IX_DenominacionesCierre_TurnoCajaId] ON [caja].[DenominacionesCierre] ([TurnoCajaId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821004317_AgregarCajaFacturacionInventario'
)
BEGIN
    CREATE INDEX [IX_FacturaDetalles_FacturaId] ON [facturacion].[FacturaDetalles] ([FacturaId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821004317_AgregarCajaFacturacionInventario'
)
BEGIN
    CREATE INDEX [IX_MovimientosCaja_TurnoCajaId] ON [caja].[MovimientosCaja] ([TurnoCajaId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821004317_AgregarCajaFacturacionInventario'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_TurnosCaja_CajaId] ON [caja].[TurnosCaja] ([CajaId]) WHERE [Estado] = 0');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821004317_AgregarCajaFacturacionInventario'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260821004317_AgregarCajaFacturacionInventario', N'10.0.11');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821005314_AgregarPedidosComandas'
)
BEGIN
    IF SCHEMA_ID(N'pedidos') IS NULL EXEC(N'CREATE SCHEMA [pedidos];');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821005314_AgregarPedidosComandas'
)
BEGIN
    ALTER TABLE [facturacion].[Facturas] ADD [ComandaId] uniqueidentifier NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821005314_AgregarPedidosComandas'
)
BEGIN
    CREATE TABLE [pedidos].[ComandaCancelaciones] (
        [Id] uniqueidentifier NOT NULL,
        [ComandaId] uniqueidentifier NOT NULL,
        [ComandaItemId] uniqueidentifier NULL,
        [CanceladoPorUsuarioId] uniqueidentifier NOT NULL,
        [RolQueCancelo] int NOT NULL,
        [Motivo] nvarchar(500) NOT NULL,
        [FechaHora] datetime2 NOT NULL,
        [InventarioRevertido] bit NOT NULL,
        CONSTRAINT [PK_ComandaCancelaciones] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821005314_AgregarPedidosComandas'
)
BEGIN
    CREATE TABLE [pedidos].[Comandas] (
        [Id] uniqueidentifier NOT NULL,
        [SucursalId] uniqueidentifier NOT NULL,
        [NumeroComanda] int NOT NULL IDENTITY,
        [MesaId] uniqueidentifier NULL,
        [MeseroId] uniqueidentifier NULL,
        [Estado] int NOT NULL,
        [OrigenCreacion] int NOT NULL,
        [CreadoEn] datetime2 NOT NULL,
        CONSTRAINT [PK_Comandas] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821005314_AgregarPedidosComandas'
)
BEGIN
    CREATE TABLE [pedidos].[Mesas] (
        [Id] uniqueidentifier NOT NULL,
        [SucursalId] uniqueidentifier NOT NULL,
        [Numero] nvarchar(20) NOT NULL,
        [Salon] nvarchar(100) NULL,
        [Capacidad] int NOT NULL,
        [Estado] int NOT NULL,
        CONSTRAINT [PK_Mesas] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821005314_AgregarPedidosComandas'
)
BEGIN
    CREATE TABLE [pedidos].[ComandaItems] (
        [Id] uniqueidentifier NOT NULL,
        [ComandaId] uniqueidentifier NOT NULL,
        [ProductoId] uniqueidentifier NOT NULL,
        [NombreProducto] nvarchar(200) NOT NULL,
        [Cantidad] decimal(18,3) NOT NULL,
        [Estado] int NOT NULL,
        [Notas] nvarchar(max) NULL,
        [PrecioUnitario] decimal(18,2) NOT NULL,
        [InventarioDescontado] bit NOT NULL,
        CONSTRAINT [PK_ComandaItems] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ComandaItems_Comandas_ComandaId] FOREIGN KEY ([ComandaId]) REFERENCES [pedidos].[Comandas] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821005314_AgregarPedidosComandas'
)
BEGIN
    CREATE TABLE [pedidos].[ComandaItemIngredientes] (
        [Id] uniqueidentifier NOT NULL,
        [ComandaItemId] uniqueidentifier NOT NULL,
        [IngredienteId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_ComandaItemIngredientes] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ComandaItemIngredientes_ComandaItems_ComandaItemId] FOREIGN KEY ([ComandaItemId]) REFERENCES [pedidos].[ComandaItems] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821005314_AgregarPedidosComandas'
)
BEGIN
    CREATE INDEX [IX_ComandaItemIngredientes_ComandaItemId] ON [pedidos].[ComandaItemIngredientes] ([ComandaItemId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821005314_AgregarPedidosComandas'
)
BEGIN
    CREATE INDEX [IX_ComandaItems_ComandaId] ON [pedidos].[ComandaItems] ([ComandaId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821005314_AgregarPedidosComandas'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Comandas_SucursalId_NumeroComanda] ON [pedidos].[Comandas] ([SucursalId], [NumeroComanda]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821005314_AgregarPedidosComandas'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260821005314_AgregarPedidosComandas', N'10.0.11');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821010102_AgregarClientes'
)
BEGIN
    IF SCHEMA_ID(N'clientes') IS NULL EXEC(N'CREATE SCHEMA [clientes];');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821010102_AgregarClientes'
)
BEGIN
    CREATE TABLE [clientes].[Clientes] (
        [Id] uniqueidentifier NOT NULL,
        [SucursalId] uniqueidentifier NOT NULL,
        [NombreORazonSocial] nvarchar(200) NOT NULL,
        [RncOCedula] nvarchar(20) NULL,
        [Telefono] nvarchar(max) NULL,
        [Email] nvarchar(max) NULL,
        [Direccion] nvarchar(max) NULL,
        [TipoCliente] int NOT NULL,
        [Activo] bit NOT NULL,
        [CreadoEn] datetime2 NOT NULL,
        CONSTRAINT [PK_Clientes] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821010102_AgregarClientes'
)
BEGIN
    CREATE INDEX [IX_Clientes_SucursalId_RncOCedula] ON [clientes].[Clientes] ([SucursalId], [RncOCedula]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821010102_AgregarClientes'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260821010102_AgregarClientes', N'10.0.11');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821010332_AgregarCxcCxp'
)
BEGIN
    IF SCHEMA_ID(N'cxccxp') IS NULL EXEC(N'CREATE SCHEMA [cxccxp];');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821010332_AgregarCxcCxp'
)
BEGIN
    CREATE TABLE [cxccxp].[CuentasPorCobrar] (
        [Id] uniqueidentifier NOT NULL,
        [SucursalId] uniqueidentifier NOT NULL,
        [ClienteId] uniqueidentifier NOT NULL,
        [FacturaId] uniqueidentifier NOT NULL,
        [MontoOriginal] decimal(18,2) NOT NULL,
        [SaldoPendiente] decimal(18,2) NOT NULL,
        [FechaVencimiento] datetime2 NOT NULL,
        [Estado] int NOT NULL,
        [CreadoEn] datetime2 NOT NULL,
        CONSTRAINT [PK_CuentasPorCobrar] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821010332_AgregarCxcCxp'
)
BEGIN
    CREATE TABLE [cxccxp].[CuentasPorPagar] (
        [Id] uniqueidentifier NOT NULL,
        [SucursalId] uniqueidentifier NOT NULL,
        [ProveedorId] uniqueidentifier NOT NULL,
        [DocumentoReferencia] nvarchar(50) NOT NULL,
        [MontoOriginal] decimal(18,2) NOT NULL,
        [SaldoPendiente] decimal(18,2) NOT NULL,
        [FechaVencimiento] datetime2 NOT NULL,
        [Estado] int NOT NULL,
        [CreadoEn] datetime2 NOT NULL,
        CONSTRAINT [PK_CuentasPorPagar] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821010332_AgregarCxcCxp'
)
BEGIN
    CREATE TABLE [cxccxp].[Proveedores] (
        [Id] uniqueidentifier NOT NULL,
        [SucursalId] uniqueidentifier NOT NULL,
        [NombreORazonSocial] nvarchar(200) NOT NULL,
        [Rnc] nvarchar(20) NULL,
        [Telefono] nvarchar(max) NULL,
        [Activo] bit NOT NULL,
        CONSTRAINT [PK_Proveedores] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821010332_AgregarCxcCxp'
)
BEGIN
    CREATE TABLE [cxccxp].[PagosCxC] (
        [Id] uniqueidentifier NOT NULL,
        [CuentaPorCobrarId] uniqueidentifier NOT NULL,
        [Monto] decimal(18,2) NOT NULL,
        [FechaPago] datetime2 NOT NULL,
        [FormaPago] nvarchar(30) NOT NULL,
        CONSTRAINT [PK_PagosCxC] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PagosCxC_CuentasPorCobrar_CuentaPorCobrarId] FOREIGN KEY ([CuentaPorCobrarId]) REFERENCES [cxccxp].[CuentasPorCobrar] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821010332_AgregarCxcCxp'
)
BEGIN
    CREATE TABLE [cxccxp].[PagosCxP] (
        [Id] uniqueidentifier NOT NULL,
        [CuentaPorPagarId] uniqueidentifier NOT NULL,
        [Monto] decimal(18,2) NOT NULL,
        [FechaPago] datetime2 NOT NULL,
        [FormaPago] nvarchar(30) NOT NULL,
        CONSTRAINT [PK_PagosCxP] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PagosCxP_CuentasPorPagar_CuentaPorPagarId] FOREIGN KEY ([CuentaPorPagarId]) REFERENCES [cxccxp].[CuentasPorPagar] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821010332_AgregarCxcCxp'
)
BEGIN
    CREATE INDEX [IX_PagosCxC_CuentaPorCobrarId] ON [cxccxp].[PagosCxC] ([CuentaPorCobrarId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821010332_AgregarCxcCxp'
)
BEGIN
    CREATE INDEX [IX_PagosCxP_CuentaPorPagarId] ON [cxccxp].[PagosCxP] ([CuentaPorPagarId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821010332_AgregarCxcCxp'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260821010332_AgregarCxcCxp', N'10.0.11');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821011429_AgregarPropinaYSmtp'
)
BEGIN
    ALTER TABLE [sucursales].[Sucursales] ADD [SmtpHost] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821011429_AgregarPropinaYSmtp'
)
BEGIN
    ALTER TABLE [sucursales].[Sucursales] ADD [SmtpPassword] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821011429_AgregarPropinaYSmtp'
)
BEGIN
    ALTER TABLE [sucursales].[Sucursales] ADD [SmtpPuerto] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821011429_AgregarPropinaYSmtp'
)
BEGIN
    ALTER TABLE [sucursales].[Sucursales] ADD [SmtpRemitente] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821011429_AgregarPropinaYSmtp'
)
BEGIN
    ALTER TABLE [sucursales].[Sucursales] ADD [SmtpUsaSsl] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821011429_AgregarPropinaYSmtp'
)
BEGIN
    ALTER TABLE [sucursales].[Sucursales] ADD [SmtpUsuario] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821011429_AgregarPropinaYSmtp'
)
BEGIN
    ALTER TABLE [facturacion].[Facturas] ADD [Propina] decimal(18,2) NOT NULL DEFAULT 0.0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821011429_AgregarPropinaYSmtp'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260821011429_AgregarPropinaYSmtp', N'10.0.11');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821011851_AgregarAuditoriaYAutorizaciones'
)
BEGIN
    IF SCHEMA_ID(N'comun') IS NULL EXEC(N'CREATE SCHEMA [comun];');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821011851_AgregarAuditoriaYAutorizaciones'
)
BEGIN
    CREATE TABLE [identidad].[AutorizacionesSupervisor] (
        [Id] uniqueidentifier NOT NULL,
        [UsuarioAutorizanteId] uniqueidentifier NOT NULL,
        [Accion] nvarchar(50) NOT NULL,
        [Expira] datetime2 NOT NULL,
        [Usada] bit NOT NULL,
        CONSTRAINT [PK_AutorizacionesSupervisor] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821011851_AgregarAuditoriaYAutorizaciones'
)
BEGIN
    CREATE TABLE [comun].[LogsAuditoria] (
        [Id] uniqueidentifier NOT NULL,
        [SucursalId] uniqueidentifier NULL,
        [UsuarioId] uniqueidentifier NOT NULL,
        [Accion] nvarchar(100) NOT NULL,
        [Entidad] nvarchar(100) NOT NULL,
        [EntidadId] uniqueidentifier NULL,
        [Detalle] nvarchar(1000) NULL,
        [FechaHora] datetime2 NOT NULL,
        CONSTRAINT [PK_LogsAuditoria] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821011851_AgregarAuditoriaYAutorizaciones'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260821011851_AgregarAuditoriaYAutorizaciones', N'10.0.11');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821012946_AgregarSerieNcf'
)
BEGIN
    ALTER TABLE [facturacion].[SecuenciasNcf] ADD [Serie] nvarchar(5) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821012946_AgregarSerieNcf'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260821012946_AgregarSerieNcf', N'10.0.11');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821013237_AgregarNotasCreditoDebito'
)
BEGIN
    CREATE TABLE [facturacion].[NotasCreditoDebito] (
        [Id] uniqueidentifier NOT NULL,
        [SucursalId] uniqueidentifier NOT NULL,
        [FacturaOriginalId] uniqueidentifier NOT NULL,
        [Tipo] int NOT NULL,
        [Motivo] nvarchar(500) NOT NULL,
        [Monto] decimal(18,2) NOT NULL,
        [NumeroNcf] nvarchar(20) NULL,
        [TipoComprobante] nvarchar(10) NULL,
        [EstadoDgii] int NOT NULL,
        [FechaEmision] datetime2 NOT NULL,
        [CreadoPorUsuarioId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_NotasCreditoDebito] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821013237_AgregarNotasCreditoDebito'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260821013237_AgregarNotasCreditoDebito', N'10.0.11');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821013544_AgregarCombos'
)
BEGIN
    ALTER TABLE [catalogo].[Productos] ADD [EsCombo] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821013544_AgregarCombos'
)
BEGIN
    CREATE TABLE [catalogo].[ComboItems] (
        [Id] uniqueidentifier NOT NULL,
        [ComboId] uniqueidentifier NOT NULL,
        [ProductoIncluidoId] uniqueidentifier NOT NULL,
        [Cantidad] decimal(18,3) NOT NULL,
        CONSTRAINT [PK_ComboItems] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ComboItems_Productos_ComboId] FOREIGN KEY ([ComboId]) REFERENCES [catalogo].[Productos] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_ComboItems_Productos_ProductoIncluidoId] FOREIGN KEY ([ProductoIncluidoId]) REFERENCES [catalogo].[Productos] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821013544_AgregarCombos'
)
BEGIN
    CREATE INDEX [IX_ComboItems_ComboId] ON [catalogo].[ComboItems] ([ComboId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821013544_AgregarCombos'
)
BEGIN
    CREATE INDEX [IX_ComboItems_ProductoIncluidoId] ON [catalogo].[ComboItems] ([ProductoIncluidoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821013544_AgregarCombos'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260821013544_AgregarCombos', N'10.0.11');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821013821_AgregarCostoUnitarioProducto'
)
BEGIN
    ALTER TABLE [catalogo].[Productos] ADD [CostoUnitario] decimal(18,2) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821013821_AgregarCostoUnitarioProducto'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260821013821_AgregarCostoUnitarioProducto', N'10.0.11');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821190903_AgregarDatosDgiiAFactura'
)
BEGIN
    ALTER TABLE [facturacion].[Facturas] ADD [CodigoSeguridadDgii] nvarchar(6) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821190903_AgregarDatosDgiiAFactura'
)
BEGIN
    ALTER TABLE [facturacion].[Facturas] ADD [TrackIdDgii] nvarchar(100) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821190903_AgregarDatosDgiiAFactura'
)
BEGIN
    ALTER TABLE [facturacion].[Facturas] ADD [XmlFirmadoDgii] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821190903_AgregarDatosDgiiAFactura'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260821190903_AgregarDatosDgiiAFactura', N'10.0.11');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821195134_AgregarTasaItbisYRespuestaDgii'
)
BEGIN
    ALTER TABLE [catalogo].[Productos] ADD [TasaItbis] decimal(5,4) NOT NULL DEFAULT 0.0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821195134_AgregarTasaItbisYRespuestaDgii'
)
BEGIN
    ALTER TABLE [facturacion].[Facturas] ADD [FechaEnvioDgii] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821195134_AgregarTasaItbisYRespuestaDgii'
)
BEGIN
    ALTER TABLE [facturacion].[Facturas] ADD [FechaRespuestaDgii] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821195134_AgregarTasaItbisYRespuestaDgii'
)
BEGIN
    ALTER TABLE [facturacion].[Facturas] ADD [MensajeDgii] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821195134_AgregarTasaItbisYRespuestaDgii'
)
BEGIN
    ALTER TABLE [facturacion].[FacturaDetalles] ADD [TasaItbis] decimal(5,4) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821195134_AgregarTasaItbisYRespuestaDgii'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260821195134_AgregarTasaItbisYRespuestaDgii', N'10.0.11');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821195214_CorrigeDefaultTasaItbis'
)
BEGIN
    DECLARE @var nvarchar(max);
    SELECT @var = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[catalogo].[Productos]') AND [c].[name] = N'TasaItbis');
    IF @var IS NOT NULL EXEC(N'ALTER TABLE [catalogo].[Productos] DROP CONSTRAINT ' + @var + ';');
    ALTER TABLE [catalogo].[Productos] ADD DEFAULT 0.18 FOR [TasaItbis];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260821195214_CorrigeDefaultTasaItbis'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260821195214_CorrigeDefaultTasaItbis', N'10.0.11');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822024916_AgregaCodigoYCodigosBarraProducto'
)
BEGIN
    DROP INDEX [IX_Productos_CodigoBarra] ON [catalogo].[Productos];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822024916_AgregaCodigoYCodigosBarraProducto'
)
BEGIN
    ALTER TABLE [catalogo].[Productos] ADD [Codigo] nvarchar(50) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822024916_AgregaCodigoYCodigosBarraProducto'
)
BEGIN
    CREATE TABLE [catalogo].[CodigosBarraProducto] (
        [Id] uniqueidentifier NOT NULL,
        [ProductoId] uniqueidentifier NOT NULL,
        [CodigoBarra] nvarchar(50) NOT NULL,
        CONSTRAINT [PK_CodigosBarraProducto] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CodigosBarraProducto_Productos_ProductoId] FOREIGN KEY ([ProductoId]) REFERENCES [catalogo].[Productos] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822024916_AgregaCodigoYCodigosBarraProducto'
)
BEGIN
    INSERT INTO catalogo.CodigosBarraProducto (Id, ProductoId, CodigoBarra)
    SELECT NEWID(), Id, CodigoBarra
    FROM catalogo.Productos
    WHERE CodigoBarra IS NOT NULL AND LTRIM(RTRIM(CodigoBarra)) <> '';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822024916_AgregaCodigoYCodigosBarraProducto'
)
BEGIN
    DECLARE @var1 nvarchar(max);
    SELECT @var1 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[catalogo].[Productos]') AND [c].[name] = N'CodigoBarra');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [catalogo].[Productos] DROP CONSTRAINT ' + @var1 + ';');
    ALTER TABLE [catalogo].[Productos] DROP COLUMN [CodigoBarra];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822024916_AgregaCodigoYCodigosBarraProducto'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Productos_SucursalId_Codigo] ON [catalogo].[Productos] ([SucursalId], [Codigo]) WHERE [Codigo] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822024916_AgregaCodigoYCodigosBarraProducto'
)
BEGIN
    CREATE INDEX [IX_CodigosBarraProducto_CodigoBarra] ON [catalogo].[CodigosBarraProducto] ([CodigoBarra]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822024916_AgregaCodigoYCodigosBarraProducto'
)
BEGIN
    CREATE INDEX [IX_CodigosBarraProducto_ProductoId] ON [catalogo].[CodigosBarraProducto] ([ProductoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822024916_AgregaCodigoYCodigosBarraProducto'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260822024916_AgregaCodigoYCodigosBarraProducto', N'10.0.11');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822031643_AgregaNumeroFacturaInternoYCodigos'
)
BEGIN
    ALTER TABLE [sucursales].[Sucursales] ADD [Codigo] nvarchar(2) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822031643_AgregaNumeroFacturaInternoYCodigos'
)
BEGIN
    ALTER TABLE [facturacion].[Facturas] ADD [NumeroFactura] nvarchar(9) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822031643_AgregaNumeroFacturaInternoYCodigos'
)
BEGIN
    ALTER TABLE [caja].[Cajas] ADD [ProximoNumeroFactura] bigint NOT NULL DEFAULT CAST(1 AS bigint);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822031643_AgregaNumeroFacturaInternoYCodigos'
)
BEGIN

                    ;WITH SucCodigos AS (
                        SELECT Id, RIGHT('00' + CAST(ROW_NUMBER() OVER (ORDER BY Nombre, Id) AS VARCHAR(2)), 2) AS Codigo
                        FROM sucursales.Sucursales
                    )
                    UPDATE s SET s.Codigo = sc.Codigo
                    FROM sucursales.Sucursales s
                    JOIN SucCodigos sc ON sc.Id = s.Id;
                
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822031643_AgregaNumeroFacturaInternoYCodigos'
)
BEGIN

                    ;WITH FacturaCaja AS (
                        SELECT f.Id AS FacturaId, tc.CajaId,
                               ROW_NUMBER() OVER (PARTITION BY tc.CajaId ORDER BY f.FechaEmision, f.Id) AS Secuencia
                        FROM facturacion.Facturas f
                        JOIN caja.TurnosCaja tc ON tc.Id = f.CajaTurnoId
                    )
                    UPDATE f
                    SET f.NumeroFactura = s.Codigo + c.Numero + RIGHT('00000' + CAST(fc.Secuencia AS VARCHAR(5)), 5)
                    FROM facturacion.Facturas f
                    JOIN FacturaCaja fc ON fc.FacturaId = f.Id
                    JOIN caja.Cajas c ON c.Id = fc.CajaId
                    JOIN sucursales.Sucursales s ON s.Id = c.SucursalId;
                
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822031643_AgregaNumeroFacturaInternoYCodigos'
)
BEGIN

                    UPDATE c
                    SET c.ProximoNumeroFactura = ISNULL(mx.MaxSecuencia, 0) + 1
                    FROM caja.Cajas c
                    OUTER APPLY (
                        SELECT MAX(CAST(RIGHT(f.NumeroFactura, 5) AS BIGINT)) AS MaxSecuencia
                        FROM facturacion.Facturas f
                        JOIN caja.TurnosCaja tc ON tc.Id = f.CajaTurnoId
                        WHERE tc.CajaId = c.Id AND f.NumeroFactura IS NOT NULL
                    ) mx;
                
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822031643_AgregaNumeroFacturaInternoYCodigos'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Sucursales_Codigo] ON [sucursales].[Sucursales] ([Codigo]) WHERE [Codigo] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822031643_AgregaNumeroFacturaInternoYCodigos'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Facturas_NumeroFactura] ON [facturacion].[Facturas] ([NumeroFactura]) WHERE [NumeroFactura] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822031643_AgregaNumeroFacturaInternoYCodigos'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260822031643_AgregaNumeroFacturaInternoYCodigos', N'10.0.11');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822032928_AgregaSesionesActivas'
)
BEGIN
    CREATE TABLE [identidad].[SesionesActivas] (
        [Id] uniqueidentifier NOT NULL,
        [UsuarioId] uniqueidentifier NOT NULL,
        [SucursalId] uniqueidentifier NULL,
        [FechaInicio] datetime2 NOT NULL,
        [FechaUltimaActividad] datetime2 NOT NULL,
        [FechaCierre] datetime2 NULL,
        [IpOrigen] nvarchar(45) NULL,
        CONSTRAINT [PK_SesionesActivas] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_SesionesActivas_Usuarios_UsuarioId] FOREIGN KEY ([UsuarioId]) REFERENCES [identidad].[Usuarios] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822032928_AgregaSesionesActivas'
)
BEGIN
    CREATE INDEX [IX_UsuarioSucursales_SucursalId] ON [identidad].[UsuarioSucursales] ([SucursalId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822032928_AgregaSesionesActivas'
)
BEGIN
    CREATE INDEX [IX_SesionesActivas_FechaCierre] ON [identidad].[SesionesActivas] ([FechaCierre]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822032928_AgregaSesionesActivas'
)
BEGIN
    CREATE INDEX [IX_SesionesActivas_UsuarioId] ON [identidad].[SesionesActivas] ([UsuarioId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822032928_AgregaSesionesActivas'
)
BEGIN
    ALTER TABLE [identidad].[UsuarioSucursales] ADD CONSTRAINT [FK_UsuarioSucursales_Sucursales_SucursalId] FOREIGN KEY ([SucursalId]) REFERENCES [sucursales].[Sucursales] ([Id]) ON DELETE CASCADE;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822032928_AgregaSesionesActivas'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260822032928_AgregaSesionesActivas', N'10.0.11');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822034528_ObligaCodigoProductoCostoYClienteContado'
)
BEGIN
    DROP INDEX [IX_Productos_SucursalId_Codigo] ON [catalogo].[Productos];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822034528_ObligaCodigoProductoCostoYClienteContado'
)
BEGIN
    ALTER TABLE [clientes].[Clientes] ADD [EsGenerico] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822034528_ObligaCodigoProductoCostoYClienteContado'
)
BEGIN

                    UPDATE catalogo.Productos SET CostoUnitario = 0 WHERE CostoUnitario IS NULL;
                
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822034528_ObligaCodigoProductoCostoYClienteContado'
)
BEGIN

                    ;WITH SinCodigo AS (
                        SELECT Id, ROW_NUMBER() OVER (PARTITION BY SucursalId ORDER BY Id) AS Fila
                        FROM catalogo.Productos
                        WHERE Codigo IS NULL OR LTRIM(RTRIM(Codigo)) = ''
                    )
                    UPDATE p SET p.Codigo = 'GEN' + RIGHT('0000' + CAST(sc.Fila AS VARCHAR(4)), 4)
                    FROM catalogo.Productos p
                    JOIN SinCodigo sc ON sc.Id = p.Id;
                
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822034528_ObligaCodigoProductoCostoYClienteContado'
)
BEGIN

                    INSERT INTO clientes.Clientes (Id, SucursalId, NombreORazonSocial, TipoCliente, Activo, CreadoEn, EsGenerico)
                    SELECT NEWID(), s.SucursalId, 'Cliente Contado', 1, 1, SYSUTCDATETIME(), 1
                    FROM (SELECT DISTINCT SucursalId FROM facturacion.Facturas WHERE ClienteId IS NULL) s
                    WHERE NOT EXISTS (
                        SELECT 1 FROM clientes.Clientes c WHERE c.SucursalId = s.SucursalId AND c.EsGenerico = 1
                    );

                    UPDATE f
                    SET f.ClienteId = cc.Id
                    FROM facturacion.Facturas f
                    JOIN clientes.Clientes cc ON cc.SucursalId = f.SucursalId AND cc.EsGenerico = 1
                    WHERE f.ClienteId IS NULL;
                
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822034528_ObligaCodigoProductoCostoYClienteContado'
)
BEGIN
    DECLARE @var2 nvarchar(max);
    SELECT @var2 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[catalogo].[Productos]') AND [c].[name] = N'CostoUnitario');
    IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [catalogo].[Productos] DROP CONSTRAINT ' + @var2 + ';');
    EXEC(N'UPDATE [catalogo].[Productos] SET [CostoUnitario] = 0.0 WHERE [CostoUnitario] IS NULL');
    ALTER TABLE [catalogo].[Productos] ALTER COLUMN [CostoUnitario] decimal(18,2) NOT NULL;
    ALTER TABLE [catalogo].[Productos] ADD DEFAULT 0.0 FOR [CostoUnitario];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822034528_ObligaCodigoProductoCostoYClienteContado'
)
BEGIN
    DECLARE @var3 nvarchar(max);
    SELECT @var3 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[catalogo].[Productos]') AND [c].[name] = N'Codigo');
    IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [catalogo].[Productos] DROP CONSTRAINT ' + @var3 + ';');
    EXEC(N'UPDATE [catalogo].[Productos] SET [Codigo] = N'''' WHERE [Codigo] IS NULL');
    ALTER TABLE [catalogo].[Productos] ALTER COLUMN [Codigo] nvarchar(50) NOT NULL;
    ALTER TABLE [catalogo].[Productos] ADD DEFAULT N'' FOR [Codigo];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822034528_ObligaCodigoProductoCostoYClienteContado'
)
BEGIN
    DECLARE @var4 nvarchar(max);
    SELECT @var4 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[facturacion].[Facturas]') AND [c].[name] = N'ClienteId');
    IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [facturacion].[Facturas] DROP CONSTRAINT ' + @var4 + ';');
    EXEC(N'UPDATE [facturacion].[Facturas] SET [ClienteId] = ''00000000-0000-0000-0000-000000000000'' WHERE [ClienteId] IS NULL');
    ALTER TABLE [facturacion].[Facturas] ALTER COLUMN [ClienteId] uniqueidentifier NOT NULL;
    ALTER TABLE [facturacion].[Facturas] ADD DEFAULT '00000000-0000-0000-0000-000000000000' FOR [ClienteId];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822034528_ObligaCodigoProductoCostoYClienteContado'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Productos_SucursalId_Codigo] ON [catalogo].[Productos] ([SucursalId], [Codigo]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822034528_ObligaCodigoProductoCostoYClienteContado'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260822034528_ObligaCodigoProductoCostoYClienteContado', N'10.0.11');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822035922_AgregaDetalleNotaCreditoYMotivos'
)
BEGIN
    ALTER TABLE [sucursales].[Sucursales] ADD [ProximoNumeroNota] bigint NOT NULL DEFAULT CAST(1 AS bigint);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822035922_AgregaDetalleNotaCreditoYMotivos'
)
BEGIN
    ALTER TABLE [facturacion].[NotasCreditoDebito] ADD [MotivoId] uniqueidentifier NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822035922_AgregaDetalleNotaCreditoYMotivos'
)
BEGIN
    ALTER TABLE [facturacion].[NotasCreditoDebito] ADD [NumeroNota] nvarchar(9) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822035922_AgregaDetalleNotaCreditoYMotivos'
)
BEGIN
    ALTER TABLE [facturacion].[FacturaDetalles] ADD [CantidadAcreditada] decimal(18,3) NOT NULL DEFAULT 0.0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822035922_AgregaDetalleNotaCreditoYMotivos'
)
BEGIN
    ALTER TABLE [facturacion].[FacturaDetalles] ADD [Codigo] nvarchar(50) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822035922_AgregaDetalleNotaCreditoYMotivos'
)
BEGIN
    ALTER TABLE [facturacion].[FacturaDetalles] ADD [UnidadMedida] nvarchar(50) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822035922_AgregaDetalleNotaCreditoYMotivos'
)
BEGIN
    CREATE TABLE [facturacion].[MotivosNotaCredito] (
        [Id] uniqueidentifier NOT NULL,
        [Nombre] nvarchar(200) NOT NULL,
        [Activo] bit NOT NULL,
        CONSTRAINT [PK_MotivosNotaCredito] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822035922_AgregaDetalleNotaCreditoYMotivos'
)
BEGIN
    CREATE TABLE [facturacion].[NotasCreditoDebitoDetalle] (
        [Id] uniqueidentifier NOT NULL,
        [NotaCreditoDebitoId] uniqueidentifier NOT NULL,
        [FacturaDetalleId] uniqueidentifier NOT NULL,
        [Cantidad] decimal(18,3) NOT NULL,
        [Monto] decimal(18,2) NOT NULL,
        CONSTRAINT [PK_NotasCreditoDebitoDetalle] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_NotasCreditoDebitoDetalle_FacturaDetalles_FacturaDetalleId] FOREIGN KEY ([FacturaDetalleId]) REFERENCES [facturacion].[FacturaDetalles] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_NotasCreditoDebitoDetalle_NotasCreditoDebito_NotaCreditoDebitoId] FOREIGN KEY ([NotaCreditoDebitoId]) REFERENCES [facturacion].[NotasCreditoDebito] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822035922_AgregaDetalleNotaCreditoYMotivos'
)
BEGIN
    CREATE INDEX [IX_NotasCreditoDebito_MotivoId] ON [facturacion].[NotasCreditoDebito] ([MotivoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822035922_AgregaDetalleNotaCreditoYMotivos'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_NotasCreditoDebito_NumeroNota] ON [facturacion].[NotasCreditoDebito] ([NumeroNota]) WHERE [NumeroNota] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822035922_AgregaDetalleNotaCreditoYMotivos'
)
BEGIN
    CREATE UNIQUE INDEX [IX_MotivosNotaCredito_Nombre] ON [facturacion].[MotivosNotaCredito] ([Nombre]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822035922_AgregaDetalleNotaCreditoYMotivos'
)
BEGIN
    CREATE INDEX [IX_NotasCreditoDebitoDetalle_FacturaDetalleId] ON [facturacion].[NotasCreditoDebitoDetalle] ([FacturaDetalleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822035922_AgregaDetalleNotaCreditoYMotivos'
)
BEGIN
    CREATE INDEX [IX_NotasCreditoDebitoDetalle_NotaCreditoDebitoId] ON [facturacion].[NotasCreditoDebitoDetalle] ([NotaCreditoDebitoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822035922_AgregaDetalleNotaCreditoYMotivos'
)
BEGIN
    ALTER TABLE [facturacion].[NotasCreditoDebito] ADD CONSTRAINT [FK_NotasCreditoDebito_MotivosNotaCredito_MotivoId] FOREIGN KEY ([MotivoId]) REFERENCES [facturacion].[MotivosNotaCredito] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822035922_AgregaDetalleNotaCreditoYMotivos'
)
BEGIN

                    UPDATE fd
                    SET fd.Codigo = p.Codigo, fd.UnidadMedida = p.UnidadMedida
                    FROM facturacion.FacturaDetalles fd
                    JOIN catalogo.Productos p ON p.Id = fd.ProductoId
                    WHERE fd.UnidadMedida = '' OR fd.UnidadMedida IS NULL;

                    UPDATE facturacion.FacturaDetalles SET UnidadMedida = 'Unidad' WHERE UnidadMedida = '' OR UnidadMedida IS NULL;
                
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822035922_AgregaDetalleNotaCreditoYMotivos'
)
BEGIN

                    INSERT INTO facturacion.MotivosNotaCredito (Id, Nombre, Activo) VALUES
                        (NEWID(), 'Producto dañado o defectuoso', 1),
                        (NEWID(), 'Error de cobro', 1),
                        (NEWID(), 'Cliente insatisfecho', 1),
                        (NEWID(), 'Devolución de mercancía', 1),
                        (NEWID(), 'Otro', 1);
                
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822035922_AgregaDetalleNotaCreditoYMotivos'
)
BEGIN

                    ;WITH NotaConTipo AS (
                        SELECT n.Id, n.SucursalId, n.Tipo,
                               CASE WHEN n.Tipo = 0 THEN '34' ELSE '33' END AS TipoInterno,
                               ROW_NUMBER() OVER (PARTITION BY n.SucursalId, n.Tipo ORDER BY n.FechaEmision, n.Id) AS Secuencia
                        FROM facturacion.NotasCreditoDebito n
                        WHERE n.NumeroNota IS NULL
                    )
                    UPDATE n
                    SET n.NumeroNota = s.Codigo + nct.TipoInterno + RIGHT('00000' + CAST(nct.Secuencia AS VARCHAR(5)), 5),
                        n.MotivoId = ISNULL(
                            (SELECT TOP 1 m.Id FROM facturacion.MotivosNotaCredito m WHERE m.Nombre = 'Otro'),
                            NULL)
                    FROM facturacion.NotasCreditoDebito n
                    JOIN NotaConTipo nct ON nct.Id = n.Id
                    JOIN sucursales.Sucursales s ON s.Id = n.SucursalId;

                    UPDATE s
                    SET s.ProximoNumeroNota = ISNULL(mx.MaxSecuencia, 0) + 1
                    FROM sucursales.Sucursales s
                    OUTER APPLY (
                        SELECT MAX(CAST(RIGHT(n.NumeroNota, 5) AS BIGINT)) AS MaxSecuencia
                        FROM facturacion.NotasCreditoDebito n
                        WHERE n.SucursalId = s.Id AND n.NumeroNota IS NOT NULL
                    ) mx;
                
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822035922_AgregaDetalleNotaCreditoYMotivos'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260822035922_AgregaDetalleNotaCreditoYMotivos', N'10.0.11');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822041802_AgregaSnapshotsPagosYCamposObligatorios'
)
BEGIN
    ALTER TABLE [caja].[TurnosCaja] ADD [CodigoCaja] nvarchar(50) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822041802_AgregaSnapshotsPagosYCamposObligatorios'
)
BEGIN
    ALTER TABLE [caja].[TurnosCaja] ADD [CodigoSucursal] nvarchar(2) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822041802_AgregaSnapshotsPagosYCamposObligatorios'
)
BEGIN

                    DECLARE @AdminId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM identidad.Usuarios WHERE NombreUsuario = 'admin');
                    UPDATE catalogo.Productos SET CreadoPorUsuarioId = @AdminId WHERE CreadoPorUsuarioId = '00000000-0000-0000-0000-000000000000';
                    UPDATE catalogo.Categorias SET CreadoPorUsuarioId = @AdminId WHERE CreadoPorUsuarioId = '00000000-0000-0000-0000-000000000000';
                
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822041802_AgregaSnapshotsPagosYCamposObligatorios'
)
BEGIN

                    DECLARE @AdminId2 UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM identidad.Usuarios WHERE NombreUsuario = 'admin');

                    INSERT INTO catalogo.Categorias (Id, SucursalId, Nombre, Orden, CreadoEn, CreadoPorUsuarioId)
                    SELECT NEWID(), s.SucursalId, 'General', 0, SYSUTCDATETIME(), @AdminId2
                    FROM (SELECT DISTINCT SucursalId FROM catalogo.Productos WHERE CategoriaId = '00000000-0000-0000-0000-000000000000') s
                    WHERE NOT EXISTS (SELECT 1 FROM catalogo.Categorias c WHERE c.SucursalId = s.SucursalId);

                    ;WITH CategoriaPorDefecto AS (
                        SELECT SucursalId, MIN(Id) AS CategoriaId
                        FROM catalogo.Categorias
                        GROUP BY SucursalId
                    )
                    UPDATE p
                    SET p.CategoriaId = cpd.CategoriaId
                    FROM catalogo.Productos p
                    JOIN CategoriaPorDefecto cpd ON cpd.SucursalId = p.SucursalId
                    WHERE p.CategoriaId = '00000000-0000-0000-0000-000000000000';
                
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822041802_AgregaSnapshotsPagosYCamposObligatorios'
)
BEGIN
    DECLARE @var5 nvarchar(max);
    SELECT @var5 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[catalogo].[Productos]') AND [c].[name] = N'CreadoPorUsuarioId');
    IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [catalogo].[Productos] DROP CONSTRAINT ' + @var5 + ';');
    EXEC(N'UPDATE [catalogo].[Productos] SET [CreadoPorUsuarioId] = ''00000000-0000-0000-0000-000000000000'' WHERE [CreadoPorUsuarioId] IS NULL');
    ALTER TABLE [catalogo].[Productos] ALTER COLUMN [CreadoPorUsuarioId] uniqueidentifier NOT NULL;
    ALTER TABLE [catalogo].[Productos] ADD DEFAULT '00000000-0000-0000-0000-000000000000' FOR [CreadoPorUsuarioId];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822041802_AgregaSnapshotsPagosYCamposObligatorios'
)
BEGIN
    DECLARE @var6 nvarchar(max);
    SELECT @var6 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[catalogo].[Productos]') AND [c].[name] = N'CategoriaId');
    IF @var6 IS NOT NULL EXEC(N'ALTER TABLE [catalogo].[Productos] DROP CONSTRAINT ' + @var6 + ';');
    EXEC(N'UPDATE [catalogo].[Productos] SET [CategoriaId] = ''00000000-0000-0000-0000-000000000000'' WHERE [CategoriaId] IS NULL');
    ALTER TABLE [catalogo].[Productos] ALTER COLUMN [CategoriaId] uniqueidentifier NOT NULL;
    ALTER TABLE [catalogo].[Productos] ADD DEFAULT '00000000-0000-0000-0000-000000000000' FOR [CategoriaId];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822041802_AgregaSnapshotsPagosYCamposObligatorios'
)
BEGIN
    ALTER TABLE [facturacion].[Facturas] ADD [CajaCodigo] nvarchar(50) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822041802_AgregaSnapshotsPagosYCamposObligatorios'
)
BEGIN
    ALTER TABLE [facturacion].[Facturas] ADD [ClienteNombre] nvarchar(200) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822041802_AgregaSnapshotsPagosYCamposObligatorios'
)
BEGIN
    ALTER TABLE [facturacion].[Facturas] ADD [ClienteRncOCedula] nvarchar(20) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822041802_AgregaSnapshotsPagosYCamposObligatorios'
)
BEGIN
    ALTER TABLE [facturacion].[Facturas] ADD [SucursalCodigo] nvarchar(2) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822041802_AgregaSnapshotsPagosYCamposObligatorios'
)
BEGIN
    DECLARE @var7 nvarchar(max);
    SELECT @var7 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[facturacion].[FacturaDetalles]') AND [c].[name] = N'TasaItbis');
    IF @var7 IS NOT NULL EXEC(N'ALTER TABLE [facturacion].[FacturaDetalles] DROP CONSTRAINT ' + @var7 + ';');
    EXEC(N'UPDATE [facturacion].[FacturaDetalles] SET [TasaItbis] = 0.0 WHERE [TasaItbis] IS NULL');
    ALTER TABLE [facturacion].[FacturaDetalles] ALTER COLUMN [TasaItbis] decimal(5,4) NOT NULL;
    ALTER TABLE [facturacion].[FacturaDetalles] ADD DEFAULT 0.0 FOR [TasaItbis];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822041802_AgregaSnapshotsPagosYCamposObligatorios'
)
BEGIN
    DECLARE @var8 nvarchar(max);
    SELECT @var8 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[facturacion].[FacturaDetalles]') AND [c].[name] = N'Codigo');
    IF @var8 IS NOT NULL EXEC(N'ALTER TABLE [facturacion].[FacturaDetalles] DROP CONSTRAINT ' + @var8 + ';');
    EXEC(N'UPDATE [facturacion].[FacturaDetalles] SET [Codigo] = N'''' WHERE [Codigo] IS NULL');
    ALTER TABLE [facturacion].[FacturaDetalles] ALTER COLUMN [Codigo] nvarchar(50) NOT NULL;
    ALTER TABLE [facturacion].[FacturaDetalles] ADD DEFAULT N'' FOR [Codigo];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822041802_AgregaSnapshotsPagosYCamposObligatorios'
)
BEGIN
    ALTER TABLE [facturacion].[FacturaDetalles] ADD [AplicaItbis] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822041802_AgregaSnapshotsPagosYCamposObligatorios'
)
BEGIN

                    UPDATE facturacion.FacturaDetalles SET AplicaItbis = 1 WHERE Itbis > 0 OR TasaItbis > 0;
                
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822041802_AgregaSnapshotsPagosYCamposObligatorios'
)
BEGIN
    ALTER TABLE [caja].[DenominacionesCierre] ADD [CodigoCaja] nvarchar(50) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822041802_AgregaSnapshotsPagosYCamposObligatorios'
)
BEGIN
    ALTER TABLE [caja].[DenominacionesCierre] ADD [CodigoSucursal] nvarchar(2) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822041802_AgregaSnapshotsPagosYCamposObligatorios'
)
BEGIN
    DECLARE @var9 nvarchar(max);
    SELECT @var9 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[catalogo].[Categorias]') AND [c].[name] = N'CreadoPorUsuarioId');
    IF @var9 IS NOT NULL EXEC(N'ALTER TABLE [catalogo].[Categorias] DROP CONSTRAINT ' + @var9 + ';');
    EXEC(N'UPDATE [catalogo].[Categorias] SET [CreadoPorUsuarioId] = ''00000000-0000-0000-0000-000000000000'' WHERE [CreadoPorUsuarioId] IS NULL');
    ALTER TABLE [catalogo].[Categorias] ALTER COLUMN [CreadoPorUsuarioId] uniqueidentifier NOT NULL;
    ALTER TABLE [catalogo].[Categorias] ADD DEFAULT '00000000-0000-0000-0000-000000000000' FOR [CreadoPorUsuarioId];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822041802_AgregaSnapshotsPagosYCamposObligatorios'
)
BEGIN
    ALTER TABLE [caja].[Cajas] ADD [CodigoSucursal] nvarchar(2) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822041802_AgregaSnapshotsPagosYCamposObligatorios'
)
BEGIN
    CREATE TABLE [facturacion].[FacturaPagos] (
        [Id] uniqueidentifier NOT NULL,
        [FacturaId] uniqueidentifier NOT NULL,
        [FormaPago] int NOT NULL,
        [Monto] decimal(18,2) NOT NULL,
        [NumeroComprobante] nvarchar(50) NULL,
        CONSTRAINT [PK_FacturaPagos] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_FacturaPagos_Facturas_FacturaId] FOREIGN KEY ([FacturaId]) REFERENCES [facturacion].[Facturas] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822041802_AgregaSnapshotsPagosYCamposObligatorios'
)
BEGIN
    CREATE INDEX [IX_FacturaPagos_FacturaId] ON [facturacion].[FacturaPagos] ([FacturaId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822041802_AgregaSnapshotsPagosYCamposObligatorios'
)
BEGIN

                    UPDATE c
                    SET c.CodigoSucursal = s.Codigo
                    FROM caja.Cajas c
                    JOIN sucursales.Sucursales s ON s.Id = c.SucursalId;

                    UPDATE tc
                    SET tc.CodigoSucursal = c.CodigoSucursal, tc.CodigoCaja = c.Numero
                    FROM caja.TurnosCaja tc
                    JOIN caja.Cajas c ON c.Id = tc.CajaId;

                    UPDATE dc
                    SET dc.CodigoSucursal = tc.CodigoSucursal, dc.CodigoCaja = tc.CodigoCaja
                    FROM caja.DenominacionesCierre dc
                    JOIN caja.TurnosCaja tc ON tc.Id = dc.TurnoCajaId;

                    UPDATE f
                    SET f.SucursalCodigo = tc.CodigoSucursal, f.CajaCodigo = tc.CodigoCaja
                    FROM facturacion.Facturas f
                    JOIN caja.TurnosCaja tc ON tc.Id = f.CajaTurnoId;

                    UPDATE f
                    SET f.ClienteNombre = cl.NombreORazonSocial, f.ClienteRncOCedula = cl.RncOCedula
                    FROM facturacion.Facturas f
                    JOIN clientes.Clientes cl ON cl.Id = f.ClienteId;
                
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822041802_AgregaSnapshotsPagosYCamposObligatorios'
)
BEGIN

                    INSERT INTO facturacion.FacturaPagos (Id, FacturaId, FormaPago, Monto, NumeroComprobante)
                    SELECT NEWID(), m.FacturaId, m.FormaPago, m.Monto, NULL
                    FROM caja.MovimientosCaja m
                    WHERE m.Tipo = 0 AND m.FacturaId IS NOT NULL;
                
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822041802_AgregaSnapshotsPagosYCamposObligatorios'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260822041802_AgregaSnapshotsPagosYCamposObligatorios', N'10.0.11');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822140611_AgregaEmpresa'
)
BEGIN
    ALTER TABLE [sucursales].[Sucursales] ADD [EmpresaId] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822140611_AgregaEmpresa'
)
BEGIN
    CREATE TABLE [sucursales].[Empresas] (
        [Id] uniqueidentifier NOT NULL,
        [Nombre] nvarchar(200) NOT NULL,
        [Rnc] nvarchar(20) NULL,
        [Activa] bit NOT NULL,
        [CreadoEn] datetime2 NOT NULL,
        CONSTRAINT [PK_Empresas] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822140611_AgregaEmpresa'
)
BEGIN

                    CREATE TABLE #MapaEmpresaSucursal (SucursalId UNIQUEIDENTIFIER, EmpresaId UNIQUEIDENTIFIER);

                    DECLARE @SucursalId UNIQUEIDENTIFIER, @NuevaEmpresaId UNIQUEIDENTIFIER, @Nombre NVARCHAR(200), @Rnc NVARCHAR(20);

                    DECLARE cur CURSOR LOCAL FAST_FORWARD FOR
                        SELECT Id, Nombre, Rnc FROM sucursales.Sucursales WHERE EmpresaId = '00000000-0000-0000-0000-000000000000';

                    OPEN cur;
                    FETCH NEXT FROM cur INTO @SucursalId, @Nombre, @Rnc;
                    WHILE @@FETCH_STATUS = 0
                    BEGIN
                        SET @NuevaEmpresaId = NEWID();
                        INSERT INTO sucursales.Empresas (Id, Nombre, Rnc, Activa, CreadoEn)
                        VALUES (@NuevaEmpresaId, @Nombre, @Rnc, 1, SYSUTCDATETIME());

                        INSERT INTO #MapaEmpresaSucursal (SucursalId, EmpresaId) VALUES (@SucursalId, @NuevaEmpresaId);

                        FETCH NEXT FROM cur INTO @SucursalId, @Nombre, @Rnc;
                    END
                    CLOSE cur;
                    DEALLOCATE cur;

                    UPDATE s
                    SET s.EmpresaId = m.EmpresaId
                    FROM sucursales.Sucursales s
                    JOIN #MapaEmpresaSucursal m ON m.SucursalId = s.Id;

                    DROP TABLE #MapaEmpresaSucursal;
                
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822140611_AgregaEmpresa'
)
BEGIN
    CREATE INDEX [IX_Sucursales_EmpresaId] ON [sucursales].[Sucursales] ([EmpresaId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822140611_AgregaEmpresa'
)
BEGIN
    ALTER TABLE [sucursales].[Sucursales] ADD CONSTRAINT [FK_Sucursales_Empresas_EmpresaId] FOREIGN KEY ([EmpresaId]) REFERENCES [sucursales].[Empresas] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822140611_AgregaEmpresa'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260822140611_AgregaEmpresa', N'10.0.11');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822142045_AgregaMetodoPago'
)
BEGIN
    CREATE TABLE [catalogo].[MetodosPago] (
        [Id] uniqueidentifier NOT NULL,
        [Nombre] nvarchar(100) NOT NULL,
        [EsEfectivo] bit NOT NULL,
        [RequiereComprobante] bit NOT NULL,
        [Activo] bit NOT NULL,
        CONSTRAINT [PK_MetodosPago] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822142045_AgregaMetodoPago'
)
BEGIN

                    INSERT INTO catalogo.MetodosPago (Id, Nombre, EsEfectivo, RequiereComprobante, Activo) VALUES
                    ('9c3b1a10-0001-4a00-8000-000000000001', 'Efectivo', 1, 0, 1),
                    ('9c3b1a10-0001-4a00-8000-000000000002', 'Tarjeta', 0, 1, 1),
                    ('9c3b1a10-0001-4a00-8000-000000000003', 'Transferencia', 0, 0, 1),
                    ('9c3b1a10-0001-4a00-8000-000000000004', 'Deposito', 0, 0, 1);
                
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822142045_AgregaMetodoPago'
)
BEGIN
    ALTER TABLE [cxccxp].[PagosCxP] ADD [MetodoPagoId] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822142045_AgregaMetodoPago'
)
BEGIN
    ALTER TABLE [cxccxp].[PagosCxC] ADD [MetodoPagoId] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822142045_AgregaMetodoPago'
)
BEGIN
    ALTER TABLE [caja].[MovimientosCaja] ADD [MetodoPagoId] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822142045_AgregaMetodoPago'
)
BEGIN
    ALTER TABLE [facturacion].[FacturaPagos] ADD [MetodoPagoId] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822142045_AgregaMetodoPago'
)
BEGIN
    ALTER TABLE [caja].[DenominacionesCierre] ADD [MetodoPagoId] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822142045_AgregaMetodoPago'
)
BEGIN

                    UPDATE caja.MovimientosCaja SET MetodoPagoId = CASE FormaPago
                        WHEN 0 THEN '9c3b1a10-0001-4a00-8000-000000000001'
                        WHEN 1 THEN '9c3b1a10-0001-4a00-8000-000000000002'
                        WHEN 2 THEN '9c3b1a10-0001-4a00-8000-000000000003'
                        WHEN 3 THEN '9c3b1a10-0001-4a00-8000-000000000004'
                        ELSE '9c3b1a10-0001-4a00-8000-000000000001'
                    END;

                    UPDATE facturacion.FacturaPagos SET MetodoPagoId = CASE FormaPago
                        WHEN 0 THEN '9c3b1a10-0001-4a00-8000-000000000001'
                        WHEN 1 THEN '9c3b1a10-0001-4a00-8000-000000000002'
                        WHEN 2 THEN '9c3b1a10-0001-4a00-8000-000000000003'
                        WHEN 3 THEN '9c3b1a10-0001-4a00-8000-000000000004'
                        ELSE '9c3b1a10-0001-4a00-8000-000000000001'
                    END;

                    UPDATE caja.DenominacionesCierre SET MetodoPagoId = CASE FormaPago
                        WHEN 0 THEN '9c3b1a10-0001-4a00-8000-000000000001'
                        WHEN 1 THEN '9c3b1a10-0001-4a00-8000-000000000002'
                        WHEN 2 THEN '9c3b1a10-0001-4a00-8000-000000000003'
                        WHEN 3 THEN '9c3b1a10-0001-4a00-8000-000000000004'
                        ELSE '9c3b1a10-0001-4a00-8000-000000000001'
                    END;
                
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822142045_AgregaMetodoPago'
)
BEGIN

                    UPDATE p SET p.MetodoPagoId = ISNULL(m.Id, '9c3b1a10-0001-4a00-8000-000000000001')
                    FROM cxccxp.PagosCxC p
                    LEFT JOIN catalogo.MetodosPago m ON m.Nombre = p.FormaPago;

                    UPDATE p SET p.MetodoPagoId = ISNULL(m.Id, '9c3b1a10-0001-4a00-8000-000000000001')
                    FROM cxccxp.PagosCxP p
                    LEFT JOIN catalogo.MetodosPago m ON m.Nombre = p.FormaPago;
                
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822142045_AgregaMetodoPago'
)
BEGIN
    DECLARE @var10 nvarchar(max);
    SELECT @var10 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[cxccxp].[PagosCxP]') AND [c].[name] = N'FormaPago');
    IF @var10 IS NOT NULL EXEC(N'ALTER TABLE [cxccxp].[PagosCxP] DROP CONSTRAINT ' + @var10 + ';');
    ALTER TABLE [cxccxp].[PagosCxP] DROP COLUMN [FormaPago];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822142045_AgregaMetodoPago'
)
BEGIN
    DECLARE @var11 nvarchar(max);
    SELECT @var11 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[cxccxp].[PagosCxC]') AND [c].[name] = N'FormaPago');
    IF @var11 IS NOT NULL EXEC(N'ALTER TABLE [cxccxp].[PagosCxC] DROP CONSTRAINT ' + @var11 + ';');
    ALTER TABLE [cxccxp].[PagosCxC] DROP COLUMN [FormaPago];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822142045_AgregaMetodoPago'
)
BEGIN
    DECLARE @var12 nvarchar(max);
    SELECT @var12 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[caja].[MovimientosCaja]') AND [c].[name] = N'FormaPago');
    IF @var12 IS NOT NULL EXEC(N'ALTER TABLE [caja].[MovimientosCaja] DROP CONSTRAINT ' + @var12 + ';');
    ALTER TABLE [caja].[MovimientosCaja] DROP COLUMN [FormaPago];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822142045_AgregaMetodoPago'
)
BEGIN
    DECLARE @var13 nvarchar(max);
    SELECT @var13 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[facturacion].[FacturaPagos]') AND [c].[name] = N'FormaPago');
    IF @var13 IS NOT NULL EXEC(N'ALTER TABLE [facturacion].[FacturaPagos] DROP CONSTRAINT ' + @var13 + ';');
    ALTER TABLE [facturacion].[FacturaPagos] DROP COLUMN [FormaPago];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822142045_AgregaMetodoPago'
)
BEGIN
    DECLARE @var14 nvarchar(max);
    SELECT @var14 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[caja].[DenominacionesCierre]') AND [c].[name] = N'FormaPago');
    IF @var14 IS NOT NULL EXEC(N'ALTER TABLE [caja].[DenominacionesCierre] DROP CONSTRAINT ' + @var14 + ';');
    ALTER TABLE [caja].[DenominacionesCierre] DROP COLUMN [FormaPago];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822142045_AgregaMetodoPago'
)
BEGIN
    CREATE INDEX [IX_PagosCxP_MetodoPagoId] ON [cxccxp].[PagosCxP] ([MetodoPagoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822142045_AgregaMetodoPago'
)
BEGIN
    CREATE INDEX [IX_PagosCxC_MetodoPagoId] ON [cxccxp].[PagosCxC] ([MetodoPagoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822142045_AgregaMetodoPago'
)
BEGIN
    CREATE INDEX [IX_MovimientosCaja_MetodoPagoId] ON [caja].[MovimientosCaja] ([MetodoPagoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822142045_AgregaMetodoPago'
)
BEGIN
    CREATE INDEX [IX_FacturaPagos_MetodoPagoId] ON [facturacion].[FacturaPagos] ([MetodoPagoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822142045_AgregaMetodoPago'
)
BEGIN
    CREATE INDEX [IX_DenominacionesCierre_MetodoPagoId] ON [caja].[DenominacionesCierre] ([MetodoPagoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822142045_AgregaMetodoPago'
)
BEGIN
    CREATE UNIQUE INDEX [IX_MetodosPago_Nombre] ON [catalogo].[MetodosPago] ([Nombre]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822142045_AgregaMetodoPago'
)
BEGIN
    ALTER TABLE [caja].[DenominacionesCierre] ADD CONSTRAINT [FK_DenominacionesCierre_MetodosPago_MetodoPagoId] FOREIGN KEY ([MetodoPagoId]) REFERENCES [catalogo].[MetodosPago] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822142045_AgregaMetodoPago'
)
BEGIN
    ALTER TABLE [facturacion].[FacturaPagos] ADD CONSTRAINT [FK_FacturaPagos_MetodosPago_MetodoPagoId] FOREIGN KEY ([MetodoPagoId]) REFERENCES [catalogo].[MetodosPago] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822142045_AgregaMetodoPago'
)
BEGIN
    ALTER TABLE [caja].[MovimientosCaja] ADD CONSTRAINT [FK_MovimientosCaja_MetodosPago_MetodoPagoId] FOREIGN KEY ([MetodoPagoId]) REFERENCES [catalogo].[MetodosPago] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822142045_AgregaMetodoPago'
)
BEGIN
    ALTER TABLE [cxccxp].[PagosCxC] ADD CONSTRAINT [FK_PagosCxC_MetodosPago_MetodoPagoId] FOREIGN KEY ([MetodoPagoId]) REFERENCES [catalogo].[MetodosPago] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822142045_AgregaMetodoPago'
)
BEGIN
    ALTER TABLE [cxccxp].[PagosCxP] ADD CONSTRAINT [FK_PagosCxP_MetodosPago_MetodoPagoId] FOREIGN KEY ([MetodoPagoId]) REFERENCES [catalogo].[MetodosPago] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822142045_AgregaMetodoPago'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260822142045_AgregaMetodoPago', N'10.0.11');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822151717_EmpresaSingleton'
)
BEGIN
    DECLARE @var15 nvarchar(max);
    SELECT @var15 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[sucursales].[Empresas]') AND [c].[name] = N'Activa');
    IF @var15 IS NOT NULL EXEC(N'ALTER TABLE [sucursales].[Empresas] DROP CONSTRAINT ' + @var15 + ';');
    ALTER TABLE [sucursales].[Empresas] DROP COLUMN [Activa];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822151717_EmpresaSingleton'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260822151717_EmpresaSingleton', N'10.0.11');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822182309_EliminaAplicaItbis'
)
BEGIN
    DECLARE @var16 nvarchar(max);
    SELECT @var16 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[catalogo].[Productos]') AND [c].[name] = N'AplicaItbis');
    IF @var16 IS NOT NULL EXEC(N'ALTER TABLE [catalogo].[Productos] DROP CONSTRAINT ' + @var16 + ';');
    ALTER TABLE [catalogo].[Productos] DROP COLUMN [AplicaItbis];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822182309_EliminaAplicaItbis'
)
BEGIN
    DECLARE @var17 nvarchar(max);
    SELECT @var17 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[facturacion].[FacturaDetalles]') AND [c].[name] = N'AplicaItbis');
    IF @var17 IS NOT NULL EXEC(N'ALTER TABLE [facturacion].[FacturaDetalles] DROP CONSTRAINT ' + @var17 + ';');
    ALTER TABLE [facturacion].[FacturaDetalles] DROP COLUMN [AplicaItbis];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822182309_EliminaAplicaItbis'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260822182309_EliminaAplicaItbis', N'10.0.11');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822183431_AgregaUnidadMedida'
)
BEGIN
    CREATE TABLE [catalogo].[UnidadesMedida] (
        [Id] uniqueidentifier NOT NULL,
        [Nombre] nvarchar(50) NOT NULL,
        [Activo] bit NOT NULL,
        CONSTRAINT [PK_UnidadesMedida] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822183431_AgregaUnidadMedida'
)
BEGIN

                    INSERT INTO catalogo.UnidadesMedida (Id, Nombre, Activo)
                    SELECT NEWID(), valores.Nombre, 1
                    FROM (SELECT DISTINCT UnidadMedida AS Nombre FROM catalogo.Productos
                          WHERE UnidadMedida IS NOT NULL AND LTRIM(RTRIM(UnidadMedida)) <> '') AS valores
                    WHERE NOT EXISTS (SELECT 1 FROM catalogo.UnidadesMedida um WHERE um.Nombre = valores.Nombre);

                    INSERT INTO catalogo.UnidadesMedida (Id, Nombre, Activo)
                    SELECT NEWID(), comunes.Nombre, 1
                    FROM (VALUES ('Unidad'), ('Libra'), ('Onza'), ('Kg'), ('Gramo'), ('Litro'), ('Galon'), ('Docena')) AS comunes(Nombre)
                    WHERE NOT EXISTS (SELECT 1 FROM catalogo.UnidadesMedida um WHERE um.Nombre = comunes.Nombre);
                
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822183431_AgregaUnidadMedida'
)
BEGIN
    ALTER TABLE [catalogo].[Productos] ADD [UnidadMedidaId] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822183431_AgregaUnidadMedida'
)
BEGIN

                    UPDATE p SET p.UnidadMedidaId = um.Id
                    FROM catalogo.Productos p
                    JOIN catalogo.UnidadesMedida um ON um.Nombre = p.UnidadMedida;

                    UPDATE p SET p.UnidadMedidaId = (SELECT TOP 1 Id FROM catalogo.UnidadesMedida WHERE Nombre = 'Unidad')
                    FROM catalogo.Productos p
                    WHERE p.UnidadMedidaId = '00000000-0000-0000-0000-000000000000';
                
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822183431_AgregaUnidadMedida'
)
BEGIN
    DECLARE @var18 nvarchar(max);
    SELECT @var18 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[catalogo].[Productos]') AND [c].[name] = N'UnidadMedida');
    IF @var18 IS NOT NULL EXEC(N'ALTER TABLE [catalogo].[Productos] DROP CONSTRAINT ' + @var18 + ';');
    ALTER TABLE [catalogo].[Productos] DROP COLUMN [UnidadMedida];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822183431_AgregaUnidadMedida'
)
BEGIN
    CREATE INDEX [IX_Productos_UnidadMedidaId] ON [catalogo].[Productos] ([UnidadMedidaId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822183431_AgregaUnidadMedida'
)
BEGIN
    CREATE UNIQUE INDEX [IX_UnidadesMedida_Nombre] ON [catalogo].[UnidadesMedida] ([Nombre]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822183431_AgregaUnidadMedida'
)
BEGIN
    ALTER TABLE [catalogo].[Productos] ADD CONSTRAINT [FK_Productos_UnidadesMedida_UnidadMedidaId] FOREIGN KEY ([UnidadMedidaId]) REFERENCES [catalogo].[UnidadesMedida] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822183431_AgregaUnidadMedida'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260822183431_AgregaUnidadMedida', N'10.0.11');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822184536_AgregaActivoACategoria'
)
BEGIN
    ALTER TABLE [catalogo].[Categorias] ADD [Activo] bit NOT NULL DEFAULT CAST(1 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822184536_AgregaActivoACategoria'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260822184536_AgregaActivoACategoria', N'10.0.11');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822194450_AgregaValidacionCliente'
)
BEGIN
    DROP INDEX [IX_Clientes_SucursalId_RncOCedula] ON [clientes].[Clientes];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822194450_AgregaValidacionCliente'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Clientes_SucursalId_RncOCedula] ON [clientes].[Clientes] ([SucursalId], [RncOCedula]) WHERE [RncOCedula] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822194450_AgregaValidacionCliente'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260822194450_AgregaValidacionCliente', N'10.0.11');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822202609_ProductoGlobalStockPorSucursal'
)
BEGIN
    CREATE TABLE [catalogo].[StockSucursal] (
        [Id] uniqueidentifier NOT NULL,
        [ProductoId] uniqueidentifier NOT NULL,
        [SucursalId] uniqueidentifier NOT NULL,
        [StockActual] decimal(18,3) NOT NULL,
        [StockMinimo] decimal(18,3) NULL,
        [StockMaximo] decimal(18,3) NULL,
        CONSTRAINT [PK_StockSucursal] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_StockSucursal_Productos_ProductoId] FOREIGN KEY ([ProductoId]) REFERENCES [catalogo].[Productos] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822202609_ProductoGlobalStockPorSucursal'
)
BEGIN

                    INSERT INTO catalogo.StockSucursal (Id, ProductoId, SucursalId, StockActual, StockMinimo, StockMaximo)
                    SELECT NEWID(), Id, SucursalId, StockActual, StockMinimo, StockMaximo
                    FROM catalogo.Productos
                    WHERE TipoProducto = 0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822202609_ProductoGlobalStockPorSucursal'
)
BEGIN
    DROP INDEX [IX_Productos_SucursalId_Codigo] ON [catalogo].[Productos];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822202609_ProductoGlobalStockPorSucursal'
)
BEGIN
    DECLARE @var19 nvarchar(max);
    SELECT @var19 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[catalogo].[Productos]') AND [c].[name] = N'StockActual');
    IF @var19 IS NOT NULL EXEC(N'ALTER TABLE [catalogo].[Productos] DROP CONSTRAINT ' + @var19 + ';');
    ALTER TABLE [catalogo].[Productos] DROP COLUMN [StockActual];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822202609_ProductoGlobalStockPorSucursal'
)
BEGIN
    DECLARE @var20 nvarchar(max);
    SELECT @var20 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[catalogo].[Productos]') AND [c].[name] = N'StockMaximo');
    IF @var20 IS NOT NULL EXEC(N'ALTER TABLE [catalogo].[Productos] DROP CONSTRAINT ' + @var20 + ';');
    ALTER TABLE [catalogo].[Productos] DROP COLUMN [StockMaximo];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822202609_ProductoGlobalStockPorSucursal'
)
BEGIN
    DECLARE @var21 nvarchar(max);
    SELECT @var21 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[catalogo].[Productos]') AND [c].[name] = N'StockMinimo');
    IF @var21 IS NOT NULL EXEC(N'ALTER TABLE [catalogo].[Productos] DROP CONSTRAINT ' + @var21 + ';');
    ALTER TABLE [catalogo].[Productos] DROP COLUMN [StockMinimo];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822202609_ProductoGlobalStockPorSucursal'
)
BEGIN
    DECLARE @var22 nvarchar(max);
    SELECT @var22 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[catalogo].[Productos]') AND [c].[name] = N'SucursalId');
    IF @var22 IS NOT NULL EXEC(N'ALTER TABLE [catalogo].[Productos] DROP CONSTRAINT ' + @var22 + ';');
    ALTER TABLE [catalogo].[Productos] DROP COLUMN [SucursalId];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822202609_ProductoGlobalStockPorSucursal'
)
BEGIN
    DECLARE @var23 nvarchar(max);
    SELECT @var23 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[catalogo].[Categorias]') AND [c].[name] = N'SucursalId');
    IF @var23 IS NOT NULL EXEC(N'ALTER TABLE [catalogo].[Categorias] DROP CONSTRAINT ' + @var23 + ';');
    ALTER TABLE [catalogo].[Categorias] DROP COLUMN [SucursalId];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822202609_ProductoGlobalStockPorSucursal'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Productos_Codigo] ON [catalogo].[Productos] ([Codigo]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822202609_ProductoGlobalStockPorSucursal'
)
BEGIN
    CREATE UNIQUE INDEX [IX_StockSucursal_ProductoId_SucursalId] ON [catalogo].[StockSucursal] ([ProductoId], [SucursalId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822202609_ProductoGlobalStockPorSucursal'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260822202609_ProductoGlobalStockPorSucursal', N'10.0.11');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822221612_EliminaRncDeSucursal'
)
BEGIN
    DECLARE @var24 nvarchar(max);
    SELECT @var24 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[sucursales].[Sucursales]') AND [c].[name] = N'Rnc');
    IF @var24 IS NOT NULL EXEC(N'ALTER TABLE [sucursales].[Sucursales] DROP CONSTRAINT ' + @var24 + ';');
    ALTER TABLE [sucursales].[Sucursales] DROP COLUMN [Rnc];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260822221612_EliminaRncDeSucursal'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260822221612_EliminaRncDeSucursal', N'10.0.11');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823000142_RenombraNotaCreditoDebitoANotaCredito'
)
BEGIN
    EXEC sp_rename N'[facturacion].[NotasCreditoDebitoDetalle]', N'NotasCreditoDetalle', 'OBJECT';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823000142_RenombraNotaCreditoDebitoANotaCredito'
)
BEGIN
    EXEC sp_rename N'[facturacion].[NotasCreditoDebito]', N'NotasCredito', 'OBJECT';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823000142_RenombraNotaCreditoDebitoANotaCredito'
)
BEGIN
    EXEC sp_rename N'[facturacion].[NotasCreditoDetalle].[NotaCreditoDebitoId]', N'NotaCreditoId', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823000142_RenombraNotaCreditoDebitoANotaCredito'
)
BEGIN
    EXEC sp_rename N'[facturacion].[NotasCreditoDetalle].[IX_NotasCreditoDebitoDetalle_NotaCreditoDebitoId]', N'IX_NotasCreditoDetalle_NotaCreditoId', 'INDEX';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823000142_RenombraNotaCreditoDebitoANotaCredito'
)
BEGIN
    EXEC sp_rename N'[facturacion].[NotasCreditoDetalle].[IX_NotasCreditoDebitoDetalle_FacturaDetalleId]', N'IX_NotasCreditoDetalle_FacturaDetalleId', 'INDEX';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823000142_RenombraNotaCreditoDebitoANotaCredito'
)
BEGIN
    EXEC sp_rename N'[facturacion].[NotasCredito].[IX_NotasCreditoDebito_MotivoId]', N'IX_NotasCredito_MotivoId', 'INDEX';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823000142_RenombraNotaCreditoDebitoANotaCredito'
)
BEGIN
    EXEC sp_rename N'[facturacion].[NotasCredito].[IX_NotasCreditoDebito_NumeroNota]', N'IX_NotasCredito_NumeroNota', 'INDEX';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823000142_RenombraNotaCreditoDebitoANotaCredito'
)
BEGIN
    DECLARE @var25 nvarchar(max);
    SELECT @var25 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[facturacion].[NotasCredito]') AND [c].[name] = N'Tipo');
    IF @var25 IS NOT NULL EXEC(N'ALTER TABLE [facturacion].[NotasCredito] DROP CONSTRAINT ' + @var25 + ';');
    ALTER TABLE [facturacion].[NotasCredito] DROP COLUMN [Tipo];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823000142_RenombraNotaCreditoDebitoANotaCredito'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260823000142_RenombraNotaCreditoDebitoANotaCredito', N'10.0.11');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823001202_AgregaRolAutorizaNotaCredito'
)
BEGIN

                    IF NOT EXISTS (SELECT 1 FROM identidad.Roles WHERE Nombre = N'AutorizaNotaCredito')
                    INSERT INTO identidad.Roles (Id, Nombre) VALUES (NEWID(), N'AutorizaNotaCredito');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823001202_AgregaRolAutorizaNotaCredito'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260823001202_AgregaRolAutorizaNotaCredito', N'10.0.11');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823021110_AgregaNombreIngredienteExcluido'
)
BEGIN
    ALTER TABLE [pedidos].[ComandaItemIngredientes] ADD [NombreIngrediente] nvarchar(max) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823021110_AgregaNombreIngredienteExcluido'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260823021110_AgregaNombreIngredienteExcluido', N'10.0.11');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823031043_AgregaSnapshotMesaMeseroEnComanda'
)
BEGIN
    ALTER TABLE [pedidos].[Comandas] ADD [NombreMesero] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823031043_AgregaSnapshotMesaMeseroEnComanda'
)
BEGIN
    ALTER TABLE [pedidos].[Comandas] ADD [NumeroMesa] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823031043_AgregaSnapshotMesaMeseroEnComanda'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260823031043_AgregaSnapshotMesaMeseroEnComanda', N'10.0.11');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823140446_AgregaSalonYQuitaOrigenCreacionEnComanda'
)
BEGIN
    DECLARE @var26 nvarchar(max);
    SELECT @var26 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[pedidos].[Comandas]') AND [c].[name] = N'OrigenCreacion');
    IF @var26 IS NOT NULL EXEC(N'ALTER TABLE [pedidos].[Comandas] DROP CONSTRAINT ' + @var26 + ';');
    ALTER TABLE [pedidos].[Comandas] DROP COLUMN [OrigenCreacion];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823140446_AgregaSalonYQuitaOrigenCreacionEnComanda'
)
BEGIN
    ALTER TABLE [pedidos].[Comandas] ADD [NombreSalon] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823140446_AgregaSalonYQuitaOrigenCreacionEnComanda'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260823140446_AgregaSalonYQuitaOrigenCreacionEnComanda', N'10.0.11');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823141800_AgregaAuditoriaCreadoActualizadoVariasEntidades'
)
BEGIN
    ALTER TABLE [identidad].[Usuarios] ADD [ActualizadoEn] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823141800_AgregaAuditoriaCreadoActualizadoVariasEntidades'
)
BEGIN
    ALTER TABLE [identidad].[Usuarios] ADD [ActualizadoPorUsuarioId] uniqueidentifier NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823141800_AgregaAuditoriaCreadoActualizadoVariasEntidades'
)
BEGIN
    ALTER TABLE [identidad].[Usuarios] ADD [CreadoEn] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823141800_AgregaAuditoriaCreadoActualizadoVariasEntidades'
)
BEGIN
    ALTER TABLE [identidad].[Usuarios] ADD [CreadoPorUsuarioId] uniqueidentifier NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823141800_AgregaAuditoriaCreadoActualizadoVariasEntidades'
)
BEGIN
    ALTER TABLE [sucursales].[Sucursales] ADD [ActualizadoEn] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823141800_AgregaAuditoriaCreadoActualizadoVariasEntidades'
)
BEGIN
    ALTER TABLE [sucursales].[Sucursales] ADD [ActualizadoPorUsuarioId] uniqueidentifier NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823141800_AgregaAuditoriaCreadoActualizadoVariasEntidades'
)
BEGIN
    ALTER TABLE [sucursales].[Sucursales] ADD [CreadoPorUsuarioId] uniqueidentifier NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823141800_AgregaAuditoriaCreadoActualizadoVariasEntidades'
)
BEGIN
    ALTER TABLE [facturacion].[SecuenciasNcf] ADD [ActualizadoEn] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823141800_AgregaAuditoriaCreadoActualizadoVariasEntidades'
)
BEGIN
    ALTER TABLE [facturacion].[SecuenciasNcf] ADD [ActualizadoPorUsuarioId] uniqueidentifier NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823141800_AgregaAuditoriaCreadoActualizadoVariasEntidades'
)
BEGIN
    ALTER TABLE [facturacion].[SecuenciasNcf] ADD [CreadoPorUsuarioId] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823141800_AgregaAuditoriaCreadoActualizadoVariasEntidades'
)
BEGIN
    ALTER TABLE [cxccxp].[Proveedores] ADD [CreadoEn] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823141800_AgregaAuditoriaCreadoActualizadoVariasEntidades'
)
BEGIN
    ALTER TABLE [cxccxp].[Proveedores] ADD [CreadoPorUsuarioId] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823141800_AgregaAuditoriaCreadoActualizadoVariasEntidades'
)
BEGIN
    ALTER TABLE [cxccxp].[PagosCxP] ADD [CreadoPorUsuarioId] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823141800_AgregaAuditoriaCreadoActualizadoVariasEntidades'
)
BEGIN
    ALTER TABLE [cxccxp].[PagosCxC] ADD [CreadoPorUsuarioId] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823141800_AgregaAuditoriaCreadoActualizadoVariasEntidades'
)
BEGIN
    ALTER TABLE [clientes].[Clientes] ADD [CreadoPorUsuarioId] uniqueidentifier NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823141800_AgregaAuditoriaCreadoActualizadoVariasEntidades'
)
BEGIN
    ALTER TABLE [caja].[Cajas] ADD [ActualizadoEn] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823141800_AgregaAuditoriaCreadoActualizadoVariasEntidades'
)
BEGIN
    ALTER TABLE [caja].[Cajas] ADD [ActualizadoPorUsuarioId] uniqueidentifier NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823141800_AgregaAuditoriaCreadoActualizadoVariasEntidades'
)
BEGIN
    ALTER TABLE [caja].[Cajas] ADD [CreadoEn] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823141800_AgregaAuditoriaCreadoActualizadoVariasEntidades'
)
BEGIN
    ALTER TABLE [caja].[Cajas] ADD [CreadoPorUsuarioId] uniqueidentifier NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823141800_AgregaAuditoriaCreadoActualizadoVariasEntidades'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260823141800_AgregaAuditoriaCreadoActualizadoVariasEntidades', N'10.0.11');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823160738_AgregaPorcentajesPropinaYDenominacionesEfectivo'
)
BEGIN
    CREATE TABLE [caja].[DenominacionesEfectivo] (
        [Id] uniqueidentifier NOT NULL,
        [Valor] int NOT NULL,
        [Activo] bit NOT NULL,
        CONSTRAINT [PK_DenominacionesEfectivo] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823160738_AgregaPorcentajesPropinaYDenominacionesEfectivo'
)
BEGIN
    CREATE TABLE [caja].[PorcentajesPropina] (
        [Id] uniqueidentifier NOT NULL,
        [Valor] decimal(5,2) NOT NULL,
        [Activo] bit NOT NULL,
        CONSTRAINT [PK_PorcentajesPropina] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823160738_AgregaPorcentajesPropinaYDenominacionesEfectivo'
)
BEGIN
    CREATE UNIQUE INDEX [IX_DenominacionesEfectivo_Valor] ON [caja].[DenominacionesEfectivo] ([Valor]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823160738_AgregaPorcentajesPropinaYDenominacionesEfectivo'
)
BEGIN
    CREATE UNIQUE INDEX [IX_PorcentajesPropina_Valor] ON [caja].[PorcentajesPropina] ([Valor]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823160738_AgregaPorcentajesPropinaYDenominacionesEfectivo'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260823160738_AgregaPorcentajesPropinaYDenominacionesEfectivo', N'10.0.11');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823183258_AgregaInventariableAProducto'
)
BEGIN
    ALTER TABLE [catalogo].[Productos] ADD [Inventariable] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823183258_AgregaInventariableAProducto'
)
BEGIN
    UPDATE [catalogo].[Productos] SET [Inventariable] = 1 WHERE [TipoProducto] = 0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823183258_AgregaInventariableAProducto'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260823183258_AgregaInventariableAProducto', N'10.0.11');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823183706_MarcaBebidasDemoComoInventariables'
)
BEGIN

                    UPDATE [catalogo].[Productos]
                    SET [Inventariable] = 1
                    WHERE [Codigo] IN ('PLT-012', 'PLT-013', 'PLT-014');

                    INSERT INTO [catalogo].[StockSucursal] ([Id], [ProductoId], [SucursalId], [StockActual], [StockMinimo], [StockMaximo])
                    SELECT NEWID(), p.[Id], s.[Id], 50, NULL, NULL
                    FROM [catalogo].[Productos] p
                    CROSS JOIN [sucursales].[Sucursales] s
                    WHERE p.[Codigo] IN ('PLT-012', 'PLT-013', 'PLT-014')
                      AND NOT EXISTS (
                          SELECT 1 FROM [catalogo].[StockSucursal] ss
                          WHERE ss.[ProductoId] = p.[Id] AND ss.[SucursalId] = s.[Id]
                      );
                
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823183706_MarcaBebidasDemoComoInventariables'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260823183706_MarcaBebidasDemoComoInventariables', N'10.0.11');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823192535_AgregaComprobanteYCreadorEnCxcCxp'
)
BEGIN
    ALTER TABLE [cxccxp].[PagosCxP] ADD [NumeroComprobante] nvarchar(50) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823192535_AgregaComprobanteYCreadorEnCxcCxp'
)
BEGIN
    ALTER TABLE [cxccxp].[PagosCxC] ADD [NumeroComprobante] nvarchar(50) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823192535_AgregaComprobanteYCreadorEnCxcCxp'
)
BEGIN
    ALTER TABLE [cxccxp].[CuentasPorPagar] ADD [CreadoPorUsuarioId] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823192535_AgregaComprobanteYCreadorEnCxcCxp'
)
BEGIN
    ALTER TABLE [cxccxp].[CuentasPorCobrar] ADD [CreadoPorUsuarioId] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823192535_AgregaComprobanteYCreadorEnCxcCxp'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260823192535_AgregaComprobanteYCreadorEnCxcCxp', N'10.0.11');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823193148_AgregaAnulacionPagoCxcCxp'
)
BEGIN
    ALTER TABLE [cxccxp].[PagosCxP] ADD [Anulado] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823193148_AgregaAnulacionPagoCxcCxp'
)
BEGIN
    ALTER TABLE [cxccxp].[PagosCxP] ADD [AnuladoPorUsuarioId] uniqueidentifier NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823193148_AgregaAnulacionPagoCxcCxp'
)
BEGIN
    ALTER TABLE [cxccxp].[PagosCxP] ADD [FechaAnulacion] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823193148_AgregaAnulacionPagoCxcCxp'
)
BEGIN
    ALTER TABLE [cxccxp].[PagosCxP] ADD [MotivoAnulacion] nvarchar(300) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823193148_AgregaAnulacionPagoCxcCxp'
)
BEGIN
    ALTER TABLE [cxccxp].[PagosCxC] ADD [Anulado] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823193148_AgregaAnulacionPagoCxcCxp'
)
BEGIN
    ALTER TABLE [cxccxp].[PagosCxC] ADD [AnuladoPorUsuarioId] uniqueidentifier NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823193148_AgregaAnulacionPagoCxcCxp'
)
BEGIN
    ALTER TABLE [cxccxp].[PagosCxC] ADD [FechaAnulacion] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823193148_AgregaAnulacionPagoCxcCxp'
)
BEGIN
    ALTER TABLE [cxccxp].[PagosCxC] ADD [MotivoAnulacion] nvarchar(300) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823193148_AgregaAnulacionPagoCxcCxp'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260823193148_AgregaAnulacionPagoCxcCxp', N'10.0.11');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823203659_AgregaNumeroTurnoCaja'
)
BEGIN
    ALTER TABLE [caja].[TurnosCaja] ADD [NumeroTurno] int NOT NULL IDENTITY;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823203659_AgregaNumeroTurnoCaja'
)
BEGIN
    CREATE UNIQUE INDEX [IX_TurnosCaja_NumeroTurno] ON [caja].[TurnosCaja] ([NumeroTurno]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260823203659_AgregaNumeroTurnoCaja'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260823203659_AgregaNumeroTurnoCaja', N'10.0.11');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260824203548_AgregaActivoAMesa'
)
BEGIN
    ALTER TABLE [pedidos].[Mesas] ADD [Activo] bit NOT NULL DEFAULT CAST(1 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260824203548_AgregaActivoAMesa'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260824203548_AgregaActivoAMesa', N'10.0.11');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260825004003_CreaPromociones'
)
BEGIN
    CREATE TABLE [catalogo].[Promociones] (
        [Id] uniqueidentifier NOT NULL,
        [SucursalId] uniqueidentifier NULL,
        [Nombre] nvarchar(200) NOT NULL,
        [ProductoId] uniqueidentifier NULL,
        [CategoriaId] uniqueidentifier NULL,
        [TipoDescuento] int NOT NULL,
        [Valor] decimal(18,2) NOT NULL,
        [FechaInicio] datetime2 NOT NULL,
        [FechaFin] datetime2 NOT NULL,
        [Activo] bit NOT NULL DEFAULT CAST(1 AS bit),
        CONSTRAINT [PK_Promociones] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260825004003_CreaPromociones'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260825004003_CreaPromociones', N'10.0.11');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260825010729_AgregaCodigoAPromocion'
)
BEGIN
    ALTER TABLE [catalogo].[Promociones] ADD [Codigo] nvarchar(20) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260825010729_AgregaCodigoAPromocion'
)
BEGIN

                    UPDATE p
                    SET p.Codigo = 'PROMO-' + RIGHT('00000' + CAST(nums.Numero AS varchar(5)), 5)
                    FROM [catalogo].[Promociones] p
                    JOIN (
                        SELECT Id, ROW_NUMBER() OVER (ORDER BY FechaInicio) AS Numero
                        FROM [catalogo].[Promociones]
                    ) nums ON nums.Id = p.Id;
                
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260825010729_AgregaCodigoAPromocion'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Promociones_Codigo] ON [catalogo].[Promociones] ([Codigo]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260825010729_AgregaCodigoAPromocion'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260825010729_AgregaCodigoAPromocion', N'10.0.11');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260825025823_AgregaRecibirAlertaStockBajoAUsuario'
)
BEGIN
    ALTER TABLE [identidad].[Usuarios] ADD [RecibirAlertaStockBajo] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260825025823_AgregaRecibirAlertaStockBajoAUsuario'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260825025823_AgregaRecibirAlertaStockBajoAUsuario', N'10.0.11');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260825155315_AgregaIndiceSucursalAStockSucursal'
)
BEGIN
    CREATE INDEX [IX_StockSucursal_SucursalId] ON [catalogo].[StockSucursal] ([SucursalId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260825155315_AgregaIndiceSucursalAStockSucursal'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260825155315_AgregaIndiceSucursalAStockSucursal', N'10.0.11');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260910003110_CreaDeliveries'
)
BEGIN
    IF SCHEMA_ID(N'deliveries') IS NULL EXEC(N'CREATE SCHEMA [deliveries];');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260910003110_CreaDeliveries'
)
BEGIN
    CREATE TABLE [deliveries].[Deliveries] (
        [Id] uniqueidentifier NOT NULL,
        [SucursalId] uniqueidentifier NOT NULL,
        [Nombre] nvarchar(150) NOT NULL,
        [Telefono] nvarchar(30) NULL,
        [Activo] bit NOT NULL,
        [SaldoPendiente] decimal(18,2) NOT NULL,
        [CreadoEn] datetime2 NOT NULL,
        [CreadoPorUsuarioId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_Deliveries] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260910003110_CreaDeliveries'
)
BEGIN
    CREATE TABLE [deliveries].[AbonosDelivery] (
        [Id] uniqueidentifier NOT NULL,
        [DeliveryId] uniqueidentifier NOT NULL,
        [SucursalId] uniqueidentifier NOT NULL,
        [Monto] decimal(18,2) NOT NULL,
        [FechaPago] datetime2 NOT NULL,
        [MetodoPagoId] uniqueidentifier NOT NULL,
        [NumeroComprobante] nvarchar(50) NULL,
        [CreadoPorUsuarioId] uniqueidentifier NOT NULL,
        [Anulado] bit NOT NULL,
        [FechaAnulacion] datetime2 NULL,
        [AnuladoPorUsuarioId] uniqueidentifier NULL,
        [MotivoAnulacion] nvarchar(300) NULL,
        CONSTRAINT [PK_AbonosDelivery] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AbonosDelivery_Deliveries_DeliveryId] FOREIGN KEY ([DeliveryId]) REFERENCES [deliveries].[Deliveries] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_AbonosDelivery_MetodosPago_MetodoPagoId] FOREIGN KEY ([MetodoPagoId]) REFERENCES [catalogo].[MetodosPago] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260910003110_CreaDeliveries'
)
BEGIN
    CREATE TABLE [deliveries].[FacturasDelivery] (
        [Id] uniqueidentifier NOT NULL,
        [DeliveryId] uniqueidentifier NOT NULL,
        [FacturaId] uniqueidentifier NOT NULL,
        [SucursalId] uniqueidentifier NOT NULL,
        [MontoFactura] decimal(18,2) NOT NULL,
        [MontoDelivery] decimal(18,2) NOT NULL,
        [FechaAsignacion] datetime2 NOT NULL,
        [AsignadoPorUsuarioId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_FacturasDelivery] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_FacturasDelivery_Deliveries_DeliveryId] FOREIGN KEY ([DeliveryId]) REFERENCES [deliveries].[Deliveries] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260910003110_CreaDeliveries'
)
BEGIN
    CREATE INDEX [IX_AbonosDelivery_DeliveryId] ON [deliveries].[AbonosDelivery] ([DeliveryId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260910003110_CreaDeliveries'
)
BEGIN
    CREATE INDEX [IX_AbonosDelivery_MetodoPagoId] ON [deliveries].[AbonosDelivery] ([MetodoPagoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260910003110_CreaDeliveries'
)
BEGIN
    CREATE INDEX [IX_FacturasDelivery_DeliveryId] ON [deliveries].[FacturasDelivery] ([DeliveryId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260910003110_CreaDeliveries'
)
BEGIN
    CREATE UNIQUE INDEX [IX_FacturasDelivery_FacturaId] ON [deliveries].[FacturasDelivery] ([FacturaId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260910003110_CreaDeliveries'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260910003110_CreaDeliveries', N'10.0.11');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260910011023_AgregaFormatoImpresionACaja'
)
BEGIN
    ALTER TABLE [caja].[Cajas] ADD [FormatoImpresion] int NOT NULL DEFAULT 0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260910011023_AgregaFormatoImpresionACaja'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260910011023_AgregaFormatoImpresionACaja', N'10.0.11');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260910015156_AgregaFormatoImpresionDeliveryASucursal'
)
BEGIN
    ALTER TABLE [sucursales].[Sucursales] ADD [FormatoImpresionDelivery] int NOT NULL DEFAULT 0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260910015156_AgregaFormatoImpresionDeliveryASucursal'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260910015156_AgregaFormatoImpresionDeliveryASucursal', N'10.0.11');
END;

COMMIT;
GO

