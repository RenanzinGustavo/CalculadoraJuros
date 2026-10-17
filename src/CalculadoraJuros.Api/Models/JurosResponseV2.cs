namespace CalculadoraJuros.Api.Models;

public sealed record JurosResponseV2(
    string VersaoApi,
    decimal ValorInicial,
    decimal Juros,
    decimal Montante,
    decimal TaxaMensalPercentual,
    int Meses,
    string TipoJuros,
    decimal? ParcelaEstimada);