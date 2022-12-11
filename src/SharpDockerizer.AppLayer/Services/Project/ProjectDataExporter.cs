using SharpDockerizer.Core;
using SharpDockerizer.Core.Models;
using SharpDockerizer.AppLayer.Contracts;
using System.Xml.Linq;

namespace SharpDockerizer.AppLayer.Services.Project;
public class ProjectDataExporter : IProjectDataExporter
{
    private readonly ICurrentSolutionInfo _currentSolutionInfo;

    public ProjectDataExporter(ICurrentSolutionInfo currentSolutionInfo)
    {
        _currentSolutionInfo = currentSolutionInfo;
    }

    public async Task<ProjectData> GetProjectData(string path)
    {
        var relativePath = Path.GetRelativePath(_currentSolutionInfo.CurrentSolution.RootPath, path);
        var cancellationToken = new CancellationTokenSource().Token;
        using (var fileStream = File.OpenRead(path))
        {
            XDocument xml = await XDocument.LoadAsync(fileStream, LoadOptions.None, cancellationToken);

            var projectName = Path.GetFileNameWithoutExtension(path);
            var version = xml.Element("Project").Element("PropertyGroup").Element("TargetFramework").Value;
            var referencedProjects = xml.Root.Elements("ProjectReference").Select(x => x.Attribute("Include")?.Value);


            var projectData = new ProjectData()
            {
                ProjectName = projectName,
                DotNetVersion = version,
                AbsolutePathToProjFile = path,
                RelativePath = relativePath
            };

            return projectData;
        }

    }
}
