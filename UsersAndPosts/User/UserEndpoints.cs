using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace UsersAndPosts.User;

public static class UserEndpoints
{
  public static void MapUserEndpoints(this IEndpointRouteBuilder app)
  {
    app.MapPost("auth/login", async (UserDtos.LoginDto dto, UserRepo repo, IConfiguration config) =>
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

      var issuer = config["Jwt:Issuer"] ?? "UsersAndPosts";
      var audience = config["Jwt:Audience"] ?? "UsersAndPosts.Client";
      var key = config["Jwt:Key"] ?? "replace-this-dev-key-with-32-plus-chars";
      var expiresInMinutes = int.TryParse(config["Jwt:ExpiresMinutes"], out var configuredMinutes)
          ? configuredMinutes
          : 60;

      Claim[] claims =
      {
        new(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new(ClaimTypes.Name, user.Username),
        new("display_name", user.DisplayName)
      };

      var expiresAt = DateTime.UtcNow.AddMinutes(expiresInMinutes);
      var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
      var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

      var jwt = new JwtSecurityToken(
          issuer: issuer,
          audience: audience,
          claims: claims,
          notBefore: DateTime.UtcNow,
          expires: expiresAt,
          signingCredentials: credentials
      );

      var token = new JwtSecurityTokenHandler().WriteToken(jwt);

      return Results.Ok(new UserDtos.AuthLoginResponseDto(
          token,
          "Bearer",
          (int)Math.Max(1, (expiresAt - DateTime.UtcNow).TotalSeconds),
          new UserDtos.SessionUserDto(user.Id, user.Username, user.DisplayName)
      ));
    });

    app.MapPost("auth/logout", () =>
    {
      // Stateless JWT has no server-side session to clear.
      return Results.NoContent();
    });

    app.MapGet("auth/me", async (ClaimsPrincipal principal, UserRepo repo) =>
    {
      if (principal.Identity?.IsAuthenticated != true)
      {
        return Results.Unauthorized();
      }

      var userIdClaim = principal.FindFirstValue(ClaimTypes.NameIdentifier);
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
    }).RequireAuthorization();

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
