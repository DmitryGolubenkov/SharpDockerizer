using ByteDev.DotNet.Project;
using SharpDockerizer.Core.Models;
using SharpDockerizer.AppLayer.Contracts;
using Serilog;

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
    /// <returns>Projects that the <paramref name="project"/> depends on.</returns>

    public List<ProjectData> GetDependencies(ProjectData project)
    {
        // Don't add project if it was already referenced 
        return GetDependencies(project.AbsolutePathToProjFile).DistinctBy(x => x.AbsolutePathToProjFile).ToList();
    }

    /// <summary>
    /// Recursively searches through project dependencies and finds all projects that are needed to build. Uses path to Project file as a parameter.
    /// </summary>
    /// <param name="pathToProj">Path to project file.</param>
    /// <returns>Projects that the <paramref name="pathToProj"/> depends on.</returns>
    private List<ProjectData> GetDependencies(string pathToProj)
    {
        var dependencies = new List<ProjectData>();


        var dotNetProject = DotNetProject.Load(pathToProj);

        Log.Information($"Searching for dependencies of project {pathToProj}");
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
                Log.Information($"Found dependency for project at path {pathToProj} ", dep);
                dependencies.Add(dep);
            }
        }


        return dependencies;
    }
}
