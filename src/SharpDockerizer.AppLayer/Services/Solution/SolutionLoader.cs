using ByteDev.DotNet.Solution;
using SharpDockerizer.Core.Models;
using SharpDockerizer.AppLayer.Contracts;
using Serilog;

namespace SharpDockerizer.AppLayer.Services.Solution;
public class SolutionLoader : ISolutionLoader
{
    private readonly ICurrentSolutionInfo _currentSolutionInfo;
    private readonly IProjectDataExporter _projectDataExporter;

    public SolutionLoader(ICurrentSolutionInfo currentSolutionInfo, IProjectDataExporter projectDataExporter)
    {
        _currentSolutionInfo = currentSolutionInfo;
        _projectDataExporter = projectDataExporter;
    }

    /// <summary>
    /// Loads .NET solution into application
    /// </summary>
    /// <param name="solutionFilePath">Absolute path to .sln file</param>
    public async Task LoadSolution(string solutionFilePath)
    {
        var solution = DotNetSolution.Load(solutionFilePath);

        try
        {
            // Check if solution was loaded correctly. As values in library are lazily evaluated we need to access them 
            // to check if solution was loaded.
            _ = solution.FormatVersion;


            _currentSolutionInfo.CurrentSolution = new SolutionData
            {
                RootPath = Path.GetDirectoryName(solutionFilePath),
            };


            _currentSolutionInfo.Projects = new List<ProjectData>();

            foreach (var project in solution.Projects)
            {
                // Ignore items that are not csproj or fsproj. Such items can include .NET projects for other  CLR languages or .dcproj
                // and they either are not supported or can't be dockerized at all
                if (!project.Path.EndsWith(".csproj") && !project.Path.EndsWith(".fsproj")) { continue; }

                var absoluteProjectPath = Path.Combine(Path.GetDirectoryName(solutionFilePath), project.Path);
                _currentSolutionInfo.Projects.Add(await _projectDataExporter.GetProjectData(absoluteProjectPath));
            }
        }
        catch (InvalidDotNetSolutionException ex)
        {
            Log.Error(ex, "Error occurred while loading solution.");
        }
    }
}
