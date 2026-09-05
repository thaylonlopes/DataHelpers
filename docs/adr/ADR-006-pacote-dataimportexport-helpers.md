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

---

## Consequências e Trade-offs

- **Universalidade:** Capacidade de importar e exportar os 4 principais formatos corporativos através de APIs homogêneas.
- **Eficiência de Memória:** O fluxo em streaming viabiliza o processamento de planilhas e arquivos densos com uso estável de memória.
- **Trade-off de Dependências:** O pacote inclui dependências consolidadas (`ClosedXML`, `CsvHelper`), justificadas pelo suporte robusto aos padrões e normas de arquivos corporativos.

