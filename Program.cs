using Microsoft.EntityFrameworkCore;
using ApiBackend.Models;
using ApiBackend.Models.Context;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddDbContext<TestContext>(opt =>
    opt.UseInMemoryDatabase("TestList"));
builder.Services.AddDbContext<AnalyticsReportContext>(opt =>
    opt.UseInMemoryDatabase("AnalyticsReportList"));
builder.Services.AddDbContext<ComplienceReportContext>(opt =>
    opt.UseInMemoryDatabase("ComplienceReportList"));
builder.Services.AddDbContext<TaskContext>(opt =>
    opt.UseInMemoryDatabase("TaskList"));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
