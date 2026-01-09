namespace UsersAndPosts.User;

public sealed record User(
    int Id,
    string Username,
    string DisplayName,
    DateTime CreatedAtUtc
);

