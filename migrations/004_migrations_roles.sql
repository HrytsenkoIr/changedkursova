DECLARE @CurrentUser NVARCHAR(256) = SUSER_NAME();
DECLARE @IsSysAdmin BIT = 0;
IF IS_SRVROLEMEMBER('sysadmin') = 1
    SET @IsSysAdmin = 1;

IF @IsSysAdmin = 1 OR @CurrentUser IN ('sa', 'dbo')
BEGIN
    IF NOT EXISTS (SELECT name FROM sys.server_principals WHERE name = 'AdminUser')
        CREATE LOGIN AdminUser WITH PASSWORD = 'AdminPass123';

    IF NOT EXISTS (SELECT name FROM sys.database_principals WHERE name = 'AdminUser')
    BEGIN
        CREATE USER AdminUser FOR LOGIN AdminUser;
        ALTER SERVER ROLE dbcreator ADD MEMBER AdminUser;
    END

    IF NOT EXISTS (SELECT name FROM sys.server_principals WHERE name = 'ManagerUser')
        CREATE LOGIN ManagerUser WITH PASSWORD = 'ManagerPass123';

    IF NOT EXISTS (SELECT name FROM sys.database_principals WHERE name = 'ManagerUser')
        CREATE USER ManagerUser FOR LOGIN ManagerUser;

    IF NOT EXISTS (SELECT name FROM sys.database_principals WHERE name = 'ManagerRole' AND type = 'R')
        CREATE ROLE ManagerRole;

    GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::dbo TO ManagerRole;
    ALTER ROLE ManagerRole ADD MEMBER ManagerUser;
END
