// using SmartCharging.Contracts.Interfaces;

using System.Text.Json.Serialization;
using SmartCharging.Domain.Entities;
using SmartCharging.Persistence.Context;
using SmartCharging.Repository;
using SmartCharging.Service;
using SmartCharging.Service.Contracts;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});;
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<SmartChargingDbContext>();

// Registering Repositories
builder.Services.AddTransient(typeof(IRepository<Group>), typeof(GroupRepository));

// Registering Services
builder.Services.AddTransient(typeof(ISmartChargingService<Group>), typeof(SmartChargingService<Group>));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}



app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();