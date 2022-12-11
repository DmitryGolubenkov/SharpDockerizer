using SharpDockerizer.Core.Models;

namespace SharpDockerizer.AppLayer.Events;
public class ProjectSelectedEvent
{
    public required ProjectData SelectedProject { get; set; }
}
