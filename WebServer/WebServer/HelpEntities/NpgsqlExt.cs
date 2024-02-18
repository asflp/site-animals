using System.Data;
using Npgsql;

namespace WebServer;

public static class NpgsqlExt
{
    public static async Task OpenConnectionIfClosed(this NpgsqlConnection connection)
    {
        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync();
        }
    }
}