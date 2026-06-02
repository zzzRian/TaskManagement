-- Crea la base de datos. El esquema completo lo generan las migraciones de EF Core
-- al ejecutar `dotnet ef database update`. El seed (roles, permisos, usuario admin)
-- se ejecuta automáticamente al iniciar la aplicación.
CREATE DATABASE IF NOT EXISTS TaskManagementDb CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE TaskManagementDb;
