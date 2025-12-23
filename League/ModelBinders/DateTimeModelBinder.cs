using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

namespace League.ModelBinders;

/// <summary>
/// Model binder used for <see cref="DateTime"/> values instead of <see cref="SimpleTypeModelBinder"/>,
/// which is the fallback model binder.
/// </summary>
public class DateTimeModelBinder : IModelBinder
{
    private readonly SimpleTypeModelBinder _fallbackBinder;
    private readonly ILogger<DateTimeModelBinder> _logger;

    private const DateTimeStyles _dateTimeStyles = DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AllowLeadingWhite | DateTimeStyles.AllowTrailingWhite;

    private readonly string[] _formats =
    [
        // H := 24-hour clock, h := 12-hour clock
        "yyyyMMddHHmmss",
        "yyyyMMddHHmm",
        "yyyyMMdd",
        "yyyy-MM-dd HH:mm:ss",
        "yyyy-MM-dd HH:mm",
        "yyyy-MM-dd",
        "M/d/yyyy hh:mm:sstt",
        "M/d/yyyy HH:mm:ss",
        "M/d/yyyy hh:mmtt",
        "M/d/yyyy HH:mm",
        "M/d/yyyy",
        "d.M.yyyy HH:mm:ss",
        "d.M.yyyy HH:mm",
        "d.M.yyyy"
    ];

    /// <summary>
    /// CTOR.
    /// </summary>
    /// <param name="loggerFactory"></param>
    public DateTimeModelBinder(ILoggerFactory loggerFactory)
    {
        _fallbackBinder = new SimpleTypeModelBinder(typeof(DateTime), loggerFactory);
        _logger = loggerFactory.CreateLogger<DateTimeModelBinder>();
    }

    /// <inheritdoc/>
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
        if (valueProviderResult == ValueProviderResult.None) return _fallbackBinder.BindModelAsync(bindingContext);
            
        bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueProviderResult);
        var valueAsString = valueProviderResult.FirstValue;

        if (!TryParseDateTime(valueAsString, out var dateTime))
        {
            _logger.LogDebug("Could not bind model '{ModelName}' to value '{ValueAsString}', falling back to {fallbackBinder}", bindingContext.ModelName, valueAsString, nameof(SimpleTypeModelBinder));
            return _fallbackBinder.BindModelAsync(bindingContext);
        }

        bindingContext.Result = ModelBindingResult.Success(dateTime);
        _logger.LogDebug("Parsed string '{OriginalValue}': {DateTime} ", valueAsString, dateTime);
        return Task.CompletedTask;
    }

    private bool TryParseDateTime(string? text, out DateTime dateTime)
    {
        if (text != null)
        {
            foreach (var format in _formats)
            {
                if (!DateTime.TryParseExact(text, format, CultureInfo.InvariantCulture, _dateTimeStyles,
                        out dateTime)) continue;

                return true;
            }
        }

        dateTime = DateTime.MinValue;
        return false;
    }
}
