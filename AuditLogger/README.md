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
| **Operações de Auditoria** | `LogCreate()`, `LogUpdate()`, `LogDelete()` (sync e async) | Registro de mutações retornando `Guid` com identificação de usuário, entidade e contexto. |
| **Cálculo de Diff Estruturado** | `AuditLogEntry`, `JsonDiffCalculator` | Geração automática de histórico diferencial JSON estruturado (`Old`, `New`, `Diff`). |
| **Mascaramento de Dados Sensíveis** | `[SensitiveData]`, heurísticas | Mascaramento automático (`***`) de senhas, CPFs, tokens e dados confidenciais (LGPD/GDPR). |
| **Higienização CRLF** | `CrlfSanitizer` | Neutralização rigorosa de `\r` e `\n` em campos textuais contra Log Injection. |
| **Contexto Distribuído** | `AuditCorrelationContext` | Captura automática de `CorrelationId` (W3C/OpenTelemetry) e `TenantId` multi-inquilino. |
| **Emissão & Sinks** | `ILogger<AuditLogger>`, `MemoryCache` | Emissão via `ILogger`, desacoplada de dependências de terceiros, com adaptadores legados. |
| **Injeção de Dependência** | `AddAuditLogger(IConfiguration)` | Configuração fluente no container nativo do .NET com validação de opções. |

---

## 💡 Exemplos de Uso

### 1. Entidade com Proteção de Dados Sensíveis

```csharp
using AuditLogger;

public class Usuario
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;

    [SensitiveData]
    public string SenhaHash { get; set; } = string.Empty;

    public string Cpf { get; set; } = string.Empty; // Mascarado automaticamente por convenção heurística
}
```

### 2. Registro em Injeção de Dependência

```csharp
using AuditLogger.DependencyInjection;

// No Program.cs
builder.Services.AddAuditLogger(builder.Configuration);
```

### 3. Configuração no `appsettings.json`

```json
{
  "AuditLoggerSettings": {
    "StorageType": "Logger",
    "EnableHeuristicMasking": true,
    "DefaultMask": "***"
  }
}
```

### 4. Registro Estruturado com Cálculo Automático de Diff

```csharp
using AuditLogger.Interfaces;

public class UsuarioService
{
    private readonly IAuditLogger _auditLogger;

    public UsuarioService(IAuditLogger auditLogger)
    {
        _auditLogger = auditLogger;
    }

    public async Task AtualizarUsuarioAsync(Usuario original, Usuario modificado, string usuarioId)
    {
        modificado.Nome = "Novo Nome";
        modificado.SenhaHash = "novo_hash_secreto";

        // SenhaHash e CPF são automaticamente mascarados como "***" no payload JSON
        Guid logId = await _auditLogger.LogUpdateAsync(original, modificado, usuarioId);
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
