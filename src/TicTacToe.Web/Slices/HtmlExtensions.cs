using System.Web;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TicTacToe.Web.Slices
{
    public static class HtmlExtensions
    {
        /// <summary>
        /// Renders validation data attributes for forms
        /// </summary>
        public static IHtmlContent FormValidationAttributes(
            this IHtmlHelper html,
            string? error = null
        ) =>
            string.IsNullOrEmpty(error)
                ? HtmlString.Empty
                : new HtmlString(
                    $"""data-error="{HttpUtility.HtmlAttributeEncode(error)}" aria-invalid="true" """
                );

        /// <summary>
        /// Renders an error message with datastar attributes
        /// </summary>
        public static IHtmlContent FormValidationMessage(
            this IHtmlHelper html,
            string? error = null
        ) =>
            string.IsNullOrEmpty(error)
                ? HtmlString.Empty
                : new HtmlString(
                    $"""<div class='validation-message' role='alert' data-error-message>{HttpUtility.HtmlEncode(error)}</div>"""
                );

        public static IHtmlContent FormAntiforgeryToken(
            this IHtmlHelper html,
            AntiforgeryTokenSet token
        ) =>
            new HtmlString(
                $"""<input name="{token.FormFieldName}" type="hidden" value="{token.RequestToken}" />"""
            );
    }
}
