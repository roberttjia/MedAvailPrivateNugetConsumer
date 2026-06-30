using System;
using System.Collections.Generic;
using MedAvail.Business;
using MedAvail.Common.Models;
using MedAvail.Common.Repositories;
using Moq;
using Xunit;

namespace MedAvail.Business.Tests;

public class PackageDefinitionServiceRepositoryTests
{
    private static PackageDefinitionDto ValidDefinition() => new()
    {
        ProductName = "Ibuprofen 200mg",
        PackageCode = "099999999901",
        PackageHeight = 2m,
        PackageWidth = 2m,
        PackageLength = 2m,
        Weight = 0.5m
    };

    [Fact]
    public void CreateValidated_ValidInput_InsertsAndReturnsNewId()
    {
        var repo = new Mock<IPackageDefinitionRepository>();
        repo.Setup(r => r.Insert(It.IsAny<PackageDefinitionDto>())).Returns(4242);

        var service = new PackageDefinitionService(repo.Object);
        var now = DateTime.UtcNow;

        var result = service.CreateValidated(ValidDefinition(), "creator", now);

        Assert.True(result.IsSuccess);
        Assert.Equal(4242, result.Value);
        repo.Verify(r => r.Insert(It.Is<PackageDefinitionDto>(d =>
            d.CreatedBy == "creator" && d.ChangedBy == "creator" &&
            d.CreatedOn == now && d.Valid)), Times.Once);
    }

    [Fact]
    public void CreateValidated_InvalidInput_ReturnsFailureAndNeverHitsRepository()
    {
        var repo = new Mock<IPackageDefinitionRepository>(MockBehavior.Strict);
        var service = new PackageDefinitionService(repo.Object);

        var invalid = ValidDefinition();
        invalid.ProductName = "";

        var result = service.CreateValidated(invalid, "creator", DateTime.UtcNow);

        Assert.False(result.IsSuccess);
        Assert.Contains("Product name", result.ErrorSummary);
        repo.Verify(r => r.Insert(It.IsAny<PackageDefinitionDto>()), Times.Never);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(-50, 1)]
    [InlineData(100, 100)]
    [InlineData(9999, 500)]
    public void GetRecent_ClampsPageSizeBeforeCallingRepository(int requested, int expected)
    {
        var repo = new Mock<IPackageDefinitionRepository>();
        repo.Setup(r => r.GetRecent(It.IsAny<int>()))
            .Returns(new List<PackageDefinitionDto>());

        var service = new PackageDefinitionService(repo.Object);
        service.GetRecent(requested);

        repo.Verify(r => r.GetRecent(expected), Times.Once);
    }

    [Fact]
    public void GetRecent_ReturnsRepositoryResults()
    {
        var rows = new List<PackageDefinitionDto>
        {
            new() { PackageDefinitionId = 1, ProductName = "A" },
            new() { PackageDefinitionId = 2, ProductName = "B" },
        };
        var repo = new Mock<IPackageDefinitionRepository>();
        repo.Setup(r => r.GetRecent(It.IsAny<int>())).Returns(rows);

        var service = new PackageDefinitionService(repo.Object);
        var result = service.GetRecent(10);

        Assert.Equal(2, result.Count);
        Assert.Equal("A", result[0].ProductName);
    }

    [Fact]
    public void Constructor_NullRepository_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new PackageDefinitionService(null!));
    }
}
