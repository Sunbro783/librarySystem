using LibraryApi.Data;
using Microsoft.EntityFrameworkCore;
using Prometheus;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Адреса берутся из переменных окружения (заданы в docker-compose.yml)
// При локальном запуске без Docker используются значения по умолчанию
var redisHost = Environment.GetEnvironmentVariable("REDIS_HOST") ?? "localhost";
var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING")
    ?? "Host=localhost;Database=librarydb;Username=postgres;Password=postgres";

builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect(redisHost));

builder.Services.AddDbContext<LibraryDbContext>(options =>
    options.UseNpgsql(connectionString));

var app = builder.Build();

// Применяем миграции автоматически при старте
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
    db.Database.Migrate();
}

app.UseSwagger();
app.UseSwaggerUI();

// Prometheus: сбор HTTP-метрик
app.UseHttpMetrics();

app.UseAuthorization();
app.MapControllers();

// Эндпоинт /metrics для Prometheus
app.MapMetrics();

app.Run();
