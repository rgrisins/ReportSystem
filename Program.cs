using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Minio;
using ReportSystem.Data;
using ReportSystem.Models;
using ReportSystem.Services;
using StackExchange.Redis;
using System.Security.Claims;
using System.Text;

// Create the web application builder
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<ReportSystemDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure Redis connection
var muxer = ConnectionMultiplexer.Connect(builder.Configuration["Redis:Connection"]);

// Configure MinIO client
var secure = builder.Configuration.GetValue("Minio:Secure", false);
var minioBuilder = new MinioClient()
    .WithEndpoint(builder.Configuration["Minio:Endpoint"])
    .WithCredentials(builder.Configuration["Minio:Username"], builder.Configuration["Minio:Password"]);
if (secure)
    minioBuilder = minioBuilder.WithSSL();
var minioClient = minioBuilder.Build();

// Register services for dependency injection
builder.Services.AddSingleton<IConnectionMultiplexer>(muxer);
builder.Services.AddSingleton<JwtService>();
builder.Services.AddSingleton<SessionService>();
builder.Services.AddScoped<FileService>();
builder.Services.AddSingleton<IMinioClient>(minioClient);

builder.Services.AddDefaultIdentity<User>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
})
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ReportSystemDbContext>()
    .AddDefaultTokenProviders();

// Add controllers with views support
builder.Services.AddControllersWithViews();

// Configure antiforgery token
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
});

// Configure JWT authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JwtConfig:Issuer"],
            ValidAudience = builder.Configuration["JwtConfig:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
              Encoding.UTF8.GetBytes(builder.Configuration["JwtConfig:Key"] ?? string.Empty)
            ),
            ClockSkew = TimeSpan.Zero,
            RoleClaimType = ClaimTypes.Role,
            NameClaimType = ClaimTypes.Name
        };
    });

// Add authorization services
builder.Services.AddAuthorization();

// Build the web application
var app = builder.Build();

// Seed the database with initial data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var db = services.GetRequiredService<ReportSystemDbContext>();
    await db.Database.MigrateAsync();
    await SeedData.Initialize(services);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Only use HTTPS redirection in development
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseRouting();

app.UseMiddleware<ReportSystem.Middlewares.TokenRefreshMiddleware>();

app.Use(async (context, next) =>
{
    var token = context.Request.Cookies["accessToken"];
    if (!string.IsNullOrEmpty(token) && !context.Request.Headers.ContainsKey("Authorization"))
        context.Request.Headers.Authorization = $"Bearer {token}";

    await next();
});

app.UseAuthentication();

app.UseStatusCodePages(async context =>
{
    var http = context.HttpContext;
    var status = http.Response.StatusCode;
    var originalPath = Uri.EscapeDataString(http.Request.Path + http.Request.QueryString);

    switch (status)
    {
        case 401:
            http.Response.Redirect("/Auth/Login");
            break;

        case 403:
            http.Response.Redirect($"/Home/ForbiddenPage?originalPath={originalPath}");
            break;

        case 404:
            http.Response.Redirect($"/Home/NotFoundPage?originalPath={originalPath}");
            break;
    }
});

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// Run the application
app.Run();