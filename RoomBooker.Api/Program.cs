using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RoomBooker.Infrastructure.Data;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Konfiguracja JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

// Dodaj us³ugê Swaggera
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "RoomBooker API", Version = "v1" });
});

builder.Services.AddControllers();
builder.Services.AddDbContext<RoomBookerDbContext>();

var app = builder.Build();

// Ustawienie middleware do autentykacji i autoryzacji
app.UseAuthentication();
app.UseAuthorization();

// Konfiguracja Swagger UI
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "RoomBooker API v1");
        c.RoutePrefix = string.Empty; // Swagger bêdzie dostêpny na root URL
    });
}

app.MapControllers();
app.Run();
