using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace OnlineStore.Services;

public class DatabaseConnection
{
    private readonly string _connectionString;

    public DatabaseConnection(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("OnlineStore") 
            ?? throw new InvalidOperationException("Connection string not found.");
    }

    public SqlConnection CreateConnection()
    {
        return new SqlConnection(_connectionString);
    }
}