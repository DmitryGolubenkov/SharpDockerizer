using SharpDockerizer.AppLayer.Contracts;
using SharpDockerizer.AppLayer.Models;
using Serilog;
using SharpDockerizer.AppLayer.Utilities;
using SharpDockerizer.AppLayer.Services.Templates;

namespace SharpDockerizer.AppLayer.Generation;

public class DockerfileGenerator : IDockerfileGenerator
{
    #region Fields

    private readonly IAspNetDockerImageVersionSelector _aspNetDockerImageVersionSelector;
    private readonly IDotNetSdkImageVersionSelector _dotNetSdkImageVersionSelector;
    private readonly IProjectDependenciesExporter _projectDependenciesExporter;
    private readonly ICurrentSolutionInfo _currentSolutionInfo;
    private readonly INuGetConfigExtractor _nugetConfigExtractor;
    private readonly DockerfileTemplateService _dockerfileTemplateService;

    #endregion

    #region Constructor

    public DockerfileGenerator(
        IAspNetDockerImageVersionSelector aspNetDockerImageVersionSelector,
        IDotNetSdkImageVersionSelector dotNetSdkImageVersionSelector,
        IProjectDependenciesExporter projectDependenciesExporter,
        ICurrentSolutionInfo currentSolutionInfo,
        INuGetConfigExtractor nugetConfigExtractor,
        DockerfileTemplateService dockerfileTemplateService
        )
    {
        _aspNetDockerImageVersionSelector = aspNetDockerImageVersionSelector;
        _dotNetSdkImageVersionSelector = dotNetSdkImageVersionSelector;
        _projectDependenciesExporter = projectDependenciesExporter;
        _currentSolutionInfo = currentSolutionInfo;
        _nugetConfigExtractor = nugetConfigExtractor;
        _dockerfileTemplateService = dockerfileTemplateService;
    }

    #endregion

    #region Default Template

    private const string _defaultTemplate = $"""
            # Auto-generated from default template by SharpDockerizer

            FROM sdDotNetSdkImageVersion AS build
            sdArgumentsList
            WORKDIR /src
            sdDetectedNuGetConfigs
            sdCopyOnlyProjFileInstructions
            sdNuGetSourceInstructions
            RUN dotnet restore "sdSelectedProjFileRelativePath"

            sdCopyEverythingInstructions
            FROM build AS publish
            WORKDIR "sdProjectFolderRelativePath"
            RUN dotnet publish "sdProjectFileName" --no-restore -c Release -o /app/publish /p:UseAppHost=false
            
            FROM sdAspNetDockerImageVersion AS final
            WORKDIR /app
            sdExposedPorts
            COPY --from=publish /app/publish .
            ENTRYPOINT ["dotnet", "sdProjectName.dll"]
            """;

    #endregion

    #region Methods

    public string Execute(DockerfileGeneratorInputModel model)
    {
        // First we need to load template file from disk

        // We need to select from where we load the file: from project or from solution.
        // Priority is:
        // 1. Use project template
        // 2. Use solution template
        // 3. Fallback to default generator
        var projectTemplateExists = model.SelectedProjectData.HasTemplate;
        var solutionTemplateExists = _currentSolutionInfo.CurrentSolution.HasTemplate;

        string usedTemplate = null;
        if (!projectTemplateExists && !solutionTemplateExists)
            usedTemplate = _defaultTemplate;
        else
        {
            usedTemplate = _dockerfileTemplateService.LoadTemplate(
                projectTemplateExists ? Path.GetDirectoryName(model.SelectedProjectData.AbsolutePathToProjFile)
                : _currentSolutionInfo.CurrentSolution.SolutionRootDirectoryPath);

            if(projectTemplateExists)
                usedTemplate = "# Auto-generated from project template by SharpDockerizer\n" + usedTemplate ;
            else 
                usedTemplate = "# Auto-generated from solution template by SharpDockerizer\n" + usedTemplate ;
            
        }

        // Now we need to generate Dockerfile using template
        // First we need to get some data
        // Then we will need to 
        Log.Information("Generating dockerfile for project:", model.SelectedProjectData.ProjectName);

        // Prepare data
        var sdDotNetSdkImageVersion = _dotNetSdkImageVersionSelector.GetLinkToImageForVersion(model.SelectedProjectData.DotNetVersion);
        var sdAspNetDockerImageVersion = _aspNetDockerImageVersionSelector.GetLinkToImageForVersion(model.SelectedProjectData.DotNetVersion);
        var sdProjectFileName = Path.GetFileName(model.SelectedProjectData.AbsolutePathToProjFile); // Ex: Project.File.Name.csproj
        var sdProjectFolderRelativePath = Path.GetDirectoryName(model.SelectedProjectData.RelativePathToProjFile)
            .Replace('\\', '/'); // Ex: /src/Project.API/
        var sdProjectName = model.SelectedProjectData.ProjectName;
        var sdExposedPorts = DockerfileUtilities.GetExposedPorts(model.ExposedPorts);
        var sdSelectedProjFileRelativePath = model.SelectedProjectData.RelativePathToProjFile;

        // Dockerfile arguments
        var dockerfileArgumentsList = new List<string>();

        // NuGet repos
        string sdNuGetSourceInstructions = DockerfileUtilities.GetNuGetInstructions(model.NuGetSources, ref dockerfileArgumentsList);
        var sdArgumentsList = DockerfileUtilities.GetArgumentsString(dockerfileArgumentsList);
        // NuGet configs
        string sdDetectedNuGetConfigs = _nugetConfigExtractor.GetNuGetConfigFiles(model.SelectedProjectData);

        // First find all dependencies of current project
        var projectsThatAreUsedForGeneration = _projectDependenciesExporter.GetDependencies(model.SelectedProjectData);
        // The add current project
        projectsThatAreUsedForGeneration.Add(model.SelectedProjectData);

        // Copy instructions for project files that will be used to restore packages
        var sdCopyOnlyProjFileInstructions = DockerfileUtilities.GetCopyProjFilesDockerfileInstructions(projectsThatAreUsedForGeneration.OrderByDescending(x => x.IsAspNetProject).ToList());
        // Copy instructions for other files that will be used to build and publish assemblies 
        var sdCopyEverythingInstructions = DockerfileUtilities.GetCopyEverythingDockerfileInstructions(projectsThatAreUsedForGeneration.OrderByDescending(x => x.IsAspNetProject).ToList());

        // Finally generate dockerfile from template
        var generatedDockerFile = usedTemplate
            .Replace("sdDotNetSdkImageVersion", sdDotNetSdkImageVersion) // Version of .NET SDK image that is used in dockerfile
            .Replace("sdAspNetDockerImageVersion", sdAspNetDockerImageVersion) // Version of ASP.NET image that is used in dockerfile
            .Replace("sdProjectFileName", sdProjectFileName) // Name of project file for which dockerfile is generated.
            .Replace("sdProjectFolderRelativePath", sdProjectFolderRelativePath) // Relative path from solution folder to project folder
            .Replace("sdProjectName", sdProjectName) // Name of project for which dockerfile is generated. Parsed from proj file name.
            .Replace("sdExposedPorts", sdExposedPorts) // Ports that should be exposed inside container. Doesn't guarantee that the app really listens for this port.
            .Replace("sdSelectedProjFileRelativePath", sdSelectedProjFileRelativePath) // Relative path to project file from solution folder.
            .Replace("sdNuGetSourceInstructions", sdNuGetSourceInstructions) // NuGet instructions generated depending on settings.
            .Replace("sdArgumentsList", sdArgumentsList) // ARG instructions generated depending on settings
            .Replace("sdDetectedNuGetConfigs", sdDetectedNuGetConfigs) // NuGet config files that were detected in solution and projects
            .Replace("sdCopyOnlyProjFileInstructions", sdCopyOnlyProjFileInstructions) // All COPY instructions for proj files, without the rest of files. Can be used to restore nuget packages.
            .Replace("sdCopyEverythingInstructions", sdCopyEverythingInstructions); // All COPY instructions for all files of projects that would be built.


        Log.Information("Successfully generated dockerfile for project:", model.SelectedProjectData.ProjectName);


        return generatedDockerFile;
    }

    #endregion
}


#region Legacy

//public class StandartDockerfileGenerator : IDockerfileGenerator
//{
//    #region Fields

//    private readonly IAspNetDockerImageVersionSelector _aspNetDockerImageVersionSelector;
//    private readonly IDotNetSdkImageVersionSelector _dotNetSdkImageVersionSelector;
//    private readonly IProjectDependenciesExporter _projectDependenciesExporter;
//    private readonly ICurrentSolutionInfo _currentSolutionInfo;
//    private readonly INuGetConfigExtractor _nugetConfigExtractor;

//    #endregion

//    #region Constructor

//    public StandartDockerfileGenerator(
//        IAspNetDockerImageVersionSelector aspNetDockerImageVersionSelector,
//        IDotNetSdkImageVersionSelector dotNetSdkImageVersionSelector,
//        IProjectDependenciesExporter projectDependenciesExporter,
//        ICurrentSolutionInfo currentSolutionInfo,
//        INuGetConfigExtractor nugetConfigExtractor)
//    {
//        _aspNetDockerImageVersionSelector = aspNetDockerImageVersionSelector;
//        _dotNetSdkImageVersionSelector = dotNetSdkImageVersionSelector;
//        _projectDependenciesExporter = projectDependenciesExporter;
//        _currentSolutionInfo = currentSolutionInfo;
//        _nugetConfigExtractor = nugetConfigExtractor;
//    }

//    #endregion

//    #region Methods

//    /// <summary>
//    /// Generates a Dockerfile using parsed solution data and user input.
//    /// </summary>
//    /// <returns>Dockerfile text as a multiline string.</returns>
//    public string Execute(DockerfileGeneratorInputModel model)
//    {
//        try
//        {
//            Log.Information("Generating dockerfile for model:", model);

//            // Prepare data
//            var projectFileName = Path.GetFileName(model.SelectedProjectData.AbsolutePathToProjFile); // Ex: Project.File.Name.csproj
//            var projectFolderRelativePath = Path.GetDirectoryName(model.SelectedProjectData.RelativePath); // Ex: /src/Project.API/

//            var exposedPorts = DockerfileUtilities.GetExposedPorts(model.ExposedPorts);

//            var projectDependencies = _projectDependenciesExporter.GetDependencies(model.SelectedProjectData);

//            // Dockerfile arguments
//            var dockerfileArgumentsList = new List<string>();

//            // NuGet repos
//            string nuGetInstructions = DockerfileUtilities.GetNuGetInstructions(model.NuGetSources, ref dockerfileArgumentsList);
//            // NuGet configs
//            string detectedNuGetConfigs = _nugetConfigExtractor.GetNuGetConfigFiles(model.SelectedProjectData);
//            // Copy instructions for project files that will be used to restore packages
//            var copyOnlyProjFileInstructions = DockerfileUtilities.GetCopyProjFilesDockerfileInstructions(projectDependencies);
//            // Copy instructions for other files that will be used to build and publish assemblies 
//            var copyEverythingInstructions = DockerfileUtilities.GetCopyEverythingDockerfileInstructions(projectDependencies);

//            ////Build resulting docker file
//            //var result = $"""
//            //FROM {_aspNetDockerImageVersionSelector.GetLinkToImageForVersion(model.SelectedProjectData.DotNetVersion)} AS base
//            //WORKDIR /app
//            //{exposedPorts}
//            //FROM {_dotNetSdkImageVersionSelector.GetLinkToImageForVersion(model.SelectedProjectData.DotNetVersion)} AS build
//            //{DockerfileUtilities.GetArgumentsString(dockerfileArgumentsList)}
//            //WORKDIR /src

//            //{detectedNuGetConfigs}
//            //COPY ["{model.SelectedProjectData.RelativePath}", "{projectFolderRelativePath}/"]
//            //{copyOnlyProjFileInstructions}
//            //{nuGetInstructions}
//            //RUN dotnet restore "{model.SelectedProjectData.RelativePath}"

//            //COPY ["{projectFolderRelativePath}/", "{projectFolderRelativePath}/"]
//            //{copyEverythingInstructions}
//            //FROM build AS publish
//            //WORKDIR "{projectFolderRelativePath}"
//            //RUN dotnet publish "{projectFileName}" --no-restore -c Release -o /app/publish /p:UseAppHost=false

//            //FROM base AS final
//            //WORKDIR /app
//            //COPY --from=publish /app/publish .
//            //ENTRYPOINT ["dotnet", "{model.SelectedProjectData.ProjectName}.dll"]
//            //""";

//            //Build resulting docker file
//            var result = $"""
//            FROM {_dotNetSdkImageVersionSelector.GetLinkToImageForVersion(model.SelectedProjectData.DotNetVersion)} AS build
//            {DockerfileUtilities.GetArgumentsString(dockerfileArgumentsList)}
//            WORKDIR /src
//            {detectedNuGetConfigs}
//            COPY ["{model.SelectedProjectData.RelativePath}", "{projectFolderRelativePath}/"]
//            {copyOnlyProjFileInstructions}
//            {nuGetInstructions}
//            RUN dotnet restore "{model.SelectedProjectData.RelativePath}"

//            COPY ["{projectFolderRelativePath}/", "{projectFolderRelativePath}/"]
//            {copyEverythingInstructions}
//            FROM build AS publish
//            WORKDIR "{projectFolderRelativePath}"
//            RUN dotnet publish "{projectFileName}" --no-restore -c Release -o /app/publish /p:UseAppHost=false

//            FROM {_aspNetDockerImageVersionSelector.GetLinkToImageForVersion(model.SelectedProjectData.DotNetVersion)} AS final
//            WORKDIR /app
//            {exposedPorts}
//            COPY --from=publish /app/publish .
//            ENTRYPOINT ["dotnet", "{model.SelectedProjectData.ProjectName}.dll"]
//            """;

//            Log.Information("Dockerfile generated successfuly");

//            // Fix Windows line separators
//            return result.Replace('\\', '/');
//        }
//        catch (Exception ex)
//        {
//            Log.Error("Exception while generating Dockerfile!", ex);
//            return "";
//        }
//    }

//    #endregion

//}


#endregion