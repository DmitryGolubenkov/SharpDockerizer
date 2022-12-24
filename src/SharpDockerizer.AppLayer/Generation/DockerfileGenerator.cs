using SharpDockerizer.Core.Models;
using SharpDockerizer.AppLayer.Contracts;
using SharpDockerizer.AppLayer.Models;
using System.Text;

namespace SharpDockerizer.AppLayer.Generation;
public class DockerfileGenerator : IDockerfileGenerator
{
    #region Fields

    private readonly IAspNetDockerImageVersionSelector _aspNetDockerImageVersionSelector;
    private readonly IDotNetSdkImageVersionSelector _dotNetSdkImageVersionSelector;
    private readonly IProjectDependenciesExporter _projectDependenciesExporter;
    private readonly ICurrentSolutionInfo _currentSolutionInfo;

    #endregion

    #region Constructor

    public DockerfileGenerator(
        IAspNetDockerImageVersionSelector aspNetDockerImageVersionSelector,
        IDotNetSdkImageVersionSelector dotNetSdkImageVersionSelector,
        IProjectDependenciesExporter projectDependenciesExporter,
        ICurrentSolutionInfo currentSolutionInfo,
        ISolutionUpdater solutionUpdater)
    {
        _aspNetDockerImageVersionSelector = aspNetDockerImageVersionSelector;
        _dotNetSdkImageVersionSelector = dotNetSdkImageVersionSelector;
        _projectDependenciesExporter = projectDependenciesExporter;
        _currentSolutionInfo = currentSolutionInfo;
    }

    #endregion

    /// <summary>
    /// Generates a Dockerfile using parsed solution data and user input.
    /// </summary>
    /// <returns>Dockerfile text as a multiline string.</returns>
    public string Execute(DockerfileGeneratorInputModel model)
    {
        // Prepare data
        var projectFileName = Path.GetFileName(model.SelectedProjectData.AbsolutePathToProjFile); // Ex: Project.File.Name.csproj
        var projectFolderRelativePath = Path.GetDirectoryName(model.SelectedProjectData.RelativePath); // Ex: /src/Project.API/

        var exposedPorts = GetExposedPorts(model.ExposedPorts);

        var projectDependencies = _projectDependenciesExporter.GetDependencies(model.SelectedProjectData);

        // Dockerfile arguments
        var dockerfileArgumentsList = new List<string>();

        // NuGet repos
        string nuGetInstructions = GetNuGetInstructions(model.NuGetSources, ref dockerfileArgumentsList);
        // NuGet configs
        string detectedNuGetConfigs = GetNuGetConfigFiles(model.SelectedProjectData);
        // Copy instructions for project files that will be used to restore packages
        var copyOnlyProjFileInstructions = GetCopyProjFilesDockerfileInstructions(projectDependencies);
        // Copy instructions for other files that will be used to build and publish assemblies 
        var copyEverythingInstructions = GetCopyEverythingDockerfileInstructions(projectDependencies);

        //Build resulting docker file
        var result = $"""
            FROM {_aspNetDockerImageVersionSelector.GetLinkToImageForVersion(model.SelectedProjectData.DotNetVersion)} AS base
            WORKDIR /app
            {exposedPorts}
            FROM {_dotNetSdkImageVersionSelector.GetLinkToImageForVersion(model.SelectedProjectData.DotNetVersion)} AS build
            {GetArgumentsString(dockerfileArgumentsList)}
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
            
            FROM base AS final
            WORKDIR /app
            COPY --from=publish /app/publish .
            ENTRYPOINT ["dotnet", "{model.SelectedProjectData.ProjectName}.dll"]
            """;

        // Fix Windows line separators
        return result.Replace('\\', '/');
    }

    /// <summary>
    /// Checks if "nuget.config" file exists in directories from solution folder 
    /// to project folder and generates COPY instructions for them.
    /// </summary>
    /// <param name="selectedProjectData">Data about selected project</param>
    /// <returns>Multiline string that contains COPY instructions for nuget.config</returns>
    private string GetNuGetConfigFiles(ProjectData selectedProjectData)
    {
        StringBuilder sb = new StringBuilder();

        // Search from project folder back to solution folder
        var currentCheckedPath = new DirectoryInfo(Path.GetDirectoryName(selectedProjectData.AbsolutePathToProjFile));
        while (currentCheckedPath.FullName != _currentSolutionInfo.CurrentSolution.SolutionRootDirectoryPath)
        {
            SearchDirectory();
            currentCheckedPath = currentCheckedPath.Parent;
        }

        // Search root directory too
        SearchDirectory();


        return sb.ToString();

        /// ====================
        /// Searches a directory for nuget.config files. If found, appends a 
        /// copy command to output that contains relative paths to files.
        /// ====================
        void SearchDirectory()
        {
            foreach (var configPath in currentCheckedPath.EnumerateFiles()
           .Where(filePath => Path.GetFileName(filePath.Name).ToLowerInvariant() == "nuget.config"))
            {
                var source = Path.GetRelativePath(_currentSolutionInfo.CurrentSolution.SolutionRootDirectoryPath, configPath.FullName);
                var destination = Path.GetDirectoryName(Path.GetRelativePath(_currentSolutionInfo.CurrentSolution.SolutionRootDirectoryPath, configPath.FullName));
                destination = !string.IsNullOrEmpty(destination) ? $"{destination}/" : "." ;
                sb.AppendLine($"COPY [\"{source}\", \"{destination}\"]");
            }
        }
    }

    /// <summary>
    /// Converts list of strings ("ARG ..."; "ARG...") into one string, each string on new line.
    /// </summary>
    private string GetArgumentsString(List<string> dockerfileArgumentsList)
    {
        var sb = new StringBuilder();

        foreach (string arg in dockerfileArgumentsList)
        {
            sb.AppendLine($@"ARG {arg}");
        }

        return sb.ToString();
    }

    /// <summary>
    /// Generates NuGet instructions for simple authentication.
    /// </summary>
    /// <param name="nuGetSources">List of NuGet sources.</param>
    /// <param name="dockerfileArgumentsList">List of strings, that will be modified.</param>
    /// <returns>"RUN dotnet nuget add source ..."  "RUN dotnet nuget update source..." as one multiline string. </returns>
    private string GetNuGetInstructions(List<NuGetSource> nuGetSources, ref List<string> dockerfileArgumentsList)
    {
        var sb = new StringBuilder();

        // TODO: Maybe don't store tokens in args? They are displayed in docker history. There should be a way to use secrets.
        foreach (var source in nuGetSources)
        {
            var argName = source.Name.ToLowerInvariant() + "token";
            dockerfileArgumentsList.Add(argName);
            sb.AppendLine($@"RUN dotnet nuget add source --name {source.Name} {source.Link}");
            // Authenticate if needed
            if (source.AuthenticationRequired)
            {
                sb.AppendLine(
                    $@"RUN dotnet nuget update source {source.Name} --store-password-in-clear-text --valid-authentication-types basic --username {source.Username} --password ${argName}"
                    );
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="exposedPorts">List of ports to expose.</param>
    /// <returns>A multiline string like "EXPOSE 80 \n EXPOSE 443"</returns>
    private string GetExposedPorts(List<int> exposedPorts)
    {
        var sb = new StringBuilder();

        foreach (var port in exposedPorts)
        {
            sb.AppendLine($@"EXPOSE {port}");
        }

        return sb.ToString();
    }

    private string GetCopyProjFilesDockerfileInstructions(List<ProjectData> projectDependencies)
    {
        var sb = new StringBuilder();

        foreach (ProjectData projectData in projectDependencies)
        {
            var projectFolderRelativePath = Path.GetDirectoryName(projectData.RelativePath);
            sb.AppendLine($@"COPY [""{projectData.RelativePath}"", ""{projectFolderRelativePath}/""]");
        }

        return sb.ToString();
    }

    private string GetCopyEverythingDockerfileInstructions(List<ProjectData> projectDependencies)
    {
        var sb = new StringBuilder();

        foreach (ProjectData projectData in projectDependencies)
        {
            var projectFolderRelativePath = Path.GetDirectoryName(projectData.RelativePath);
            sb.AppendLine($@"COPY [""{projectFolderRelativePath}/"", ""{projectFolderRelativePath}/""]");
        }

        return sb.ToString();

    }
}


