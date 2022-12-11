using SharpDockerizer.Core.Models;
using SharpDockerizer.AppLayer.Contracts;
using SharpDockerizer.AppLayer.Models;
using System.Text;

namespace SharpDockerizer.AppLayer.Generation;
public class DockerfileGenerator : IDockerfileGenerator
{
    private readonly IAspNetDockerImageVersionSelector _aspNetDockerImageVersionSelector;
    private readonly IDotNetSdkImageVersionSelector _dotNetSdkImageVersionSelector;
    private readonly IProjectDependenciesExporter _projectDependenciesExporter;

    public DockerfileGenerator(
        IAspNetDockerImageVersionSelector aspNetDockerImageVersionSelector,
        IDotNetSdkImageVersionSelector dotNetSdkImageVersionSelector,
        IProjectDependenciesExporter projectDependenciesExporter)
    {
        _aspNetDockerImageVersionSelector = aspNetDockerImageVersionSelector;
        _dotNetSdkImageVersionSelector = dotNetSdkImageVersionSelector;
        _projectDependenciesExporter = projectDependenciesExporter;
    }

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

            COPY ["{model.SelectedProjectData.RelativePath}", "{projectFolderRelativePath}/"]
            {copyOnlyProjFileInstructions}

            {nuGetInstructions}
            RUN dotnet restore "{model.SelectedProjectData.RelativePath}"

            COPY ["{projectFolderRelativePath}/", "{projectFolderRelativePath}/"]
            {copyEverythingInstructions}

            WORKDIR "{projectFolderRelativePath}"
            RUN dotnet build "{projectFileName}" -c Release -o /app/build
            
            FROM build AS publish
            RUN dotnet publish "{projectFileName}" -c Release -o /app/publish
            
            FROM base AS final
            WORKDIR /app
            COPY --from=publish /app/publish .
            ENTRYPOINT ["dotnet", "{model.SelectedProjectData.ProjectName}.dll"]
            """;

        // Fix Windows line separators
        return result.Replace('\\', '/');
    }

    private string GetArgumentsString(List<string> dockerfileArgumentsList)
    {
        var sb = new StringBuilder();

        foreach (string arg in dockerfileArgumentsList)
        {
            sb.AppendLine($@"ARG {arg}");
        }

        return sb.ToString();
    }

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


