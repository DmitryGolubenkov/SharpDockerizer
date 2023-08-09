namespace SharpDockerizer.Core.Models;
public class SolutionData
{
    /// <summary>
    /// Solution name
    /// </summary>
    public required string Name { get; set; }
    /// <summary>
    /// Absolute path to solution directory.
    /// </summary>
    public required string SolutionRootDirectoryPath { get; set; }
    /// <summary>
    /// Absolute path to .sln file.
    /// </summary>
    public required string SolutionFilePath { get; set; }

    /// <summary>
    /// If solution contains Dockerfile template data
    /// </summary>
    public bool HasTemplate { get; set; }

}
