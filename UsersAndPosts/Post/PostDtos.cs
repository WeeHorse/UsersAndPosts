using UsersAndPosts.Shared;

namespace UsersAndPosts.Post;

public static class PostDtos
{
    [DtoContract("Post")]
    public sealed record PostDto(
        int Id,
        int UserId,
        string Content,
        DateTime CreatedAtUtc,
        string AuthorUsername
    );

    [DtoContract("Post")]
    public sealed record PostCreateDto(
        int UserId,
        string Content
    );
}
