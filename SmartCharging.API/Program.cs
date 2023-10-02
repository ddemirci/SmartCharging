using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json.Serialization;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SmartCharging.API.Manager;
using SmartCharging.API.Requests.Validators;
using SmartCharging.Domain.Entities;
using SmartCharging.Persistence.Context;
using SmartCharging.Repository;

var builder = WebApplication.CreateBuilder(args);

IEnumerable<Assembly> mapperAssemblies = new[]
{
    AppDomain.CurrentDomain.Load("SmartCharging.API")
};

// Add services to the container.

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<SmartChargingDbContext>();
builder.Services.AddAutoMapper(mapperAssemblies);
// Registering Repositories
builder.Services.AddTransient(typeof(IRepository<Group>), typeof(GroupRepository));

// Registering Managers
builder.Services.AddTransient<GroupManager>();

// Registering FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<CreateGroupRequestValidator>();
builder.Services.AddFluentValidationAutoValidation();
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