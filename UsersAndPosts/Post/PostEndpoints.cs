using System.Security.Claims;
using UsersAndPosts.User;

namespace UsersAndPosts.Post;

public static class PostEndpoints
{
  public static void MapPostEndpoints(this IEndpointRouteBuilder app)
  {
    app.MapGet("posts", async (PostRepo repo) =>
    {
      var items = await repo.GetAllAsync();
      var dtos = items.Select(x => new PostDtos.PostDto(
              x.post.Id, x.post.UserId, x.post.Content, x.post.CreatedAtUtc, x.authorUsername
          ));
      return Results.Ok(dtos);
    });

    app.MapGet("users/{userId:int}/posts", async (int userId, PostRepo repo, UserRepo userRepo) =>
    {
      var user = await userRepo.GetByIdAsync(userId);
      if (user is null) return Results.NotFound(new { error = "User not found." });

      var items = await repo.GetByUserIdAsync(userId);
      var dtos = items.Select(x => new PostDtos.PostDto(
              x.post.Id, x.post.UserId, x.post.Content, x.post.CreatedAtUtc, x.authorUsername
          ));
      return Results.Ok(dtos);
    });

    app.MapPost("posts", async (PostDtos.PostCreateDto dto, PostRepo repo, UserRepo userRepo, ClaimsPrincipal user) =>
    {
      var userIdClaim = user.FindFirstValue(ClaimTypes.NameIdentifier);
      if (!int.TryParse(userIdClaim, out var userId))
      {
        return Results.Unauthorized();
      }

      var existingUser = await userRepo.GetByIdAsync(userId);
      if (existingUser is null) return Results.Unauthorized();

      var (ok, error, newId) = await repo.CreateAsync(userId, dto.Content);
      if (!ok) return Results.BadRequest(new { error });

      return Results.Created($"posts/{newId}", new { id = newId });
    }).RequireAuthorization();
  }
}
