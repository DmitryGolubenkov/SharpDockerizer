using SharpDockerizer.Core.Models;

namespace SharpDockerizer.AppLayer.Contracts;
public interface ICurrentSolutionInfo
{
    SolutionData? CurrentSolution { get; set; }
    List<ProjectData>? Projects { get; set; }
}
