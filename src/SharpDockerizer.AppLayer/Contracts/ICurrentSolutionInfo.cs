using SharpDockerizer.Core.Models;

namespace SharpDockerizer.AppLayer.Contracts;
/// <summary>
/// Information about currently opened solution.
/// </summary>
public interface ICurrentSolutionInfo
{
    /// <summary>
    /// Parsed solution data.
    /// </summary>
    SolutionData? CurrentSolution { get; set; }
    /// <summary>
    /// Projects inside solution.
    /// </summary>
    List<ProjectData>? Projects { get; set; }
}
