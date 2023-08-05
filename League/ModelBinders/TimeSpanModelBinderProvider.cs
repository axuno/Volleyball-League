using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

namespace League.ModelBinders;

/// <summary>
/// Uses <see cref="TimeSpanModelBinder"/> for <see cref="TimeSpan"/> values instead of <see cref="SimpleTypeModelBinder"/>.
/// </summary>
/// <code>
/// <remarks>Register in StartUp:</remarks>
/// services.AddMvc().AddMvcOptions(s => {
///    s.ModelBinderProviders.Insert(0, new TimeSpanModelBinderProvider());
/// });
/// </code>
public class TimeSpanModelBinderProvider : IModelBinderProvider
{
    /// <summary>
    /// /// Creates a <see cref="TimeSpanModelBinder" /> based on <see cref="ModelBinderProviderContext" />.
    /// </summary>
    /// <param name="context"></param>
    /// <returns>Returns a <see cref="TimeSpanModelBinder" /></returns>
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        if (context == null)
            throw new ArgumentNullException(nameof(context));

        return context.Metadata.ModelType == typeof(TimeSpan) || context.Metadata.ModelType == typeof(TimeSpan?)
            ? new TimeSpanModelBinder(context.Services.GetRequiredService<ILoggerFactory>())
            : null;
    }
}
