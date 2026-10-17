using System.ComponentModel.DataAnnotations;

namespace CalculadoraJuros.Api.Models;

public class JurosRequestV1
{
    [Range(typeof(decimal), "0.01", "79228162514264337593543950335",
    ParseLimitsInInvariantCulture = true)]
    public decimal ValorInicial { get; set; }

    [Range(typeof(decimal), "0", "1000")]
    public decimal TaxaMensalPercentual { get; set; }

    [Range(1, 600)]
    public int Meses { get; set; }

    [Required]
    [RegularExpression("(?i)^(simples|compostos)$", ErrorMessage = "TipoJuros deve ser 'simples' ou 'compostos'.")]
    public string TipoJuros { get; set; } = "compostos";
}
