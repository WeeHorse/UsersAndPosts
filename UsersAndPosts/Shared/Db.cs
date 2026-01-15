using Microsoft.Data.Sqlite;

namespace UsersAndPosts.Shared;

public sealed class Db
{
    private readonly string _connectionString;

    public Db(IConfiguration config)
    {
        // Liten, tydlig default
        var dbPath = config["Db:Path"] ?? "app.db";
        _connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = dbPath,
            ForeignKeys = true
        }.ToString();
    }

    public SqliteConnection OpenConnection()
    {
        var conn = new SqliteConnection(_connectionString);
        conn.Open();

        // Se till att FK är på för varje connection (SQLite är lite "special")
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "PRAGMA foreign_keys = ON;";
        cmd.ExecuteNonQuery();

        return conn;
    }
}
