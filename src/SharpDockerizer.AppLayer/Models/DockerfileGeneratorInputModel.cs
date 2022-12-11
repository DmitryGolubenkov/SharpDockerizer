using SharpDockerizer.Core.Models;

namespace SharpDockerizer.AppLayer.Models;
public class DockerfileGeneratorInputModel
{
    public required ProjectData SelectedProjectData { get; set; }
    public required List<NuGetSource> NuGetSources { get; set; }
    public required List<int> ExposedPorts { get; set; }
}
