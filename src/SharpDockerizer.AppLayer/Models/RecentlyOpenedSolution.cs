namespace SharpDockerizer.AppLayer.Models;
public class RecentlyOpenedSolution
{
    /// <summary>
    /// Solution name
    /// </summary>
    public required string Name { get; set; }
    /// <summary>
    /// Path to solution .sln file
    /// </summary>
    public required string AbsolutePath { get; set; }
}
