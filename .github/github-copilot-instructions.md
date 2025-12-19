# GitHub Copilot Instructions for developing based on Clean Architecture and DDD for Local Sharp AI

## Project Overview
This is a **Local Sharp AI** for .NET 10 that provides a platform to use local AI models and it uses Domain-Driven Design (DDD) patterns.

## Vision
A cross-platform, local-first AI studio and modular agent framework that runs entirely on the user's machine (Windows / macOS / Linux), enabling building, running, and composing “agents” on top of locally hosted models with secure, auditable execution and strong developer ergonomics.

Build a fully local, private, offline, cross-platform AI Studio enabling developers to:

- Load and run local models (GGUF, ONNX, Foundry-optimized models).
- Build and test AI agents that use tools, memory, and actions.
- Create workflows (multi-step tasks powered by agents).
- Extend with plugins (tools, agents, workflows, runtimes).
- Evaluate and benchmark local models.
- Inspect tokens, latency, cost, runtime behavior.
- Export agents as SDK or REST API.

## Core Features

### Model Runtime
- Model discovery from filesystem
- Model registry (metadata, capabilities)
- Support for:
  - ONNX Runtime
  - Microsoft Foundry local models
  - GGUF models (via external inference library)
- Model lifecycle: load, warm-up, infer, unload
- Token streaming
- Model evaluation tools

### Agent Engine
- Agent definition (YAML or JSON)
- Agent types:
  - Single-agent
  - Multi-agent orchestration
  - Tool-enabled agents
  - RAG-enabled agents
- Tool/plugin mechanism
- Memory store (vector DB)
- Agent execution logs
- Workflow builder

### Plugin System
- Tools:
  - HTTP caller
  - File reader/writer
  - Calculator
  - Web scraper (local HTML only)
- Ability to install/enable/disable plugins
- Plugins packaged as `.dll` or `.plugin.json`

### UI
- Cross-platform (Electron.NET or MAUI)
- Model selection
- Agent creation
- Workflow builder
- Real-time token viewer
- Plugin marketplace (local-only)

### Storage
- Local SQLite/Postgres for metadata
- Local vector DB (Qdrant local, SQLite vector extension, or Postgres)
- Configurations in JSON + YAML

### Non-functional Requirements

- Cross-platform: Windows, Linux, macOS
- Offline only, no network usage
- High performance (streaming architecture)
- Modular + plugin-oriented
- Testable, maintainable, clean architecture
- Documentation & CLI tooling

### Stakeholders
- Primary user: Developer / Data Scientist / Power user building AI agents locally.
- Secondary user: QA engineer for validating agents and behaviors.
- Admin: Machine owner (same as user) — manages models, resource allocation.
- Developer team: You (owner), collaborators, open-source contributors.
- External systems: Local Foundry runtime, local/embedded model files, optional external model registries (read-only).

## Subsystem breakdown

### Frontend/UI (Desktop)
- Responsibilities:
  - Model explorer
  - Agent builder
  - Workflow designer
  - Settings + plugin management
  - Visual agent composer, run control, logs/traces viewer, model manager UI.
- Options:
  - **.NET MAUI** — native cross-platform .NET UI (recommended if you prefer native .NET stack).
  - **Tauri / Electron** — web UI with local backend (if you want rich web-based UX).

### Local Backend/Core and API / Agent Orchestrator
- Responsibilities:
  - Agent Execution Engine
  - RAG subsystem
  - Logging & telemetry
  - Plugin loader
  - Project manager
  - Exposes API over IPC or local HTTP for UI/CLI.
  - Hosts Agent Orchestrator: Planner, Executor, Memory manager, Tool registry.
  - Manages lifecycle of agents, sets resource limits.
- Interfaces:
  - `/api/models`, `/api/agents`, `/api/execute`, `/api/logs`.

### Model Runtime Manager
- Responsibilities:
  - Interface to Microsoft Local Foundry or ONNX runtime.
  - ONNX Engine wrapper
  - Foundry engine wrapper
  - Model download/load/unload lifecycle management, caching.
  - GGUF runtime
  - Token streaming
  - Performance analyzer
- Notes:
  - Provide adapter interface for multiple runtimes.

### Execution Sandbox
- Responsibilities:
  - Enforce permissions (file access, shell, network).
  - Provide mock / dry-run modes.
- Implementation hints:
  - Windows: ACLs + process isolation.
  - Linux/macOS: containers / user namespaces or process sandboxing.

### Persistence
- Responsibilities:
  - Stores agent configs, execution history, memory (short/long term).
  - Prefer Postgres database for local-first.
  - Vector DB
  - Configuration store

### Tools Adapter Layer
- Tools:
  - FSTool (read/write files), HTTPTool (web requests), ShellTool (shell execution via sandbox), PythonBridge (run Python snippets).
  - Each tool implements a standard interface `ITool` and can be loaded dynamically.
  - Plugin System
  - Tool registration
  - Plugin discovery
  - Plugin sandboxing

### CLI
- Responsibilities:
  - Dev-first scripting interface; supports `run`, `compose`, `list-models`, `test-agent`.
- Implementation:
  - .NET CLI app (dotnet global tool optional).

### Example more detailed subsystem flow: Run an agent
1. UI/CLI -> POST /api/execute { agentId, inputs }
2. Backend validates permissions (ExecutionSandbox)
3. Planner composes steps (calls model via ModelRuntime)
4. Executor invokes tools via ToolsAdapter (FS, Shell, Python)
5. Persistence logs steps and saves memory to DB
6. UI subscribes to execution stream and displays results

## Architecture & Project Structure

### C# Conventions
- Use standard Microsoft naming conventions
- Use `PascalCase` for types and methods, `camelCase` for parameters and private fields
- Use `I` prefix for interfaces (e.g., `IRepository`)
- Use `Async` suffix for async methods (e.g., `GetByIdAsync`)
- Prefix private fields with `_` (e.g., `_repository`)
- Always use {} for blocks except single-line exits (e.g. `return`, `throw`)
- Always keep single line blocks on one line (e.g., `if (x) return y;`)
- Prefer primary constructors for required dependencies
- Never use primary constructor parameters directly - always assign to private fields for clarity and testability

### Context diagram (Level 1)
**System**: Local AI Studio & Modular Agent Framework

**People**
- *Developer (you)* — composes agents, manages models, runs experiments.
- *End user* — runs agents for automation tasks (same person in local-first model).

**External Systems**
- *Local Model Runtime* — Microsoft Local Foundry or ONNX runtime (runs on same machine).
- *External Model Registries* (optional) — read-only model download sources.

**Key interactions**
- Developer uses Desktop UI or CLI to interact with system.
- System interacts with Local Model Runtime to run inference.
- System optionally connects to remote model registries for download.

### Container diagram (Level 2)
List of containers and responsibilities:

   - Role: Composition canvas, run control, execution trace UI.  
   - Tech suggestions: .NET MAUI (recommended) or Tauri/Electron.
   - **Local Backend API (Container: backend)**  
   - Role: Orchestrates agents; exposes restful/IPC endpoints for UI/CLI; houses Agent Orchestrator and Tool Registry.  
   - Role: Abstraction over Local Foundry/ONNX runtimes; loads models, sends inference requests.
   - Role: Permission enforcement for tool executions and system actions.
   - **Persistence (Container: db)**  
   - **Frontend UI (Container: frontend)**  
   - **Local Model Runtime (Container: local-runtime)**  
   - **CLI (Container: cli)**  
   - Role: Command line interface interacting over local HTTP/IPC to backend.

   - **Worker (optional) (Container: worker)**  
   - Role: Background tasks, model loading/unloading, garbage collection.

#### Container interactions
- UI <-> Backend via HTTP/IPC.
- CLI <-> Backend via HTTP/IPC.
- Backend <-> Model Runtime via adapter interface.
- Backend <-> Sandbox for permission checks.
- Backend <-> DB for history and memory.

### Component diagram (Level 3) — Agent Orchestrator (backend)
Break the backend into components:

1. **Agent Manager**
   - Stores agent definitions, versions, entry points.

2. **Planner**
   - Converts agent goals into ordered actions / prompts. Can use LM to plan steps.

3. **Executor**
   - Executes planned steps, orchestrates calls to Tools and Model Runtime.

4. **Memory Manager**
   - Short-term working memory + long-term persistence. Interfaces to DB.

5. **Tool Registry**
   - Dynamic registry of `ITool` implementations with lifecycle & permission metadata.

6. **Model Manager**
   - Manages model lifecycle, caching, model selection & configuration.

7. **Permission / Sandbox Controller**
   - Enforces allowlists, user confirmations, policy decisions.

8. **Telemetry / Logging**
   - Exposes traces, metrics via OpenTelemetry (local files + UI view).

9. **Plugin Loader**
   - Loads external plugin assemblies or scripts (sandboxed).

**Component interactions (run flow)**
- `Executor` asks `Planner` for steps -> `Executor` calls `Tool Registry` to invoke a tool -> Tool may call `Model Manager` or OS operations through `Sandbox Controller` -> `Memory Manager` updated -> `Telemetry` logs the activity.

### Key Projects
- **Core.Domain**: Domain entities, aggregates, value objects, specifications, interfaces
- **Core.Application**: Commands/queries (CQRS), Mediator handlers, application logic  
- **Infrastructure.Persistence**: EF Core, external services, email, file access
- **Infrastructure.Ai**: Access to local model runtimes (Local Foundry, ONNX)
- **API**: ASP.NET Core API, REPR pattern, validation

## Development Patterns

### Domain Model (Core)
- Entities use encapsulation - minimize public setters
- Group related entities into Aggregates
- Use Value Objects (e.g., `Name.From()`)
- Domain Events for cross-aggregate communication
- Repository interfaces defined in Core, implemented in Infrastructure

### Use Cases (Server.Core.Application) (CQRS)
- Commands for mutations, Queries for reads
- Queries can bypass repository pattern for performance (very read-heavy)
- Use Mediator (source generator) for command/query handling
- Chain of responsibility for cross-cutting concerns (logging, validation)

### Validation Strategy
- **API Level**: FluentValidation on request DTOs (FastEndpoints integration)
- **Use Case Level**: Validate commands/queries (defensive coding)
- **Domain Level**: Business invariants throw exceptions, assume pre-validated input

## Essential Commands

### Build & Test
```bash
dotnet build LocalSharpAI.sln
dotnet test LocalSharpAI.sln
```

### Entity Framework Migrations
```bash
```

### Template Installation & Usage
```bash
```

## Key Dependencies & Patterns

### Primary Libraries
- **ASP.NET Core**: API endpoints (replaced Controllers/Minimal APIs)
- **Mediator**: Command/query handling in UseCases
- **EF Core**: Data access (SQLite default, easily changed to SQL Server)
- **Serilog**: Structured logging
- **FluentValidation**: Validation framework for API requests

### Central Package Management
- All package versions in `Directory.Packages.props`
- Use `<PackageReference Include="..." />` without Version attribute

### Test Organization
- **UnitTests**: Core business logic, use cases
- **IntegrationTests**: Database, infrastructure components
- **FunctionalTests**: API endpoints (subcutaneous testing)
- Use `Microsoft.AspNetCore.Mvc.Testing` for API tests

## File Organization Conventions

## Important Notes: General guidelines for adding new code/projects
- Keep domain logic inside `LocalSharpAi.Core.Domain` and keep API surface in `LocalSharpAi.Api`
- `LocalSharpAi.Infrastructure.ModelRuntimes` contains adapters for running models locally (e.g., ONNX, LocalFoundry)
- Tests are separated by scope: unit (`LocalSharpAi.Core.UnitTests`), integration, and end-to-end.
- Don't include hyphens in project names (template limitation)
- Database path in `appsettings.json` for SQLite
- Use absolute paths in EF migration commands
- Keep LocalSharpAi.Core.Domain free of dependencies on other projects and it is pure C#
- Keep Domain objects separated from EF Core entities
- Use value objects for domain object identifiers (e.g., `AgentId`, `ModelId`)
- Add new csproj under solution folder `src` or `tests` as appropriate
- Add new csproj for large infrastructure components (e.g., new model runtime) under `Infrastructure` solution folder
- Add every new Minimal API endpoint at `LocalSharpAi.Api` static class for discoverability under `Endpoints` folder same as Health endpoint
- Add API versioning via URL segment (e.g., `/api/v1/models`)
- Use extension methods to keep Program.cs clean
- Each csproj has its own DI registration extension method for clarity
- Keep documentation up to date with any project and code changes
- **Localization**: Ensure support for multi-language requirements in the UI

## Testing Guidelines
- Ensure unit tests cover all business logic thoroughly
- Follow naming conventions for test cases based on functionality
- Include integration tests to verify interactions between components
- Ensure functional tests cover all API endpoints accurately

## VS Code Tasks
Use the predefined tasks: `build`, `publish`, `watch` instead of manual `dotnet` commands when possible
