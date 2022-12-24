using SharpDockerizer.Core.Models;

namespace SharpDockerizer.AppLayer.Events;
/// <summary>
/// Raised when a project was selected.
/// </summary>
public class ProjectSelectedEvent
{
    /// <summary>
    /// Project that was selected.
    /// </summary>
    public required ProjectData SelectedProject { get; set; }
}
