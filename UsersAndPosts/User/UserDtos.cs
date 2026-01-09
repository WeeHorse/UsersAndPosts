using UsersAndPosts.Shared;

namespace UsersAndPosts.User;

public static class UserDtos
{
  [DtoContract("User")]
  public sealed record UserDto(
      int Id,
      string Username,
      string DisplayName,
      DateTime CreatedAtUtc
  );

  [DtoContract("User")]
  public sealed record UserCreateDto(
      string Username,
      string DisplayName
  );
}
