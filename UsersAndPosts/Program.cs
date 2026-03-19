using UsersAndPosts.Shared;
using UsersAndPosts.User;
using UsersAndPosts.Post;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services
        .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie(options =>
        {
            options.Cookie.Name = "usersandposts.session";
            options.SlidingExpiration = true;
            options.Events = new CookieAuthenticationEvents
            {
                OnRedirectToLogin = context =>
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return Task.CompletedTask;
                },
                OnRedirectToAccessDenied = context =>
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    return Task.CompletedTask;
                }
            };
        });
builder.Services.AddAuthorization();

builder.Services.AddSingleton<Db>();
builder.Services.AddSingleton<UserRepo>();
builder.Services.AddSingleton<PostRepo>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

// 1) Servera klienten från wwwroot på /
app.UseDefaultFiles();
app.UseStaticFiles();

// 2) API under /api (separat namespace)
var api = app.MapGroup("/api");

// Endpoints för API (se nästa steg)
api.MapUserEndpoints();
api.MapPostEndpoints();

// DTO-kontraktet under /api/dtos
api.MapGet("/dtos", async (IWebHostEnvironment env) =>
{
    var path = Path.Combine(env.ContentRootPath, "dtos.json");
    var json = await File.ReadAllTextAsync(path);
    return Results.Text(json, "application/json");
});

// DB seed
var db = app.Services.GetRequiredService<Db>();
await DbSeeder.SeedAsync(db);

// 3) SPA fallback: allt som inte matchar fil eller API-route => index.html
app.MapFallbackToFile("index.html");

app.Run();
