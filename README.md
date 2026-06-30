# MedAvail Private NuGet Consumer

A variant of [MedAvailMultiDBSample](https://github.com/roberttjia/MedAvailMultiDBSample)
that depends on a **private NuGet package** (`MedAvail.Utilities`) which is NOT
included in this repo. This is used to test the Code Transform Agent's handling
of missing private NuGet packages — the agent must prompt the user to upload the
`.nupkg` before the build can succeed.

## Expected Behavior

Out of the box, `dotnet restore` will fail with:

```
error NU1101: Unable to find package MedAvail.Utilities.
No packages exist with this id in source(s): MedAvailPrivateFeed, nuget.org
```

To fix, place `MedAvail.Utilities.1.0.0.nupkg` into the `local-packages/` directory.
The `nuget.config` at the solution root is already configured to use that folder:

```xml
<add key="MedAvailPrivateFeed" value="./local-packages" />
```

## Why the dependency is structural

The `MedAvail.Business` layer inherits from `ServiceBase<T>`, returns
`OperationResult<T>`, and implements `IValidator<T>` — all types exported by
`MedAvail.Utilities`. Removing the package reference would require rewriting
the entire service layer. The agent cannot work around this by deleting the
dependency.

## Package source

The `.nupkg` is built from a separate repository:
[MedAvailPrivateNugetPackage](https://github.com/roberttjia/MedAvailPrivateNugetPackage)

```bash
cd MedAvailPrivateNugetPackage
dotnet pack MedAvail.Utilities/MedAvail.Utilities.csproj -c Release -o ./local-feed
cp local-feed/MedAvail.Utilities.1.0.0.nupkg ../MedAvailPrivateNugetConsumer/local-packages/
```

## Projects

```
src/
  MedAvail.Common            (netstandard2.0)  abstractions, DTOs, repo interfaces
  MedAvail.Business          (netstandard2.0)  business services (inherits ServiceBase<T>)
  MedAvail.DataAccess.Ado    (netstandard2.0)  ADO.NET implementation
  MedAvail.DataAccess.EfCore (net8.0)          EF Core 8 implementation
  MedAvail.DataAccess.Ef6    (net8.0)          EF6 implementation
test-runner/
  MedAvail.TestRunner        (net8.0)          console host + scenarios
tests/
  MedAvail.Business.Tests    (net8.0)          xUnit unit tests
```

## Building (after providing the package)

```bash
dotnet build
```

## Unit tests

```bash
dotnet test
```
