# RPA - Exchange Rate Collector

Sistema automatizado para coleta periódica de taxas de câmbio USD/BRL através de web scraping, construído com .NET 10 e Clean Architecture.

## 📋 Índice

- [Visão Geral](#visão-geral)
- [Arquitetura](#arquitetura)
- [Estrutura do Projeto](#estrutura-do-projeto)
- [Tecnologias Utilizadas](#tecnologias-utilizadas)
- [Como Executar](#como-executar)
- [Configuração](#configuração)
- [ADRs - Architecture Decision Records](#adrs---architecture-decision-records)

---

## 🎯 Visão Geral

Este projeto é um **RPA (Robotic Process Automation)** worker service que realiza automaticamente:

1. **Coleta periódica** de dados de câmbio USD/BRL do site Wise
2. **Parsing robusto** do HTML utilizando múltiplos seletores CSS com fallback
3. **Validação** da taxa coletada com guardrails
4. **Persistência** em banco de dados SQLite
5. **Logging estruturado** com Serilog
6. **Resiliência** com retry policies usando Polly

---

## 🏗️ Arquitetura

O projeto segue os princípios da **Clean Architecture** (também conhecida como Arquitetura Hexagonal ou Ports & Adapters), garantindo:

- ✅ **Separação de responsabilidades**
- ✅ **Independência de frameworks e bibliotecas externas**
- ✅ **Testabilidade**
- ✅ **Inversão de dependências**

### Camadas da Arquitetura

```
┌─────────────────────────────────────────────────────────┐
│                   RPA.Worker                            │
│              (Presentation Layer)                        │
│  • UsdBrlCollectorWorker (BackgroundService)           │
│  • Program.cs (Composition Root)                        │
└────────────────┬────────────────────────────────────────┘
                 │
┌────────────────▼────────────────────────────────────────┐
│              Rpa.Application                            │
│              (Application Layer)                         │
│  • Use Cases / Handlers                                 │
│  • CollectUsdBrlHandler                                 │
│  • Commands / Queries                                   │
└────────────────┬────────────────────────────────────────┘
                 │
┌────────────────▼────────────────────────────────────────┐
│               Rpa.Domain                                │
│               (Domain Layer)                            │
│  • Entidades: ExchangeRate                             │
│  • Value Objects: Currency, DataSource                  │
│  • Interfaces (Ports): IExchangeRateRepository,        │
│    IExchangeRateHtmlClient, IUsdBrlHtmlParser          │
└─────────────────────────────────────────────────────────┘
                 ▲
┌────────────────┴────────────────────────────────────────┐
│            Rpa.Infrastructure                           │
│            (Infrastructure Layer)                        │
│  • Adapters:                                            │
│    - Persistence (SQLite/Dapper)                        │
│    - Scraping (HTTP Client + AngleSharp)               │
│    - Time (SystemClock)                                 │
│  • Dependency Injection Configuration                   │
└─────────────────────────────────────────────────────────┘
```

### Fluxo de Dados

```
Worker Timer
    │
    ├─► CollectUsdBrlHandler
    │       │
    │       ├─► IExchangeRateHtmlClient (Wise)
    │       │       └─► HttpClient + Polly Retry
    │       │
    │       ├─► IUsdBrlHtmlParser
    │       │       └─► AngleSharp + Regex
    │       │
    │       ├─► Validação Guardrails
    │       │
    │       ├─► ExchangeRate.Create() [Domain]
    │       │
    │       └─► IExchangeRateRepository
    │               └─► Dapper + SQLite
    │
    └─► Logging (Serilog)
```

---

## 📁 Estrutura do Projeto

```
rpa/
├── app/
│   ├── Rpa.slnx                    # Arquivo de solução .NET
│   ├── src/
│   │   ├── Rpa.Domain/             # 🟦 DOMAIN LAYER
│   │   │   ├── Abstractions/       # Interfaces (Ports)
│   │   │   │   ├── IClock.cs
│   │   │   │   ├── IExchangeRateHtmlClient.cs
│   │   │   │   ├── IExchangeRateRepository.cs
│   │   │   │   └── IUsdBrlHtmlParser.cs
│   │   │   └── Exchange/           # Entidades e Value Objects
│   │   │       ├── Currency.cs
│   │   │       ├── DataSource.cs
│   │   │       └── ExchangeRate.cs
│   │   │
│   │   ├── Rpa.Application/        # 🟩 APPLICATION LAYER
│   │   │   └── UseCases/
│   │   │       └── CollectUsdBrl/
│   │   │           ├── CollectUsdBrlCommand.cs
│   │   │           └── CollectUsdBrlHandler.cs
│   │   │
│   │   ├── Rpa.Infrastructure/     # 🟨 INFRASTRUCTURE LAYER
│   │   │   ├── DependencyInjection.cs
│   │   │   ├── Persistence/        # Adapter: Banco de Dados
│   │   │   │   ├── DatabaseInitializer.cs
│   │   │   │   ├── ExchangeRateRecord.cs
│   │   │   │   ├── ExchangeRateRepository.cs
│   │   │   │   ├── IDbConnectionFactory.cs
│   │   │   │   ├── SqliteConnectionFactory.cs
│   │   │   │   └── SqliteOptions.cs
│   │   │   ├── Scraping/           # Adapter: Web Scraping
│   │   │   │   ├── Parsing/
│   │   │   │   └── Wise/
│   │   │   │       ├── WiseHtmlClient.cs
│   │   │   │       ├── WiseOptions.cs
│   │   │   │       └── WiseUsdBrlHtmlParser.cs
│   │   │   └── Time/               # Adapter: Clock
│   │   │       └── SystemClock.cs
│   │   │
│   │   └── RPA.Worker/             # 🟥 PRESENTATION LAYER
│   │       ├── Program.cs          # Composition Root
│   │       ├── UsdBrlCollectorWorker.cs
│   │       ├── Options/
│   │       │   └── WorkerOptions.cs
│   │       ├── appsettings.json
│   │       └── appsettings.Development.json
│   │
│   └── tests/
│       └── Rpa.UnitTests/          # 🧪 Testes Unitários
│
└── infra/                          # 🔧 Infraestrutura (Docker, Scripts)
```

---

## 🛠️ Tecnologias Utilizadas

### Core
- **.NET 10** - Framework principal
- **C# 13** - Linguagem de programação

### Bibliotecas
- **Serilog** - Logging estruturado
- **Dapper** - Micro-ORM para acesso a dados
- **SQLitePCLRaw** - Provider SQLite nativo
- **AngleSharp** - Parser HTML/CSS
- **Polly** - Resiliência e retry policies
- **Microsoft.Extensions.Http.Polly** - Integração Polly com HttpClient

### Padrões e Práticas
- **Clean Architecture** - Separação de camadas
- **Domain-Driven Design (DDD)** - Modelagem do domínio
- **CQRS (simplificado)** - Separação de comandos
- **Dependency Injection** - Inversão de controle
- **Repository Pattern** - Abstração de persistência
- **Options Pattern** - Configuração tipada

---

## 🚀 Como Executar

### Pré-requisitos

- .NET 10 SDK instalado
- Visual Studio 2025 / VS Code / Rider (opcional)

### Passos

1. **Clone o repositório**
   ```bash
   git clone <repository-url>
   cd rpa
   ```

2. **Navegue até o projeto Worker**
   ```bash
   cd app/src/RPA.Worker
   ```

3. **Restaure as dependências**
   ```bash
   dotnet restore
   ```

4. **Execute o projeto**
   ```bash
   dotnet run
   ```

5. **Verifique os logs**
   - O worker iniciará e coletará dados a cada 60 segundos (configurável)
   - Os dados serão salvos em `rpa_scraping.db` (SQLite)
   - Logs estruturados aparecerão no console

### Executar Testes

```bash
cd app/tests/Rpa.UnitTests
dotnet test
```

---

## ⚙️ Configuração

Edite o arquivo `appsettings.json` no projeto RPA.Worker:

```json
{
  "Worker": {
    "IntervalSeconds": 60              // Intervalo entre coletas
  },
  "Wise": {
    "Url": "https://wise.com/br/currency-converter/dolar-hoje"
  },
  "Sqlite": {
    "ConnectionString": "Data Source=rpa_scraping.db;Cache=Shared"
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Debug"
    }
  }
}
```

### Variáveis de Ambiente (Desenvolvimento)

Crie `appsettings.Development.json` para sobrescrever configurações em dev:

```json
{
  "Worker": {
    "IntervalSeconds": 10              // Coleta mais frequente em dev
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Debug"
    }
  }
}
```

---

## 📚 ADRs - Architecture Decision Records

### ADR-001: Adoção da Clean Architecture

**Status:** Aceito

**Contexto:**
Necessidade de criar uma aplicação escalável, testável e manutenível, com clara separação de responsabilidades e independência de frameworks externos.

**Decisão:**
Adotamos a **Clean Architecture** com 4 camadas:
1. **Domain** - Lógica de negócio pura
2. **Application** - Casos de uso e orquestração
3. **Infrastructure** - Implementações técnicas (DB, HTTP, etc.)
4. **Presentation** - Entry point (Worker Service)

**Consequências:**
- ✅ Testabilidade elevada (domain pode ser testado sem dependências externas)
- ✅ Facilidade para trocar implementações (ex: SQLite → PostgreSQL)
- ✅ Código mais organizado e com responsabilidades claras
- ⚠️ Maior número de arquivos e abstrações
- ⚠️ Curva de aprendizado inicial para desenvolvedores não familiarizados

---

### ADR-002: Uso de SQLite como Banco de Dados

**Status:** Aceito

**Contexto:**
Necessidade de persistir dados coletados de forma simples, sem overhead de servidor de banco de dados dedicado.

**Decisão:**
Utilizamos **SQLite** como banco de dados embedded:
- Arquivo local `rpa_scraping.db`
- Sem necessidade de servidor externo
- Configuração via `SqliteConnectionFactory`

**Alternativas consideradas:**
- **PostgreSQL**: Mais robusto, mas exige servidor dedicado e configuração adicional
- **In-Memory**: Não persiste dados entre reinicializações

**Consequências:**
- ✅ Zero configuração de infraestrutura
- ✅ Simplicidade de deployment
- ✅ Adequado para volume moderado de dados
- ⚠️ Limitações de concorrência (aceitável para este caso de uso)
- ⚠️ Migração futura para PostgreSQL pode ser necessária em escala

**Nota:** A arquitetura suporta facilmente migração para PostgreSQL através da interface `IDbConnectionFactory`.

---

### ADR-003: Resiliência com Polly e Retry Policies

**Status:** Aceito

**Contexto:**
Web scraping está sujeito a falhas transitórias: timeouts, rate limiting (429), instabilidade de rede.

**Decisão:**
Implementamos **Polly** com política de retry exponencial:
- 4 tentativas com backoff exponencial
- Jitter aleatório para evitar thundering herd
- Delay base: 300ms × 2^(attempt-1)
- Jitter adicional: 0-250ms

```csharp
WaitAndRetryAsync(
    retryCount: 4,
    sleepDurationProvider: attempt =>
    {
        var baseDelay = TimeSpan.FromMilliseconds(300 * Math.Pow(2, attempt - 1));
        return baseDelay + TimeSpan.FromMilliseconds(jitter.Next(0, 250));
    });
```

**Consequências:**
- ✅ Maior robustez contra falhas transitórias
- ✅ Proteção contra rate limiting com retry inteligente
- ✅ Logs detalhados de tentativas
- ⚠️ Possível aumento de latência em caso de falhas

---

### ADR-004: Domain-Driven Design (DDD) no Domínio

**Status:** Aceito

**Contexto:**
Necessidade de modelar corretamente os conceitos de negócio (taxa de câmbio, moeda, fonte de dados).

**Decisão:**
Aplicamos princípios de **DDD**:

1. **Entidade**: `ExchangeRate` (com identidade própria)
   ```csharp
   public sealed class ExchangeRate
   {
       public Guid Id { get; }
       public Currency BaseCurrency { get; }
       public Currency QuoteCurrency { get; }
       public decimal Rate { get; }
       // ...
   }
   ```

2. **Value Objects**: `Currency`, `DataSource` (sem identidade, comparados por valor)

3. **Invariantes**: Validação no construtor (`rate > 0`)

4. **Factory Method**: `ExchangeRate.Create()` garante criação válida

**Consequências:**
- ✅ Model rico e expressivo
- ✅ Invariantes garantidos em tempo de compilação
- ✅ Código autodocumentado
- ⚠️ Mais classes e complexidade inicial

---

### ADR-005: Worker Service com BackgroundService

**Status:** Aceito

**Contexto:**
Necessidade de executar tarefas periódicas em background de forma confiável.

**Decisão:**
Utilizamos **BackgroundService** do .NET:
- Herança de `BackgroundService`
- Loop infinito com `Task.Delay` configurável
- Tratamento de exceções para não interromper o worker
- Suporte a `CancellationToken` para shutdown graceful

**Alternativas consideradas:**
- **Hangfire/Quartz**: Overhead desnecessário para tarefa simples
- **Azure Functions Timer Trigger**: Requer infraestrutura cloud

**Consequências:**
- ✅ Simplicidade e leveza
- ✅ Integrado no .NET Generic Host
- ✅ Suporte nativo a dependency injection
- ✅ Deploy como Windows Service/systemd
- ⚠️ Menos features que schedulers completos (aceitável para este caso)

---

### ADR-006: Dependency Injection e Inversão de Controle

**Status:** Aceito

**Contexto:**
Necessidade de desacoplar implementações e permitir testabilidade.

**Decisão:**
Utilizamos o **container DI nativo do .NET**:
- Interfaces definidas no Domain (`IExchangeRateRepository`, `IExchangeRateHtmlClient`, etc.)
- Implementações registradas na Infrastructure
- Composition root no `Program.cs`
- Extension method `AddInfrastructure()` para encapsular registros

**Consequências:**
- ✅ Fácil troca de implementações
- ✅ Testabilidade com mocks
- ✅ Código desacoplado
- ✅ Sem dependência de containers third-party

---

### ADR-007: Parser HTML Robusto com Múltiplos Seletores

**Status:** Aceito

**Contexto:**
Sites de terceiros frequentemente alteram a estrutura HTML, quebrando scrapers com seletores rígidos.

**Decisão:**
Implementamos estratégia de **fallback progressivo**:

1. **Seletor primário** (mais específico): `div[class*="_midMarketRateAmount_"] span[dir="ltr"]`
2. **Seletor secundário** (mais genérico): `span[dir="ltr"]`
3. **Fallback regex** em todo o body: `1 USD = X.XX BRL`

```csharp
if (TryExtractRate(primary, out rate)) return true;
if (TryExtractRate(secondary, out rate)) return true;
if (TryExtractRate(bodyText, out rate)) return true;
```

**Guardrails adicionais:**
- Validação de faixa: `0.50 <= rate <= 20.00`
- Regex compilado para performance

**Consequências:**
- ✅ Maior resiliência a mudanças no HTML
- ✅ Logs indicam qual seletor funcionou
- ✅ Redução de quebras em produção
- ⚠️ Complexidade adicional no código de parsing

---

### ADR-008: Uso de AngleSharp para Parsing HTML

**Status:** Aceito

**Contexto:**
Necessidade de parsear HTML de forma eficiente e com API rica para seletores CSS.

**Decisão:**
Escolhemos **AngleSharp** como parser HTML:
- API moderna e fluente
- Suporte completo a seletores CSS
- Performance superior ao HtmlAgilityPack
- Sem dependências nativas

**Alternativas consideradas:**
- **HtmlAgilityPack**: API mais verbosa, menos moderna
- **Regex puro**: Frágil e difícil de manter

**Consequências:**
- ✅ Código limpo e legível
- ✅ Seletores CSS familiares (padrão web)
- ✅ Performance adequada
- ⚠️ Dependência adicional no projeto

---

### ADR-009: Logging Estruturado com Serilog

**Status:** Aceito

**Contexto:**
Necessidade de logs ricos para debugging e monitoramento em produção.

**Decisão:**
Utilizamos **Serilog** com structured logging:
- Templates com propriedades nomeadas: `{Base}/{Quote} rate={Rate}`
- Enrichers: MachineName, ThreadId, LogContext
- Sink para Console (extensível para arquivo/seq/elk)

Exemplo:
```csharp
logger.LogInformation(
    "Collected {Base}/{Quote} rate={Rate} at={AtUtc} source={Source}",
    rate.BaseCurrency, rate.QuoteCurrency, rate.Rate, 
    rate.CollectedAtUtc, rate.Source);
```

**Consequências:**
- ✅ Logs estruturados facilitam queries e análises
- ✅ Integração fácil com sistemas de observabilidade
- ✅ Performance superior ao logging convencional
- ⚠️ Sintaxe específica do Serilog (templates)

---

## 📈 Próximos Passos

- [ ] Adicionar testes de integração
- [ ] Implementar API REST para consulta de dados históricos
- [ ] Dashboard com gráficos de evolução do câmbio
- [ ] Suporte a múltiplas moedas
- [ ] Monitoramento com Application Insights
- [ ] CI/CD com GitHub Actions

---

## 📄 Licença

Este projeto é privado e de uso interno.