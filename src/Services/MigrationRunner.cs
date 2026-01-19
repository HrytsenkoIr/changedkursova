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
        //помилка якщо підключення відсутнє
        _dbConnection = dbConnection ?? throw new ArgumentNullException(nameof(dbConnection));
        //шлях до міграцій через appsettings.json
        _migrationsPath = configuration["MigrationsPath"]
            ?? throw new InvalidOperationException("MigrationsPath is not configured");
    }

    public async Task RunMigrationsAsync(CancellationToken cancellationToken = default)
    {
        Console.WriteLine("Starting database migrations...");

        await using var connection = _dbConnection.CreateConnection();
        await connection.OpenAsync(cancellationToken);

//перевірка чи існує таблиця для історії міграцій
        await EnsureMigrationHistoryTableAsync(connection, cancellationToken);

        if (!Directory.Exists(_migrationsPath))
        {
            Console.WriteLine($"Migrations folder not found: {_migrationsPath}");
            return;
        }

//отримка всіх sql з сортуванням
        var files = Directory.GetFiles(_migrationsPath, "*.sql").OrderBy(f => f).ToList();
//прохід по кожному файлу
        foreach (var file in files)
        {
            var name = Path.GetFileName(file);
            var content = await File.ReadAllTextAsync(file, cancellationToken);
            //чек хешу на те чи змінювався файл
            var hash = ComputeSha256Hash(content);

//чи вже застосовувалась міграція
            if (await IsMigrationAppliedAsync(connection, name, hash, cancellationToken))
            {
                Console.WriteLine($"Migration '{name}' already applied — skipping");
                continue;
            }

            Console.WriteLine($"Applying migration '{name}'...");

            await using var tx = await connection.BeginTransactionAsync(cancellationToken);

            try
            {
                //виконує
                await ExecuteMigrationAsync(connection, tx, content, cancellationToken);
                //записує в історію
                await RecordMigrationAsync(connection, tx, name, hash, cancellationToken);
//фіксить транзакцію
                await tx.CommitAsync(cancellationToken);
                Console.WriteLine($"Migration '{name}' applied successfully");
            }
            catch (Exception ex)
            {
                //ролбек транзакції при помилці
                await tx.RollbackAsync(cancellationToken);
                Console.WriteLine($"Migration '{name}' failed — rolled back");
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        Console.WriteLine("All migrations completed successfully.");
    }
//створення таблиці історії міграцій якщо її немає
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
//захист від SQL ін'єкцій не потрібен оскільки sql константа
        await using var cmd = new SqlCommand(sql, conn);
        await cmd.ExecuteNonQueryAsync(ct);
    }
//перевірка чи міграція вже застосовувалась
    private async Task<bool> IsMigrationAppliedAsync(SqlConnection conn, string name, string hash, CancellationToken ct)
    {
        const string sql = @"
SELECT COUNT(*)
FROM __MigrationHistory
WHERE MigrationName = @name AND MigrationHash = @hash";

        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@name", name);//безпека від SQL ін'єкцій
        cmd.Parameters.AddWithValue("@hash", hash);

        return (int)(await cmd.ExecuteScalarAsync(ct) ?? 0) > 0;
    }
//виконує sql з підтримкою GO
    private async Task ExecuteMigrationAsync(SqlConnection conn, DbTransaction tx, string content, CancellationToken ct)
    {
        //розбиття на рядки
        var lines = content.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
        //сюди збираються sql рядки
        var batchBuilder = new StringBuilder();

        foreach (var line in lines)
        {//якщо го то це межа
            if (line.Trim().Equals("GO", StringComparison.OrdinalIgnoreCase))
            {
                //якщо блок не порожній то виконуємо
                var batch = batchBuilder.ToString();
                if (!string.IsNullOrWhiteSpace(batch))
                {
                    await using var cmd = new SqlCommand(batch, conn, (SqlTransaction)tx)
                    {//таймаут збільшено для великих міграцій
                        CommandTimeout = 300
                    };
                    await cmd.ExecuteNonQueryAsync(ct);
                }
                //очищення буферу
                batchBuilder.Clear();
            }
            else
            {
                //рядок до поточного sql блоку
                batchBuilder.AppendLine(line);
            }
        }
//останній скл блок якщо є
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
//запис інфи про міграцію в таблицю історії
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
//тест на хеш щоб визначити зміни в міграції
    private static string ComputeSha256Hash(string data)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(data));
        var sb = new StringBuilder(bytes.Length * 2);
        foreach (var b in bytes) sb.Append(b.ToString("x2"));
        return sb.ToString();
    }
}
