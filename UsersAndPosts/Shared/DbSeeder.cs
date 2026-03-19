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
        Password TEXT NOT NULL,
            DisplayName TEXT NOT NULL,
            CreatedAtUtc TEXT NOT NULL
        );
        """);

    // Migration for existing DB files created before Password existed
    var hasPasswordColumn = await ScalarIntAsync(conn, """
      SELECT COUNT(1)
      FROM pragma_table_info('Users')
      WHERE name = 'Password';
      """);
    if (hasPasswordColumn == 0)
    {
      await ExecuteAsync(conn, "ALTER TABLE Users ADD COLUMN Password TEXT NOT NULL DEFAULT ''; ");
    }

    // Ensure no empty passwords remain after migration
    await ExecuteAsync(conn, "UPDATE Users SET Password = Username WHERE Password = ''; ");

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
          "INSERT INTO Users (Username, Password, DisplayName, CreatedAtUtc) VALUES (@u, @p, @d, @c);",
          ("@u", "alice"), ("@p", "abc123"), ("@d", "Alice"), ("@c", DateTime.UtcNow.ToString("O")));

      await ExecuteAsync(conn,
          "INSERT INTO Users (Username, Password, DisplayName, CreatedAtUtc) VALUES (@u, @p, @d, @c);",
          ("@u", "bob"), ("@p", "abc123"), ("@d", "Bob"), ("@c", DateTime.UtcNow.ToString("O")));
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
