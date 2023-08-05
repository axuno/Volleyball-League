namespace League.Components;

/// <summary>
/// Interface for navigation node builders.
/// </summary>
public interface IMainNavigationNodeBuilder
{
    /// <summary>
    /// Gets the <see cref="MainNavigationComponentModel.NavigationNode"/>s.
    /// </summary>
    /// <returns>The <see cref="MainNavigationComponentModel.NavigationNode"/>s</returns>
    List<MainNavigationComponentModel.NavigationNode> GetNavigationNodes();

    /// <summary>
    /// Inserts a <see cref="MainNavigationComponentModel.NavigationNode"/> at the position of the <see cref="MainNavigationComponentModel.NavigationNode"/> with the given <see cref="MainNavigationComponentModel.NavigationNode.Key"/> name.
    /// If the <see cref="MainNavigationComponentModel.NavigationNode.Key"/> name cannot be found, the node is inserted at index zero.
    /// </summary>
    /// <param name="nodeToInsert"></param>
    /// <param name="nodeKeyName"></param>
    void InsertTopNavigationNode(MainNavigationComponentModel.NavigationNode nodeToInsert, string nodeKeyName);

    /// <summary>
    /// Tries to remove a <see cref="MainNavigationComponentModel.NavigationNode"/> at the position of the <see cref="MainNavigationComponentModel.NavigationNode"/> with the given <see cref="MainNavigationComponentModel.NavigationNode.Key"/> name.
    /// </summary>
    /// <param name="nodeKeyName"></param>
    /// <returns><see langword="true"/> if the node could be removed.</returns>
    bool TryRemoveTopNavigationNode(string nodeKeyName);
}