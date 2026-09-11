# ADR 003: Decisões Arquiteturais do Pacote TL.AuditLogger

---

## Contexto

Trilhas de auditoria corporativa são requisitos regulatórios e operacionais indispensáveis em aplicações corporativas modernas (LGPD, SOX, PCI-DSS). O registro de auditoria necessita capturar operações de CRUD com rastreabilidade de identificador de usuário, tipo de entidade, timestamps precisos e, crucialmente, o diferencial de mutação de estado antes e após a alteração.

O pacote `TL.AuditLogger` foi concebido para desacoplar a captura de auditoria das regras de negócio, provendo cálculo diferencial automático em JSON e múltiplos backends pluggáveis de persistência sem risco de vazamento de memória.

---

## Decisões Arquiteturais

### 1. Cálculo Automático de Diff Estruturado JSON e Mascaramento de Dados Sensíveis
- O método `LogUpdate<T>(oldItem, newItem, userId)` compara iterativamente as propriedades públicas dos objetos antigo e novo.
- Computa dinamicamente a estrutura delta diferencial com formato estruturado:
  ```json
  {
    "Old": { ... },
    "New": { ... },
    "Diff": {
      "Preco": { "From": 100.0, "To": 120.0 }
    }
  }
  ```
- **Mascaramento de Dados Sensíveis:** Propriedades anotadas com `[SensitiveData]` ou nomes sensíveis por convenção (`Password`, `Token`, `Secret`, `Cpf`, `CreditCard`) são automaticamente mascaradas com `***` (ou máscara customizada), garantindo que dados confidenciais nunca sejam persistidos em texto plano nos logs.
- Propriedades de negócio não sensíveis são preservadas integralmente.

### 2. Desacoplamento Total de Sinks e Adoção da Abstração ILogger
- O pacote eliminou dependências forçadas de pacotes terceiros (`Serilog`, `NLog`), adotando exclusivamente a abstração `Microsoft.Extensions.Logging.Abstractions` (`ILogger<AuditLogger>`).
- O despacho via `LoggerAuditLogStorage` permite total compatibilidade com qualquer coletor moderno (OpenTelemetry, Application Insights, Datadog, Seq) sem conflitos transitivos de versão.
- `MemoryCacheAuditLogStorage` é mantido para cenários de cache volátil e testes.

### 3. Captura Automática de Contexto Distribuído (W3C / OpenTelemetry)
- Suporte a `CorrelationId` e `TenantId` através de `AuditCorrelationContext` com integração nativa ao `Activity.Current` do .NET, permitindo rastrear operações distribuídas ponta a ponta sem acoplamento manual.

### 4. Higienização contra Log / CRLF Injection
- Todos os identificadores de usuário e campos textuais mutados são higienizados contra caracteres de quebra de linha (`\r`, `\n`), prevenindo falsificação de registros de log e quebra de parsing em agregadores centrais.

---

## Consequências e Trade-offs

- **Conformidade Regulatória Nativa:** LGPD, GDPR e PCI-DSS atendidos por padrão com mascaramento automático de dados confidenciais.
- **Leveza e Portabilidade:** Zero dependências de terceiros no core de auditoria, reduzindo o grafo transitivo da biblioteca.
- **Rastreabilidade Fina:** Histórico transparente de mutações com CorrelationId e TenantId integrados.
- **Imunidade contra Log Injection:** Sanitização rigorosa contra injeção de quebras de linha nos campos de auditoria.

