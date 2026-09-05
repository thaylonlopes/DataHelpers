# 🗄️ TL.Dapper.Helpers

[![NuGet](https://img.shields.io/nuget/v/TL.Dapper.Helpers.svg?style=flat-square&label=TL.Dapper.Helpers)](https://www.nuget.org/packages/TL.Dapper.Helpers/)
[![.NET](https://img.shields.io/badge/.NET-net8.0%20%7C%20net9.0-blue.svg)](https://dotnet.microsoft.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](../LICENSE.txt)

> **Productive Dapper micro-ORM utilities for .NET: transactional Unit of Work, high-speed bulk insert, and generic SQL command repositories.**  
> *Utilitários produtivos para o micro-ORM Dapper em .NET: Unit of Work transacional, inserção em lote de alta velocidade e repositórios genéricos de comandos SQL.*

O **`TL.Dapper.Helpers`** amplia os recursos do micro-ORM Dapper com padrões corporativos essenciais: controle transacional atômico multi-repositório via `IUnitOfWork`, inserção massiva de dados com `BulkInsertAsync` e repositórios baseados em `Microsoft.Data.SqlClient`.

---

## 📦 Instalação

Adicione o pacote ao seu projeto através do .NET CLI:

```bash
dotnet add package TL.Dapper.Helpers
```

---

## 🚀 Funcionalidades Principais

| Categoria | Componentes & Métodos | Descrição |
| :--- | :--- | :--- |
| **Execução de Consultas e Comandos** | `IDapperHelper`, `QueryAsync`, `ExecuteAsync` | Métodos assíncronos para consultas tipadas, queries escalares e execução de comandos SQL. |
| **Unit of Work Transacional** | `IUnitOfWork`, `ExecuteInTransactionAsync` | Orquestração atômica de transações com rollback automático em falhas e suporte a múltiplos repositórios. |
| **Inserção em Massa (Bulk)** | `BulkInsertAsync` | Carga de alto throughput em lotes configuráveis com `SqlBulkCopy` / `Microsoft.Data.SqlClient`. |
| **Repositório Genérico** | `DapperCommandRepository<T>` | Operações CRUD pré-construídas com geração dinâmica de comandos `INSERT`, `UPDATE` e `DELETE`. |
| **Filtros Dinâmicos em SQL** | `GenerateWhereClause` | Tradução segura de árvores de expressão LINQ para cláusulas SQL parametrizadas contra injeção. |

---

## 💡 Exemplos de Uso

### 1. Injeção de Dependência no `Program.cs`

```csharp
using Microsoft.Extensions.DependencyInjection;

// Configuração via string de conexão
builder.Services.AddDapperHelpers(builder.Configuration.GetConnectionString("DefaultConnection")!);
```

### 2. Operações Transacionais com `IUnitOfWork` e `BulkInsertAsync`

```csharp
using Dapper.Helpers.Interfaces;

public class VendaService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDapperHelper _dapperHelper;

    public VendaService(IUnitOfWork unitOfWork, IDapperHelper dapperHelper)
    {
        _unitOfWork = unitOfWork;
        _dapperHelper = dapperHelper;
    }

    public async Task FinalizarVendaAsync(Venda venda, List<ItemVenda> itens)
    {
        await _unitOfWork.ExecuteInTransactionAsync(async transaction =>
        {
            string sqlVenda = "INSERT INTO Vendas (ClienteId, ValorTotal) VALUES (@ClienteId, @ValorTotal);";
            await _dapperHelper.ExecuteAsync(sqlVenda, venda, transaction);

            await _dapperHelper.BulkInsertAsync("ItensVenda", itens, batchSize: 500, transaction: transaction);
            return true;
        });
    }
}
```

---

## 🏛️ Decisões Arquiteturais e Segurança

Para detalhes sobre governança de conexões, escopo transacional e compatibilidade com provedores:
- 📄 [ADR-004: Decisões Arquiteturais do TL.Dapper.Helpers](../docs/adr/ADR-004-pacote-dapper-helpers.md)

---

## 📄 Licença

Distribuído sob a licença [MIT](../LICENSE.txt).