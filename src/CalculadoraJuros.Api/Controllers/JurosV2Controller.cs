using CalculadoraJuros.Api.Models;
using CalculadoraJuros.Application.Interfaces;
using CalculadoraJuros.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace CalculadoraJuros.Api.Controllers;

[ApiController]
[Route("api/v2/juros")]
public sealed class JurosV2Controller : ControllerBase
{
    private readonly ICalculadoraJurosService _service;

    public JurosV2Controller(ICalculadoraJurosService service)
    {
        _service = service;
    }

    [HttpPost]
    [ProducesResponseType(typeof(JurosResponseV2), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<JurosResponseV2> Calcular([FromBody] JurosRequestV2 request)
    {
        try
        {
            TipoJuros tipo = request.TipoJuros.ToLowerInvariant() switch
            {
                "simples" => TipoJuros.Simples,
                "compostos" => TipoJuros.Compostos,
                _ => throw new ArgumentException("TipoJuros deve ser 'simples' ou 'compostos'.")
            };

            var resultado = _service.Calcular(
                request.ValorInicial,
                request.TaxaMensalPercentual,
                request.Meses,
                tipo);

            decimal? parcela = null;
            if (request.IncluirParcelaEstimada)
            {
                // Estimativa: montante dividido igualmente pelo número de meses.
                // Não representa necessariamente uma parcela bancária real.
                parcela = decimal.Round(resultado.Montante / request.Meses, 2, MidpointRounding.AwayFromZero);
            }

            return Ok(new JurosResponseV2(
            "v2",
            resultado.ValorInicial,
            resultado.Juros,
            resultado.Montante,
            resultado.TaxaMensalPercentual,
            resultado.Meses,
            resultado.TipoJuros.ToString().ToLowerInvariant(),
            parcela));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { erro = ex.Message });
        }
        catch (OverflowException)
        {
            return BadRequest(new { erro = "O resultado excedeu o limite numérico suportado. Reduza a taxa, o valor inicial ou o prazo." });
        }
    }
}
