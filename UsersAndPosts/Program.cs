using UsersAndPosts.Shared;
using UsersAndPosts.User;
using UsersAndPosts.Post;
using Microsoft.AspNetCore.Http.Json;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "UsersAndPosts";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "UsersAndPosts.Client";
var jwtKey = builder.Configuration["Jwt:Key"] ?? "replace-this-dev-key-with-32-plus-chars";

if (string.IsNullOrWhiteSpace(builder.Configuration["urls"]))
{
    builder.WebHost.UseUrls("http://*:80", "https://*:443");
}

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "UsersAndPosts API",
        Version = "v1",
        Description = "Minimal API for users, posts, and JWT authentication."
    });

    options.AddSecurityDefinition("bearerAuth", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "JWT bearer token from POST /api/auth/login"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "bearerAuth"
                }
            },
            Array.Empty<string>()
        }
    });
});
builder.Services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                ValidIssuer = jwtIssuer,
                ValidAudience = jwtAudience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                ClockSkew = TimeSpan.Zero
            };
        });
builder.Services.AddAuthorization();
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
});

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
