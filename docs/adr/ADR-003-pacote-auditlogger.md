# ADR 003: Decisões Arquiteturais do Pacote TL.AuditLogger

---

## Contexto

Trilhas de auditoria corporativa são requisitos regulatórios e operacionais indispensáveis em aplicações corporativas modernas (LGPD, SOX, PCI-DSS). O registro de auditoria necessita capturar operações de CRUD com rastreabilidade de identificador de usuário, tipo de entidade, timestamps precisos e, crucialmente, o diferencial de mutação de estado antes e após a alteração.

O pacote `TL.AuditLogger` foi concebido para desacoplar a captura de auditoria das regras de negócio, provendo cálculo diferencial automático em JSON e múltiplos backends pluggáveis de persistência sem risco de vazamento de memória.

---

## Decisões Arquiteturais

### 1. Cálculo Automático de Diff Estruturado JSON
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
- O cálculo elimina a necessidade de desenvolvedores escreverem rotinas manuais de comparação campo a campo em cada entidade.

### 2. Provedores Pluggáveis de Armazenamento (`IAuditLogStorage`)
- A persistência é desacoplada da captura via Factory Pattern e interface `IAuditLogStorage`:
  - `MemoryCacheAuditLogStorage`: Buffer com política de expiração absoluta configurável, ideal para testes unitários e ambientes de desenvolvimento.
  - `SerilogAuditLogStorage`: Despacho estruturado para coletores como Seq, Elasticsearch e Datadog.
  - `NLogAuditLogStorage`: Suporte a ecossistemas legados baseados em NLog.

### 3. Eliminação de Retenção Estática (Zero Memory Leak)
- Todo o ciclo de vida do armazenamento opera através do container de injeção de dependência do .NET, sem coleções estáticas globais infinitas, prevenindo vazamentos de memória comuns em implementações ingênuas de auditoria.

---

## Consequências e Trade-offs

- **Rastreabilidade Fina:** Histórico transparente de quem alterou o quê e quais campos foram modificados.
- **Isolamento de Destino:** Facilidade para alternar entre armazenamento em log ou banco sem alterar o código consumidor.
- **Trade-off de Computação:** A comparação de propriedades para objetos muito densos tem pequeno custo de inspeção de propriedades, mitigado pelo foco exclusivo nas propriedades públicas de dados.

