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

### 4. Transações ACID Explícitas e Auto-Rollback Defensivo (v0.4.0)
- Adição dos métodos `BeginTransactionAsync`, `CommitTransactionAsync` e `RollbackTransactionAsync` no `IMongoContext` e `MongoContext`.
- Mecanismo transacional de segurança no `SaveChanges()`: se qualquer comando da lista de mutações falhar, o `MongoContext` invoca automaticamente `AbortTransactionAsync` na sessão ativa antes de relançar a exceção, garantindo integridade estrita e prevenindo commits parciais órfãos.

### 5. Registro Idempotente e Thread-Safe de BSON no Container de DI (v0.4.0)
- Criação do utilitário `BsonRegistrationHelper`:
  - Isolamento de concorrência com `lock` e rastreamento estático em `HashSet` para convenções (`RegisterConvention`) e serializadores (`RegisterSerializer`).
  - Tratamento resiliente de corridas na inicialização concorrente de hosts de microsserviços ou suítes de testes paralelas, evitando exceções `BsonSerializationException` ("There is already a serializer registered for type...").

---

## Consequências e Trade-offs

- **Clareza Arquitetural:** Total alinhamento com padrões CQRS e Clean Architecture nas camadas de persistência NoSQL.
- **Atomicidade Garantida:** Transações ACID multi-documento com auto-rollback previnem corrupção silenciosa de estado.
- **Idempotência de DI:** Inicialização estável e livre de concorrência em contêineres e testes de integração paralelos.
- **Trade-off de Transações:** Sessões transacionais multi-documento no MongoDB exigem cluster operando em modo Replica Set (padrão em ambientes de produção).


