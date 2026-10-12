using System.Reflection;
using Calculator.Api.Mapping;
using Calculator.Contracts;
using Calculator.Data;

namespace Calculator.Tests.Architecture;

/// <summary>
/// Guards the layering: Api → Services → Data, with Contracts holding only the frontend DTOs
/// that nothing but the API knows about.
/// </summary>
public class LayerDependencyTests
{
    private static readonly Assembly Contracts = typeof(ErrorResponse).Assembly;
    private static readonly Assembly Services = typeof(Calculator.Services.DependencyInjection).Assembly;
    private static readonly Assembly Data = typeof(AppDbContext).Assembly;
    private static readonly Assembly Api = typeof(DtoMapper).Assembly;

    private static IEnumerable<string> CalculatorReferences(Assembly assembly) =>
        assembly.GetReferencedAssemblies()
            .Select(a => a.Name!)
            .Where(name => name.StartsWith("Calculator.", StringComparison.Ordinal))
            .Order();

    [Fact]
    public void Contracts_DependOnNoOtherCalculatorAssembly() =>
        Assert.Empty(CalculatorReferences(Contracts));

    [Fact]
    public void Data_DependsOnNoOtherCalculatorAssembly() =>
        Assert.Empty(CalculatorReferences(Data));

    [Fact]
    public void Services_DependOnlyOnData() =>
        Assert.Equal(["Calculator.Data"], CalculatorReferences(Services));

    [Fact]
    public void Api_UsesContractsServicesAndData() =>
        Assert.Equal(["Calculator.Contracts", "Calculator.Data", "Calculator.Services"], CalculatorReferences(Api));
}
