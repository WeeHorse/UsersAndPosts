using Microsoft.Data.Sqlite;

namespace UsersAndPosts.Shared;

public static class DbSeeder
{
    public static async Task SeedAsync(Db db)
    {
        using var conn = db.OpenConnection();

        // Tables
        await ExecuteAsync(conn, """
        CREATE TABLE IF NOT EXISTS Users (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Username TEXT NOT NULL UNIQUE,
            DisplayName TEXT NOT NULL,
            CreatedAtUtc TEXT NOT NULL
        );
        """);

        await ExecuteAsync(conn, """
        CREATE TABLE IF NOT EXISTS Posts (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            UserId INTEGER NOT NULL,
            Content TEXT NOT NULL,
            CreatedAtUtc TEXT NOT NULL,
            FOREIGN KEY(UserId) REFERENCES Users(Id) ON DELETE CASCADE
        );
        """);

        // Seed Users if empty
        var userCount = await ScalarIntAsync(conn, "SELECT COUNT(1) FROM Users;");
        if (userCount == 0)
        {
            await ExecuteAsync(conn,
                "INSERT INTO Users (Username, DisplayName, CreatedAtUtc) VALUES (@u, @d, @c);",
                ("@u", "alice"), ("@d", "Alice"), ("@c", DateTime.UtcNow.ToString("O")));

            await ExecuteAsync(conn,
                "INSERT INTO Users (Username, DisplayName, CreatedAtUtc) VALUES (@u, @d, @c);",
                ("@u", "bob"), ("@d", "Bob"), ("@c", DateTime.UtcNow.ToString("O")));
        }

        // Seed Posts if empty
        var postCount = await ScalarIntAsync(conn, "SELECT COUNT(1) FROM Posts;");
        if (postCount == 0)
        {
            // Alice -> 1 om hon skapades först; men vi tar säkert via SELECT
            var aliceId = await ScalarIntAsync(conn, "SELECT Id FROM Users WHERE Username = 'alice' LIMIT 1;");
            if (aliceId > 0)
            {
                await ExecuteAsync(conn,
                    "INSERT INTO Posts (UserId, Content, CreatedAtUtc) VALUES (@uid, @content, @c);",
                    ("@uid", aliceId), ("@content", "Hello world 👋"), ("@c", DateTime.UtcNow.ToString("O")));
            }
        }
    }

    private static async Task ExecuteAsync(SqliteConnection conn, string sql, params (string key, object value)[] args)
    {
        using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        foreach (var (key, value) in args)
            cmd.Parameters.AddWithValue(key, value);

        await cmd.ExecuteNonQueryAsync();
    }

    private static async Task<int> ScalarIntAsync(SqliteConnection conn, string sql)
    {
        using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        var result = await cmd.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }
}
