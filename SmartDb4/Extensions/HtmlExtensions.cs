using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;

namespace SmartDb4.Extensions
{
    public static class HtmlExtensions
    {
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures",
            Justification = "This is an appropriate nesting of generic types")]
        public static MvcHtmlString LabelForRequired<TModel, TValue>(this HtmlHelper<TModel> html,
                                                                     Expression<Func<TModel, TValue>> expression,
                                                                     string labelText = "")
        {
            return LabelHelper(html,
                               ModelMetadata.FromLambdaExpression(expression, html.ViewData),
                               ExpressionHelper.GetExpressionText(expression), labelText);
        }

        private static MvcHtmlString LabelHelper(HtmlHelper html,
                                                 ModelMetadata metadata, string htmlFieldName, string labelText)
        {
            if (string.IsNullOrEmpty(labelText))
            {
                labelText = metadata.DisplayName ?? metadata.PropertyName ?? htmlFieldName.Split('.').Last();
            }

            if (string.IsNullOrEmpty(labelText))
            {
                return MvcHtmlString.Empty;
            }

            bool isRequired = false;

            if (metadata.ContainerType != null)
            {
                isRequired = metadata.ContainerType.GetProperty(metadata.PropertyName)
                                     .GetCustomAttributes(typeof (RequiredAttribute), false)
                                     .Length == 1;
            }

            TagBuilder tag = new TagBuilder("label");
            tag.Attributes.Add(
                "for",
                TagBuilder.CreateSanitizedId(
                    html.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(htmlFieldName)
                    )
                );

            if (isRequired)
                tag.Attributes.Add("class", "label-required");

            tag.SetInnerText(labelText);

            var output = tag.ToString(TagRenderMode.Normal);


            if (isRequired)
            {
                var asteriskTag = new TagBuilder("span");
                asteriskTag.Attributes.Add("class", "required");
                asteriskTag.SetInnerText("*");
                output += asteriskTag.ToString(TagRenderMode.Normal);
            }
            return MvcHtmlString.Create(output);
        }


        public static MvcHtmlString RequiredFieldFor<TModel, TValue>(this HtmlHelper<TModel> html,
                                                                     Expression<Func<TModel, TValue>> expression)
        {
            // Get the metadata for the model
            var metadata = ModelMetadata.FromLambdaExpression(expression, html.ViewData);

            // Check if the field is required
            var isRequired = metadata
                                 .ContainerType.GetProperty(metadata.PropertyName)
                                 .GetCustomAttributes(typeof (RequiredAttribute), false)
                                 .Length == 1;

            // If the field is required generate label with an asterix
            if (isRequired)
            {
                var labelText = "*";

                var tag = new TagBuilder("label");
                tag.MergeAttributes(new Dictionary<string, object>
                    {
                        {"style", "color: red; margin-left: -5px; margin-right: 5px; vertical-align: top;"}
                    });
                tag.SetInnerText(labelText);

                return MvcHtmlString.Create(tag.ToString(TagRenderMode.Normal));
            }

            return null;
        }
    }
}