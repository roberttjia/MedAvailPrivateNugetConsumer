# MedAvail Private NuGet Consumer

A variant of [MedAvailMultiDBSample](https://github.com/roberttjia/MedAvailMultiDBSample)
that adds a dependency on a **private NuGet package** (`MedAvail.Utilities`) resolved
via a local filesystem feed. This repo is used to test handling of private NuGet
packages in a Code Transform Agent.

## Private Package Dependency

The `MedAvail.Common` project references `MedAvail.Utilities 1.0.0`, which is
distributed as a `.nupkg` file in the `local-packages/` directory. The `nuget.config`
at the solution root configures this local feed:

```xml
<add key="MedAvailPrivateFeed" value="./local-packages" />
```

The package source (`MedAvailPrivateNugetPackage`) is a separate repo with no
direct link to this one — the only connection is the `.nupkg` file and the
`nuget.config` feed reference.

## The four databases

| Logical name (`MedAvailDatabase`) | SQL Server database     |
|-----------------------------------|-------------------------|
| `Core`                            | `MedAvailDB`            |
| `Auditing`                        | `MedAvailAuditingDb`    |
| `DataAcquisition`                 | `MedAvailDataAcquisitionDb` |
| `PackageManagement`               | `MedAvailPackageManagementDb` |

All four live on a single SQL Server instance (port 1433).

## Projects

```
src/
  MedAvail.Common            (netstandard2.0)  abstractions, DTOs, repo interfaces
  MedAvail.Business          (netstandard2.0)  business services over the repos
  MedAvail.DataAccess.Ado    (netstandard2.0)  ADO.NET implementation
  MedAvail.DataAccess.EfCore (net8.0)          EF Core 8 implementation
  MedAvail.DataAccess.Ef6    (net8.0)          EF6 implementation
test-runner/
  MedAvail.TestRunner        (net8.0)          console host + scenarios
tests/
  MedAvail.Business.Tests    (net8.0)          xUnit unit tests
```

## Building

```bash
dotnet build
```

The `nuget.config` ensures the private `MedAvail.Utilities` package is resolved
from `local-packages/` automatically during restore.

## Configuring credentials

`appsettings.json` ships with placeholders. Supply real values via user-secrets:

```bash
cd test-runner/MedAvail.TestRunner
dotnet user-secrets set "Sql:Server"   "your-sql-host"
dotnet user-secrets set "Sql:UserId"   "your-sql-login"
dotnet user-secrets set "Sql:Password" "your-sql-password"
```

## Running

```bash
dotnet run --project test-runner/MedAvail.TestRunner
```

## Unit tests

```bash
dotnet test
```
