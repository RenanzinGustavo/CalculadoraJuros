using System.Net.Http.Json;
using System.Text.Json;

namespace CalculadoraJuros.WinForms.Api;

public sealed class JurosApiClient : IDisposable
{
    // Se a API estiver em outra porta, altere esta URL.
    private const string ApiBaseUrl = "http://localhost:5080/";
    private readonly HttpClient _httpClient = new() { BaseAddress = new Uri(ApiBaseUrl) };

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<JurosApiResponse> CalcularAsync(
        string versao,
        decimal valorInicial,
        decimal taxaMensalPercentual,
        int meses,
        string tipoJuros,
        CancellationToken cancellationToken = default)
    {
        object request = versao == "v2"
            ? new
            {
                valorInicial,
                taxaMensalPercentual,
                meses,
                tipoJuros,
                incluirParcelaEstimada = true
            }
            : new
            {
                valorInicial,
                taxaMensalPercentual,
                meses,
                tipoJuros
            };

        string endpoint = versao == "v2" ? "api/v2/juros" : "api/v1/juros";
        using HttpResponseMessage response = await _httpClient.PostAsJsonAsync(endpoint, request, cancellationToken);

        string body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            try
            {
                using var document = JsonDocument.Parse(body);
                if (document.RootElement.TryGetProperty("erro", out var erro))
                    throw new InvalidOperationException(erro.GetString() ?? "A API rejeitou os dados.");
                if (document.RootElement.TryGetProperty("title", out var title))
                    throw new InvalidOperationException(title.GetString() ?? "Dados inválidos.");
            }
            catch (JsonException)
            {
                // Usa o corpo da resposta abaixo se não for JSON válido.
            }

            throw new InvalidOperationException($"Erro da API ({(int)response.StatusCode}): {body}");
        }

        return JsonSerializer.Deserialize<JurosApiResponse>(body, JsonOptions)
            ?? throw new InvalidOperationException("A API retornou uma resposta vazia.");
    }

    public void Dispose() => _httpClient.Dispose();
}

public sealed class JurosApiResponse
{
    public string VersaoApi { get; set; } = "";
    public decimal ValorInicial { get; set; }
    public decimal Juros { get; set; }
    public decimal Montante { get; set; }
    public decimal TaxaMensalPercentual { get; set; }
    public int Meses { get; set; }
    public string TipoJuros { get; set; } = "";
    public decimal? ParcelaEstimada { get; set; }

}
