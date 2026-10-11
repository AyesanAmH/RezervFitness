using Microsoft.EntityFrameworkCore;
using RezervFitness.API.DAO;
using RezervFitness.API.Data;
using RezervFitness.API.Services;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

//services
builder.Services.AddScoped<PackageDao>();
builder.Services.AddScoped<PackageService>();

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

//connection
var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "DefaultConnection is missing from configuration.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString)));

//package
builder.Services.AddScoped<PackageDao>();
builder.Services.AddScoped<PackageService>();

//timetable
builder.Services.AddScoped<TimetableDao>();
builder.Services.AddScoped<TimetableService>();

//booking
builder.Services.AddScoped<BookingDao>();
builder.Services.AddScoped<BookingService>();

//waitlist
builder.Services.AddScoped<WaitlistService>();


//redis
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration =
        builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "RezervFitness:";
});

builder.Services.AddSingleton<TimetableCacheService>();

builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
    ConnectionMultiplexer.Connect(
        builder.Configuration.GetConnectionString("Redis")!));

builder.Services.AddSingleton<TimetableCacheService>();

var app = builder.Build();




//seeding
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider
        .GetRequiredService<AppDbContext>();

    await DbSeeder.SeedAsync(context);
}

//swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
