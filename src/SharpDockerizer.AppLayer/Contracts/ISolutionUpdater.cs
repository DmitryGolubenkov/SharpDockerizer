namespace SharpDockerizer.AppLayer.Contracts;
public interface ISolutionUpdater
{
    /// <summary>
    /// Refreshes currently loaded solution data.
    /// </summary>
    /// <returns>If solution was updated.</returns>
    Task<bool> RefreshSolution();
}
