namespace UsersAndPosts.Post;

public sealed record Post(
    int Id,
    int UserId,
    string Content,
    DateTime CreatedAtUtc
);
