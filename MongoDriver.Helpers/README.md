# 🍃 TL.MongoDriver.Helpers

[![NuGet](https://img.shields.io/nuget/v/TL.MongoDriver.Helpers.svg?style=flat-square&label=TL.MongoDriver.Helpers)](https://www.nuget.org/packages/TL.MongoDriver.Helpers/)
[![.NET](https://img.shields.io/badge/.NET-net8.0%20%7C%20net9.0-blue.svg)](https://dotnet.microsoft.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](../LICENSE.txt)

> **Enterprise MongoDB abstractions for .NET: CQRS command and query repositories, transactional MongoContext, domain events, and async pagination.**  
> *Abstrações corporativas para MongoDB em .NET: repositórios CQRS de comandos e consultas, MongoContext transacional, eventos de domínio e paginação assíncrona.*

O **`TL.MongoDriver.Helpers`** oferece uma camada de abstração elegante sobre o driver oficial do MongoDB (`MongoDB.Driver`), estruturando o acesso a documentos em torno do padrão CQRS, transações ACID seguras via `MongoContext` e interceptadores desacoplados de eventos de domínio.

---

## 📦 Instalação

Adicione o pacote ao seu projeto através do .NET CLI:

```bash
dotnet add package TL.MongoDriver.Helpers
```

---

## 🚀 Funcionalidades Principais

| Categoria | Componentes & Métodos | Descrição |
| :--- | :--- | :--- |
| **Segregação CQRS** | `ICommandRepository<T>`, `IQueryRepository<T>` | Separação formal entre métodos de mutação (`Add`, `Update`, `Delete`) e consultas otimizadas de leitura. |
| **Contexto Transacional** | `MongoContext`, `SaveChanges()` | Gerenciamento de sessões do MongoDB (`IClientSessionHandle`) com atomicidade multi-documento. |
| **Interceptores de Eventos** | `CaptureEventsInterceptor`, `IEventCatcher` | Disparo desacoplado de eventos de domínio antes ou após a confirmação das alterações no banco. |
| **Paginação Assíncrona** | `GetPagedAsync()` | Paginação tipada com ordenação parametrizada e contagem assíncrona de documentos. |
| **Busca Multi-Coleção** | `MultiCollectionSearchHelper<T>` | Orquestração de consultas cruzadas e pipelines de agregação entre múltiplas coleções. |

---

## 💡 Exemplos de Uso

### 1. Injeção de Dependência no `Program.cs`

```csharp
using MongoDriver.Helpers.DependencyInjection;

builder.Services.AddMongoDriver(builder.Configuration);
```

### 2. Configuração no `appsettings.json`

```json
{
  "MongoDbConfig": {
    "ConnectionStrings": "mongodb://localhost:27017",
    "Database": "EmpresaDb",
    "Seed": false
  }
}
```

### 3. Operações CQRS com Repositórios Tipados

```csharp
using MongoDriver.Helpers.Interface;

public class ClienteService
{
    private readonly ICommandRepository<Cliente> _commandRepo;
    private readonly IQueryRepository<Cliente> _queryRepo;

    public ClienteService(
        ICommandRepository<Cliente> commandRepo,
        IQueryRepository<Cliente> queryRepo)
    {
        _commandRepo = commandRepo;
        _queryRepo = queryRepo;
    }

    public async Task CriarClienteAsync(Cliente cliente)
    {
        await _commandRepo.AddAsync(cliente);
    }

    public async Task<List<Cliente>> ObterAtivosAsync()
    {
        var cursor = await _queryRepo.FindAsync(c => c.Ativo);
        return cursor.ToList();
    }
}

public class Cliente
{
    public string Id { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public bool Ativo { get; set; }
}
```

---

## 🏛️ Decisões Arquiteturais e Segurança

Para detalhes sobre sessões transacionais, serialização BSON e ciclo de vida de conexões:
- 📄 [ADR-008: Decisões Arquiteturais do TL.MongoDriver.Helpers](../docs/adr/ADR-008-pacote-mongodriver-helpers.md)

---

## 📄 Licença

Distribuído sob a licença [MIT](../LICENSE.txt).