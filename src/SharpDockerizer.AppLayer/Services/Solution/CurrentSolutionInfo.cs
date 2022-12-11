using SharpDockerizer.Core.Models;
using SharpDockerizer.AppLayer.Contracts;

namespace SharpDockerizer.AppLayer.Services.Solution;
public class CurrentSolutionInfo : ICurrentSolutionInfo
{
    public SolutionData? CurrentSolution { get; set; }
    public List<ProjectData>? Projects { get; set; }
}
