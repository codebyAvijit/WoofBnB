using Microsoft.EntityFrameworkCore;
using WoofBnB.Infrastructure.Persistence;
using WoofBnB.Application.PetSitters;
using WoofBnB.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// SERVICES
// ============================================================

// Controllers
builder.Services.AddControllers();

// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database
builder.Services.AddDbContext<WoofBnBDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

builder.Services.AddScoped<IPetSitterRepository, PetSitterRepository>();

builder.Services.AddScoped<IPetSitterService, PetSitterService>();

// Health Checks

builder.Services.AddHealthChecks()
    .AddSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")!
    );

var app = builder.Build();

// ============================================================
// HTTP REQUEST PIPELINE
// ============================================================

// Development tools
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// HTTPS
app.UseHttpsRedirection();

// API Controllers
app.MapControllers();

// Health Check
app.MapHealthChecks("/health");

app.Run();