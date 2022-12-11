using SharpDockerizer.Core;
using SharpDockerizer.Core.Models;

namespace SharpDockerizer.AppLayer.Contracts;
public interface IProjectDependenciesExporter
{
    List<ProjectData> GetDependencies(ProjectData project);
}
