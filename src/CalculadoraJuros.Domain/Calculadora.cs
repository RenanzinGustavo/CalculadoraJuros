using CalculadoraJuros.Domain.Models;

namespace CalculadoraJuros.Domain;

public sealed class Calculadora
{
    public ResultadoJuros Calcular(
        decimal valorInicial,
        decimal taxaMensalPercentual,
        int meses,
        TipoJuros tipoJuros)
    {
        if (valorInicial <= 0)
            throw new ArgumentOutOfRangeException(nameof(valorInicial), "O valor inicial deve ser maior que zero.");

        if (taxaMensalPercentual < 0)
            throw new ArgumentOutOfRangeException(nameof(taxaMensalPercentual), "A taxa não pode ser negativa.");

        if (meses <= 0 || meses > 600)
            throw new ArgumentOutOfRangeException(nameof(meses), "O prazo deve estar entre 1 e 600 meses.");

        decimal taxa = taxaMensalPercentual / 100m;
        decimal montante;

        switch (tipoJuros)
        {
            case TipoJuros.Simples:
                montante = valorInicial * (1m + taxa * meses);
                break;

            case TipoJuros.Compostos:
                // Cálculo decimal iterativo: evita converter o resultado de Math.Pow (double).
                montante = valorInicial;
                for (int i = 0; i < meses; i++)
                    montante *= 1m + taxa;
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(tipoJuros), "Tipo de juros inválido.");
        }

        montante = decimal.Round(montante, 2, MidpointRounding.AwayFromZero);
        decimal juros = decimal.Round(montante - valorInicial, 2, MidpointRounding.AwayFromZero);

        return new ResultadoJuros(
            valorInicial,
            juros,
            montante,
            taxaMensalPercentual,
            meses,
            tipoJuros);
    }
}
