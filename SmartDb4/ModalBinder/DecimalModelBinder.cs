using System;
using System.Globalization;
using System.Web.Mvc;

namespace SmartDb4.ModalBinder
{
    public class DecimalModelBinder : IModelBinder
    {
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            ValueProviderResult valueResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

            var modelState = new ModelState { Value = valueResult };

            object actualValue = null;
            try
            {
                actualValue = Convert.ToDecimal(valueResult.AttemptedValue, CultureInfo.CurrentCulture);
            }
            catch (FormatException e)
            {
                modelState.Errors.Add(e);
            }

            bindingContext.ModelState.Add(bindingContext.ModelName, modelState);

            return actualValue;
        }
    }
}