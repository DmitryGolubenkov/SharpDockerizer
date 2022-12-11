namespace SharpDockerizer.Core.Models;
public class ProjectData
{
    /// <summary>
    /// Project name as displayed in Solution Explorer.
    /// </summary>
    public required string ProjectName { get; set; }
    /// <summary>
    /// Absolute path to .*proj file (including project file)
    /// </summary>
    public required string AbsolutePathToProjFile { get; set; }
    /// <summary>
    /// .NET version extracted from .*proj
    /// </summary>
    public required string DotNetVersion { get; set; }

    /// <summary>
    /// Relative path from solution folder to project folder (including project file)
    /// </summary>
    public required string RelativePath { get; set; }


    //public required bool IsAspNetCoreProject { get; set; }
    //public string? AspNetCoreVersion { get; set; }
}
