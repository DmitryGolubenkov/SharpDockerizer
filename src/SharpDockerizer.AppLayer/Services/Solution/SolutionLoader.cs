using ByteDev.DotNet.Solution;
using SharpDockerizer.Core.Models;
using SharpDockerizer.AppLayer.Contracts;
using Serilog;
using System.Net;

namespace SharpDockerizer.AppLayer.Services.Solution;
public class SolutionLoader : ISolutionLoader, ISolutionUpdater
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
        var tempFile = Path.GetTempFileName();

        solutionFilePath = WebUtility.UrlDecode(solutionFilePath);

        try
        {
            // Create temp file to avoid locking solution file.
            File.Copy(solutionFilePath, tempFile, true);
            var solution = DotNetSolution.Load(tempFile);

            // Check if solution was loaded correctly. As values in library are lazily evaluated we need to access them 
            // to check if solution was loaded.
            _ = solution.FormatVersion;

            // Create new solution data
            _currentSolutionInfo.CurrentSolution = new SolutionData
            {
                Name = Path.GetFileNameWithoutExtension(solutionFilePath),
                SolutionRootDirectoryPath = Path.GetDirectoryName(solutionFilePath),
                SolutionFilePath = solutionFilePath,
            };

            // Read projects
            var projects = await LoadProjects(solution);
            _currentSolutionInfo.Projects = projects;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error occurred while loading solution.");
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    /// <summary>
    /// Refreshes loaded solution data.
    /// </summary>
    public async Task<bool> RefreshSolution()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            File.Copy(_currentSolutionInfo.CurrentSolution.SolutionFilePath, tempFile, true);
            var solution = DotNetSolution.Load(tempFile);

            // Read projects
            var newProjects = await LoadProjects(solution);
            // Then check if any changes were introduced (like new projects added, other removed etc).
            // Also we don't care about their order but SequenceEqual cares, so we need to sort them before comparing.


            if (!newProjects.OrderBy(x => x.ProjectName)
                .SequenceEqual(_currentSolutionInfo.Projects.OrderBy(x => x.ProjectName)))
            {
                _currentSolutionInfo.Projects = newProjects;

                return true;
            }
            else
            {
                // No changes
                return false;
            }
        }
        catch (InvalidDotNetSolutionException ex)
        {
            Log.Error(ex, "Error occurred while loading solution.");
            return false;
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    /// <summary>
    /// Loads projects that are mentioned in .sln file of solution.
    /// </summary>
    private async Task<List<ProjectData>> LoadProjects(DotNetSolution solution)
    {
        List<ProjectData> result = new List<ProjectData>();
        if (_currentSolutionInfo.CurrentSolution is null)
        {
            throw new InvalidOperationException("No solution was loaded.");
        }

        foreach (var project in solution.Projects)
        {
            // Ignore items that are not csproj or fsproj. Such items can include .NET projects for other  CLR languages or .dcproj
            // and they either are not supported or can't be dockerized at all
            if (!project.Path.EndsWith(".csproj") && !project.Path.EndsWith(".fsproj")) { continue; }

            var absoluteProjectPath = Path.Combine(
                Path.GetDirectoryName(_currentSolutionInfo.CurrentSolution.SolutionFilePath), project.Path);
            result.Add(await _projectDataExporter.GetProjectData(absoluteProjectPath));
        }

        return result;
    }
}
