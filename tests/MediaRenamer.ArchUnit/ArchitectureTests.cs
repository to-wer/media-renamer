using ArchUnitNET.Domain;
using ArchUnitNET.Fluent;
using ArchUnitNET.Loader;
using ArchUnitNET.NUnit;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace MediaRenamer.ArchUnit;

[TestFixture]
public class ArchitectureTests
{
    private static readonly Architecture Architecture = new ArchLoader()
        .LoadAssemblies(
            typeof(Core.Abstractions.IProposalStore).Assembly,
            typeof(Api.Controllers.MediaController).Assembly,
            typeof(Web.Components.App).Assembly)
        .Build();

    [Test]
    public void Core_Should_Not_Have_Dependencies_On_Other_Layers()
    {
        var coreLayer = "MediaRenamer.Core";
        var apiLayer = "MediaRenamer.Api";
        var webLayer = "MediaRenamer.Web";

        var otherLayersClasses = Classes().That()
            .ResideInAssembly(apiLayer)
            .Or().ResideInAssembly(webLayer);

        var rule = Classes().That().ResideInAssembly(coreLayer)
            .Should().NotDependOnAny(otherLayersClasses)
            .Because("Core should be independent of higher layers.")
            .WithoutRequiringPositiveResults();

        rule.Check(Architecture);
    }

    [Test]
    public void Web_Should_Not_Have_Dependency_On_Api()
    {
        var webLayer = "MediaRenamer.Web";
        var apiLayer = "MediaRenamer.Api";

        var apiClasses = Classes().That().ResideInAssembly(apiLayer);

        var rule = Classes().That().ResideInAssembly(webLayer)
            .Should().NotDependOnAny(apiClasses)
            .Because("Web should be decoupled from Api and communicate via HTTP.")
            .WithoutRequiringPositiveResults();
        
        rule.Check(Architecture);
    }

    [Test]
    public void Controllers_Should_Be_In_Correct_Namespace_And_End_With_Controller()
    {
        var rule = Classes().That().HaveNameEndingWith("Controller")
            .And().ResideInNamespace("MediaRenamer.Api.Controllers")
            .Should().BePublic()
            .Because("All API controllers should be public and located in the correct namespace.");

        rule.Check(Architecture);
    }
    
    [Test]
    public void Interfaces_Should_Be_In_Abstractions_Namespace()
    {
        var rule = Interfaces().That().ResideInAssembly("MediaRenamer.Core")
            .Should().ResideInNamespace("MediaRenamer.Core.Abstractions")
            .Because("All interfaces in the Core project should be defined in the Abstractions namespace for clear separation.")
            .WithoutRequiringPositiveResults();

        rule.Check(Architecture);
    }
}
