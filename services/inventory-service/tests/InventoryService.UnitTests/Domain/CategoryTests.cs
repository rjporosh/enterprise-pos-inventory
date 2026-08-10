using FluentAssertions;
using InventoryService.Domain.Catalog;
using Xunit;

namespace InventoryService.UnitTests.Domain;

public class CategoryTests
{
    [Fact]
    public void CreateCategory_WithValidName_ShouldSucceed()
    {
        var category = new Category("Grocery", "Grocery items");
        
        category.Name.Should().Be("Grocery");
        category.Description.Should().Be("Grocery items");
        category.IsActive.Should().BeTrue();
    }

    [Fact]
    public void CreateCategory_WithEmptyName_ShouldThrow()
    {
        Action act = () => new Category(string.Empty);
        
        act.Should().Throw<ArgumentException>()
           .WithMessage("*cannot be null or empty*");
    }

    [Fact]
    public void RenameCategory_WithValidName_ShouldUpdateName()
    {
        var category = new Category("OldName");
        category.Rename("NewName");
        
        category.Name.Should().Be("NewName");
    }

    [Fact]
    public void RenameCategory_WithEmptyName_ShouldThrow()
    {
        var category = new Category("ValidName");
        Action act = () => category.Rename(string.Empty);
        
        act.Should().Throw<ArgumentException>();
    }
}
