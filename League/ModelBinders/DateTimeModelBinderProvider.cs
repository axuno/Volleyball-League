// 
// Copyright SmartFormat Project maintainers and contributors.
// Licensed under the MIT license.

using System;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace League.ModelBinders;

/// <summary>
/// Uses <see cref="DateTimeModelBinder"/> for <see cref="DateTime"/> values instead of <see cref="SimpleTypeModelBinder"/>.
/// </summary>
/// <code>
/// <remarks>Register in StartUp:</remarks>
/// services.AddMvc().AddMvcOptions(s => {
///    s.ModelBinderProviders.Insert(0, new TimeSpanModelBinderProvider());
/// });
/// </code>
public class DateTimeModelBinderProvider : IModelBinderProvider
{
    /// <inheritdoc/>
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        if (context == null)
            throw new ArgumentNullException(nameof(context));

        return context.Metadata.ModelType == typeof(DateTime) || context.Metadata.ModelType == typeof(DateTime?)
            ? new DateTimeModelBinder(context.Services.GetRequiredService<ILoggerFactory>())
            : null;
    }
}
