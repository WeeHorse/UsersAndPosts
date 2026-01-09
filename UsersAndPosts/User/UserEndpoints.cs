namespace UsersAndPosts.User;

public static class UserEndpoints
{
  public static void MapUserEndpoints(this IEndpointRouteBuilder app)
  {
    app.MapGet("users", async (UserRepo repo) =>
    {
      var users = await repo.GetAllAsync();
      var dtos = users.Select(u => new UserDtos.UserDto(u.Id, u.Username, u.DisplayName, u.CreatedAtUtc));
      return Results.Ok(dtos);
    });

    app.MapGet("users/{id:int}", async (int id, UserRepo repo) =>
    {
      var user = await repo.GetByIdAsync(id);
      return user is null
              ? Results.NotFound()
              : Results.Ok(new UserDtos.UserDto(user.Id, user.Username, user.DisplayName, user.CreatedAtUtc));
    });

    app.MapPost("users", async (UserDtos.UserCreateDto dto, UserRepo repo) =>
    {
      var (ok, error, newId) = await repo.CreateAsync(dto.Username, dto.DisplayName);
      if (!ok) return Results.BadRequest(new { error });

      var created = await repo.GetByIdAsync(newId!.Value);
      return Results.Created($"users/{newId}", new UserDtos.UserDto(
              created!.Id, created.Username, created.DisplayName, created.CreatedAtUtc));
    });
  }
}
