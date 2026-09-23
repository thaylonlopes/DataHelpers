# ADR 006: Decisões Arquiteturais do Pacote TL.DataImportExport.Helpers

---

## Contexto

A integração entre sistemas legados, portais externos e microsserviços modernos frequentemente exige importação e exportação de dados em formatos heterogêneos (CSV, Excel `.xlsx`, JSON e XML). Em cenários de processamento em lote, ferramentas ingênuas carregam o arquivo completo na memória Heap, gerando gargalos de alocação de memória e risco de `OutOfMemoryException`.

O pacote `TL.DataImportExport.Helpers` foi desenvolvido para unificar a ingestão e geração de dados sob interfaces assíncronas baseadas em *Streaming* e mapeamento fluente de layouts externos (`ColumnMap<T>`).

---

## Decisões Arquiteturais

### 1. Processamento Baseado em Streaming
- Operações de importação e exportação utilizam sobrecargas baseadas em `Stream` e `FileStream`, evitando buffering integral de dados na memória.
- Suporte a cancelamento cooperativo via `CancellationToken` para abortar downloads e exportações demoradas.

### 2. Mapeamento Heterogêneo de Colunas (`ColumnMap<T>`)
- Permite mapear de forma declarativa e fluente propriedades do modelo C# para nomes arbitrários de colunas em arquivos externos:
  ```csharp
  var map = new ColumnMap<Cliente>()
      .Map(c => c.Id, "COD_CLIENTE")
      .Map(c => c.Nome, "NOME_RAZAO_SOCIAL");
  ```
- Garante total desacoplamento entre as entidades internas da aplicação e o layout fornecido por terceiros.

### 3. Integração com Padrões Consolidados da Indústria
- **CSV:** `CsvHelper` com suporte a delimitadores configuráveis e proteção contra CSV Injection.
- **Excel:** `ClosedXML` para geração de planilhas `.xlsx` em conformidade com o formato OpenXML, sem necessidade de dependências COM do Office.
- **JSON:** `System.Text.Json` de alto throughput com zero overhead de conversão intermediária.
- **XML:** Streaming nativo tipado com segurança defensiva contra XXE (*XML External Entity*).

### 4. Adoção de ILogger Nativo da BCL e Erradicação de Console.WriteLine (v0.4.0)
- Expurgo de implementações manuais de logging (`SimpleLogger`) e de chamadas diretas a `Console.WriteLine` (Regra #8).
- Adoção estrita de `Microsoft.Extensions.Logging.ILogger` com default `NullLogger.Instance`, viabilizando integração transparente com Serilog, OpenTelemetry e Application Insights.

### 5. Mitigação de Injeção de Fórmulas em Arquivos CSV (v0.4.0)
- Implementação de `SafeCsvFormulaStringConverter` integrado ao pipeline de escrita do `CsvHelper`.
- Sanitização preventiva: qualquer célula de texto que inicie com os caracteres perigosos `=`, `+`, `-`, `@`, `\t` ou `\r` é prefixada com apóstrofo seguro (`'`), neutralizando execuções de comandos arbitrários e fórmulas dinâmicas ao abrir relatórios no Excel ou Calc.

### 6. Guardrail de Memória no ClosedXML (v0.4.0)
- O `ClosedXML` instancia todo o modelo DOM da planilha em memória Heap, gerando risco de esgotamento de memória em planilhas volumosas.
- Implementação da propriedade `MaxRowsLimit` (padrão 15.000 linhas) no `ExcelDataImporter`. Se o volume ultrapassar o teto, a leitura é abortada imediatamente com `InvalidOperationException` defensiva, instruindo o uso de streaming CSV via `SpanDelimitedParser`.

---

## Consequências e Trade-offs

- **Universalidade:** Capacidade de importar e exportar os 4 principais formatos corporativos através de APIs homogêneas.
- **Segurança em Exportação e Importação:** Proteção nativa contra execução indevida de fórmulas em arquivos CSV e ataques de entidade externa (XXE) em XML.
- **Proteção de Recursos:** Guardrail determinístico no Excel previne esgotamento de memória em ambientes conteinerizados ou servidores de recursos limitados.
- **Logging Corporativo Limpo:** Zero poluição em `Console.Out` e rastreamento via `ILogger` padrão do ecossistema .NET.


