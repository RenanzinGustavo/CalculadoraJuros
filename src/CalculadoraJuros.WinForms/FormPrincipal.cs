using System.Globalization;
using CalculadoraJuros.WinForms.Api;

namespace CalculadoraJuros.WinForms;

public sealed class FormPrincipal : Form
{
    private readonly NumericUpDown _valor = new();
    private readonly NumericUpDown _taxa = new();
    private readonly NumericUpDown _meses = new();
    private readonly ComboBox _tipo = new();
    private readonly ComboBox _versao = new();
    private readonly Button _calcular = new();
    private readonly Label _status = new();
    private readonly Label _resultado = new();
    private readonly JurosApiClient _apiClient = new();

    public FormPrincipal()
    {
        Text = "Calculadora de Juros";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(560, 520);
        Size = new Size(640, 600);
        Font = new Font("Segoe UI", 10);
        BuildInterface();
    }

    private void BuildInterface()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(22),
            ColumnCount = 2,
            RowCount = 8,
            AutoSize = false
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58));
        for (int i = 0; i < 8; i++)
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, i == 0 ? 48 : i == 6 ? 54 : 42));

        var titulo = new Label
        {
            Text = "Calculadora de Juros",
            Font = new Font("Segoe UI", 17, FontStyle.Bold),
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft
        };
        root.Controls.Add(titulo, 0, 0);
        root.SetColumnSpan(titulo, 2);

        ConfigurarNumero(_valor, 0.01m, 1000000000m, 1000m, 2);
        ConfigurarNumero(_taxa, 0m, 1000m, 2m, 2);
        ConfigurarNumero(_meses, 1m, 600m, 12m, 0);

        _tipo.DropDownStyle = ComboBoxStyle.DropDownList;
        _tipo.Items.AddRange(new object[] { "Compostos", "Simples" });
        _tipo.SelectedIndex = 0;
        _versao.DropDownStyle = ComboBoxStyle.DropDownList;
        _versao.Items.AddRange(new object[] { "V1 — contrato original", "V2 — inclui parcela estimada" });
        _versao.SelectedIndex = 1;

        AdicionarCampo(root, "Valor inicial (R$)", _valor, 1);
        AdicionarCampo(root, "Taxa mensal (%)", _taxa, 2);
        AdicionarCampo(root, "Prazo (meses)", _meses, 3);
        AdicionarCampo(root, "Tipo de juros", _tipo, 4);
        AdicionarCampo(root, "Versão da API", _versao, 5);

        _calcular.Text = "Calcular";
        _calcular.Dock = DockStyle.Fill;
        _calcular.Height = 42;
        _calcular.Click += async (_, _) => await CalcularAsync();
        root.Controls.Add(_calcular, 0, 6);
        root.SetColumnSpan(_calcular, 2);

        _status.Text = "API: http://localhost:5080";
        _status.Dock = DockStyle.Fill;
        _status.ForeColor = Color.DimGray;
        _status.TextAlign = ContentAlignment.MiddleLeft;
        root.Controls.Add(_status, 0, 7);
        root.SetColumnSpan(_status, 2);

        var panel = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
        var outer = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2
        };

        outer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        outer.RowStyles.Add(new RowStyle(SizeType.Absolute, 430));
        outer.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        root.Dock = DockStyle.Top;
        root.Height = 390;
        panel.Controls.Add(root);

        _resultado.Text = "Preencha os dados e clique em Calcular.";
        _resultado.Dock = DockStyle.Fill;
        _resultado.Padding = new Padding(24, 10, 10, 10);
        _resultado.Font = new Font("Segoe UI", 11);
        _resultado.AutoSize = false;

        outer.Controls.Add(panel, 0, 0);
        outer.Controls.Add(_resultado, 0, 1);
        Controls.Add(outer);
    }

    private static void ConfigurarNumero(NumericUpDown controle, decimal min, decimal max, decimal valor, int casas)
    {
        controle.Minimum = min;
        controle.Maximum = max;
        controle.Value = valor;
        controle.DecimalPlaces = casas;
        controle.Increment = casas == 0 ? 1m : 0.5m;
        controle.Dock = DockStyle.Fill;
        controle.ThousandsSeparator = true;
    }

    private static void AdicionarCampo(TableLayoutPanel painel, string texto, Control controle, int linha)
    {
        var label = new Label
        {
            Text = texto,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft
        };
        painel.Controls.Add(label, 0, linha);
        painel.Controls.Add(controle, 1, linha);
    }

    private async Task CalcularAsync()
    {
        _calcular.Enabled = false;
        _status.Text = "Calculando...";
        try
        {
            string versao = _versao.SelectedIndex == 1 ? "v2" : "v1";
            string tipo = _tipo.SelectedIndex == 1 ? "simples" : "compostos";

            JurosApiResponse resposta = await _apiClient.CalcularAsync(
                versao,
                _valor.Value,
                _taxa.Value,
                decimal.ToInt32(_meses.Value),
                tipo);

            var ptBr = CultureInfo.GetCultureInfo("pt-BR");
            string texto = $"Versão da API: {resposta.VersaoApi.ToUpperInvariant()}\n\n" +
                           $"Valor inicial: {resposta.ValorInicial.ToString("C2", ptBr)}\n" +
                           $"Juros calculados: {resposta.Juros.ToString("C2", ptBr)}\n" +
                           $"Montante final: {resposta.Montante.ToString("C2", ptBr)}\n" +
                           $"Taxa mensal: {resposta.TaxaMensalPercentual:N2}%\n" +
                           $"Prazo: {resposta.Meses} meses\n" +
                           $"Tipo: {resposta.TipoJuros}";

            if (resposta.ParcelaEstimada.HasValue)
                texto += $"\nParcela mensal estimada: {resposta.ParcelaEstimada.Value.ToString("C2", ptBr)}";


            _resultado.Text = texto;
            _status.Text = "Cálculo concluído.";
        }
        catch (HttpRequestException)
        {
            _status.Text = "Não foi possível conectar à API.";
            MessageBox.Show(
                "Verifique se o projeto CalculadoraJuros.Api está em execução e se a URL é http://localhost:5080.",
                "Falha de conexão",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }
        catch (Exception ex)
        {
            _status.Text = "Não foi possível concluir o cálculo.";
            MessageBox.Show(ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _calcular.Enabled = true;
        }
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        _apiClient.Dispose();
        base.OnFormClosed(e);
    }
}