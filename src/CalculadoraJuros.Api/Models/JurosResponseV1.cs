namespace CalculadoraJuros.Api.Models;

public sealed record JurosResponseV1(
    string VersaoApi,
    decimal ValorInicial,
    decimal Juros,
    decimal Montante,
    decimal TaxaMensalPercentual,
    int Meses,
    string TipoJuros);
