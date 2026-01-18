

DECLARE @CurrentUser NVARCHAR(256) = SUSER_NAME();
DECLARE @IsSysAdmin BIT = IIF(IS_SRVROLEMEMBER('sysadmin') = 1, 1, 0);

IF @IsSysAdmin = 0
BEGIN
    THROW 50000, 'This script must be executed by sysadmin.', 1;
END

   --SERVER LEVEL: LOGINS

IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name = 'AdminUser')
    CREATE LOGIN [AdminUser] WITH PASSWORD = 'AdminPass123';

IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name = 'ManagerUser')
    CREATE LOGIN [ManagerUser] WITH PASSWORD = 'ManagerPass123';

IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name = 'WorkerUser')
    CREATE LOGIN [WorkerUser] WITH PASSWORD = 'WorkerPass123';


   --SERVER ROLE

IF NOT EXISTS (
    SELECT 1
    FROM sys.server_role_members rm
    JOIN sys.server_principals r ON rm.role_principal_id = r.principal_id
    JOIN sys.server_principals m ON rm.member_principal_id = m.principal_id
    WHERE r.name = 'dbcreator' AND m.name = 'AdminUser'
)
    ALTER SERVER ROLE dbcreator ADD MEMBER [AdminUser];


   --3. DATABASE LEVEL: USERS

-- Admin
IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = 'AdminUser')
    CREATE USER [AdminUser] FOR LOGIN [AdminUser];

-- Manager
IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = 'ManagerUser')
    CREATE USER [ManagerUser] FOR LOGIN [ManagerUser];

-- Worker
IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = 'WorkerUser')
    CREATE USER [WorkerUser] FOR LOGIN [WorkerUser];



IF NOT EXISTS (
    SELECT 1
    FROM sys.database_role_members drm
    JOIN sys.database_principals r ON drm.role_principal_id = r.principal_id
    JOIN sys.database_principals m ON drm.member_principal_id = m.principal_id
    WHERE r.name = 'db_owner' AND m.name = 'AdminUser'
)
    ALTER ROLE db_owner ADD MEMBER [AdminUser];


   -- MANAGER ROLE


IF NOT EXISTS (
    SELECT 1 FROM sys.database_principals
    WHERE name = 'ManagerRole' AND type = 'R'
)
    CREATE ROLE [ManagerRole];

GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::dbo TO [ManagerRole];
GRANT CREATE TABLE TO [ManagerRole];
GRANT ALTER ON SCHEMA::dbo TO [ManagerRole];

IF NOT EXISTS (
    SELECT 1
    FROM sys.database_role_members drm
    JOIN sys.database_principals r ON drm.role_principal_id = r.principal_id
    JOIN sys.database_principals m ON drm.member_principal_id = m.principal_id
    WHERE r.name = 'ManagerRole' AND m.name = 'ManagerUser'
)
    ALTER ROLE [ManagerRole] ADD MEMBER [ManagerUser];


   --WORKER ROLE


IF NOT EXISTS (
    SELECT 1 FROM sys.database_principals
    WHERE name = 'WorkerRole' AND type = 'R'
)
    CREATE ROLE [WorkerRole];

GRANT SELECT ON SCHEMA::dbo TO [WorkerRole];

IF NOT EXISTS (
    SELECT 1
    FROM sys.database_role_members drm
    JOIN sys.database_principals r ON drm.role_principal_id = r.principal_id
    JOIN sys.database_principals m ON drm.member_principal_id = m.principal_id
    WHERE r.name = 'WorkerRole' AND m.name = 'WorkerUser'
)
    ALTER ROLE [WorkerRole] ADD MEMBER [WorkerUser];


   -- VERIFICATION
 

SELECT name, type_desc
FROM sys.database_principals
WHERE name IN (
    'AdminUser',
    'ManagerUser',
    'WorkerUser',
    'ManagerRole',
    'WorkerRole',
    'dbo'
)
ORDER BY name;
