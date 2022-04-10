using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

namespace League.ModelBinders
{
    /// <summary>
    /// Model binder used for <see langword="string"/> values instead of <see cref="SimpleTypeModelBinder"/>,
    /// which is the fallback model binder. It removes all leading and trailing white-space characters from the <see langword="string"/>.
    /// </summary>
    public class StringTrimmingModelBinder : IModelBinder
    {
        private readonly IModelBinder _fallbackBinder;
        private readonly ILogger<DateTimeModelBinder> _logger;

        /// <summary>
        /// CTOR.
        /// </summary>
        /// <param name="loggerFactory"></param>
        public StringTrimmingModelBinder(ILoggerFactory loggerFactory)
        {
            _fallbackBinder = new SimpleTypeModelBinder(typeof(string), loggerFactory);
            _logger = loggerFactory.CreateLogger<DateTimeModelBinder>();
        }

        /// <inheritdoc/>
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            if (valueProviderResult == ValueProviderResult.None) return _fallbackBinder.BindModelAsync(bindingContext);

            var attr = (bindingContext.ModelMetadata as DefaultModelMetadata)?.Attributes.Attributes;
            if (!(attr != null && attr.All(a => a.GetType() != typeof(NoTrimmingAttribute))))
            {
                return _fallbackBinder.BindModelAsync(bindingContext);
            }
            
            bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueProviderResult);

            bindingContext.Result = ModelBindingResult.Success(valueProviderResult.FirstValue?.Trim());
            return Task.CompletedTask;
        }
    }
}
