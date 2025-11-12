using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ReportSystem.Data;
using ReportSystem.Models;
using ReportSystem.Services;
using StackExchange.Redis;
using System.Text;

// Initialize Redis connection
var muxer = ConnectionMultiplexer.Connect("localhost");

// Create the web application builder
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<ReportSystemContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ReportSystemContext") ?? throw new InvalidOperationException("Connection string 'ReportSystemContext' not found.")));

// Register services for dependency injection
builder.Services.AddSingleton<IConnectionMultiplexer>(muxer);
builder.Services.AddSingleton<JwtService>();
builder.Services.AddSingleton<SessionService>();
builder.Services.AddScoped<UserSessionService>();

builder.Services.AddDefaultIdentity<User>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
})
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ReportSystemContext>()
    .AddDefaultTokenProviders();

// Add controllers with views support
builder.Services.AddControllersWithViews();

// Configure JWT authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
                Encoding.UTF8.GetBytes(builder.Configuration["JwtConfig:Key"])
            ),
            ClockSkew = TimeSpan.Zero
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

    await SeedData.Initialize(services);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Middleware configuration
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();

app.Use(async (context, next) =>
{
    if (context.User?.Identity?.IsAuthenticated != true)
    {
        var token = context.Request.Cookies["accessToken"];
        if (!string.IsNullOrEmpty(token))
        {
            var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            var claims = jwt.Claims.Select(c =>
            {
                if (c.Type == "role")
                    return new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, c.Value);
                return new System.Security.Claims.Claim(c.Type, c.Value);
            });

            var identity = new System.Security.Claims.ClaimsIdentity(claims, JwtBearerDefaults.AuthenticationScheme);
            context.User = new System.Security.Claims.ClaimsPrincipal(identity);
        }
    }

    await next();
});

app.UseStatusCodePages(context =>
{
    var response = context.HttpContext.Response;

    if (response.StatusCode == 401)
    {
        context.HttpContext.Response.Redirect("/Auth/Login");
    }
    else if (response.StatusCode == 403)
    {
        var originalPath = context.HttpContext.Request.Path;
        context.HttpContext.Response.Redirect($"/Home/ForbiddenPage?originalPath={originalPath}");
    }
    else if (response.StatusCode == 404)
    {
        context.HttpContext.Response.Redirect("/Home/NotFoundPage");
    }
    return Task.CompletedTask;
});

app.UseAuthorization();
app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// Run the application
app.Run();
