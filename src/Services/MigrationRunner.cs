using System.Data.Common;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Data.SqlClient;

namespace OnlineStoreSystem.Services;

public class MigrationRunner
{
    private readonly DatabaseConnection _dbConnection;
    private readonly string _migrationsPath;

    public MigrationRunner(DatabaseConnection dbConnection, IConfiguration configuration)
    {
        _dbConnection = dbConnection ?? throw new ArgumentNullException(nameof(dbConnection));
        _migrationsPath = configuration["MigrationsPath"]
            ?? throw new InvalidOperationException("MigrationsPath is not configured");
    }

    public async Task RunMigrationsAsync(CancellationToken cancellationToken = default)
    {
        Console.WriteLine("Starting database migrations...");

        await using var connection = _dbConnection.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await EnsureMigrationHistoryTableAsync(connection, cancellationToken);

        if (!Directory.Exists(_migrationsPath))
        {
            Console.WriteLine($"Migrations folder not found: {_migrationsPath}");
            return;
        }

        var files = Directory.GetFiles(_migrationsPath, "*.sql").OrderBy(f => f).ToList();

        foreach (var file in files)
        {
            var name = Path.GetFileName(file);
            var content = await File.ReadAllTextAsync(file, cancellationToken);
            var hash = ComputeSha256Hash(content);

            if (await IsMigrationAppliedAsync(connection, name, hash, cancellationToken))
            {
                Console.WriteLine($"Migration '{name}' already applied — skipping");
                continue;
            }

            Console.WriteLine($"Applying migration '{name}'...");

            await using var tx = await connection.BeginTransactionAsync(cancellationToken);

            try
            {
                await ExecuteMigrationAsync(connection, tx, content, cancellationToken);
                await RecordMigrationAsync(connection, tx, name, hash, cancellationToken);

                await tx.CommitAsync(cancellationToken);
                Console.WriteLine($"Migration '{name}' applied successfully");
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync(cancellationToken);
                Console.WriteLine($"Migration '{name}' failed — rolled back");
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        Console.WriteLine("All migrations completed successfully.");
    }

    private async Task EnsureMigrationHistoryTableAsync(SqlConnection conn, CancellationToken ct)
    {
        const string sql = @"
IF NOT EXISTS (
    SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[__MigrationHistory]') AND type = N'U'
)
BEGIN
    CREATE TABLE __MigrationHistory (
        MigrationID INT IDENTITY(1,1) PRIMARY KEY,
        MigrationName NVARCHAR(255) NOT NULL,
        MigrationHash NVARCHAR(64) NOT NULL,
        AppliedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT UQ_Migration UNIQUE (MigrationName, MigrationHash)
    );
END";

        await using var cmd = new SqlCommand(sql, conn);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    private async Task<bool> IsMigrationAppliedAsync(SqlConnection conn, string name, string hash, CancellationToken ct)
    {
        const string sql = @"
SELECT COUNT(*)
FROM __MigrationHistory
WHERE MigrationName = @name AND MigrationHash = @hash";

        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@name", name);
        cmd.Parameters.AddWithValue("@hash", hash);

        return (int)(await cmd.ExecuteScalarAsync(ct) ?? 0) > 0;
    }

    private async Task ExecuteMigrationAsync(SqlConnection conn, DbTransaction tx, string content, CancellationToken ct)
    {
        var lines = content.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
        var batchBuilder = new StringBuilder();

        foreach (var line in lines)
        {
            if (line.Trim().Equals("GO", StringComparison.OrdinalIgnoreCase))
            {
                var batch = batchBuilder.ToString();
                if (!string.IsNullOrWhiteSpace(batch))
                {
                    await using var cmd = new SqlCommand(batch, conn, (SqlTransaction)tx)
                    {
                        CommandTimeout = 300
                    };
                    await cmd.ExecuteNonQueryAsync(ct);
                }
                batchBuilder.Clear();
            }
            else
            {
                batchBuilder.AppendLine(line);
            }
        }

        var lastBatch = batchBuilder.ToString();
        if (!string.IsNullOrWhiteSpace(lastBatch))
        {
            await using var cmd = new SqlCommand(lastBatch, conn, (SqlTransaction)tx)
            {
                CommandTimeout = 300
            };
            await cmd.ExecuteNonQueryAsync(ct);
        }
    }

    private async Task RecordMigrationAsync(SqlConnection conn, DbTransaction tx, string name, string hash, CancellationToken ct)
    {
        const string sql = @"
INSERT INTO __MigrationHistory (MigrationName, MigrationHash)
VALUES (@name, @hash)";

        await using var cmd = new SqlCommand(sql, conn, (SqlTransaction)tx);
        cmd.Parameters.AddWithValue("@name", name);
        cmd.Parameters.AddWithValue("@hash", hash);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    private static string ComputeSha256Hash(string data)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(data));
        var sb = new StringBuilder(bytes.Length * 2);
        foreach (var b in bytes) sb.Append(b.ToString("x2"));
        return sb.ToString();
    }
}
