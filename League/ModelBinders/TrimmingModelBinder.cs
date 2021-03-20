using System.Collections.Generic;
using System.Linq;
using System;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace League.ModelBinders
{
    /// <summary>
    /// When this attribute is set to a model property of type <see cref="string"/>, <see cref="TrimmingComplexModelBinder"/> will not trim the property value.
    /// </summary>
    public class NoTrimmingAttribute : Attribute
    { }

    /// <summary>
    /// Used in place of <see cref="ComplexTypeModelBinder"/> to trim beginning and ending whitespace from user input.
    /// </summary>
    public class TrimmingComplexModelBinder : ComplexTypeModelBinder
    {
        private readonly ILogger<TrimmingComplexModelBinder> _logger;

        public TrimmingComplexModelBinder(IDictionary<ModelMetadata, IModelBinder> propertyBinders,
            ILoggerFactory loggerFactory) : base(propertyBinders, loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<TrimmingComplexModelBinder>();
            //_logger.LogInformation("Instance created");
        }

        protected override void SetProperty(ModelBindingContext bindingContext, string modelName, ModelMetadata propertyMetadata, ModelBindingResult result)
        {
            var attr = (propertyMetadata as DefaultModelMetadata)?.Attributes.Attributes;
            if (result.Model is string value && attr != null && attr.All(a => a.GetType() != typeof(NoTrimmingAttribute)))
                result = ModelBindingResult.Success(value.Trim());
            base.SetProperty(bindingContext, modelName, propertyMetadata, result);
        }
    }

    /// <summary>
    /// Used in place of <see cref="ComplexTypeModelBinderProvider"/> to trim beginning and ending whitespace from user input.
    /// <see cref="GetBinder"/> is the same code as in <see cref="ComplexTypeModelBinderProvider"/>.
    /// </summary>
    /// <code>
    /// <remarks>Register in StartUp (in this example, ComplexTypeModelBinderProvider will be replaced):</remarks>
    /// services.AddMvc().AddMvcOptions(s => {
    ///    s.ModelBinderProviders[s.ModelBinderProviders.TakeWhile(p => !(p is ComplexTypeModelBinderProvider)).Count()] = new TrimmingModelBinderProvider();
    /// });
    /// </code>
    public class TrimmingComplexModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            if (!context.Metadata.IsComplexType || context.Metadata.IsCollectionType)
                return null;

            var propertyBinders = new Dictionary<ModelMetadata, IModelBinder>();
            foreach (var property in context.Metadata.Properties)
            {
                propertyBinders.Add(property, context.CreateBinder(property));
            }

            return new TrimmingComplexModelBinder(propertyBinders, context.Services.GetRequiredService<ILoggerFactory>());
        }
    }
}
