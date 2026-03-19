using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace UsersAndPosts.User;

public static class UserEndpoints
{
  public static void MapUserEndpoints(this IEndpointRouteBuilder app)
  {
    app.MapPost("auth/login", async (UserDtos.LoginDto dto, UserRepo repo, HttpContext httpContext) =>
    {
      if (string.IsNullOrWhiteSpace(dto.Username))
      {
        return Results.BadRequest(new { error = "Username is required." });
      }

      if (string.IsNullOrWhiteSpace(dto.Password))
      {
        return Results.BadRequest(new { error = "Password is required." });
      }

      var user = await repo.ValidateCredentialsAsync(dto.Username.Trim(), dto.Password);
      if (user is null)
      {
        return Results.BadRequest(new { error = "Invalid username or password." });
      }

      var claims = new List<Claim>
      {
        new(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new(ClaimTypes.Name, user.Username),
        new("display_name", user.DisplayName)
      };

      var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
      var principal = new ClaimsPrincipal(identity);

      await httpContext.SignInAsync(
          CookieAuthenticationDefaults.AuthenticationScheme,
          principal,
          new AuthenticationProperties { IsPersistent = true }
      );

      return Results.Ok(new UserDtos.SessionUserDto(user.Id, user.Username, user.DisplayName));
    });

    app.MapPost("auth/logout", async (HttpContext httpContext) =>
    {
      await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
      return Results.NoContent();
    });

    app.MapGet("auth/me", async (HttpContext httpContext, UserRepo repo) =>
    {
      if (httpContext.User?.Identity?.IsAuthenticated != true)
      {
        return Results.Unauthorized();
      }

      var userIdClaim = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
      if (!int.TryParse(userIdClaim, out var userId))
      {
        return Results.Unauthorized();
      }

      var user = await repo.GetByIdAsync(userId);
      if (user is null)
      {
        return Results.Unauthorized();
      }

      return Results.Ok(new UserDtos.SessionUserDto(user.Id, user.Username, user.DisplayName));
    });

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
      var (ok, error, newId) = await repo.CreateAsync(dto.Username, dto.Password, dto.DisplayName);
      if (!ok) return Results.BadRequest(new { error });

      var created = await repo.GetByIdAsync(newId!.Value);
      return Results.Created($"users/{newId}", new UserDtos.UserDto(
              created!.Id, created.Username, created.DisplayName, created.CreatedAtUtc));
    });
  }
}
