# 🛡️ TL.AuditLogger

[![NuGet](https://img.shields.io/nuget/v/TL.AuditLogger.svg?style=flat-square&label=TL.AuditLogger)](https://www.nuget.org/packages/TL.AuditLogger/)
[![.NET](https://img.shields.io/badge/.NET-net8.0%20%7C%20net9.0-blue.svg)](https://dotnet.microsoft.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](../LICENSE.txt)

> **Structured audit logging utility for .NET: automatic JSON property diff calculation, pluggable storage backends, and zero-leak event dispatching.**  
> *Biblioteca de auditoria estruturada para .NET: cálculo automático de diff diferencial JSON por propriedade, provedores plugáveis de armazenamento e despacho sem vazamento de memória.*

O **`TL.AuditLogger`** fornece uma infraestrutura robusta e desacoplada para rastrear operações de alteração de estado (CRUD) em aplicações .NET. Ele captura o estado anterior e posterior das entidades, computa automaticamente o diferencial estruturado em JSON e despacha os registros para destinos de armazenamento configuráveis.

---

## 📦 Instalação

Adicione o pacote ao seu projeto através do .NET CLI:

```bash
dotnet add package TL.AuditLogger
```

---

## 🚀 Funcionalidades Principais

| Categoria | Componentes & Métodos | Descrição |
| :--- | :--- | :--- |
| **Operações de Auditoria** | `LogCreate()`, `LogUpdate()`, `LogDelete()` | Métodos da interface `IAuditLogger` para registro de mutações com identificação de usuário e entidade. |
| **Cálculo de Diff Diferencial** | `AuditLogEntry`, `Changes` | Geração automática de histórico diferencial em JSON destacando propriedades adicionadas, modificadas e removidas. |
| **Armazenamento Pluggável** | `IAuditLogStorage`, `MemoryCache`, `Serilog`, `NLog` | Estratégia extensível com provedores prontos para logs estruturados em arquivo, console ou cache temporário. |
| **Injeção de Dependência** | `AddAuditLogger(IConfiguration)` | Configuração fluente no container nativo do .NET com validação de opções no startup. |

---

## 💡 Exemplos de Uso

### 1. Registro em Injeção de Dependência

```csharp
using AuditLogger.DependencyInjection;

// No Program.cs
builder.Services.AddAuditLogger(builder.Configuration);
```

### 2. Configuração no `appsettings.json`

```json
{
  "AuditLoggerSettings": {
    "StorageType": "MemoryCache",
    "CacheDuration": "00:15:00"
  }
}
```

### 3. Registro Estruturado com Cálculo Automático de Diff

```csharp
using AuditLogger.Interfaces;

public class PedidoService
{
    private readonly IAuditLogger _auditLogger;

    public PedidoService(IAuditLogger auditLogger)
    {
        _auditLogger = auditLogger;
    }

    public void AtualizarStatus(Pedido pedidoOriginal, Pedido pedidoModificado, string usuarioId)
    {
        pedidoModificado.Status = "Aprovado";
        pedidoModificado.ValorTotal = 1500.00m;

        string logId = _auditLogger.LogUpdate(pedidoOriginal, pedidoModificado, usuarioId);
    }
}
```

---

## 🏛️ Decisões Arquiteturais e Segurança

Para detalhes sobre governança, ciclo de vida de instâncias e estratégias de isolamento de log:
- 📄 [ADR-003: Decisões Arquiteturais do TL.AuditLogger](../docs/adr/ADR-003-pacote-auditlogger.md)

---

## 📄 Licença

Distribuído sob a licença [MIT](../LICENSE.txt).
