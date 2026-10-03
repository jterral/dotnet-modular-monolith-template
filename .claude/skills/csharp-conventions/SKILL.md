---
name: csharp-conventions
description: C# and .NET coding conventions. Use automatically when creating, modifying, or reviewing C# files (.cs), classes, tests, or ASP.NET Core endpoints.
---

# C# Development

## C# Instructions

- Use the C# version set by `LangVersion` (or the default of the project's `TargetFramework`); never use a feature newer than that.
- Comments explain the why (design decisions, non-obvious logic), never the what. The only exceptions are the `// Arrange`, `// Act` and `// Assert` markers in tests, and the XML doc comments of the HTTP API surface (see "API Documentation (OpenAPI)").
- If you need to write a comment, it must be in English.

## General Instructions

- When reviewing code, report only issues you can anchor to a file and line and explain with a concrete failure scenario; skip style points already covered by `.editorconfig`.
- Catch only exceptions you can handle; never swallow one silently.
- Rethrow with `throw;`, never `throw ex;`: the latter resets the stack trace.

## Libraries and Frameworks

- **Do not use MediatR** — prefer direct service injection. The library went commercial, and MediatR adds indirection without benefit at our scale and makes call chains harder to trace.
- **Do not use AutoMapper** — prefer explicit mapping. AutoMapper adds indirection and makes call chains harder to trace.
- **Do not use FluentAssertions** — prefer explicit assertions. FluentAssertions went commercial.
- Always use `xunit.v3` for testing.
- Always use `Moq` for mocking dependencies in unit tests.

## Naming Conventions

- Use `_camelCase` (underscore prefix) for private fields, and camelCase for local variables and parameters.
- Use Async suffix for async methods, except for overrides or interface implementations where the name is imposed by the base class or interface.

## Types

- Mark classes `sealed` unless they are designed as a base class.
- Use primary constructors for dependency injection in classes. Copy each parameter into a `private readonly` field (`_name`) and never reference the parameter anywhere else: a primary constructor parameter is mutable, so a stray assignment would silently change the captured value.
- Use `sealed record` for immutable data carriers (requests, results, DTOs). The field-copy rule above does not apply to records, whose parameters become properties.
- Use collection expressions (`[]`, `[a, b]`) instead of `new List<T> { ... }`, `new[] { ... }` or `Array.Empty<T>()`.

## Formatting

- Apply code-formatting style defined in `.editorconfig`.
- Use a switch expression instead of an `if/else if` chain or `switch` statement when every branch only returns or assigns a value.
- Use `is` / `is not` patterns instead of `as` followed by a null check.
- Use `nameof` instead of string literals when referring to member names.

## API Documentation (OpenAPI)

Applies to projects that expose an HTTP API. The XML comments feed the OpenAPI/Swagger documentation (`GenerateDocumentationFile` + `IncludeXmlComments`), so write them for the API consumer, not for the maintainer.

- Document every controller, every action and every request/response model (type and properties) of the API project. Nowhere else: use cases, domain and infrastructure follow the why-only comment rule.
- Use plain business language: say what the endpoint or field means for the caller, not how it is implemented. No tautologies (`<summary>The user id.</summary>` on `UserId` says nothing): add the format, unit, allowed values, default, or where the value comes from.
- Actions: a one-sentence `<summary>` starting with a verb ("Creates…", "Returns…"); `<remarks>` for what a caller must know (preconditions, idempotency, feature-flagged behavior); `<returns>`; and a `<param>` for every parameter, `[FromServices]` ones included, because the compiler warns (CS1573) as soon as one is missing.
- Responses: one `<response code="NNN">` per `[ProducesResponseType]`, stating when the caller gets that status. Keep both lists in sync.
- Models: a `<summary>` on the type and on each property, plus an `<example>` with a realistic value on each property. Omit the `<example>` when a single value would lead clients to hardcode it, and say why in `<remarks>`.
- Use `<c>` for literal values (`<c>PHONE_VALIDATION</c>`) and `<code>` inside `<remarks>` for a sample payload when the body is not obvious.
- Update the comments in the same change as the signature, status code or property they describe.

## Nullable Reference Types

- Declare variables non-nullable, and check for `null` at entry points.
- Always use `is null` or `is not null` instead of `== null` or `!= null`.
- Trust the C# null annotations and don't add null checks when the type system says a value cannot be null.
- Never suppress nullable warnings with ! unless accompanied by an explanatory comment

## Async

- Never block on async code: no `.Result`, `.Wait()` or `.GetAwaiter().GetResult()`. Make the caller async instead.
- Always `await` an async call inside an `async` method; never `return` the `Task` directly (no Task elision). An elided method disappears from the async stack trace, and `using` / `try-catch` around the call stop covering the awaited work.
- Only exception: a synchronous implementation of an async interface may return `Task.CompletedTask` or `Task.FromResult(...)`.
- Every async method that does I/O takes a `CancellationToken` as its last parameter and forwards it to every async call it makes.

## Data Access Patterns

- Use Entity Framework Core for data access.
- Filter and project in the database: apply `Where` and `Select` before `ToListAsync()`, never after materializing.
- Use `AsNoTracking()` for read-only queries.
- Avoid N+1 queries: load related data with `Include` or a projection, never inside a loop.
- Do not put business logic in repositories — repositories are data-access only.

## Logging and Monitoring

- Use `ILogger<T>` with Serilog for structured logging; never `Console.WriteLine`.
- Log with message templates and named placeholders (`"User {UserId} not found", userId`); never string interpolation inside the message.
- Attach the request correlation ID to every log entry (enrich the Serilog `LogContext` in middleware).

## Testing

- Always include test cases for critical paths of the application.
- Name tests as `MethodName_Scenario_ExpectedBehavior`
- Mark the three test phases with `// Arrange`, `// Act` and `// Assert` comments.
- Use `sealed` test classes to prevent inheritance and ensure isolation
- Declare mocks as `private readonly` fields, suffixed with `Mock` — e.g. `_myServiceMock`
- Declare the system under test as `private readonly MyService _sut`
- Initialize mocks inline with `new Mock<T>()`
- Initialize `_sut` in the constructor using the mocked `.Object` properties — never inline
- Implement `IDisposable` and call `VerifyNoOtherCalls()` on every mock in `Dispose`
- **Never** mock or verify `ILogger` / `ILogger<T>` — logging is not behavior under test. Inject `NullLogger<T>.Instance` from `Microsoft.Extensions.Logging.Abstractions` instead (e.g. `new MyService(_repositoryMock.Object, NullLogger<MyService>.Instance)`), and do not declare a `_loggerMock` field.
