using System;
using MedAvail.Business;
using MedAvail.Common;
using MedAvail.Common.Repositories;
using MedAvail.DataAccess.Ado;
using MedAvail.DataAccess.EfCore;
using MedAvail.DataAccess.EfCore.Repositories;
using MedAvail.DataAccess.Ef6;
using MedAvail.DataAccess.Ef6.Repositories;
using MedAvail.TestRunner.Harness;

namespace MedAvail.TestRunner.Tests;

public sealed class BusinessLayerPackageIdMapTests
{
    private readonly IConnectionStringProvider _connections;
    private readonly Random _rng = new();

    public BusinessLayerPackageIdMapTests(IConnectionStringProvider connections) => _connections = connections;

    public void RegisterAll(TestRunnerHarness harness)
    {
        RunFor(harness, "Ado",
            new AdoPackageIdMapRepository(_connections, MedAvailDatabase.PackageManagement));

        RunFor(harness, "EfCore",
            new EfCorePackageIdMapRepository(new EfCoreContextFactory(_connections),
                MedAvailDatabase.PackageManagement));

        RunFor(harness, "Ef6",
            new Ef6PackageIdMapRepository(new Ef6ContextFactory(_connections)));
    }

    private void RunFor(TestRunnerHarness harness, string tech, IPackageIdMapRepository repo)
    {
        const string cat = "BusinessLayer.PackageIdMap";
        var service = new PackageIdMapService(repo);
        var packageId = 2_000_000_000 + _rng.Next(1, 90_000_000);
        var guid = Guid.NewGuid().ToString("B").ToUpperInvariant();

        harness.Run($"Service.Register normalizes + persists ({tech})", cat, tech, () =>
        {
            var result = service.Register(packageId, guid);
            Assert.True(result.IsSuccess, "register should succeed");
            Assert.Equal(1, result.Value, "register should insert one row");
            Assert.True(service.IsRegistered(packageId), "id should be registered after Register");
            Assert.Equal(guid.Trim('{', '}').ToLowerInvariant(), service.GetGuid(packageId),
                "stored guid should be normalized");
        });

        harness.Run($"Service.Register rejects duplicate ({tech})", cat, tech, () =>
        {
            var result = service.Register(packageId, Guid.NewGuid().ToString());
            Assert.True(!result.IsSuccess, "second Register of the same id should fail");
        });

        harness.Run($"Service.RegisterOrUpdate updates existing ({tech})", cat, tech, () =>
        {
            var newGuid = Guid.NewGuid().ToString();
            var result = service.RegisterOrUpdate(packageId, newGuid);
            Assert.True(result.IsSuccess, "upsert should succeed");
            Assert.Equal(1, result.Value, "upsert should update one row");
            Assert.Equal(newGuid.ToLowerInvariant(), service.GetGuid(packageId),
                "guid should be updated to the new normalized value");
        });

        harness.Run($"Service.Unregister removes row ({tech})", cat, tech, () =>
        {
            var affected = service.Unregister(packageId);
            Assert.Equal(1, affected, "unregister should delete the row");
            Assert.True(!service.IsRegistered(packageId), "id should be gone after Unregister");
        });
    }
}
