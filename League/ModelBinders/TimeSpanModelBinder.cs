using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace League.ModelBinders;

/// <summary>
/// Model binder used for <see cref="TimeSpan"/> values instead of <see cref="SimpleTypeModelBinder"/>,
/// which is the fallback model binder.
/// </summary>
public class TimeSpanModelBinder : IModelBinder
{
    private readonly IModelBinder _fallbackBinder;
    private readonly ILogger<TimeSpanModelBinder> _logger;

    private const DateTimeStyles _dateTimeStyles = DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AssumeLocal | DateTimeStyles.NoCurrentDateDefault;

    private readonly string[] _formats = 
    {
        "HH:mm:ss", "HH.mm.ss", "HHmmss",
        "HH:mm", "HH.mm", "HHmm", "HH",
        "H:mm", "H.mm",
        "hh:mm:sstt", "hh.mm.sstt", "hhmmsstt",
        "hh:mmtt", "hh.mmtt", "hhmmtt", "hhtt",
        "h:mmtt", "h.mmtt", "hmmtt", "htt"
    };
    
    public TimeSpanModelBinder(ILoggerFactory loggerFactory)
    {
        _fallbackBinder = new SimpleTypeModelBinder(typeof(DateTime), loggerFactory);
        _logger = loggerFactory.CreateLogger<TimeSpanModelBinder>();
    }

    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
        if (valueProviderResult == ValueProviderResult.None) return _fallbackBinder.BindModelAsync(bindingContext);

        bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueProviderResult);
        var valueAsString = valueProviderResult.FirstValue;

        if (!TryParseTime(valueAsString, out var time))
        {
            _logger.LogDebug("Could not bind model '{modelName}' to value '{valueAsString}', falling back to {fallbackBinder}", bindingContext.ModelName, valueAsString, nameof(SimpleTypeModelBinder));
            return _fallbackBinder.BindModelAsync(bindingContext);
        }

        bindingContext.Result = ModelBindingResult.Success(time);
        _logger.LogDebug("Parsed string '{originalValue}': {timeSpan} ", valueAsString, time);
        return Task.CompletedTask;
    }

    private bool TryParseTime(string text, out TimeSpan time)
    {
        if (text != null)
        {
            text = Regex.Replace(text, "([^0-9]|^)([0-9])([0-9]{2})([^0-9]|$)", "$1$2:$3$4");
            text = Regex.Replace(text, "^[0-9]$", "0$0");

            foreach (var format in _formats)
            {
                if (!DateTime.TryParseExact(text, format, CultureInfo.InvariantCulture, _dateTimeStyles,
                        out var value)) continue;

                time = value.TimeOfDay;
                return true;
            }
        }

        time = TimeSpan.Zero;
        return false;
    }
}