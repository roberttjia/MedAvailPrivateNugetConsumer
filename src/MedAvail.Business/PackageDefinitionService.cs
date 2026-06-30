using System;
using System.Collections.Generic;
using MedAvail.Common.Models;
using MedAvail.Common.Repositories;
using MedAvail.Utilities;

namespace MedAvail.Business
{
    public sealed class PackageDefinitionService
        : ServiceBase<IPackageDefinitionRepository>, IValidator<PackageDefinitionDto>
    {
        public PackageDefinitionService(IPackageDefinitionRepository repository)
            : base(repository)
        {
        }

        public ValidationResult Validate(PackageDefinitionDto definition)
        {
            if (definition is null) throw new ArgumentNullException(nameof(definition));

            return ValidationResult.Builder()
                .AddErrorIf(string.IsNullOrWhiteSpace(definition.ProductName), "Product name is required.")
                .AddErrorIf(string.IsNullOrWhiteSpace(definition.PackageCode), "Package code is required.")
                .AddErrorIf(definition.PackageHeight <= 0 || definition.PackageWidth <= 0 || definition.PackageLength <= 0,
                    "Package dimensions must be positive.")
                .AddErrorIf(definition.Weight < 0, "Weight cannot be negative.")
                .AddErrorIf(definition.Cap && (definition.CapDiameter <= 0 || definition.CapLength <= 0),
                    "Capped packages must have positive cap dimensions.")
                .Build();
        }

        public decimal CalculateVolume(PackageDefinitionDto definition)
        {
            if (definition is null) throw new ArgumentNullException(nameof(definition));
            return definition.PackageHeight * definition.PackageWidth * definition.PackageLength;
        }

        public PackageDefinitionDto ApplyCreationDefaults(PackageDefinitionDto definition, string createdBy, DateTime utcNow)
        {
            if (definition is null) throw new ArgumentNullException(nameof(definition));
            GuardNotNullOrEmpty(createdBy, nameof(createdBy));

            definition.CreatedBy = createdBy;
            definition.ChangedBy = createdBy;
            definition.CreatedOn = utcNow;
            definition.ChangedDate = utcNow;
            if (!definition.Valid)
                definition.Valid = true;
            return definition;
        }

        public OperationResult<int> CreateValidated(PackageDefinitionDto definition, string createdBy, DateTime utcNow)
        {
            var prepared = ApplyCreationDefaults(definition, createdBy, utcNow);
            var validation = Validate(prepared);
            if (!validation.IsValid)
                return Failure<int>(validation.Summary);
            return Success(Repository.Insert(prepared));
        }

        public IReadOnlyList<PackageDefinitionDto> GetRecent(int requested)
        {
            var capped = Math.Max(1, Math.Min(requested, 500));
            return Repository.GetRecent(capped);
        }
    }
}
