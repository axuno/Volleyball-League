using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

namespace League.ModelBinders;

/// <summary>
/// Model binder used for <see cref="TimeOnly"/> values instead of <see cref="SimpleTypeModelBinder"/>,
/// which is the fallback model binder.
/// </summary>
public class TimeOnlyModelBinder : IModelBinder
{
    private readonly IModelBinder _fallbackBinder;
    private readonly ILogger<TimeOnlyModelBinder> _logger;

    private const DateTimeStyles _dateTimeStyles = DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AllowLeadingWhite | DateTimeStyles.AllowTrailingWhite;

    private readonly string[] _formats = 
    {
        // 24-hour clock 
        "HH:mm:ss", "HH.mm.ss", "HHmmss",
        "HH:mm", "HH.mm", "HHmm", "HH",
        "H:mm", "H.mm",
        // 12-hour clock
        "hh:mm:sstt", "hh.mm.sstt", "hhmmsstt",
        "hh:mmtt", "hh.mmtt", "hhmmtt", "hhtt",
        "h:mmtt", "h.mmtt", "hmmtt", "htt"
    };

    public TimeOnlyModelBinder(ILoggerFactory loggerFactory)
    {
        _fallbackBinder = new SimpleTypeModelBinder(typeof(DateTime), loggerFactory);
        _logger = loggerFactory.CreateLogger<TimeOnlyModelBinder>();
    }

    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
        if (valueProviderResult == ValueProviderResult.None) return _fallbackBinder.BindModelAsync(bindingContext);

        bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueProviderResult);
        var valueAsString = valueProviderResult.FirstValue;

        if (!TryParseTimeOnly(valueAsString, out var time))
        {
            _logger.LogDebug("Could not bind model '{modelName}' to value '{valueAsString}', falling back to {fallbackBinder}", bindingContext.ModelName, valueAsString, nameof(SimpleTypeModelBinder));
            return _fallbackBinder.BindModelAsync(bindingContext);
        }

        bindingContext.Result = ModelBindingResult.Success(time);
        _logger.LogDebug("Parsed string '{originalValue}': {timeSpan} ", valueAsString, time);
        return Task.CompletedTask;
    }

    private bool TryParseTimeOnly(string? text, out TimeOnly time)
    {
        if (text != null)
        {
            text = Regex.Replace(text, "([^0-9]|^)([0-9])([0-9]{2})([^0-9]|$)", "$1$2:$3$4");
            text = Regex.Replace(text, "^[0-9]$", "0$0");

            foreach (var format in _formats)
            {
                if (!TimeOnly.TryParseExact(text, format, CultureInfo.InvariantCulture, _dateTimeStyles,
                        out time)) continue;

                return true;
            }
        }
        
        time = TimeOnly.MinValue;
        return false;
    }
}
