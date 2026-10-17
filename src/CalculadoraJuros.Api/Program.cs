using CalculadoraJuros.Application.Interfaces;
using CalculadoraJuros.Application.Services;
using CalculadoraJuros.Domain;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// IoC/DI: o contêiner cria e injeta as dependências.
builder.Services.AddSingleton<Calculadora>();
builder.Services.AddScoped<ICalculadoraJurosService, CalculadoraJurosService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();

app.Run();
