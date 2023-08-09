using SharpDockerizer.Core.Models;
using SharpDockerizer.AppLayer.Contracts;
using System.Xml.Linq;
using Serilog;
using SharpDockerizer.AppLayer.Services.Templates;

namespace SharpDockerizer.AppLayer.Services.Project;
public class ProjectDataExporter : IProjectDataExporter
{
    private readonly ICurrentSolutionInfo _currentSolutionInfo;
    private readonly DockerfileTemplateService dockerfileTemplateService;

    public ProjectDataExporter(ICurrentSolutionInfo currentSolutionInfo,
        DockerfileTemplateService dockerfileTemplateService)
    {
        _currentSolutionInfo = currentSolutionInfo;
        this.dockerfileTemplateService = dockerfileTemplateService;
    }

    /// <summary>
    /// Extracts data from proj file and returns ProjectData object.
    /// </summary>
    /// <param name="path">Full path to proj file. </param>
    public async Task<ProjectData> GetProjectData(string path)
    {
        Log.Information($"Extracting data from project at path {path}");
        var relativePath = Path.GetRelativePath(_currentSolutionInfo.CurrentSolution.SolutionRootDirectoryPath, path);
        var cancellationToken = new CancellationTokenSource().Token;
        using (var fileStream = File.OpenRead(path))
        {
            XDocument xml = await XDocument.LoadAsync(fileStream, LoadOptions.None, cancellationToken);

            var projectName = Path.GetFileNameWithoutExtension(path);

            var isAspNetProject = xml.Root.Attribute("Sdk")?.Value == "Microsoft.NET.Sdk.Web";
            var targetFramework = xml.Root.Descendants().Where(element => element.Name == "TargetFramework" || element.Name == "TargetFrameworks").FirstOrDefault();
            var version = targetFramework is not null ? ParseDotNetVersion(targetFramework.Value) : null;

            var projectData = new ProjectData()
            {
                ProjectName = projectName,
                DotNetVersion = version,
                AbsolutePathToProjFile = path.Replace('\\', '/'),
                RelativePathToProjFile = relativePath.Replace('\\', '/'),
                IsAspNetProject = isAspNetProject,
                HasTemplate = dockerfileTemplateService.CheckIfPathContainsTemplate(Path.GetDirectoryName(path))
            };

            Log.Information($"Project data extracted for project: {projectData.ProjectName}");

            return projectData;
        }

    }

    /// <summary>
    /// Parses string that contains dotnet versions and returns one of them, or null, if passed data is null.
    /// </summary>
    private string? ParseDotNetVersion(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var values = value.Split(';');
        if (values.Length == 1)
            return values[0].Trim();

        return values.Last();
    }
}
