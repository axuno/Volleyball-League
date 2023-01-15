using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace League.Components;

/// <summary>
/// <see cref="ViewComponent"/> to display the main navigation based on bootstrap 4
/// </summary>
public class MainNavigation : ViewComponent
{
    private readonly IMainNavigationNodeBuilder _mainNavigationNodeBuilder;
    private readonly ILogger<MainNavigation> _logger;

    /// <summary>
    /// CTOR.
    /// </summary>
    /// <param name="mainNavigationNodeBuilder"></param>
    /// <param name="logger"></param>
    public MainNavigation(IMainNavigationNodeBuilder mainNavigationNodeBuilder, ILogger<MainNavigation> logger)
    {
        _mainNavigationNodeBuilder = mainNavigationNodeBuilder;
        _logger = logger;
    }

    /// <summary>
    /// Creates the model for the component and renders it.
    /// </summary>
    /// <returns>The <see cref="IViewComponentResult"/>.</returns>
    public IViewComponentResult Invoke()
    {
        var model = new MainNavigationComponentModel();
        model.TopNavigationNodes.AddRange(_mainNavigationNodeBuilder.GetNavigationNodes());
        _logger.LogDebug($"{nameof(MainNavigation)} created");
            
        return View(model);
    }
}