using SharpDockerizer.Core.Models;

namespace SharpDockerizer.AppLayer.Contracts;
public interface IProjectDataExporter
{
    /// <summary>
    /// Reads .*proj XML file and retrieves information about project
    /// </summary>
    /// <param name="path">Path to project file including file name and extension.</param>
    Task<ProjectData> GetProjectData(string path);
}
