using UsersAndPosts.Shared;

namespace UsersAndPosts.Post;

public sealed class PostRepo
{
  private readonly Db _db;
  public PostRepo(Db db) => _db = db;

  public async Task<IReadOnlyList<(Post post, string authorUsername)>> GetAllAsync()
  {
    using var conn = _db.OpenConnection();
    using var cmd = conn.CreateCommand();
    cmd.CommandText = """
            SELECT p.Id, p.UserId, p.Content, p.CreatedAtUtc, u.Username
            FROM Posts p
            JOIN Users u ON u.Id = p.UserId
            ORDER BY p.Id DESC;
        """;

    using var reader = await cmd.ExecuteReaderAsync();
    var list = new List<(Post, string)>();

    while (await reader.ReadAsync())
    {
      var post = new Post(
          reader.GetInt32(0),
          reader.GetInt32(1),
          reader.GetString(2),
          DateTime.Parse(reader.GetString(3))
      );
      var username = reader.GetString(4);
      list.Add((post, username));
    }

    return list;
  }

  public async Task<IReadOnlyList<(Post post, string authorUsername)>> GetByUserIdAsync(int userId)
  {
    using var conn = _db.OpenConnection();
    using var cmd = conn.CreateCommand();
    cmd.CommandText = """
            SELECT p.Id, p.UserId, p.Content, p.CreatedAtUtc, u.Username
            FROM Posts p
            JOIN Users u ON u.Id = p.UserId
            WHERE p.UserId = @uid
            ORDER BY p.Id DESC;
        """;
    cmd.Parameters.AddWithValue("@uid", userId);

    using var reader = await cmd.ExecuteReaderAsync();
    var list = new List<(Post, string)>();

    while (await reader.ReadAsync())
    {
      var post = new Post(
          reader.GetInt32(0),
          reader.GetInt32(1),
          reader.GetString(2),
          DateTime.Parse(reader.GetString(3))
      );
      var username = reader.GetString(4);
      list.Add((post, username));
    }

    return list;
  }

  public async Task<(bool ok, string? error, int? newId)> CreateAsync(int userId, string content)
  {
    if (userId <= 0) return (false, "UserId must be a positive integer.", null);
    if (string.IsNullOrWhiteSpace(content)) return (false, "Content is required.", null);

    using var conn = _db.OpenConnection();
    using var cmd = conn.CreateCommand();
    cmd.CommandText = """
            INSERT INTO Posts (UserId, Content, CreatedAtUtc)
            VALUES (@uid, @content, @c);
            SELECT last_insert_rowid();
        """;
    cmd.Parameters.AddWithValue("@uid", userId);
    cmd.Parameters.AddWithValue("@content", content.Trim());
    cmd.Parameters.AddWithValue("@c", DateTime.UtcNow.ToString("O"));

    try
    {
      var idObj = await cmd.ExecuteScalarAsync();
      var id = Convert.ToInt32(idObj);
      return (true, null, id);
    }
    catch (Microsoft.Data.Sqlite.SqliteException ex) when (ex.SqliteErrorCode == 19)
    {
      // FK-fel (user finns inte) eller constraint
      return (false, "User does not exist (foreign key constraint).", null);
    }
  }
}
