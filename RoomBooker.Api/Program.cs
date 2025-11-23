using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RoomBooker.Core.Services;
using RoomBooker.Infrastructure.Data;
using RoomBooker.Infrastructure.Services;
using FluentValidation;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 1. Database Configuration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=(localdb)\\MSSQLLocalDB;Database=RoomBookerDb;Trusted_Connection=True;MultipleActiveResultSets=true";

builder.Services.AddDbContext<RoomBookerDbContext>(options =>
    options.UseSqlServer(connectionString));

// 2. Register Core Services (Business Logic & Google)
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IRoomService, RoomService>();
builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddScoped<RoomBooker.Infrastructure.Services.GoogleAuthService>();

// 3. Security Setup (JWT Authentication & CORS)
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var key = builder.Configuration["Jwt:Key"] ?? "super-secret-key-12345-change-me-in-production";

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "RoomBooker",
        ValidAudience = builder.Configuration["Jwt:Audience"] ?? "RoomBookerUsers",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
    };
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<RoomBooker.Infrastructure.Data.DataSeeder>();
builder.Services.AddValidatorsFromAssemblyContaining<RoomBooker.Core.Validators.ReservationValidator>();

// 4. Build Application
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<RoomBooker.Infrastructure.Data.DataSeeder>();
    await seeder.SeedAsync();
}
// --- HTTP PIPELINE ---

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();