using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ReportSystem.Data;
using ReportSystem.Models;
using ReportSystem.Services;
using StackExchange.Redis;
using System.Text;

var muxer = ConnectionMultiplexer.Connect("localhost");

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<ReportSystemContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ReportSystemContext") ?? throw new InvalidOperationException("Connection string 'ReportSystemContext' not found.")));

builder.Services.AddSingleton<IConnectionMultiplexer>(muxer);
builder.Services.AddSingleton<JwtService>();
builder.Services.AddSingleton<SessionService>();
builder.Services.AddScoped<UserSessionService>();

builder.Services.AddControllersWithViews();

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

builder.Services.AddAuthorization();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    SeedData.Initialize(services);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseStatusCodePagesWithReExecute("/Home/NotFoundPage");

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
