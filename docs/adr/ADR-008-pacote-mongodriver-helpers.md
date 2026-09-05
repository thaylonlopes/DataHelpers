# ADR 008: Decisões Arquiteturais do Pacote TL.MongoDriver.Helpers

---

## Contexto

O MongoDB C# Driver oficial provê ampla cobertura do protocolo de banco de dados NoSQL. Contudo, em microsserviços corporativos, o consumo direto do driver tende a espalhar código repetitivo de gerenciamento de sessões, filtros BSON ad-hoc e manipulação de cursores pelas camadas de aplicação e domínio, violando princípios de separação de responsabilidades.

O pacote `TL.MongoDriver.Helpers` foi concebido para estruturar o acesso a documentos sob o padrão CQRS (Command Query Responsibility Segregation), transações atômicas com `MongoContext` e paginação tipada assíncrona.

---

## Decisões Arquiteturais

### 1. Segregação de Responsabilidades CQRS (Command vs. Query)
- **`ICommandRepository<T>` / `MongoCommandRepository<T>`:** Foco estrito em mutações de estado (`AddAsync`, `UpdateAsync`, `RemoveAsync`), garantindo que comandos de escrita não sobrecarreguem o pipeline de consulta.
- **`IQueryRepository<T>` / `MongoQueryRepository<T>`:** Foco em operações de leitura com filtros tipados via Expression Trees (`Expression<Func<T, bool>>`), paginação assíncrona (`GetPagedAsync`) e projeções.

### 2. Contexto Transacional e Sessões (`MongoContext`)
- Encapsula `IClientSessionHandle` para habilitar transações atômicas multi-documento no MongoDB Replica Set.
- Provê o método `SaveChanges()` para confirmação determinística de transações no ciclo de vida da requisição.

### 3. Interceptors de Domínio e Eventos
- Habilita interceptadores (`CaptureEventsInterceptor`) para capturar eventos de domínio vinculados à entidade e despachá-los assincronamente após o commit transacional.

---

## Consequências e Trade-offs

- **Clareza Arquitetural:** Total alinhamento com padrões CQRS e Clean Architecture nas camadas de persistência NoSQL.
- **Tipagem Forte:** Consultas e projeções protegidas pelo compilador C# contra erros de digitação em nomes de propriedades BSON.
- **Trade-off de Transações:** Sessões transacionais multi-documento no MongoDB exigem cluster operando em modo Replica Set (padrão em ambientes de produção).

