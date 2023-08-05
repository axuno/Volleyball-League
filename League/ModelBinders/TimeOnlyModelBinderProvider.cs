using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

namespace League.ModelBinders;

/// <summary>
/// Uses <see cref="TimeOnlyModelBinder"/> for <see cref="TimeOnly"/> values instead of <see cref="SimpleTypeModelBinder"/>.
/// </summary>
/// <code>
/// <remarks>Register in StartUp:</remarks>
/// services.AddMvc().AddMvcOptions(s => {
///    s.ModelBinderProviders.Insert(0, new TimeSpanModelBinderProvider());
/// });
/// </code>
public class TimeOnlyModelBinderProvider : IModelBinderProvider
{
    /// <summary>
    /// /// Creates a <see cref="TimeOnlyModelBinder" /> based on <see cref="ModelBinderProviderContext" />.
    /// </summary>
    /// <param name="context"></param>
    /// <returns>Returns a <see cref="TimeOnlyModelBinder" /></returns>
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        if (context == null)
            throw new ArgumentNullException(nameof(context));

        return context.Metadata.ModelType == typeof(TimeOnly) || context.Metadata.ModelType == typeof(TimeOnly?)
            ? new TimeOnlyModelBinder(context.Services.GetRequiredService<ILoggerFactory>())
            : null;
    }
}
