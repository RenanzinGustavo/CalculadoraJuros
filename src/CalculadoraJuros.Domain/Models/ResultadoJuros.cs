namespace CalculadoraJuros.Domain.Models;

public sealed record ResultadoJuros(
    decimal ValorInicial,
    decimal Juros,
    decimal Montante,
    decimal TaxaMensalPercentual,
    int Meses,
    TipoJuros TipoJuros);
