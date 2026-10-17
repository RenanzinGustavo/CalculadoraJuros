using CalculadoraJuros.Api.Models;
using CalculadoraJuros.Application.Interfaces;
using CalculadoraJuros.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace CalculadoraJuros.Api.Controllers;

[ApiController]
[Route("api/v1/juros")]
public sealed class JurosV1Controller : ControllerBase
{
    private readonly ICalculadoraJurosService _service;

    public JurosV1Controller(ICalculadoraJurosService service)
    {
        _service = service;
    }

    [HttpPost]
    [ProducesResponseType(typeof(JurosResponseV1), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<JurosResponseV1> Calcular([FromBody] JurosRequestV1 request)
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

            return Ok(new JurosResponseV1(
                "v1",
                resultado.ValorInicial,
                resultado.Juros,
                resultado.Montante,
                resultado.TaxaMensalPercentual,
                resultado.Meses,
                resultado.TipoJuros.ToString().ToLowerInvariant()));
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