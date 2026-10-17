using CalculadoraJuros.Domain.Models;

namespace CalculadoraJuros.Application.Interfaces;

public interface ICalculadoraJurosService
{
    ResultadoJuros Calcular(decimal valorInicial, decimal taxaMensalPercentual, int meses, TipoJuros tipoJuros);
}
