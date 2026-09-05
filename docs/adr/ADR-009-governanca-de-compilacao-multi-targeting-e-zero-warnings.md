# ADR 009: Governança de Compilação, Multi-Targeting e Política de Zero Warnings

---

## Contexto

A suíte **DataHelpers** é distribuída como um conjunto de 8 bibliotecas utilitárias NuGet independentes. Para assegurar interoperabilidade contínua em ambientes corporativos e prevenir quebras de contrato em atualizações de runtime, é fundamental estabelecer uma política rigorosa de governança de compilação, tratamento de nulidade, documentação e qualidade estática de código.

Este documento formaliza as regras invioláveis de compilação e isolamento impostas em todos os projetos de produção da solução `DataHelpers.sln`.

---

## Decisões Arquiteturais

### 1. Multi-Targeting Obrigatório (.NET 8 e .NET 9)
- Todos os projetos de bibliotecas de classes compilam simultaneamente para `.NET 8.0` (LTS) e `.NET 9.0` (STS):
  ```xml
  <TargetFrameworks>net8.0;net9.0</TargetFrameworks>
  ```
- Garante suporte de longo prazo para ambientes corporativos estáveis e acesso imediato aos novos recursos de performance do runtime .NET 9.

### 2. Política de Zero Warnings e Nullable Reference Types
- Nenhum aviso de compilação ou de nulidade é tolerado na suíte:
  ```xml
  <Nullable>enable</Nullable>
  <ImplicitUsings>enable</ImplicitUsings>
  <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
  <GenerateDocumentationFile>true</GenerateDocumentationFile>
  ```
- **Proibição de NoWarn:** Qualquer supressão cega de diagnósticos via tag `<NoWarn>` é estritamente proibida em projetos de produção. Alertas do compilador devem ser resolvidos na causa raiz.

### 3. Versionamento SemVer Unificado
- Todos os pacotes compartilham versionamento unificado com metadados NuGet completos (Autores, Empresa, Licença MIT, Símbolos `.snupkg`, RepositoryUrl).

### 4. Isolamento Total entre Pacotes (Zero Acoplamento Cruzado)
- Nenhum pacote de produção pode referenciar outro pacote de produção irmão (`ProjectReference` entre bibliotecas de produção é proibido).
- Cada pacote deve ser 100% autônomo e consumível de forma isolada, impedindo acoplamento acidental e explosão de dependências transitivas.

---

## Consequências e Trade-offs

- **Qualidade Contínua:** Zero regressões ou warnings nos pipelines de CI/CD.
- **Autonomia de Consumo:** Aplicações clientes instalam exclusivamente o pacote desejado sem carregar o restante da suíte.
- **Trade-off de Tempo de Build:** A compilação simultânea para múltiplos frameworks consome tempo ligeiramente maior no primeiro build, mitigado por compilações incrementais eficientes do MSBuild.
