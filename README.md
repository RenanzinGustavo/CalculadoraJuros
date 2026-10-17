# Calculadora de Juros — versão corrigida

Projeto didático em C# / .NET 8 com Windows Forms, API REST, separação por camadas e duas versões de API.

## O que está incluído

- `CalculadoraJuros.Domain`: modelos e regras matemáticas de juros simples e compostos.
- `CalculadoraJuros.Application`: interface de serviço e serviço de aplicação que delega o cálculo ao domínio.
- `CalculadoraJuros.Api`: API ASP.NET Core, injeção de dependências e endpoints V1/V2.
- `CalculadoraJuros.WinForms`: aplicação desktop que envia os dados para a API usando `HttpClient`.
- `CalculadoraJuros.sln`: solução para abrir no Visual Studio 2022.

## Requisitos

- Windows.
- Visual Studio 2022 com os workloads **Desenvolvimento para desktop com .NET** e **ASP.NET e desenvolvimento Web**.
- SDK do .NET 8 instalado.
- Acesso à internet na primeira compilação para restaurar o pacote do Swagger.

## Como executar

1. Extraia o ZIP para uma pasta.
2. Abra `CalculadoraJuros.sln` no Visual Studio 2022.
3. Compile a solução (`Compilar > Compilar Solução`).
4. Inicie primeiro o projeto `CalculadoraJuros.Api`. Se necessário, clique com o botão direito nele e escolha **Definir como Projeto de Inicialização**.
5. Confirme que a API está em `http://localhost:5080/swagger`. A página Swagger permite testar os endpoints.
6. Sem fechar a API, inicie também `CalculadoraJuros.WinForms` (clique com o botão direito no projeto e escolha **Depurar > Iniciar nova instância**, ou configure vários projetos de inicialização).
7. Na janela desktop, informe o valor inicial, a taxa mensal, o prazo, o tipo de juros e a versão da API; depois clique em **Calcular**.

Se a API iniciar em outra porta, atualize `ApiBaseUrl` no arquivo `src/CalculadoraJuros.WinForms/Api/JurosApiClient.cs` para corresponder à URL HTTP exibida pelo Visual Studio.

## Como o sistema funciona

1. A pessoa preenche os campos no Windows Forms.
2. `JurosApiClient` converte os valores em JSON e envia uma requisição HTTP `POST` para a versão escolhida.
3. O controller da API valida/recebe o contrato da versão e chama `ICalculadoraJurosService`.
4. O contêiner de injeção de dependências do .NET entrega `CalculadoraJurosService` ao controller e a `Calculadora` ao serviço.
5. A camada Domain valida os parâmetros e aplica a fórmula de juros.
6. A API devolve JSON; o cliente desktop transforma a resposta em valores formatados em reais.

## Organização e responsabilidades

- **Domain (domínio):** contém `TipoJuros`, `ResultadoJuros` e `Calculadora`. Não depende da interface gráfica nem de HTTP.
- **Application (aplicação):** contém `ICalculadoraJurosService` e `CalculadoraJurosService`, funcionando como fronteira entre a API e o domínio.
- **API:** recebe HTTP, expõe contratos de entrada/saída, registra dependências e chama a aplicação.
- **WinForms:** cuida da interface e conversa com a API. Não executa diretamente a fórmula.

### IoC e injeção de dependências (DI)

No `Program.cs` da API, `AddSingleton<Calculadora>()` e `AddScoped<ICalculadoraJurosService, CalculadoraJurosService>()` registram as implementações no contêiner do .NET. Os controllers recebem `ICalculadoraJurosService` no construtor. Assim, o framework cria e injeta o serviço em vez de o controller instanciá-lo com `new`.

### Boundaries de domínio

As regras financeiras e suas validações ficam em `CalculadoraJuros.Domain`. A API não deve duplicar a fórmula e o Windows Forms não deve calcular por conta própria. Isso separa regra de negócio de detalhes de interface e comunicação.

## Fórmulas

Considere:
- `C`: capital inicial;
- `i`: taxa mensal em forma decimal (por exemplo, 2% = 0,02);
- `n`: número de meses.

Juros simples:
- `Montante = C × (1 + i × n)`
- `Juros = Montante − C`

Juros compostos:
- `Montante = C × (1 + i)^n`
- `Juros = Montante − C`

O resultado monetário é arredondado para duas casas decimais. A API rejeita capital não positivo, taxa negativa, prazo fora de 1 a 600 meses e tipo de juros inválido. Se a conta ultrapassar o limite numérico de `decimal`, a API retorna erro HTTP 400 com uma mensagem.

## Versionamento e compatibilidade

Rotas:
- `POST /api/v1/juros`
- `POST /api/v2/juros`

A V1 usa o contrato original e devolve os campos de cálculo. A V2 aceita os mesmos campos e adiciona `incluirParcelaEstimada`; quando verdadeiro, devolve `parcelaEstimada` e `observacao`. Manter as rotas e os contratos separados permite evoluir a V2 sem alterar a V1, reduzindo o risco de quebrar clientes antigos.

### Exemplo de requisição V1

```json
{
  "valorInicial": 1000,
  "taxaMensalPercentual": 2,
  "meses": 12,
  "tipoJuros": "compostos"
}
```

### Exemplo de requisição V2

```json
{
  "valorInicial": 1000,
  "taxaMensalPercentual": 2,
  "meses": 12,
  "tipoJuros": "compostos",
  "incluirParcelaEstimada": true
}
```

Com esses valores, o montante é aproximadamente **R$ 1.268,24** e os juros são **R$ 268,24**. Na V2, a parcela estimada divide o montante pelo número de meses (aproximadamente R$ 105,69). É apenas uma estimativa matemática didática; não é uma simulação real de financiamento e não considera impostos, tarifas, seguros ou regras bancárias de amortização.

## Observações

- A solução foi revisada estaticamente nesta sessão, mas não foi possível executar `dotnet build` aqui porque o SDK do .NET não está instalado no ambiente. Compile no Visual Studio e, se aparecer algum erro, envie a mensagem para correção.
- O projeto usa Swagger para facilitar os testes da API.
