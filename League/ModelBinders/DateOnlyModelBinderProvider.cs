using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

namespace League.ModelBinders;

/// <summary>
/// Uses <see cref="DateOnlyModelBinder"/> for <see cref="DateOnly"/> values instead of <see cref="SimpleTypeModelBinder"/>.
/// </summary>
/// <code>
/// <remarks>Register in StartUp:</remarks>
/// services.AddMvc().AddMvcOptions(s => {
///    s.ModelBinderProviders.Insert(0, new DateOnlyModelBinderProvider());
/// });
/// </code>
public class DateOnlyModelBinderProvider : IModelBinderProvider
{
    /// <inheritdoc/>
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        return context.Metadata.ModelType == typeof(DateOnly) || context.Metadata.ModelType == typeof(DateOnly?)
            ? new DateOnlyModelBinder(context.Services.GetRequiredService<ILoggerFactory>())
            : null;
    }
}
