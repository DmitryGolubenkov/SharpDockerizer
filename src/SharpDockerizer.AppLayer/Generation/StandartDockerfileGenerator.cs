using SharpDockerizer.AppLayer.Contracts;
using SharpDockerizer.AppLayer.Models;
using Serilog;
using SharpDockerizer.AppLayer.Utilities;
using SharpDockerizer.AppLayer.Services.Solution;

namespace SharpDockerizer.AppLayer.Generation;
public class StandartDockerfileGenerator : IDockerfileGenerator
{
    #region Fields

    private readonly IAspNetDockerImageVersionSelector _aspNetDockerImageVersionSelector;
    private readonly IDotNetSdkImageVersionSelector _dotNetSdkImageVersionSelector;
    private readonly IProjectDependenciesExporter _projectDependenciesExporter;
    private readonly ICurrentSolutionInfo _currentSolutionInfo;
    private readonly INuGetConfigExtractor _nugetConfigExtractor;

    #endregion

    #region Constructor

    public StandartDockerfileGenerator(
        IAspNetDockerImageVersionSelector aspNetDockerImageVersionSelector,
        IDotNetSdkImageVersionSelector dotNetSdkImageVersionSelector,
        IProjectDependenciesExporter projectDependenciesExporter,
        ICurrentSolutionInfo currentSolutionInfo,
        INuGetConfigExtractor nugetConfigExtractor)
    {
        _aspNetDockerImageVersionSelector = aspNetDockerImageVersionSelector;
        _dotNetSdkImageVersionSelector = dotNetSdkImageVersionSelector;
        _projectDependenciesExporter = projectDependenciesExporter;
        _currentSolutionInfo = currentSolutionInfo;
        _nugetConfigExtractor = nugetConfigExtractor;
    }

    #endregion

    /// <summary>
    /// Generates a Dockerfile using parsed solution data and user input.
    /// </summary>
    /// <returns>Dockerfile text as a multiline string.</returns>
    public string Execute(DockerfileGeneratorInputModel model)
    {
        try
        {
            Log.Information("Generating dockerfile for model:", model);

            // Prepare data
            var projectFileName = Path.GetFileName(model.SelectedProjectData.AbsolutePathToProjFile); // Ex: Project.File.Name.csproj
            var projectFolderRelativePath = Path.GetDirectoryName(model.SelectedProjectData.RelativePath); // Ex: /src/Project.API/

            var exposedPorts = DockerfileUtilities.GetExposedPorts(model.ExposedPorts);

            var projectDependencies = _projectDependenciesExporter.GetDependencies(model.SelectedProjectData);

            // Dockerfile arguments
            var dockerfileArgumentsList = new List<string>();

            // NuGet repos
            string nuGetInstructions = DockerfileUtilities.GetNuGetInstructions(model.NuGetSources, ref dockerfileArgumentsList);
            // NuGet configs
            string detectedNuGetConfigs = _nugetConfigExtractor.GetNuGetConfigFiles(model.SelectedProjectData);
            // Copy instructions for project files that will be used to restore packages
            var copyOnlyProjFileInstructions = DockerfileUtilities.GetCopyProjFilesDockerfileInstructions(projectDependencies);
            // Copy instructions for other files that will be used to build and publish assemblies 
            var copyEverythingInstructions = DockerfileUtilities.GetCopyEverythingDockerfileInstructions(projectDependencies);

            ////Build resulting docker file
            //var result = $"""
            //FROM {_aspNetDockerImageVersionSelector.GetLinkToImageForVersion(model.SelectedProjectData.DotNetVersion)} AS base
            //WORKDIR /app
            //{exposedPorts}
            //FROM {_dotNetSdkImageVersionSelector.GetLinkToImageForVersion(model.SelectedProjectData.DotNetVersion)} AS build
            //{DockerfileUtilities.GetArgumentsString(dockerfileArgumentsList)}
            //WORKDIR /src

            //{detectedNuGetConfigs}
            //COPY ["{model.SelectedProjectData.RelativePath}", "{projectFolderRelativePath}/"]
            //{copyOnlyProjFileInstructions}
            //{nuGetInstructions}
            //RUN dotnet restore "{model.SelectedProjectData.RelativePath}"

            //COPY ["{projectFolderRelativePath}/", "{projectFolderRelativePath}/"]
            //{copyEverythingInstructions}
            //FROM build AS publish
            //WORKDIR "{projectFolderRelativePath}"
            //RUN dotnet publish "{projectFileName}" --no-restore -c Release -o /app/publish /p:UseAppHost=false

            //FROM base AS final
            //WORKDIR /app
            //COPY --from=publish /app/publish .
            //ENTRYPOINT ["dotnet", "{model.SelectedProjectData.ProjectName}.dll"]
            //""";

            //Build resulting docker file
            var result = $"""
            FROM {_dotNetSdkImageVersionSelector.GetLinkToImageForVersion(model.SelectedProjectData.DotNetVersion)} AS build
            {DockerfileUtilities.GetArgumentsString(dockerfileArgumentsList)}
            WORKDIR /src
            {detectedNuGetConfigs}
            COPY ["{model.SelectedProjectData.RelativePath}", "{projectFolderRelativePath}/"]
            {copyOnlyProjFileInstructions}
            {nuGetInstructions}
            RUN dotnet restore "{model.SelectedProjectData.RelativePath}"

            COPY ["{projectFolderRelativePath}/", "{projectFolderRelativePath}/"]
            {copyEverythingInstructions}
            FROM build AS publish
            WORKDIR "{projectFolderRelativePath}"
            RUN dotnet publish "{projectFileName}" --no-restore -c Release -o /app/publish /p:UseAppHost=false
            
            FROM {_aspNetDockerImageVersionSelector.GetLinkToImageForVersion(model.SelectedProjectData.DotNetVersion)} AS final
            WORKDIR /app
            {exposedPorts}
            COPY --from=publish /app/publish .
            ENTRYPOINT ["dotnet", "{model.SelectedProjectData.ProjectName}.dll"]
            """;

            Log.Information("Dockerfile generated successfuly");

            // Fix Windows line separators
            return result.Replace('\\', '/');
        }
        catch (Exception ex)
        {
            Log.Error("Exception while generating Dockerfile!", ex);
            return "";
        }
    }

    
}


