// Файл: DatabaseService.cs
using System;
using System.Threading.Tasks;
using Npgsql;

namespace ParabolaAnimation
{
    public class DatabaseService
    {
        private const string ConnectionString = "Host=localhost;Database=animation_db;Username=postgres;Password=1782";

        public async Task InitializeAsync()
        {
            await using var conn = new NpgsqlConnection(ConnectionString);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand(@"
                CREATE TABLE IF NOT EXISTS launches (
                    id SERIAL PRIMARY KEY,
                    launch_time TIMESTAMP NOT NULL DEFAULT NOW()
                );", conn);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task SaveLaunchAsync(DateTime launchTime)
        {
            await using var conn = new NpgsqlConnection(ConnectionString);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand("INSERT INTO launches (launch_time) VALUES (@time)", conn);
            cmd.Parameters.AddWithValue("time", launchTime);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<int> GetLaunchCountLastHourAsync()
        {
            await using var conn = new NpgsqlConnection(ConnectionString);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand(
                "SELECT COUNT(*) FROM launches WHERE launch_time >= NOW() - INTERVAL '1 hour'", conn);
            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }
    }
}