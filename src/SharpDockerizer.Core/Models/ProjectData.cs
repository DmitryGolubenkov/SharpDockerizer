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
    public required string? DotNetVersion { get; set; }

    /// <summary>
    /// Relative path from solution folder to project folder (including project file)
    /// </summary>
    public required string RelativePathToProjFile { get; set; }

    /// <summary>
    /// If ASP.NET SDK usage is detected in project - this property will be <see langword="true"/>;
    /// </summary>
    public required bool IsAspNetProject { get; set; }

    /// <summary>
    /// If project contains Dockerfile template data
    /// </summary>
    public bool HasTemplate { get; set; }


    /// <summary>
    /// Checks if all properties of ProjectData are equal.
    /// </summary>
    public override bool Equals(object? obj)
    {
        if (obj is ProjectData projectData)
        {
            return ProjectName == projectData.ProjectName
                && AbsolutePathToProjFile == projectData.AbsolutePathToProjFile
                && DotNetVersion == projectData.DotNetVersion
                && RelativePathToProjFile == projectData.RelativePathToProjFile
                && IsAspNetProject == projectData.IsAspNetProject;
        }

        return false;
    }

    /// <summary>
    /// Updates reference with data from another instance.
    /// </summary>
    /// <param name="projectData">Instance to copy data from.</param>
    public void UpdateWithData(ProjectData projectData)
    {
        ProjectName = projectData.ProjectName;
        AbsolutePathToProjFile = projectData.AbsolutePathToProjFile;
        RelativePathToProjFile = projectData.RelativePathToProjFile;
        DotNetVersion = projectData.DotNetVersion;
        IsAspNetProject = projectData.IsAspNetProject;
    }
}
