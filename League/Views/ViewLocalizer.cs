// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Taken from: https://github.com/dotnet/aspnetcore/blob/main/src/Mvc/Mvc.Localization/src/ViewLocalizer.cs
// 2021-02-07: Modifications by axuno
// 2023-01-20: Modifications by axuno

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using MailMergeLib.AspNet;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Localization;

namespace League.Views;

/// <summary>
/// An <see cref="IViewLocalizer"/> implementation that derives the resource location from the
/// assembly name the <see cref="ControllerActionDescriptor"/> belongs to.
/// </summary>
/// <remarks>
/// See more details on this topic:
/// https://terryaney.wordpress.com/2021/01/04/migrating-to-net-core-overridable-localization-in-razor-class-libraries/
/// </remarks>
public class ViewLocalizer : IViewLocalizer, IViewContextAware
{
    private readonly IHtmlLocalizerFactory _localizerFactory;
    private string _applicationName;
    private IHtmlLocalizer? _localizer;

    /// <summary>
    /// Creates a new <see cref="ViewLocalizer"/>.
    /// </summary>
    /// <param name="localizerFactory">The <see cref="IHtmlLocalizerFactory"/>.</param>
    /// <param name="hostingEnvironment">The <see cref="IWebHostEnvironment"/>.</param>
    public ViewLocalizer(IHtmlLocalizerFactory localizerFactory, IWebHostEnvironment hostingEnvironment)
    {
        if (hostingEnvironment == null)
        {
            throw new ArgumentNullException(nameof(hostingEnvironment));
        }

        _applicationName = hostingEnvironment.ApplicationName;
            
        _localizerFactory = localizerFactory ?? throw new ArgumentNullException(nameof(localizerFactory));
    }

    /// <inheritdoc />
    public virtual LocalizedHtmlString this[string key]
    {
        get
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            return _localizer![key];
        }
    }

    /// <inheritdoc />
    public virtual LocalizedHtmlString this[string key, params object[] arguments]
    {
        get
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            return _localizer![key, arguments];
        }
    }

    /// <inheritdoc />
    public LocalizedString GetString(string name)
    {
        return _localizer!.GetString(name);
    }

    /// <inheritdoc />
    public LocalizedString GetString(string name, params object[] values)
    {
        return _localizer!.GetString(name, values);
    }

    /// <inheritdoc />
    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
    {
        return _localizer!.GetAllStrings(includeParentCultures);
    }

    /// <summary>
    /// Apply the specified <see cref="ViewContext"/>.
    /// </summary>
    /// <param name="viewContext">The <see cref="ViewContext"/>.</param>
    public void Contextualize(ViewContext viewContext)
    {
        if (viewContext == null)
        {
            throw new ArgumentNullException(nameof(viewContext));
        }

        // Given a view path "/Views/Home/Index.cshtml" we want a baseName like "MyApplication.Views.Home.Index"
        var path = viewContext.ExecutingFilePath;

        if (string.IsNullOrEmpty(path))
        {
            path = viewContext.View.Path;
        }

        Debug.Assert(!string.IsNullOrEmpty(path), "Couldn't determine a path for the view");

        #region ** Modification 2021-02-07 & 2023-01-20 by axuno **
        // To generate the path to the localization resource, the original ViewLocalizer
        // is using the hostingEnvironment.ApplicationName.
        // If the view is loaded from a different assembly (e.g. from a Razor Class Library),
        // the generated path is wrong for the _localizerFactory.
        // This is the workaround to set the applicationName to the Razor Class Library assembly:
        if (viewContext.ActionDescriptor is ControllerActionDescriptor { ControllerTypeInfo.Assembly.FullName: { } } controllerActionDescriptor)
        {
            var appName = new AssemblyName(controllerActionDescriptor.ControllerTypeInfo.Assembly.FullName).Name;
            if (appName != null) _applicationName = appName;
        } else if (viewContext.ActionDescriptor is { DisplayName: nameof(RazorViewToStringRenderer) })
        {
            // Action is invoked outside a ControllerContext
            // => RazorViewToStringRenderer where he have set the DisplayName
            _applicationName = nameof(League);
        }
        #endregion
            
        _localizer = _localizerFactory.Create(BuildBaseName(path), _applicationName);
    }

    private string BuildBaseName(string path)
    {
        var extension = Path.GetExtension(path);
        var startIndex = path[0] == '/' || path[0] == '\\' ? 1 : 0;
        var length = path.Length - startIndex - extension.Length;
        var capacity = length + _applicationName.Length + 1;
        var builder = new StringBuilder(path, startIndex, length, capacity);

        builder.Replace('/', '.').Replace('\\', '.');

        // Prepend the application name
        builder.Insert(0, '.');
        builder.Insert(0, _applicationName);

        return builder.ToString();
    }
}
