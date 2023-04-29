using SharpDockerizer.Core.Models;

namespace SharpDockerizer.AppLayer.Contracts;
public interface INuGetConfigExtractor
{
    /// <summary>
    /// Checks if "nuget.config" file exists in directories from solution folder 
    /// to project folder and generates COPY instructions for them.
    /// </summary>
    /// <param name="selectedProjectData">Data about selected project</param>
    /// <returns>Multiline string that contains COPY instructions for nuget.config</returns>
    string GetNuGetConfigFiles(ProjectData selectedProjectData);
}