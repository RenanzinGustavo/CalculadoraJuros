namespace CalculadoraJuros.Api.Models;

// V2 mantém os campos da V1 e acrescenta uma opção nova.
public sealed class JurosRequestV2 : JurosRequestV1
{
    public bool IncluirParcelaEstimada { get; set; } = true;
}
