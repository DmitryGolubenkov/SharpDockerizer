using ByteDev.DotNet.Project;
using SharpDockerizer.Core.Models;
using SharpDockerizer.AppLayer.Contracts;

namespace SharpDockerizer.AppLayer.Services.Project;
public class ProjectDependenciesExporter : IProjectDependenciesExporter
{
    private readonly ICurrentSolutionInfo _currentSolutionInfo;

    public ProjectDependenciesExporter(ICurrentSolutionInfo currentSolutionInfo)
    {
        _currentSolutionInfo = currentSolutionInfo;
    }


    /// <summary>
    /// Recursively searches through project dependencies and finds all projects that are needed to build
    /// the <paramref name="project"/>
    /// </summary>
    /// <param name="project">Project which dependencies will be returned</param>
    public List<ProjectData> GetDependencies(ProjectData project)
    {
        return GetDependencies(project.AbsolutePathToProjFile);
    }

    private List<ProjectData> GetDependencies(string pathToProj)
    {
        var dependencies = new List<ProjectData>();


        var dotNetProject = DotNetProject.Load(pathToProj);

        foreach (var reference in dotNetProject.ProjectReferences)
        {
            var name = Path.GetFileNameWithoutExtension(reference.FilePath);
            var referencedProject = _currentSolutionInfo.Projects.FirstOrDefault(p => p.ProjectName == name);

            // Add this reference to dependencies
            dependencies.Add(referencedProject);

            // Get dependencies of this reference
            var refDependencies = GetDependencies(referencedProject.AbsolutePathToProjFile);

            foreach (var dep in refDependencies)
            {
                // Don't add project if it was already referenced 
                if (!dependencies.Where(x => x.RelativePath == dep.RelativePath).Any())
                {
                    dependencies.Add(dep);
                }
            }
        }


        return dependencies;
    }
}
