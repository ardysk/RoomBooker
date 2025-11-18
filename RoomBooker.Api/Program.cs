using Microsoft.EntityFrameworkCore;
using RoomBooker.Infrastructure.Data;
using RoomBooker.Core.Services;
using RoomBooker.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// 1) Rejestrujemy kontrolery (zamiast Razor Pages)
builder.Services.AddControllers();

// 2) Swagger – ¿eby mieæ dokumentacjê / testowanie endpointów
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<RoomBookerDbContext>(options =>
{
    options.UseSqlServer(connectionString);
});

builder.Services.AddScoped<IRoomService, RoomService>();

// (Na póŸniej – tu bêdziemy dodawaæ DbContext, serwisy domenowe, auth itd.)

var app = builder.Build();

// 3) Swagger tylko w trybie deweloperskim (na potrzeby projektu mo¿esz mieæ zawsze)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// (Tu kiedyœ dodamy app.UseAuthentication();)
app.UseAuthorization();

// 4) Mapujemy kontrolery
app.MapControllers();

app.Run();
