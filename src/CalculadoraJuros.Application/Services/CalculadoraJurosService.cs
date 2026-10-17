using CalculadoraJuros.Application.Interfaces;
using CalculadoraJuros.Domain;
using CalculadoraJuros.Domain.Models;

namespace CalculadoraJuros.Application.Services;

public sealed class CalculadoraJurosService : ICalculadoraJurosService
{
    private readonly Calculadora _calculadora;

    public CalculadoraJurosService(Calculadora calculadora)
    {
        _calculadora = calculadora;
    }

    public ResultadoJuros Calcular(decimal valorInicial, decimal taxaMensalPercentual, int meses, TipoJuros tipoJuros)
    {
        return _calculadora.Calcular(valorInicial, taxaMensalPercentual, meses, tipoJuros);
    }
}
