using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

namespace League.ModelBinders;

/// <summary>
/// Model binder used for <see cref="DateOnly"/> values instead of <see cref="SimpleTypeModelBinder"/>,
/// which is the fallback model binder.
/// </summary>
public class DateOnlyModelBinder : IModelBinder
{
    private readonly IModelBinder _fallbackBinder;
    private readonly ILogger<DateOnlyModelBinder> _logger;

    private const DateTimeStyles _dateTimeStyles = DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AllowLeadingWhite | DateTimeStyles.AllowTrailingWhite;

    private readonly string[] _formats = 
    {
        "yyyyMMdd",
        "yyyy-MM-dd",
        "yy-MM-dd",
        "M/d/yyyy",
        "M/d/yy",
        "d.M.yyyy",
        "d.M.yy"
    };

    /// <summary>
    /// CTOR.
    /// </summary>
    /// <param name="loggerFactory"></param>
    public DateOnlyModelBinder(ILoggerFactory loggerFactory)
    {
        _fallbackBinder = new SimpleTypeModelBinder(typeof(DateTime), loggerFactory);
        _logger = loggerFactory.CreateLogger<DateOnlyModelBinder>();
    }

    /// <inheritdoc/>
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
        if (valueProviderResult == ValueProviderResult.None) return _fallbackBinder.BindModelAsync(bindingContext);
            
        bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueProviderResult);
        var valueAsString = valueProviderResult.FirstValue;

        if (!TryParseDateOnly(valueAsString, out var dateTime))
        {
            _logger.LogDebug("Could not bind model '{ModelName}' to value '{ValueAsString}', falling back to {fallbackBinder}", bindingContext.ModelName, valueAsString, nameof(SimpleTypeModelBinder));
            return _fallbackBinder.BindModelAsync(bindingContext);
        }

        bindingContext.Result = ModelBindingResult.Success(dateTime);
        _logger.LogDebug("Parsed string '{OriginalValue}': {DateTime} ", valueAsString, dateTime);
        return Task.CompletedTask;
    }

    private bool TryParseDateOnly(string? text, out DateOnly dateOnly)
    {
        if (text != null)
        {
            foreach (var format in _formats)
            {
                if (!DateOnly.TryParseExact(text, format, CultureInfo.InvariantCulture, _dateTimeStyles,
                        out dateOnly)) continue;

                return true;
            }
        }

        dateOnly = DateOnly.MinValue;
        return false;
    }
}
