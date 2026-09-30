# dotnet-modular-monolith-template

A template for building modular monoliths with .NET 10.

## Layout

```text
src/
  Domain/            Shared/ + one folder per module
  Application/       Shared/ + one folder per module
  Infrastructure/    Shared/ + one folder per module
  Hosts/             one project per host: Orders.Api, Orders.Worker, Billing.Api...
                     (the template ships Foosball.Worker as a reference)
tests/
```

## Principles

- One project per layer, not per module. Modules are folders.
- Module isolation is the job of the architecture tests, not of project references.
- Explicit code over magic: no MediatR, no AutoMapper, no assembly scanning.

## Stack

.NET 10 · EF Core · SQLite · xUnit v3 · ArchUnitNET

## License

[MIT](LICENSE)
