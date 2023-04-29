using SharpDockerizer.AppLayer.Contracts;
using SharpDockerizer.Core.Models;
using System.Text;

namespace SharpDockerizer.AppLayer.Services.Solution;
public class NuGetConfigExtractor : INuGetConfigExtractor
{
    private readonly ICurrentSolutionInfo _currentSolutionInfo;

    public NuGetConfigExtractor(ICurrentSolutionInfo currentSolutionInfo)
    {
        _currentSolutionInfo = currentSolutionInfo;
    }

    /// <summary>
    /// Checks if "nuget.config" file exists in directories from solution folder 
    /// to project folder and generates COPY instructions for them.
    /// </summary>
    /// <param name="selectedProjectData">Data about selected project</param>
    /// <returns>Multiline string that contains COPY instructions for nuget.config</returns>
    public string GetNuGetConfigFiles(ProjectData selectedProjectData)
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
                destination = !string.IsNullOrEmpty(destination) ? $"{destination}/" : ".";
                sb.AppendLine($"COPY [\"{source}\", \"{destination}\"]");
            }
        }
    }
}
