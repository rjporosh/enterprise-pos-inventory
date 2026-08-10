using FluentAssertions;
using InventoryService.Domain.Catalog;
using Xunit;

namespace InventoryService.UnitTests.Domain;

public class BrandTests
{
    [Fact]
    public void CreateBrand_WithValidName_ShouldSucceed()
    {
        var brand = new Brand("TechPro", "Premium electronics");
        
        brand.Name.Should().Be("TechPro");
        brand.Description.Should().Be("Premium electronics");
        brand.IsActive.Should().BeTrue();
    }

    [Fact]
    public void CreateBrand_WithEmptyName_ShouldThrow()
    {
        Action act = () => new Brand(string.Empty);
        
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void RenameBrand_ShouldUpdateName()
    {
        var brand = new Brand("OldName");
        brand.Rename("NewName");
        
        brand.Name.Should().Be("NewName");
    }
}
