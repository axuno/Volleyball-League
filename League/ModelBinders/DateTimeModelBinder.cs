using System;
using System.Globalization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace League.ModelBinders
{
    /// <summary>
    /// Model binder used for <see cref="DateTime"/> values instead of <see cref="SimpleTypeModelBinder"/>,
    /// which is the fallback model binder.
    /// </summary>
    public class DateTimeModelBinder : IModelBinder
    {
        private readonly IModelBinder _fallbackBinder;
        private ILogger<DateTimeModelBinder> _logger;

        private const DateTimeStyles _dateTimeStyles = DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AssumeLocal | DateTimeStyles.NoCurrentDateDefault;

        private readonly string[] _formats = 
        {
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
        };
    
        public DateTimeModelBinder(ILoggerFactory loggerFactory)
        {
            _fallbackBinder = new SimpleTypeModelBinder(typeof(DateTime), loggerFactory);
            _logger = loggerFactory.CreateLogger<DateTimeModelBinder>();
        }

        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            if (valueProviderResult == ValueProviderResult.None) return _fallbackBinder.BindModelAsync(bindingContext);
            
            bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueProviderResult);
            var valueAsString = valueProviderResult.FirstValue;

            if (!TryParseDateTime(valueAsString, out var dateTime))
            {
                if (_logger.IsEnabled(LogLevel.Debug)) _logger.LogDebug($"Could not bind model '{bindingContext.ModelName}' to value '{valueAsString}', falling back to {nameof(SimpleTypeModelBinder)}");
                return _fallbackBinder.BindModelAsync(bindingContext);
            }

            bindingContext.Result = ModelBindingResult.Success(dateTime);
            return Task.CompletedTask;
        }

        private bool TryParseDateTime(string text, out DateTime dateTime)
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
        /// <summary>
        /// /// Creates a <see cref="DateTimeModelBinder" /> based on <see cref="ModelBinderProviderContext" />.
        /// </summary>
        /// <param name="context"></param>
        /// <returns>Returns a <see cref="DateTimeModelBinder" /></returns>
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            return context.Metadata.ModelType == typeof(DateTime) || context.Metadata.ModelType == typeof(DateTime?)
                ? new DateTimeModelBinder(context.Services.GetRequiredService<ILoggerFactory>())
                : null;
        }
    }
}
