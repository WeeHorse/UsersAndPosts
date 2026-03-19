using Microsoft.Data.Sqlite;
using UsersAndPosts.Shared;

namespace UsersAndPosts.User;

public sealed class UserRepo
{
  private readonly Db _db;
  public UserRepo(Db db) => _db = db;

  private sealed record UserWithPassword(
      int Id,
      string Username,
      string Password,
      string DisplayName,
      DateTime CreatedAtUtc
  );

  public async Task<IReadOnlyList<User>> GetAllAsync()
  {
    using var conn = _db.OpenConnection();
    using var cmd = conn.CreateCommand();
    cmd.CommandText = "SELECT Id, Username, DisplayName, CreatedAtUtc FROM Users ORDER BY Id;";

    using var reader = await cmd.ExecuteReaderAsync();
    var list = new List<User>();

    while (await reader.ReadAsync())
    {
      list.Add(new User(
          reader.GetInt32(0),
          reader.GetString(1),
          reader.GetString(2),
          DateTime.Parse(reader.GetString(3))
      ));
    }

    return list;
  }

  public async Task<User?> GetByIdAsync(int id)
  {
    using var conn = _db.OpenConnection();
    using var cmd = conn.CreateCommand();
    cmd.CommandText = "SELECT Id, Username, DisplayName, CreatedAtUtc FROM Users WHERE Id = @id LIMIT 1;";
    cmd.Parameters.AddWithValue("@id", id);

    using var reader = await cmd.ExecuteReaderAsync();
    if (!await reader.ReadAsync()) return null;

    return new User(
        reader.GetInt32(0),
        reader.GetString(1),
        reader.GetString(2),
        DateTime.Parse(reader.GetString(3))
    );
  }

  public async Task<User?> GetByUsernameAsync(string username)
  {
    var user = await GetByUsernameWithPasswordAsync(username);
    return user is null
        ? null
        : new User(user.Id, user.Username, user.DisplayName, user.CreatedAtUtc);
  }

  public async Task<User?> ValidateCredentialsAsync(string username, string password)
  {
    var user = await GetByUsernameWithPasswordAsync(username);
    if (user is null) return null;
    if (!string.Equals(user.Password, password, StringComparison.Ordinal)) return null;

    return new User(user.Id, user.Username, user.DisplayName, user.CreatedAtUtc);
  }

  private async Task<UserWithPassword?> GetByUsernameWithPasswordAsync(string username)
  {
    using var conn = _db.OpenConnection();
    using var cmd = conn.CreateCommand();
    cmd.CommandText = "SELECT Id, Username, Password, DisplayName, CreatedAtUtc FROM Users WHERE Username = @u LIMIT 1;";
    cmd.Parameters.AddWithValue("@u", username);

    using var reader = await cmd.ExecuteReaderAsync();
    if (!await reader.ReadAsync()) return null;

    return new UserWithPassword(
        reader.GetInt32(0),
        reader.GetString(1),
        reader.GetString(2),
        reader.GetString(3),
        DateTime.Parse(reader.GetString(4))
    );
  }

  public async Task<(bool ok, string? error, int? newId)> CreateAsync(string username, string password, string displayName)
  {
    if (string.IsNullOrWhiteSpace(username)) return (false, "Username is required.", null);
    if (string.IsNullOrWhiteSpace(password)) return (false, "Password is required.", null);
    if (string.IsNullOrWhiteSpace(displayName)) return (false, "DisplayName is required.", null);

    using var conn = _db.OpenConnection();

    // Enkel normalisering (liten app, men ger bättre data)
    username = username.Trim();

    try
    {
      using var cmd = conn.CreateCommand();
      cmd.CommandText = """
            INSERT INTO Users (Username, Password, DisplayName, CreatedAtUtc)
            VALUES (@u, @p, @d, @c);
                SELECT last_insert_rowid();
            """;
      cmd.Parameters.AddWithValue("@u", username);
        cmd.Parameters.AddWithValue("@p", password);
      cmd.Parameters.AddWithValue("@d", displayName.Trim());
      cmd.Parameters.AddWithValue("@c", DateTime.UtcNow.ToString("O"));

      var idObj = await cmd.ExecuteScalarAsync();
      var id = Convert.ToInt32(idObj);
      return (true, null, id);
    }
    catch (SqliteException ex) when (ex.SqliteErrorCode == 19) // constraint violation
    {
      return (false, "Username already exists.", null);
    }
  }
}
