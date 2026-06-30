using System;
using MedAvail.Common;
using MedAvail.Common.Models;
using MedAvail.Common.Repositories;
using MedAvail.Utilities;

namespace MedAvail.Business
{
    public sealed class PackageIdMapService : ServiceBase<IPackageIdMapRepository>
    {
        public PackageIdMapService(IPackageIdMapRepository repository)
            : base(repository)
        {
        }

        public MedAvailDatabase Database => Repository.Database;

        public static string? NormalizeGuid(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return null;
            if (!Guid.TryParse(raw, out var g))
                throw new FormatException($"'{raw}' is not a valid GUID.");
            return g.ToString("D").ToLowerInvariant();
        }

        public OperationResult<int> Register(int packageId, string? guid)
        {
            if (packageId < 0)
                return Failure<int>("package id cannot be negative.");

            if (Repository.CountByPackageId(packageId) > 0)
                return Failure<int>($"package id {packageId} is already registered.");

            var rows = Repository.Insert(new PackageIdMapDto
            {
                PackageId = packageId,
                PackageGuid = NormalizeGuid(guid)
            });
            return Success(rows);
        }

        public OperationResult<int> RegisterOrUpdate(int packageId, string? guid)
        {
            if (packageId < 0)
                return Failure<int>("package id cannot be negative.");

            var normalized = NormalizeGuid(guid);
            if (Repository.CountByPackageId(packageId) > 0)
                return Success(Repository.UpdateGuid(packageId, normalized));

            return Success(Repository.Insert(new PackageIdMapDto { PackageId = packageId, PackageGuid = normalized }));
        }

        public bool IsRegistered(int packageId) => Repository.CountByPackageId(packageId) > 0;

        public string? GetGuid(int packageId) => Repository.GetByPackageId(packageId)?.PackageGuid;

        public int Unregister(int packageId) => Repository.Delete(packageId);
    }
}
