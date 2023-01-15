using SharpDockerizer.AppLayer.Models;

namespace SharpDockerizer.AppLayer.Contracts;
public interface IRecentlyOpenedSolutionsService
{
    /// <summary>
    /// Returns a list of recently opened solutions.
    /// </summary>
    List<RecentlyOpenedSolution> GetSolutions();
    /// <summary>
    /// Adds a solution to recently opened solutions, or moves it to top of the list.
    /// </summary>
    Task Add(RecentlyOpenedSolution solution);
    /// <summary>
    /// Removes a solution from recently opened solutions.
    /// </summary>
    Task RemoveWithPath(string path);
}