# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this repo is

A **template** for .NET 10 modular monoliths. It ships only the reusable core (`Shared` folders, test harness, tooling). Modules and hosts are added by the projects built from it, so `Shared` must stay generic: no business code, nothing tied to one module.

The one exception is `Foosball` (with its `Foosball.Worker` host): a **reference module** showing how the pieces fit together (aggregates, value objects, commands/queries, validators, domain events, module DbContext, initializer, worker). Projects built from the template delete it and add their own modules; nothing in `Shared` may depend on it.

## Project layout (deliberate, do not change it)

- Exactly **one** `Domain`, **one** `Application` and **one** `Infrastructure` project. Never create a project per module.
- Each of these three projects holds a `Shared/` folder plus **one folder per module**:

  ```text
  src/Domain/          Shared/  Orders/  Billing/
  src/Application/     Shared/  Orders/  Billing/
  src/Infrastructure/  Shared/  Orders/  Billing/
  ```

- Hosts are the only per-module projects: one project per host in `src/Hosts/` (solution folder `/Hosts/`), named `<Module>.<HostKind>`, e.g. `Orders.Api`, `Orders.Worker`, `Billing.Api`. A host wires up `Shared` plus its own module only.
- Dependency direction: `Domain` ← `Application` ← `Infrastructure` ← hosts. `Domain` references no package.

### Module boundaries

A module folder may use its own module and `Shared`, never another module's folder. `Shared` never uses a module. Because the three core projects are shared by all modules, the compiler cannot isolate them: `internal` hides types from hosts, not from other modules.

Architecture tests will enforce these boundaries in a later PR. Until then, respect them by hand.

## How the pieces fit

- **Namespaces follow folders.** `RootNamespace` is empty, so code in `src/Application/Orders/` lives in `Application.Orders…`. Assembly names get a `ModularMonolith.` prefix (`Directory.Build.props`).
- **Commands and queries** (`Application/Shared/Messaging`): `ICommand`, `ICommand<T>` and `IQuery<T>`, with handlers returning `Result` / `Result<T>` (`Domain/Shared/Result.cs`; a failure carries an `Error(Code, Description)`). No mediator: callers inject `ICommandHandler<TCommand>` or `IQueryHandler<TQuery, T>` directly.
- **The pipeline is decorators built at registration.** `AddCommandHandler` / `AddQueryHandler` (`Application/Shared/Extensions/ServiceCollectionExtensions.cs`) wrap the handler in `ValidationBehavior` (commands only, runs every `IValidator<TCommand>`), then `LoggingBehavior`. A handler registered any other way skips the pipeline.
- **Registration is explicit and happens inside the layer.** Those helpers and `DomainEventsDispatcher` are `internal`, so each module registers its own types from within `Application` / `Infrastructure` and exposes public `Add…` extension methods for its hosts. No assembly scanning. Startup work goes in an `IModuleInitializer`, aggregated through DI (`IEnumerable<IModuleInitializer>`).
- **Persistence:** each module has its own `DbContext` targeting its own database, deriving from `Infrastructure/Shared/Persistence/ApplicationDbContext` (no `DbSet`, implements `IUnitOfWork`). SQLite via `Microsoft.EntityFrameworkCore.Sqlite`.
- **Domain events:** raised with `AggregateRoot.Raise(...)`. `ApplicationDbContext.SaveChangesAsync` collects them from tracked aggregates, saves, **then** dispatches them in the same DI scope to every registered `IDomainEventHandler<TEvent>`. Nothing saves on a handler's behalf: a handler that changes tracked entities must call `SaveChangesAsync` itself. If a handler throws, the data is already saved and the events are not replayed (no outbox).
- **Logging (Serilog + Seq):** every host calls `builder.Services.AddSerilogLogging(builder.Configuration)` (`Infrastructure/Shared/Logging`), which reads the host's `Serilog` section and adds an `ApplicationVersion` property. Sinks are pure configuration: `appsettings.Development.json` writes to `Console`, `appsettings.Production.json` to `Seq` (`serverUrl`, `apiKey` placeholder `__SEQ_API_KEY__` replaced by the deployment, e.g. `Serilog__WriteTo__0__Args__apiKey`). A new host only sets its own `Serilog:Properties:ApplicationName`.
- **Hosts without a request scope** (workers, consumers) use `ScopedExecutor` (`Infrastructure/Shared/Hosting`, registered by the host with `AddSingleton<ScopedExecutor>()`) to resolve one service per scope, typically a handler: `ExecuteAsync<ICommandHandler<TCommand>>(...)`, or `ExecuteAsync<TService, TResult>(...)` to get the result. Each handler saves on its own, so one call is one unit of work. Never use it from ASP.NET Core: it creates a sibling scope, i.e. a second `DbContext`.

## The Foosball reference module

A foosball (table football) league: `Player` (name + Elo rating) and `Match` (first to 10 goals). `Foosball.Worker` is the robot referee: each tick it plays a random goal in every match in progress, or kicks off a new one between two random players. Finishing a match raises `MatchFinished`, whose handler updates the Elo ratings and saves itself; a 10-0 also raises `FannyAchieved`.

- Wiring: `AddFoosballApplication()` and `AddFoosballInfrastructure(configuration)` (standard `ConnectionStrings:Foosball`, required and checked at registration, no default, a leftover `__PLACEHOLDER__` is rejected; each host's `appsettings.json` holds the placeholder `__SQLITE_DB_PATH__` as the whole connection string, e.g. `Data Source=/data/foosball.db`, for the deployment to replace, `appsettings.Development.json` points it at the git-ignored SQLite file `foosball.db`, `appsettings.Production.json` only configures the Seq sink). `FoosballModuleInitializer` creates the schema (`EnsureCreated`, no migrations yet) and seeds four players.
- The module has its own `IFoosballUnitOfWork` (implemented by `FoosballDbContext`) so several modules never compete for `IUnitOfWork`.
- `Foosball.Api` (minimal API, single `GET /foosball`): returns the matches in progress, or the players ranked by rating when there are none. It only reuses the existing `ListInProgressMatchesQuery` / `LeaderboardQuery`. Run it: `dotnet run --project src/Hosts/Foosball.Api` (`http://localhost:5080`).
- Run the Worker: `dotnet run --project src/Hosts/Foosball.Worker` (the launch profile sets `DOTNET_ENVIRONMENT=Development`; tick interval: `MatchSimulation:TickInterval`).

## Commands

The SDK is pinned to 10.0.401 (`mise.toml` and `global.json`). Tests run on **Microsoft.Testing.Platform** (`global.json`), so `dotnet test` takes MTP options, not VSTest ones.

```bash
dotnet build
dotnet test                                     # all tests
dotnet test --project tests/Architecture.Tests  # one project
dotnet test --project tests/Architecture.Tests --filter-method "*Domain_Always_HasNoDependencyOnApplication"  # one test
mise run dotnet:ci:test                         # the test commands CI runs
mise run dotnet:ci:format                       # the format checks CI runs (style + analyzers, severity info)
dotnet format --verify-no-changes               # stricter than CI: also checks whitespace and charset
pre-commit run --all-files                      # gitleaks, whitespace, YAML
```

CI (`.github/workflows/ci.yml`) runs pre-commit, then `dotnet format style` and `dotnet format analyzers` at `--severity info`, then build and tests.

## Dependencies

- Central Package Management, split in two: `src/Directory.Packages.props` and `tests/Directory.Packages.props`. Versions go there, never in a `.csproj`.
- `packages.lock.json` files are committed. A package change updates them on restore; commit them with it.

## Tests

- `Architecture.Tests` (ArchUnitNET) already enforce layer direction, `internal` handlers, `*CommandHandler` / `*QueryHandler` suffixes, `*Validator` classes living in Application, and entities not being records. Rules use `.WithoutRequiringPositiveResults()` so they pass on the empty template.
- Rules that need a matching type (handler in the same namespace as its command/query, `IValueObject` being a sealed record) are inert on the empty template and activate on their own once such a type exists. There is no `Skip` to remove.
- `Application` exposes its internals to `Application.UnitTests`, and `Infrastructure` to `Infrastructure.UnitTests` and `Infrastructure.DbTests` (`InternalsVisibleTo`), which is how the `internal` helpers, behaviors and dispatcher are tested. Both also expose them to `DynamicProxyGenAssembly2` so Moq can mock `ILogger<T>` where `T` is internal; types used as generic arguments of a mocked interface must be `public` in the test project.
- Use Moq for dependencies, except for `IDomainEventHandler<T>`: Moq intercepts its default interface method, so tests use small real handler classes.
- `Infrastructure.DbTests` run against in-memory SQLite: no Docker or external database needed. `ApplicationDbContextTests` pin the domain event semantics described above.
- `Testing.Shared` holds helpers shared by every test project (`AssertEx`, and `VerifyLog` to assert on `Mock<ILogger<T>>`).

## Conventions

- C# rules live in `.claude/rules/csharp.md` (loaded automatically for `*.cs` files): no MediatR, AutoMapper or FluentAssertions; xUnit v3 + Moq; test naming and structure.
- Commits follow Conventional Commits, checked by a pre-commit `commit-msg` hook. Allowed types: `fix feat test ci chore refactor wip`. PR titles must be semantic **with a scope** (`.github/workflows/pr-lint.yml`).
