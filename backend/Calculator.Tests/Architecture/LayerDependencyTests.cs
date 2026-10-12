using System.Reflection;
using Calculator.Contracts.Calculations;
using Calculator.Data;

namespace Calculator.Tests.Architecture;

/// <summary>Guards the layering: implementations depend on the contracts, never on each other.</summary>
public class LayerDependencyTests
{
    private static readonly Assembly Contracts = typeof(ICalculatorService).Assembly;
    private static readonly Assembly Services = typeof(Calculator.Services.DependencyInjection).Assembly;
    private static readonly Assembly Data = typeof(AppDbContext).Assembly;

    private static IEnumerable<string> CalculatorReferences(Assembly assembly) =>
        assembly.GetReferencedAssemblies()
            .Select(a => a.Name!)
            .Where(name => name.StartsWith("Calculator.", StringComparison.Ordinal));

    [Fact]
    public void Contracts_DependOnNoOtherCalculatorAssembly()
    {
        Assert.Empty(CalculatorReferences(Contracts));
    }

    [Fact]
    public void Services_DependOnlyOnContracts()
    {
        Assert.Equal(["Calculator.Contracts"], CalculatorReferences(Services));
    }

    [Fact]
    public void Data_DependsOnlyOnContracts()
    {
        Assert.Equal(["Calculator.Contracts"], CalculatorReferences(Data));
    }
}
