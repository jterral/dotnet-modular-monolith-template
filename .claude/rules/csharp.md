---
description: "Guidelines for building C# applications"
paths:
  - "**/*.cs"
---

# C# Development

## C# Instructions

- Always use the latest version C#, currently C# 14 features.
- Never write comments that state the obvious; focus on explaining the why behind complex logic.
- If you need to write a comment, it must be in English.

## General Instructions

- Make only high confidence suggestions when reviewing code changes.
- Write code with good maintainability practices, including comments on why certain design decisions were made.
- Handle edge cases and write clear exception handling.
- For libraries or external dependencies, mention their usage and purpose in comments.

## Libraries and Frameworks

- **Do not use MediatR** — prefer direct service injection. The library went commercial, and MediatR adds indirection without benefit at our scale and makes call chains harder to trace.
- **Do not use AutoMapper** — prefer explicit mapping. AutoMapper adds indirection and makes call chains harder to trace.
- **Do not use FluentAssertions** — prefer explicit assertions. FluentAssertions went commercial.
- Always use `xunit.v3` for testing.
- Always use `Moq` for mocking dependencies in unit tests.

## Naming Conventions

- Follow PascalCase for component names, method names, and public members.
- Use camelCase for private fields and local variables.
- Prefix interface names with "I" (e.g., IUserService).
- Use Async suffix for async methods, except for overrides or interface implementations where the name is imposed by the base class or interface.
- A method returning `Task` / `Task<T>` that calls awaitable code must be `async` and `await` it, rather than returning the inner task. Exception: members with no awaitable work (`Task.CompletedTask`, `Task.FromResult`), where `async` would only add a state machine and a CS1998 warning.

## Formatting

- Apply code-formatting style defined in `.editorconfig`.
- Prefer file-scoped namespace declarations and single-line using directives.
- Ensure that the final return statement of a method is on its own line.
- Use pattern matching and switch expressions wherever possible.
- Use `nameof` instead of string literals when referring to member names.
- Ensure that XML doc comments are created for only APIs controllers endpoints. When applicable, include `<example>` and `<code>` documentation in the comments.

## Nullable Reference Types

- Declare variables non-nullable, and check for `null` at entry points.
- Always use `is null` or `is not null` instead of `== null` or `!= null`.
- Trust the C# null annotations and don't add null checks when the type system says a value cannot be null.
- Never suppress nullable warnings with ! unless accompanied by an explanatory comment

## Data Access Patterns

- Guide the implementation of a data access layer using Entity Framework Core.
- Explain efficient query patterns to avoid common performance issues.
- Do not put business logic in repositories — repositories are data-access only.

## Logging and Monitoring

- Guide the implementation of structured logging using Serilog.
- Show how to implement custom telemetry and correlation IDs for request tracking.
- Use `ILogger<T>` for logging, never `Console.WriteLine`

## Testing

- Always include test cases for critical paths of the application.
- Name tests as `MethodName_Scenario_ExpectedBehavior`
- Always emit "Act", "Arrange" or "Assert" comments.
- Use `sealed` test classes to prevent inheritance and ensure isolation
- Declare mocks as `private readonly` fields, suffixed with `Mock` — e.g. `_myServiceMock`
- Declare the system under test as `private readonly _sut`
- Initialize mocks inline with `new Mock<T>()`
- Initialize `_sut` in the constructor using the mocked `.Object` properties — never inline
- Implement `IDisposable` and call `VerifyNoOtherCalls()` on every mock in `Dispose`
