using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

namespace League.ModelBinders;

/// <summary>
/// Uses <see cref="StringTrimmingModelBinderProvider"/> for <see langword="string"/> values instead of <see cref="SimpleTypeModelBinder"/>.
/// </summary>
/// <code>
/// <remarks>Register in class StartUp:</remarks>
/// services.AddMvc().AddMvcOptions(s => {
///    s.ModelBinderProviders.Insert(0, new StringTrimmingModelBinderProvider());
/// });
/// </code>
public class StringTrimmingModelBinderProvider : IModelBinderProvider
{
    /// <inheritdoc/>
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        return context.Metadata.ModelType == typeof(string)
            ? new StringTrimmingModelBinder(context.Services.GetRequiredService<ILoggerFactory>())
            : null;
    }
}
